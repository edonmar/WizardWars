using System.Collections;
using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    [SerializeField] private Corridor corridorScript;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject otherTrigger;
    [SerializeField] private char exitRoomChar;

    private Transform player;
    private Transform cameras;
    private GameObject exitRoom;
    private Transform newCameraPosition;

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        cameras = GameObject.Find("Cameras").transform;

        // Cada Corridor tiene dos objetos que provocan trigger, uno a cada lado. Cada uno lleva al lado contrario
        exitRoom = exitRoomChar == 'A'
            ? corridorScript.GetComponent<Corridor>().roomA
            : corridorScript.GetComponent<Corridor>().roomB;
        newCameraPosition = exitRoom.GetComponent<Room>().cameraPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(MakeTransition(player.position, exitPoint.position,
                cameras.position, newCameraPosition.position, 1.5f));
    }

    // 1 - Desactivo el trigger del otro lado del Corridor, para que el jugador no lo active al atravesar el pasillo
    // 2 - Muevo poco a poco el jugador al otro extremo del pasillo y la cámara a la nueva habitación
    // 3 - Vuelvo a activar el otro trigger del pasillo
    private IEnumerator MakeTransition(Vector3 playerInitial, Vector3 playerFinal, Vector3 cameraInitial,
        Vector3 cameraFinal, float duration)
    {
        DisableOtherTrigger();
        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            player.position = Vector3.Lerp(playerInitial, playerFinal, counter / duration);
            cameras.position = Vector3.Lerp(cameraInitial, cameraFinal, counter / duration);
            yield return null;
        }

        EnableOtherTrigger();
    }

    private void EnableOtherTrigger()
    {
        otherTrigger.SetActive(true);
    }

    private void DisableOtherTrigger()
    {
        otherTrigger.SetActive(false);
    }
}