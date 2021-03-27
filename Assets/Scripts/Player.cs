using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Camera mainCamera;

    private Sprite spriteEleWater;
    private Sprite spriteEleLife;
    private Sprite spriteEleShield;
    private Sprite spriteEleCold;
    private Sprite spriteEleLightning;
    private Sprite spriteEleArcane;
    private Sprite spriteEleEarth;
    private Sprite spriteEleFire;
    private Sprite spriteEleIce;
    private Sprite spriteEleSteam;

    private Image[] imgCurrEle;

    private List<string> loadedElements;

    private void Start()
    {
        mainCamera = Camera.main;

        spriteEleWater = Resources.Load<Sprite>("Images/ElementIcons/EleWater");
        spriteEleLife = Resources.Load<Sprite>("Images/ElementIcons/EleLife");
        spriteEleShield = Resources.Load<Sprite>("Images/ElementIcons/EleShield");
        spriteEleCold = Resources.Load<Sprite>("Images/ElementIcons/EleCold");
        spriteEleLightning = Resources.Load<Sprite>("Images/ElementIcons/EleLightning");
        spriteEleArcane = Resources.Load<Sprite>("Images/ElementIcons/EleArcane");
        spriteEleEarth = Resources.Load<Sprite>("Images/ElementIcons/EleEarth");
        spriteEleFire = Resources.Load<Sprite>("Images/ElementIcons/EleFire");
        spriteEleIce = Resources.Load<Sprite>("Images/ElementIcons/EleIce");
        spriteEleSteam = Resources.Load<Sprite>("Images/ElementIcons/EleSteam");

        Transform gameUITransform = GameObject.Find("GameUI").transform;
        imgCurrEle = new Image[5];
        imgCurrEle[0] = gameUITransform.Find("CurrentElements/Ele0").GetComponent<Image>();
        imgCurrEle[1] = gameUITransform.Find("CurrentElements/Ele1").GetComponent<Image>();
        imgCurrEle[2] = gameUITransform.transform.Find("CurrentElements/Ele2").GetComponent<Image>();
        imgCurrEle[3] = gameUITransform.transform.Find("CurrentElements/Ele3").GetComponent<Image>();
        imgCurrEle[4] = gameUITransform.transform.Find("CurrentElements/Ele4").GetComponent<Image>();

        loadedElements = new List<string>();
    }

    private void Update()
    {
        RotatePlayerTowardsCamera();
        if (Input.GetKey(KeyCode.Mouse0))
            MovePlayerForward();

        ElementInput();
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

    private void ElementInput()
    {
        string element = "";

        if (Input.GetKeyDown(KeyCode.Q))
            element = "WAT";
        else if (Input.GetKeyDown(KeyCode.W))
            element = "LIF";
        else if (Input.GetKeyDown(KeyCode.E))
            element = "SHI";
        else if (Input.GetKeyDown(KeyCode.R))
            element = "COL";
        else if (Input.GetKeyDown(KeyCode.A))
            element = "LIG";
        else if (Input.GetKeyDown(KeyCode.S))
            element = "ARC";
        else if (Input.GetKeyDown(KeyCode.D))
            element = "EAR";
        else if (Input.GetKeyDown(KeyCode.F))
            element = "FIR";
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            element = "";
            loadedElements.Clear();
            ShowLoadedElements();
        }

        if (element == "")
            return;

        LoadElement(element);
        ShowLoadedElements();
    }

    // Añade un elemento a la combinación, comprueba sus reacciones con los elementos ya añadidos y si es necesario
    // los mezcla o elimina los elementos contrarios
    private void LoadElement(string element)
    {
        loadedElements.Add(element);
        int len = loadedElements.Count;

        for (int i = len - 1; i > 0; i--)
        {
            bool stop = false;
            switch (loadedElements[i])
            {
                case "WAT":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (loadedElements[j] != "LIG")
                            continue;

                        loadedElements.RemoveAt(i);
                        loadedElements.RemoveAt(j);
                        i--;
                        stop = true;
                        break;
                    }

                    if (!stop)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            switch (loadedElements[j])
                            {
                                case "COL":
                                    loadedElements[j] = "ICE";
                                    loadedElements.RemoveAt(i);
                                    i--;
                                    stop = true;
                                    break;

                                case "FIR":
                                    loadedElements[j] = "STE";
                                    loadedElements.RemoveAt(i);
                                    i--;
                                    stop = true;
                                    break;
                            }

                            if (stop)
                                break;
                        }
                    }

                    break;

                case "LIF":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (loadedElements[j] == "ARC")
                        {
                            loadedElements.RemoveAt(i);
                            loadedElements.RemoveAt(j);
                            i--;
                            stop = true;
                        }

                        if (stop)
                            break;
                    }

                    break;

                case "SHI":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (loadedElements[j] == "SHI")
                        {
                            loadedElements.RemoveAt(i);
                            loadedElements.RemoveAt(j);
                            i--;
                            stop = true;
                        }

                        if (stop)
                            break;
                    }

                    break;

                case "COL":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        switch (loadedElements[j])
                        {
                            case "FIR":
                                loadedElements.RemoveAt(i);
                                loadedElements.RemoveAt(j);
                                i--;
                                stop = true;
                                break;

                            case "STE":
                                loadedElements[j] = "WAT";
                                loadedElements.RemoveAt(i);
                                i--;
                                stop = true;
                                break;
                        }

                        if (stop)
                            break;
                    }

                    if (!stop)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (loadedElements[j] != "WAT")
                                continue;

                            loadedElements[j] = "ICE";
                            loadedElements.RemoveAt(i);
                            i--;
                            break;
                        }
                    }

                    break;

                case "LIG":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (loadedElements[j] == "WAT" || loadedElements[j] == "EAR")
                        {
                            loadedElements.RemoveAt(i);
                            loadedElements.RemoveAt(j);
                            i--;
                            stop = true;
                        }

                        if (stop)
                            break;
                    }

                    break;

                case "ARC":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (loadedElements[j] == "LIF")
                        {
                            loadedElements.RemoveAt(i);
                            loadedElements.RemoveAt(j);
                            i--;
                            stop = true;
                        }

                        if (stop)
                            break;
                    }

                    break;

                case "EAR":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (loadedElements[j] == "LIG")
                        {
                            loadedElements.RemoveAt(i);
                            loadedElements.RemoveAt(j);
                            i--;
                            stop = true;
                        }

                        if (stop)
                            break;
                    }

                    break;

                case "FIR":
                    for (int j = i - 1; j >= 0; j--)
                    {
                        switch (loadedElements[j])
                        {
                            case "COL":
                                loadedElements.RemoveAt(i);
                                loadedElements.RemoveAt(j);
                                i--;
                                stop = true;
                                break;

                            case "ICE":
                                loadedElements[j] = "WAT";
                                loadedElements.RemoveAt(i);
                                i--;
                                stop = true;
                                break;
                        }

                        if (stop)
                            break;
                    }

                    if (!stop)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (loadedElements[j] != "WAT")
                                continue;

                            loadedElements[j] = "STE";
                            loadedElements.RemoveAt(i);
                            i--;

                            break;
                        }
                    }

                    break;
            }
        }

        if (loadedElements.Count == 6)
            loadedElements.RemoveAt(5);
    }

    // Muestra los elementos seleccionados actualmente
    private void ShowLoadedElements()
    {
        int len = loadedElements.Count;
        int casillasVacias = 5 - len;

        // Hace invisibles las casillas vacías
        for (int i = 5; i > 5 - casillasVacias; i--)
        {
            imgCurrEle[i - 1].sprite = null;
            imgCurrEle[i - 1].color = new Color(255, 255, 255, 0f);
        }

        // Muestra las casillas ocupadas
        for (int i = 0; i < len; i++)
        {
            imgCurrEle[i].color = new Color(255, 255, 255, 1f);

            imgCurrEle[i].sprite = loadedElements[i] switch
            {
                "WAT" => spriteEleWater,
                "LIF" => spriteEleLife,
                "SHI" => spriteEleShield,
                "COL" => spriteEleCold,
                "LIG" => spriteEleLightning,
                "ARC" => spriteEleArcane,
                "EAR" => spriteEleEarth,
                "FIR" => spriteEleFire,
                "ICE" => spriteEleIce,
                "STE" => spriteEleSteam,
                _ => imgCurrEle[i].sprite
            };
        }
    }
}