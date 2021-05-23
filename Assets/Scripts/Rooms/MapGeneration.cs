using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGeneration : MonoBehaviour
{
    public GameObject cube;
    private Dictionary<(int, int), bool> map; // Matriz con true o false según haya habitación en cada casilla
    private readonly int minX = -4;
    private readonly int maxX = 4;
    private readonly int minZ = -4;
    private readonly int maxZ = 4;

    private void Start()
    {
        map = new Dictionary<(int, int), bool>();

        InitializeEmptyMap();
        PlaceInitialRooms();
        PlaceRandomRooms(10); // Estas habitaciones son sin contar las 5 centrales
        PlaceMapInScene();
    }

    // Inizializa el mapa vacío, sin habitaciones
    private void InitializeEmptyMap()
    {
        for (int x = minX; x <= maxX; x++)
        for (int z = minZ; z <= maxZ; z++)
            map.Add((x, z), false);
    }

    // En cada iteración, obtiene todas las casillas en las que se podría colocar una habitación, y de esas casillas
    // escoge una al azar y coloca una habitación en ella
    private void PlaceRandomRooms(int num)
    {
        for (int i = 0; i < num; i++)
        {
            List<Tuple<int, int>> possibleNewRooms = GetPossibleNewRooms();
            if (possibleNewRooms.Count == 0)
                return;

            int pos = Random.Range(0, possibleNewRooms.Count);
            Tuple<int, int> square = possibleNewRooms[pos];
            map[(square.Item1, square.Item2)] = true;
        }
    }

    // Devuelve una lista con todas las casillas donde se podría colocar una habitación
    // Es decir, todas las casillas vacías que estén tocando una habitación ya colocada y que no se salgan del mapa
    private List<Tuple<int, int>> GetPossibleNewRooms()
    {
        List<Tuple<int, int>> possibleNewRooms = new List<Tuple<int, int>>();

        for (int x = minX; x <= maxX; x++)
        for (int z = minZ; z <= maxZ; z++)
        {
            if (!map[(x, z)])
                continue;

            List<Tuple<int, int>> adjacentSquares = GetAdjacentSquares(x, z);
            foreach (Tuple<int, int> square in adjacentSquares)
                if (IsRoomPossible(square.Item1, square.Item2, possibleNewRooms))
                    possibleNewRooms.Add(Tuple.Create(square.Item1, square.Item2));
        }

        return possibleNewRooms;
    }

    // Devuelve las 4 casillas que rodean a una habitación
    private List<Tuple<int, int>> GetAdjacentSquares(int x, int z)
    {
        List<Tuple<int, int>> adjacentSquares = new List<Tuple<int, int>>();

        adjacentSquares.Add(Tuple.Create(x, z + 1)); // Arriba
        adjacentSquares.Add(Tuple.Create(x + 1, z)); // Derecha
        adjacentSquares.Add(Tuple.Create(x, z - 1)); // Abajo
        adjacentSquares.Add(Tuple.Create(x - 1, z)); // Izquierda

        return adjacentSquares;
    }

    // Devuelve si se puede crear una habitación en esa casilla
    private bool IsRoomPossible(int x, int z, List<Tuple<int, int>> possibleNewRooms)
    {
        // Si se sale de los límites mapa
        if (x < minX || x > maxX || z < minZ || z > maxZ)
            return false;

        // Si ya está ocupada
        if (map[(x, z)])
            return false;

        // Si ya está añadida a la lista de posibles posiciones para nueva habitación
        if (possibleNewRooms.Contains(Tuple.Create(x, z)))
            return false;

        return true;
    }

    // Crea las 5 habitaciones centrales
    private void PlaceInitialRooms()
    {
        map[(0, 0)] = true; // Habitación central
        map[(0, 1)] = true; // Arriba
        map[(1, 0)] = true; // Derecha
        map[(0, -1)] = true; // Abajo
        map[(-1, 0)] = true; // Izquierda
    }

    // Instancia en el mapa todas las habitaciones del mapa, cada una en sus coordenadas
    private void PlaceMapInScene()
    {
        for (int x = minX; x <= maxX; x++)
        for (int z = minZ; z <= maxZ; z++)
        {
            if (map[(x, z)])
                Instantiate(cube, new Vector3(x, 0, z), Quaternion.identity);
        }
    }
}