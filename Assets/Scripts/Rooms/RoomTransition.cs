using System.Collections;
using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    [SerializeField] private Corridor corridorScript;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject otherTrigger;
    [SerializeField] private char nextRoomChar;

    private Transform player;
    private Transform cameras;
    private MapGeneration mapGeneration;
    private GameObject previousRoom;
    private GameObject nextRoom;
    private Room nextRoomScript;
    private Transform newCameraPosition;

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        cameras = GameObject.Find("Cameras").transform;
        mapGeneration = GameObject.Find("Manager").GetComponent<MapGeneration>();

        // Cada Corridor tiene dos objetos que provocan trigger, uno a cada lado. Cada uno lleva al lado contrario
        if (nextRoomChar == 'A')
        {
            nextRoom = corridorScript.GetComponent<Corridor>().roomA;
            previousRoom = corridorScript.GetComponent<Corridor>().roomB;
        }
        else
        {
            nextRoom = corridorScript.GetComponent<Corridor>().roomB;
            previousRoom = corridorScript.GetComponent<Corridor>().roomA;
        }

        nextRoomScript = nextRoom.GetComponent<Room>();
        newCameraPosition = nextRoom.GetComponent<Room>().cameraPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(MakeTransition(player.position, exitPoint.position,
                cameras.position, newCameraPosition.position, 1.5f));
    }

    // 1 - Desactivo el trigger del otro lado del Corridor, para que el jugador no lo active al atravesar el pasillo
    // 2 - Activo la habitación a la que voy a pasar y vuelvo a generar los NavMesh
    // 3 - Muevo poco a poco el jugador al otro extremo del pasillo y la cámara a la nueva habitación
    // 4 - Activo los enemigos de la nueva habitación y cierro sus puertas
    // 5 - Vuelvo a activar el otro trigger del pasillo y desactivo la habitación de la que acabo de salir
    private IEnumerator MakeTransition(Vector3 playerInitial, Vector3 playerFinal, Vector3 cameraInitial,
        Vector3 cameraFinal, float duration)
    {
        DisableOtherTrigger();
        EnableRoom(nextRoom);
        mapGeneration.BakeNavMeshSurfaces();

        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            player.position = Vector3.Lerp(playerInitial, playerFinal, counter / duration);
            cameras.position = Vector3.Lerp(cameraInitial, cameraFinal, counter / duration);
            yield return null;
        }

        EnableNextRoomEnemies();
        if (nextRoomScript.enemies.GetComponent<RoomEnemies>().CountEnemies() != 0)
            CloseNextRoomDoors();
        EnableOtherTrigger();
        DisableRoom(previousRoom);
    }

    private void EnableOtherTrigger()
    {
        otherTrigger.SetActive(true);
    }

    private void DisableOtherTrigger()
    {
        otherTrigger.SetActive(false);
    }

    private void CloseNextRoomDoors()
    {
        nextRoomScript.CloseDoors();
    }

    private void EnableRoom(GameObject room)
    {
        room.SetActive(true);
    }

    private void DisableRoom(GameObject room)
    {
        room.SetActive(false);
    }

    private void EnableNextRoomEnemies()
    {
        nextRoomScript.EnableEnemies();
    }
}