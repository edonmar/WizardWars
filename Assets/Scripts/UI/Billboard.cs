using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    private Transform cameraTransform;

    private void Start()
    {
        mainCamera = Camera.main;
        cameraTransform = mainCamera.transform;
    }
    
    private void LateUpdate()
    {
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}