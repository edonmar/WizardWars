using UnityEngine;

public class Player : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        RotatePlayerTowardsCamera();
        if (Input.GetKey(KeyCode.Mouse0))
            MovePlayerForward();
    }

    private void RotatePlayerTowardsCamera()
    {
        // La capa con la que chocará el rayo, ignorando las demás capas
        int layerMask = LayerMask.GetMask("CameraRayCollider");
        
        // Lanza un rayo de la cámara a la posición del cursor del ratón
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Si el rayo choca con un objeto de esa capa
        if (!Physics.Raycast(cameraRay, out RaycastHit cameraRayHit, Mathf.Infinity, layerMask))
            return;
        
        // Gira al jugador en el eje Y para que mire a la posición donde ha chocado el rayo
        Vector3 targetPosition = new Vector3(cameraRayHit.point.x, transform.position.y, cameraRayHit.point.z);
        transform.LookAt(targetPosition);
    }

    private void MovePlayerForward()
    {
        float movSpeed = 4;
        transform.Translate(transform.forward * (movSpeed * Time.deltaTime), Space.World);
    }
}