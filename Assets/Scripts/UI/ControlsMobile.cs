using UnityEngine;
using UnityEngine.UI;

public class ControlsMobile : MonoBehaviour
{
    private Transform playerTransform;
    private Player playerScript;

    [SerializeField] private Joystick rotationJoystick;
    [SerializeField] private Joystick movementJoystick;

    // Si en este mismo frame se han pulsado o dejado de pulsar los botones
    private bool castForcePressed;
    private bool castAreaPressed;
    private bool castSelfPressed;
    private bool castMagickPressed;
    private bool castForceUnpressed;
    private bool castAreaUnpressed;

    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        playerScript = playerTransform.GetComponent<Player>();
        castForcePressed = false;
        castAreaPressed = false;
    }

    private void Update()
    {
        RotationAndMovement();
        StartCastInput();
        StopCastInput();
        ResetCastInput();
    }

    // movementJoystick gira y rota al jugador
    // rotationJoystick sólamente lo rota
    // Si están pulsados los dos joysticks al mismo tiempo, sólo hace caso a movementJoystick
    private void RotationAndMovement()
    {
        bool move = true;
        bool rotate = true;

        float horizontalInput = movementJoystick.Horizontal;
        float verticalInput = movementJoystick.Vertical;

        if (horizontalInput == 0 && verticalInput == 0)
        {
            move = false;
            horizontalInput = rotationJoystick.Horizontal;
            verticalInput = rotationJoystick.Vertical;
            if (horizontalInput == 0 && verticalInput == 0)
                rotate = false;
        }

        if (rotate)
        {
            // El vector al que mira el joystick
            Vector3 rotateTo = new Vector3(horizontalInput, 0, verticalInput);
            // Roto el vector para ajustarse a la orientación de la cámara respecto al mapa.
            rotateTo = Quaternion.Euler(0, 45, 0) * rotateTo;
            // Creo un punto cuya posición sea la suma de la posición del jugador más el vector anterior
            rotateTo += playerTransform.position + rotateTo;
            // Hago que el jugador mire a esa posición
            playerScript.rotationInput = rotateTo;
        }

        if (move)
            playerScript.moveInput = true;
    }

    public void LoadWater()
    {
        playerScript.elementInput = "WAT";
    }

    public void LoadLife()
    {
        playerScript.elementInput = "LIF";
    }

    public void LoadShield()
    {
        playerScript.elementInput = "SHI";
    }

    public void LoadCold()
    {
        playerScript.elementInput = "COL";
    }

    public void LoadLightning()
    {
        playerScript.elementInput = "LIG";
    }

    public void LoadArcane()
    {
        playerScript.elementInput = "ARC";
    }

    public void LoadEarth()
    {
        playerScript.elementInput = "EAR";
    }

    public void LoadFire()
    {
        playerScript.elementInput = "FIR";
    }

    public void CastForceDown()
    {
        castForcePressed = true;
    }

    public void CastForceUp()
    {
        castForceUnpressed = true;
    }

    public void CastAreaDown()
    {
        castAreaPressed = true;
    }

    public void CastAreaUp()
    {
        castAreaUnpressed = true;
    }

    public void CastSelfDown()
    {
        castSelfPressed = true;
    }

    public void CastMagickDown()
    {
        castMagickPressed = true;
    }

    private void StartCastInput()
    {
        string castInput = "";

        if (castForcePressed)
            castInput = "FOC";
        else if (castAreaPressed)
            castInput = "ARE";
        else if (castSelfPressed)
            castInput = "SEL";
        else if (castMagickPressed)
            castInput = "MAG";

        playerScript.startCastInput = castInput;
    }

    private void StopCastInput()
    {
        if (castForceUnpressed || castAreaUnpressed)
            playerScript.stopCastInput = true;
    }

    private void ResetCastInput()
    {
        castForcePressed = false;
        castAreaPressed = false;
        castSelfPressed = false;
        castMagickPressed = false;
        castForceUnpressed = false;
        castAreaUnpressed = false;
    }
}