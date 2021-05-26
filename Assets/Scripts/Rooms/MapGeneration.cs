using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGeneration : MonoBehaviour
{
    [SerializeField] private GameObject initialRoomPrefab;
    [SerializeField] private GameObject corridorPrefab;

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
    private readonly float roomLength = 20;
    private readonly float corridorLength = 10;
    private float distanceBetweenRoomCenters;
    private int remainingFloors;

    private void Start()
    {
        roomPositions = new Dictionary<(int, int), GameObject>();
        corridorPositions = new List<Tuple<float, float>>();
        distanceBetweenRoomCenters = roomLength + corridorLength;
        remainingFloors = numberOfRooms;

        PlaceInitialRooms();
        while (remainingFloors > 0)
            PlaceRandomRoom();
        PlaceCorridors();
    }

    private void PlaceRoom(int x, int z)
    {
        GameObject newRoom = InstantiateRoom(x, z);
        roomPositions.Add((x, z), newRoom);
        remainingFloors--;
    }

    private void PlaceCorridor(float x, float z, bool rotated)
    {
        corridorPositions.Add(Tuple.Create(x, z));
        GameObject newCorridor = InstantiateCorridors(x, z, rotated);
        SetCorridorInRooms(newCorridor, x, z, rotated);
    }

    private GameObject InstantiateRoom(int x, int z)
    {
        Vector3 roomPositionInScene =
            new Vector3(distanceBetweenRoomCenters * x, 0, distanceBetweenRoomCenters * z);
        return Instantiate(initialRoomPrefab, roomPositionInScene, Quaternion.identity);
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
        }
        else
        {
            pos1 = "TR";
            pos2 = "BL";
            int x1 = (int) (x - 0.5f);
            int x2 = (int) (x + 0.5f);
            room1 = roomPositions[(x1, (int) z)];
            room2 = roomPositions[(x2, (int) z)];
        }

        room1.GetComponent<Room>().SetCorridorAndDestroyDoorSpace(pos1, corridor);
        room2.GetComponent<Room>().SetCorridorAndDestroyDoorSpace(pos2, corridor);
    }

    // Crea la habitación inicial y las habitaciones que la rodean
    private void PlaceInitialRooms()
    {
        PlaceRoom(0, 0); // Habitación central

        if (!guaranteedSurroundedInitialRoom)
            return;

        if (initialRoomPosZ + 1 <= maxZ && remainingFloors > 0) // Arriba
            PlaceRoom(0, 1);
        if (initialRoomPosX + 1 <= maxX && remainingFloors > 0) // Derecha
            PlaceRoom(1, 0);
        if (initialRoomPosZ - 1 >= minZ && remainingFloors > 0) // Abajo
            PlaceRoom(0, -1);
        if (initialRoomPosX - 1 >= minX && remainingFloors > 0) // Izquierda
            PlaceRoom(-1, 0);
    }

    // En cada iteración, obtiene todas las casillas en las que se podría colocar una habitación, y de esas casillas
    // escoge una al azar y coloca una habitación en ella
    private void PlaceRandomRoom()
    {
        List<Tuple<int, int>> possibleNewRooms = GetPossibleNewRoomPositions();
        if (possibleNewRooms.Count == 0)
            return;

        int pos = Random.Range(0, possibleNewRooms.Count);
        Tuple<int, int> square = possibleNewRooms[pos];
        PlaceRoom(square.Item1, square.Item2);
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
}