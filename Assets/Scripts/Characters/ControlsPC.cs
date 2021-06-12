using System;
using UnityEngine;

public class ControlsPC : MonoBehaviour
{
    public Player playerScript;
    private string elementInput = "";
    private string castInput = "";

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        RotationInput();
        MovementInput();
        ElementInput();
        if (elementInput == "")
            StartCastInput();
        StopCastInput();
    }

    private void RotationInput()
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

        playerScript.rotationInput = targetPosition;
    }

    private void MovementInput()
    {
        if (Input.GetKey(KeyCode.Mouse0))
            playerScript.moveInput = true;
    }

    private void ElementInput()
    {
        elementInput = "";

        if (Input.GetKeyDown(KeyCode.Q))
            elementInput = "WAT";
        else if (Input.GetKeyDown(KeyCode.W))
            elementInput = "LIF";
        else if (Input.GetKeyDown(KeyCode.E))
            elementInput = "SHI";
        else if (Input.GetKeyDown(KeyCode.R))
            elementInput = "COL";
        else if (Input.GetKeyDown(KeyCode.A))
            elementInput = "LIG";
        else if (Input.GetKeyDown(KeyCode.S))
            elementInput = "ARC";
        else if (Input.GetKeyDown(KeyCode.D))
            elementInput = "EAR";
        else if (Input.GetKeyDown(KeyCode.F))
            elementInput = "FIR";

        if (elementInput != "")
            playerScript.elementInput = elementInput;
    }

    private void StartCastInput()
    {
        castInput = "";

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Mouse1))
            castInput = "FOC";
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.LeftShift))
            castInput = "ARE";
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Mouse2))
            castInput = "SEL";
        //else if (Input.GetKeyDown(KeyCode.Alpha5))
            //castInput = "WEA";
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Space))
            castInput = "MAG";

        playerScript.startCastInput = castInput;
    }

    private void StopCastInput()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Mouse1) ||
            Input.GetKeyUp(KeyCode.LeftShift))
            playerScript.stopCastInput = true;
    }
}