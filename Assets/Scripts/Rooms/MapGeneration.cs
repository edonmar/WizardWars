using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MapGeneration : MonoBehaviour
{
    [SerializeField] private GameObject initialRoomPrefab;
    [SerializeField] private GameObject finalRoomPrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject deathZonePrefab;

    [SerializeField] private int numberOfRooms;
    [SerializeField] private int initialRoomPosX;
    [SerializeField] private int initialRoomPosZ;
    [SerializeField] private bool guaranteedSurroundedInitialRoom;

    // Límites del mapa
    [SerializeField] private int minX;
    [SerializeField] private int maxX;
    [SerializeField] private int minZ;
    [SerializeField] private int maxZ;

    private Dictionary<(int, int), GameObject> roomPositions; // Posiciones en las que habrá habitación
    private List<Tuple<float, float>> corridorPositions; // Posiciones en las que habrá pasillo
    private GameObject[] roomPrefabs; // Cada habitación entre inicio y fin será una de estas elegida al azar
    private Dictionary<GameObject, int> roomPrefabsDict; // Número de veces que ha salido cada habitación
    private readonly float roomLength = 20; // Tamaño en metros de cada habitación
    private readonly float corridorLength = 12; // Tamaño en metros de cada pasillo
    private float distanceBetweenRoomCenters;
    private int remainingFloors;
    private StageManager stageManager;
    private GameObject player;
    private GameObject navMeshSurfaces;

    private void Start()
    {
        roomPositions = new Dictionary<(int, int), GameObject>();
        corridorPositions = new List<Tuple<float, float>>();
        roomPrefabs = Resources.LoadAll<GameObject>("Rooms");
        roomPrefabsDict = roomPrefabs.ToDictionary(rp => rp, rp => 0);
        distanceBetweenRoomCenters = roomLength + corridorLength;
        remainingFloors = numberOfRooms;
        player = GameObject.Find("Player").gameObject;
        navMeshSurfaces = GameObject.Find("NavMeshSurfaces").gameObject;
        stageManager = GameObject.Find("Manager").GetComponent<StageManager>();

        DisablePlayer();
        PlaceInitialRooms();
        while (remainingFloors > 1)
            PlaceRoomInRandomSquare(GetRandomRoom());
        PlaceFinalRoom();
        PlaceCorridors();
        PlaceDeathZone();
        BakeNavMeshSurfaces();
        MovePlayer();
        EnablePlayer();
        stageManager.currentRoom = roomPositions[(initialRoomPosX, initialRoomPosZ)];
        stageManager.StartTimer();
        stageManager.SetTotalRooms(numberOfRooms - 2);
    }

    private GameObject GetRandomRoom()
    {
        // Número de veces que ha salido la habitación menos repetida
        int min = roomPrefabsDict.Values.Select(s => s).Min();

        // Lista de habitaciones con el valor mínimo de repeticiones
        List<GameObject> minRooms = (from rp in roomPrefabsDict where rp.Value == min select rp.Key).ToList();

        // Habitación aleatoria entre las menos repetidas
        int pos = Random.Range(0, minRooms.Count);
        GameObject room = minRooms[pos];

        // Sumo 1 en el número de veces que ha salido esa habitación
        roomPrefabsDict[room] += 1;

        return room;
    }

    private void PlaceRoom(int x, int z, GameObject room, bool active)
    {
        GameObject newRoom = InstantiateRoom(x, z, room);
        if (!active)
            newRoom.SetActive(false);
        roomPositions.Add((x, z), newRoom);
        remainingFloors--;
    }

    private void PlaceCorridor(float x, float z, bool rotated)
    {
        corridorPositions.Add(Tuple.Create(x, z));
        GameObject newCorridor = InstantiateCorridors(x, z, rotated);
        SetCorridorInRooms(newCorridor, x, z, rotated);
    }

    private GameObject InstantiateRoom(int x, int z, GameObject room)
    {
        Vector3 roomPositionInScene =
            new Vector3(distanceBetweenRoomCenters * x, 0, distanceBetweenRoomCenters * z);
        return Instantiate(room, roomPositionInScene, Quaternion.identity);
    }

    private GameObject InstantiateCorridors(float x, float z, bool rotated)
    {
        float posX = x * distanceBetweenRoomCenters;
        float posZ = z * distanceBetweenRoomCenters;

        Vector3 corridorPositionInScene = new Vector3(posX, 0, posZ);
        Quaternion corridorRotation = rotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        return Instantiate(corridorPrefab, corridorPositionInScene, corridorRotation);
    }

    // Obtiene las dos habitaciones que conecta el pasillo, y le pasa al script de cada habitación el objeto del pasillo
    // y su posición (en cuál de los lados de la habitación está el pasillo)
    private void SetCorridorInRooms(GameObject corridor, float x, float z, bool rotated)
    {
        Corridor corridorScript = corridor.GetComponent<Corridor>();
        GameObject room1;
        GameObject room2;
        string pos1;
        string pos2;

        if (rotated)
        {
            pos1 = "TL";
            pos2 = "BR";
            int z1 = (int) (z - 0.5f);
            int z2 = (int) (z + 0.5f);
            room1 = roomPositions[((int) x, z1)];
            room2 = roomPositions[((int) x, z2)];
            corridorScript.roomA = room2;
            corridorScript.roomB = room1;
        }
        else
        {
            pos1 = "TR";
            pos2 = "BL";
            int x1 = (int) (x - 0.5f);
            int x2 = (int) (x + 0.5f);
            room1 = roomPositions[(x1, (int) z)];
            room2 = roomPositions[(x2, (int) z)];
            corridorScript.roomA = room1;
            corridorScript.roomB = room2;
        }

        room1.GetComponent<Room>().SetCorridorAndDestroyDoorSpace(pos1, corridor);
        room2.GetComponent<Room>().SetCorridorAndDestroyDoorSpace(pos2, corridor);
    }

    // Crea la habitación inicial y las habitaciones que la rodean
    private void PlaceInitialRooms()
    {
        PlaceRoom(0, 0, initialRoomPrefab, true); // Habitación central

        if (!guaranteedSurroundedInitialRoom)
            return;

        if (initialRoomPosZ + 1 <= maxZ && remainingFloors > 0) // Arriba
            PlaceRoom(0, 1, GetRandomRoom(), false);
        if (initialRoomPosX + 1 <= maxX && remainingFloors > 0) // Derecha
            PlaceRoom(1, 0, GetRandomRoom(), false);
        if (initialRoomPosZ - 1 >= minZ && remainingFloors > 0) // Abajo
            PlaceRoom(0, -1, GetRandomRoom(), false);
        if (initialRoomPosX - 1 >= minX && remainingFloors > 0) // Izquierda
            PlaceRoom(-1, 0, GetRandomRoom(), false);
    }

    private void PlaceFinalRoom()
    {
        PlaceRoomInRandomSquare(finalRoomPrefab);
    }

    // Obtiene todas las casillas en las que se podría colocar una habitación, y de esas casillas
    // escoge una al azar y coloca una habitación en ella
    private void PlaceRoomInRandomSquare(GameObject room)
    {
        List<Tuple<int, int>> possibleNewRooms = GetPossibleNewRoomPositions();
        if (possibleNewRooms.Count == 0)
            return;

        int pos = Random.Range(0, possibleNewRooms.Count);
        Tuple<int, int> square = possibleNewRooms[pos];
        PlaceRoom(square.Item1, square.Item2, room, false);
    }

    // Devuelve una lista con todas las casillas donde se podría colocar una habitación
    // Es decir, todas las casillas vacías que estén tocando una habitación ya colocada y que no se salgan del mapa
    private List<Tuple<int, int>> GetPossibleNewRoomPositions()
    {
        return (from roomPos in roomPositions
            from adSquare in GetAdjacentSquares(roomPos.Key.Item1, roomPos.Key.Item2)
            let adX = adSquare.Item1
            let adZ = adSquare.Item2
            where !roomPositions.ContainsKey((adX, adZ))
            select Tuple.Create(adX, adZ)).ToList();
    }

    // Devuelve las 4 casillas que rodean a una habitación
    private List<Tuple<int, int>> GetAdjacentSquares(int x, int z)
    {
        List<Tuple<int, int>> adjacentSquares = new List<Tuple<int, int>>();

        if (z + 1 <= maxZ)
            adjacentSquares.Add(Tuple.Create(x, z + 1)); // Arriba
        if (x + 1 <= maxX)
            adjacentSquares.Add(Tuple.Create(x + 1, z)); // Derecha
        if (z - 1 >= minZ)
            adjacentSquares.Add(Tuple.Create(x, z - 1)); // Abajo
        if (x - 1 >= minX)
            adjacentSquares.Add(Tuple.Create(x - 1, z)); // Izquierda

        return adjacentSquares;
    }

    // TODO en mapas grandes podría ser más eficiente si no comprobara las habitaciones que ya están rodeadas
    private void PlaceCorridors()
    {
        foreach (KeyValuePair<(int, int), GameObject> roomPos in roomPositions)
        {
            int x = roomPos.Key.Item1;
            int z = roomPos.Key.Item2;
            PlaceCorridorIfPossible(x, z, 0, 1, true); // Arriba
            PlaceCorridorIfPossible(x, z, 1, 0, false); // Derecha
            PlaceCorridorIfPossible(x, z, 0, -1, true); // Abajo
            PlaceCorridorIfPossible(x, z, -1, 0, false); // Izquierda
        }
    }

    // Sabiendo que hay habitación en (x, z), comprueba si hay habitación en (x + offsetX, z + offsetZ)
    // Si la hay, crea un pasillo en la posición media entre ambas habitaciones
    private void PlaceCorridorIfPossible(int x, int z, int offsetX, int offsetZ, bool rotated)
    {
        float middleX = x + (float) offsetX / 2;
        float middleZ = z + (float) offsetZ / 2;

        if (corridorPositions.Contains(Tuple.Create(middleX, middleZ)))
            return;
        if (roomPositions.ContainsKey((x + offsetX, z + offsetZ)))
            PlaceCorridor(middleX, middleZ, rotated);
    }

    private void MovePlayer()
    {
        Vector3 initialRoomPos =
            new Vector3(distanceBetweenRoomCenters * initialRoomPosX, 0, distanceBetweenRoomCenters * initialRoomPosZ);
        player.transform.position = initialRoomPos;
    }

    private void EnablePlayer()
    {
        player.SetActive(true);
    }

    private void DisablePlayer()
    {
        player.SetActive(false);
    }

    public void BakeNavMeshSurfaces()
    {
        foreach (Transform nms in navMeshSurfaces.transform)
        {
            NavMeshSurface nmsScript = nms.GetComponent<NavMeshSurface>();
            nmsScript.BuildNavMesh();
        }
    }

    private void PlaceDeathZone()
    {
        float sizeXInRooms = Math.Abs(maxX - minX + 1);
        float sizeZInRooms = Math.Abs(maxZ - minZ + 1);
        float posX = sizeXInRooms / 2f;
        float posZ = sizeZInRooms / 2f;
        float scaleX = (sizeXInRooms + 2) * 1.5f * roomLength;
        float scaleZ = (sizeZInRooms + 2) * 1.5f * roomLength;
        Vector3 pos = new Vector3(posX, -5, posZ);

        GameObject deathZone = Instantiate(deathZonePrefab, pos, quaternion.identity);
        deathZone.transform.localScale = new Vector3(scaleX, 1, scaleZ);
    }
}