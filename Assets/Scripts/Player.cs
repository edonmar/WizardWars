using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private GameManager manager;
    private Camera mainCamera;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private CharacterStats characterStats;

    [SerializeField] private ParticleSystem selfCastParticles;
    [SerializeField] private ParticleSystem chargingParticles;
    [SerializeField] private ParticleSystem chargingFullParticles;

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

    private bool isBeamActive;
    private bool isLightningActive;
    private bool isSprayActive;
    private GameObject activeBeam;
    private GameObject activeLightning;
    private GameObject activeSpray;

    private IEnumerator chargingSpellCoroutine;
    private bool isChargingSpell;
    private string chargedSpellType;
    private int chargedSpellForce;
    private float chargedSpellDmgMultiplier;
    private float chargedSpellAngle;
    private Dictionary<string, int> chargedSpellElements;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
        mainCamera = Camera.main;

        selfCastParticles.gameObject.SetActive(true);
        chargingParticles.gameObject.SetActive(true);
        chargingFullParticles.gameObject.SetActive(true);

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
        isBeamActive = false;
        isLightningActive = false;
        isSprayActive = false;
        isChargingSpell = false;
        chargedSpellForce = 0;
        chargedSpellDmgMultiplier = 0;
        chargedSpellAngle = 0;
        chargedSpellElements = new Dictionary<string, int>();
    }

    private void Update()
    {
        RotatePlayerTowardsCamera();
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (isChargingSpell)
                CastChargingSpell();
            else
                DestroyCurrentSpells();
            MovePlayerForward();
        }

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
        float movSpeed = characterStats.movSpeed;
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

        // Si levanto el botón, desactivo los hechizos que esté lanzando
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            if (isChargingSpell)
                CastChargingSpell();
            else
                DestroyCurrentSpells();
        }

        // Si levanto el botón, desactivo los hechizos que esté lanzando
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (isLightningActive)
                DestroyLightning();
        }

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

        // Si estoy congelado, sólo puedo realizar el tipo de lanzamiento SelfCast
        if (characterStats.isFrozen && castType != "SEL")
            return;

        Dictionary<string, int> elements = GetElementDictionary();
        CastSpell(elements, castType, spellType);
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
            "LIG" => "lightning",
            // nova
            _ => "nova"
        };

        return spellType;
    }

    private string GetSpellTypeSelfCast()
    {
        string spellType = loadedElements[0] switch
        {
            // shield o ward
            "SHI" => loadedElements.Count == 1 ? "shield" : "ward",
            // icicles o rock
            "ICE" => loadedElements.Contains("EAR") ? "rock" : "icicles",
            // rock
            "EAR" => "rock",
            // self cast effect
            _ => "selfCastEffect"
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

    private Dictionary<string, int> GetElementDictionary()
    {
        Dictionary<string, int> elements = new Dictionary<string, int>();

        foreach (string e in loadedElements)
        {
            if (!elements.ContainsKey(e))
                elements.Add(e, 1);
            else
                elements[e]++;
        }

        return elements;
    }

    private void CastSpell(Dictionary<string, int> elements, string castType, string spellType)
    {
        print(castType + ", " + spellType);

        switch (spellType)
        {
            case "barrier":
                manager.CastBarrier(castType, transform);
                break;

            case "wall":
                StartCoroutine(manager.CastWalls(elements, castType, transform));
                break;

            case "mines":
                StartCoroutine(manager.CastMines(elements, castType, transform));
                break;

            case "storm":
                StartCoroutine(manager.CastStorms(elements, castType, transform));
                break;

            case "rock":
                switch (castType)
                {
                    case "FOC":
                        StartChargingSpell(elements, spellType);
                        break;
                    case "SEL":
                        manager.CastRock(elements, castType, 1000, 1, transform, shootPoint);
                        break;
                }

                break;

            case "icicles":
                switch (castType)
                {
                    case "FOC":
                        StartChargingSpell(elements, spellType);
                        break;
                    case "SEL":
                        manager.CastIcicles(elements, castType, 1000, 1, chargedSpellAngle, transform, shootPoint);
                        break;
                }

                break;

            case "beam":
                activeBeam = manager.CastBeam(elements, shootPoint);
                isBeamActive = true;
                break;

            case "lightning":
                activeLightning = manager.CastLightning(elements, castType, shootPoint, gameObject);
                isLightningActive = true;
                break;

            case "spray":
                activeSpray = manager.CastSpray(elements, shootPoint);
                isSprayActive = true;
                break;

            case "nova":
                manager.CastNova(elements, transform, "character");
                break;

            case "force":

                break;

            case "shield":
                characterStats.CastShield();
                break;

            case "ward":
                characterStats.CastWard(elements);
                break;

            case "selfCastEffect":
                Dictionary<string, int> dmgTypes = GetSelfCastEffectDamageTypesDictionary(elements);
                characterStats.TakeSpell(dmgTypes);
                PlaySelfCastParticles(elements);
                break;

            case "imbuedVerticalSwing":

                break;

            case "imbuedHorizontalSwing":

                break;

            case "imbuedStab":

                break;
        }
    }

    private void StartChargingSpell(Dictionary<string, int> elements, string spellType)
    {
        float finalDmgMultiplier = spellType switch
        {
            "rock" => 4.4f,
            "icicles" => 2,
            _ => 1
        };

        chargedSpellElements = elements;
        chargedSpellForce = 1000;
        chargedSpellDmgMultiplier = 1;
        chargedSpellAngle = 45;
        chargedSpellType = spellType;
        isChargingSpell = true;
        chargingSpellCoroutine = ChargeSpell(1000, 2000, 1, finalDmgMultiplier, 45, 2, 2.75f);
        StartCoroutine(chargingSpellCoroutine);

        chargingParticles.Play();
    }

    private void CastChargingSpell()
    {
        chargingParticles.Stop();
        chargingFullParticles.Stop();

        switch (chargedSpellType)
        {
            case "rock":
                manager.CastRock(chargedSpellElements, "FOC", chargedSpellForce, chargedSpellDmgMultiplier, transform,
                    shootPoint);
                break;
            case "icicles":
                manager.CastIcicles(chargedSpellElements, "FOC", chargedSpellForce, chargedSpellDmgMultiplier,
                    chargedSpellAngle, transform, shootPoint);
                break;
        }

        isChargingSpell = false;
        StopCoroutine(chargingSpellCoroutine);
    }

    private IEnumerator ChargeSpell(float initialForce, float finalForce, float initialDmgMultiplier,
        float finalDmgMultiplier, float initialAngle, float finalangle, float duration)
    {
        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            chargedSpellForce = (int) Mathf.Lerp(initialForce, finalForce, counter / duration);
            chargedSpellDmgMultiplier = Mathf.Lerp(initialDmgMultiplier, finalDmgMultiplier, counter / duration);
            chargedSpellAngle = Mathf.Lerp(initialAngle, finalangle, counter / duration);
            yield return null;
        }

        chargingParticles.Stop();
        chargingFullParticles.Play();
    }

    private void DestroyCurrentSpells()
    {
        if (isBeamActive)
            DestroyBeam();
        if (isLightningActive)
            DestroyLightning();
        else if (isSprayActive)
            DestroySpray();
    }

    private void DestroyBeam()
    {
        isBeamActive = false;
        if (activeBeam == null)
            return;
        Beam activeBeamScript = activeBeam.GetComponent<Beam>();
        activeBeamScript.DestroyThis();
        activeBeam = null;
    }

    private void DestroyLightning()
    {
        isLightningActive = false;
        if (activeLightning == null)
            return;
        Lightning activeLightningScript = activeLightning.GetComponent<Lightning>();
        activeLightningScript.DestroyThis();
        activeLightning = null;
    }

    private void DestroySpray()
    {
        isSprayActive = false;
        if (activeSpray == null)
            return;
        Spray activeSprayScript = activeSpray.GetComponent<Spray>();
        activeSprayScript.DestroyThis();
        activeSpray = null;
    }

    private Dictionary<string, int> GetSelfCastEffectDamageTypesDictionary(Dictionary<string, int> elements)
    {
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int>();

        int waterCount = elements.ContainsKey("WAT") ? elements["WAT"] : 0;
        int lifeCount = elements.ContainsKey("LIF") ? elements["LIF"] : 0;
        int coldCount = elements.ContainsKey("COL") ? elements["COL"] : 0;
        int lightningCount = elements.ContainsKey("LIG") ? elements["LIG"] : 0;
        int arcaneCount = elements.ContainsKey("ARC") ? elements["ARC"] : 0;
        int fireCount = elements.ContainsKey("FIR") ? elements["FIR"] : 0;
        int steamCount = elements.ContainsKey("STE") ? elements["STE"] : 0;

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 250 + 63 * (waterCount - 1));
        if (lifeCount > 0)
            dmgTypesDict.Add("LIF", 260 + 65 * (lifeCount - 1));
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 25 + 6 * (coldCount - 1));
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 250 + 63 * (lightningCount - 1));
        if (arcaneCount > 0)
            dmgTypesDict.Add("ARC", 200 + 50 * (arcaneCount - 1));
        if (fireCount > 0)
            dmgTypesDict.Add("FIR", 60 + 15 * (fireCount - 1));
        if (steamCount > 0)
            dmgTypesDict.Add("STE", 280 + 70 * (steamCount - 1));

        return dmgTypesDict;
    }

    private void PlaySelfCastParticles(Dictionary<string, int> elements)
    {
        Color startColor = manager.GetColorByElement(elements.ElementAt(0).Key);
        Color endColor = elements.Count > 1 ? manager.GetColorByElement(elements.ElementAt(1).Key) : startColor;

        ParticleSystem.ColorOverLifetimeModule colorOverTime = selfCastParticles.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
                {new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f)},
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f)
            });
        colorOverTime.color = grad;

        selfCastParticles.Play();
    }
}