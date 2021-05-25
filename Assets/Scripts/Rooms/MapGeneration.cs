using System;
using System.Collections.Generic;
using System.Linq;
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

    private List<Tuple<int, int>> roomPositions; // Posiciones en las que habrá habitación
    private readonly float roomLength = 20;
    private readonly float corridorLength = 10;
    private float distanceBetweenRoomCenters;
    private int remainingFloors;

    private void Start()
    {
        roomPositions = new List<Tuple<int, int>>();
        distanceBetweenRoomCenters = roomLength + corridorLength;
        remainingFloors = numberOfRooms;

        PlaceInitialRooms();
        while (remainingFloors > 0)
            PlaceRandomRoom();
        InstantiateRooms();
    }

    private void PlaceRoom(int x, int z)
    {
        roomPositions.Add(Tuple.Create(x, z));
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

    // Instancia en el mapa todas las habitaciones del mapa, cada una en sus coordenadas
    private void InstantiateRooms()
    {
        foreach (var (x, z) in roomPositions)
        {
            Vector3 roomPositionInScene =
                new Vector3(distanceBetweenRoomCenters * x, 0, distanceBetweenRoomCenters * z);
            Instantiate(initialRoom, roomPositionInScene, Quaternion.identity);
        }
    }
}