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

    private List<Tuple<int, int>> roomPositions; // Posiciones en las que habrá habitación
    private Dictionary<(int, int), bool> corridorPositions; // Posiciones en las que habrá pasillo, y si estará rotado
    private readonly float roomLength = 20;
    private readonly float corridorLength = 10;
    private float distanceBetweenRoomCenters;
    private int remainingFloors;

    private void Start()
    {
        roomPositions = new List<Tuple<int, int>>();
        corridorPositions = new Dictionary<(int, int), bool>();
        distanceBetweenRoomCenters = roomLength + corridorLength;
        remainingFloors = numberOfRooms;

        PlaceInitialRooms();
        while (remainingFloors > 0)
            PlaceRandomRoom();
        InstantiateRooms();
        PlaceCorridors();
        InstantiateCorridors();
    }

    private void PlaceRoom(int x, int z)
    {
        roomPositions.Add(Tuple.Create(x, z));
        remainingFloors--;
    }

    private void PlaceCorridor(int x, int z, bool rotated)
    {
        corridorPositions.Add((x, z), rotated);
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
            from adSquare in GetAdjacentSquares(roomPos.Item1, roomPos.Item2)
            let adX = adSquare.Item1
            let adZ = adSquare.Item2
            where !roomPositions.Contains(Tuple.Create(adX, adZ))
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

    /*
     * TODO tengo que modificar este algoritmo. Para mapas pequeños puede valer, pero tiene los problemas:
     * 1 - El número de pasillos siempre es igual al número de habitaciones menos uno. Aunque conecta todas las
     *     habitaciones, no conecta todas con sus circundantes, y a veces hay que dar un rodeo para ir de una habitación
     *     a otra que está al lado, porque no ha generado pasillo entre las dos
     * 2 - (No ha pasado nunca en mapas pequeños de 15 habitaciones, pero es raro el mapa de 100 habitaciones en el que
     *     no se repite el error varias veces). En ocasiones poco frecuentes crea un pasillo dentro de una habitación,
     *     o un pasillo que no lleva a ninguna parte. A veces (muy raro) esto deja habitaciones a las que no se
     *     puede acceder
     * 3 - Es muy lento si hay números muy grandes de habitaciones. Hasta 100 carga rápido, a partir de ahí el tiempo
     *     de carga aumenta exponencialmente
     */
    private void PlaceCorridors()
    {
        foreach (var (x, z) in roomPositions)
        {
            PlaceCorridorIfPossible(x, z + 1, true); // Arriba
            PlaceCorridorIfPossible(x + 1, z, false); // Derecha
            PlaceCorridorIfPossible(x, z - 1, true); // Abajo
            PlaceCorridorIfPossible(x - 1, z, false); // Izquierda
        }
    }

    private void PlaceCorridorIfPossible(int x, int z, bool rotated)
    {
        if (corridorPositions.ContainsKey((x, z)))
            return;
        if (x == 0 && z == 0)
            return;
        if (roomPositions.Contains(Tuple.Create(x, z)))
            PlaceCorridor(x, z, rotated);
    }

    // Instancia en el mapa todas las habitaciones del mapa, cada una en sus coordenadas
    private void InstantiateRooms()
    {
        foreach (var (x, z) in roomPositions)
        {
            Vector3 roomPositionInScene =
                new Vector3(distanceBetweenRoomCenters * x, 0, distanceBetweenRoomCenters * z);
            Instantiate(initialRoomPrefab, roomPositionInScene, Quaternion.identity);
        }
    }

    private void InstantiateCorridors()
    {
        foreach (KeyValuePair<(int, int), bool> corridorPos in corridorPositions)
        {
            int x = corridorPos.Key.Item1;
            int z = corridorPos.Key.Item2;
            bool rotated = corridorPos.Value;

            float offsetX = distanceBetweenRoomCenters;
            float offsetZ = distanceBetweenRoomCenters;

            if (!rotated)
                offsetX /= 2;
            else
                offsetZ /= 2;

            float posX = x == 0 ? 0 : offsetX + (Math.Abs(x) - 1) * distanceBetweenRoomCenters;
            float posZ = z == 0 ? 0 : offsetZ + (Math.Abs(z) - 1) * distanceBetweenRoomCenters;

            if (x < 0)
                posX = -posX;
            if (z < 0)
                posZ = -posZ;

            Vector3 corridorPositionInScene = new Vector3(posX, 0, posZ);
            Quaternion corridorRotation = rotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
            Instantiate(corridorPrefab, corridorPositionInScene, corridorRotation);
        }
    }
}