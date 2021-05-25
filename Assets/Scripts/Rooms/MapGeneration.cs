using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGeneration : MonoBehaviour
{
    [SerializeField] private GameObject initialRoom;
    [SerializeField] private GameObject corridor;

    [SerializeField] private int numberOfRooms;
    [SerializeField] private int initialRoomPosX;
    [SerializeField] private int initialRoomPosZ;
    [SerializeField] private bool guaranteedSurroundedInitialRoom;

    // Límites del mapa
    [SerializeField] private int minX;
    [SerializeField] private int maxX;
    [SerializeField] private int minZ;
    [SerializeField] private int maxZ;

    private Dictionary<(int, int), bool> mapRooms; // Matriz con true o false según haya habitación en cada casilla
    private readonly float roomLength = 20;
    private readonly float corridorLength = 10;
    private float distanceBetweenRoomCenters;
    private int remainingFloors;

    private void Start()
    {
        mapRooms = new Dictionary<(int, int), bool>();
        distanceBetweenRoomCenters = roomLength + corridorLength;
        remainingFloors = numberOfRooms;

        InitializeEmptyMap();
        PlaceInitialRooms();
        while (remainingFloors > 0)
            PlaceRandomRoom();
        InstantiateRooms();
    }

    // Inicializa el mapa vacío, sin habitaciones
    private void InitializeEmptyMap()
    {
        for (int x = minX; x <= maxX; x++)
        for (int z = minZ; z <= maxZ; z++)
            mapRooms.Add((x, z), false);
    }

    private void PlaceRoom(int x, int z)
    {
        mapRooms[(x, z)] = true;
        remainingFloors--;
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
        List<Tuple<int, int>> possibleNewRooms = GetPossibleNewRooms();
        if (possibleNewRooms.Count == 0)
            return;

        int pos = Random.Range(0, possibleNewRooms.Count);
        Tuple<int, int> square = possibleNewRooms[pos];
        PlaceRoom(square.Item1, square.Item2);
    }

    // Devuelve una lista con todas las casillas donde se podría colocar una habitación
    // Es decir, todas las casillas vacías que estén tocando una habitación ya colocada y que no se salgan del mapa
    private List<Tuple<int, int>> GetPossibleNewRooms()
    {
        List<Tuple<int, int>> possibleNewRooms = new List<Tuple<int, int>>();

        foreach (KeyValuePair<(int, int), bool> square in mapRooms)
        {
            if (!square.Value)
                continue;

            int x = square.Key.Item1;
            int z = square.Key.Item2;
            List<Tuple<int, int>> adjacentSquares = GetAdjacentSquares(x, z);
            foreach (Tuple<int, int> adSquare in adjacentSquares)
            {
                int adX = adSquare.Item1;
                int adZ = adSquare.Item2;
                if (IsRoomPossibleInSquare(adX, adZ, possibleNewRooms))
                    possibleNewRooms.Add(Tuple.Create(adX, adZ));
            }
        }

        return possibleNewRooms;
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

    // Devuelve si se puede crear una habitación en esa casilla
    private bool IsRoomPossibleInSquare(int x, int z, List<Tuple<int, int>> possibleNewRooms)
    {
        // Si ya está ocupada
        if (mapRooms[(x, z)])
            return false;

        // Si ya está añadida a la lista de posibles posiciones para nueva habitación
        if (possibleNewRooms.Contains(Tuple.Create(x, z)))
            return false;

        return true;
    }

    // Instancia en el mapa todas las habitaciones del mapa, cada una en sus coordenadas
    private void InstantiateRooms()
    {
        foreach (KeyValuePair<(int, int), bool> square in mapRooms)
        {
            if (!square.Value)
                continue;

            int x = square.Key.Item1;
            int z = square.Key.Item2;
            Instantiate(initialRoom, new Vector3(distanceBetweenRoomCenters * x, 0, distanceBetweenRoomCenters * z),
                Quaternion.identity);
        }
    }
}