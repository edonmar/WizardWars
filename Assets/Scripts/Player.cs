using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private GameManager manager;
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
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
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
        CastInput();
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

    private void CastInput()
    {
        string castType = "";

        if (Input.GetKeyDown(KeyCode.Alpha1))
            castType = "FOC";
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            castType = "ARE";
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            castType = "SEL";
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            castType = "WEA";

        switch (castType)
        {
            case "":
                return;
            case "WEA" when loadedElements.Count == 0:
                // Ataque de arma
                print("Ataque de arma");
                return;
        }

        List<int> priorityList = GetPriorityList();
        OrderLoadedElements(priorityList);
        string spellType = GetSpellType(castType);

        // Si intento hacer lanzamiento propio sin cargar ningún elemento, no hace nada
        if (castType == "SEL" && spellType == "force")
            return;

        HandleCastSpell(castType, spellType);
    }

    // Obtengo un array con el valor de prioridad de cada elemento de loadedElements
    // 1 = prioridad mas alta
    private List<int> GetPriorityList()
    {
        List<int> priorityList = new List<int>();

        for (int i = 0, len = loadedElements.Count; i < len; i++)
        {
            switch (loadedElements[i])
            {
                case "SHI":
                    priorityList.Add(1);
                    break;

                case "EAR":
                case "ICE":
                    priorityList.Add(2);
                    break;

                case "LIF":
                case "ARC":
                    priorityList.Add(3);
                    break;

                case "STE":
                    priorityList.Add(4);
                    break;

                case "LIG":
                    priorityList.Add(5);
                    break;

                case "WAT":
                case "COL":
                case "FIR":
                    priorityList.Add(6);
                    break;
            }
        }

        return priorityList;
    }

    // Tomando como referencia la lista de prioridad, ordena el array de elementos
    // de mayor a menor prioridad usando el método de inserción
    private void OrderLoadedElements(IList<int> priorityList)
    {
        for (int i = 1, len = priorityList.Count; i < len; i++)
        {
            int priority = priorityList[i];
            string element = loadedElements[i];
            int j = i - 1;

            while (j >= 0 && priorityList[j] > priority)
            {
                priorityList[j + 1] = priorityList[j];
                loadedElements[j + 1] = loadedElements[j];
                j--;
            }

            priorityList[j + 1] = priority;
            loadedElements[j + 1] = element;
        }
    }

    private string GetSpellType(string castType)
    {
        string spellType;

        if (loadedElements.Count == 0)
            // force
            spellType = "force";
        else
            spellType = castType switch
            {
                // Enfocado
                "FOC" => GetSpellTypeForce(),
                // Area
                "ARE" => GetSpellTypeArea(),
                // Lanzamiento propio
                "SEL" => GetSpellTypeSelfCast(),
                // Imbuir arma
                "WEA" => GetSpellTypeImbueWeapon(),
                _ => ""
            };

        return spellType;
    }

    private string GetSpellTypeForce()
    {
        string spellType = loadedElements[0] switch
        {
            // barrier / storm / mines / wall
            "SHI" => GetSpellTypeShieldNotSelfCast(),
            // icicles o rock
            "ICE" => loadedElements.Contains("EAR") ? "rock" : "icicles",
            // rock
            "EAR" => "rock",
            // beam
            "LIF" => "beam",
            "ARC" => "beam",
            // lightning
            "LIG" => "lightning",
            _ => "spray"
        };

        return spellType;
    }

    private string GetSpellTypeArea()
    {
        string spellType = loadedElements[0] switch
        {
            // barrier / storm / mines / wall
            "SHI" => GetSpellTypeShieldNotSelfCast(),
            // lightning nova
            "LIG" => "lightningNova",
            // nova
            _ => "nova"
        };

        return spellType;
    }

    private string GetSpellTypeSelfCast()
    {
        string spellType = loadedElements[0] switch
        {
            // ward
            "SHI" => "ward",
            // icicles o rock
            "ICE" => loadedElements.Contains("EAR") ? "rock" : "icicles",
            // rock
            "EAR" => "rock",
            // effect
            _ => "effect"
        };

        return spellType;
    }

    private string GetSpellTypeImbueWeapon()
    {
        string spellType = loadedElements[0] switch
        {
            // barrier / storm / mines / wall
            "SHI" => GetSpellTypeShieldNotSelfCast(),
            // imbued vertical swing
            "EAR" => "imbuedVerticalSwing",
            "ICE" => "imbuedVerticalSwing",
            // imbued horizontal swing
            "LIF" => "imbuedHorizontalSwing",
            "ARC" => "imbuedHorizontalSwing",
            // imbued stab
            _ => "imbuedStab"
        };

        return spellType;
    }

    // Hechizos que contengan el elemento Shield, con cualquier lanzamiento que no sea SelfCast
    private string GetSpellTypeShieldNotSelfCast()
    {
        string spellType;

        if (loadedElements.Count == 1)
        {
            // barrier
            spellType = "barrier";
            return spellType;
        }

        spellType = loadedElements[1] switch
        {
            // wall
            "EAR" => "wall",
            "ICE" => "wall",
            // mines
            "LIF" => "mines",
            "ARC" => "mines",
            // storm
            _ => "storm"
        };

        return spellType;
    }

    private void HandleCastSpell(string castType, string spellType)
    {
        print(castType + ", " + spellType);

        switch (spellType)
        {
            case "barrier":

                break;

            case "wall":

                break;

            case "mines":
                manager.HandleIntantiateMines(loadedElements, castType);
                break;

            case "storm":
                manager.HandleIntantiateStorm(loadedElements, castType);
                break;

            case "rock":

                break;

            case "icicles":

                break;

            case "beam":

                break;

            case "lightning":

                break;

            case "spray":

                break;

            case "lightningNova":

                break;

            case "nova":

                break;

            case "force":

                break;

            case "ward":

                break;

            case "effect":

                break;

            case "imbuedVerticalSwing":

                break;

            case "imbuedHorizontalSwing":

                break;

            case "imbuedStab":

                break;
        }
    }
}