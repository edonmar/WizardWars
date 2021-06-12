using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private SpellManager spellManager;
    private MagickManager magickManager;
    private StageManager stageManager;
    private GameManager gameManager;
    private GameUIManager gameUIManager;

    [SerializeField] private ControlsPC controlsPC;
    [HideInInspector] public bool moveInput;
    [HideInInspector] public string elementInput;
    [HideInInspector] public string startCastInput;
    [HideInInspector] public bool stopCastInput;
    [HideInInspector] public Vector3 rotationInput;

    [SerializeField] private Animator animator;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private CharacterStats characterStats;

    [SerializeField] private ParticleSystem selfCastParticles;
    [SerializeField] private ParticleSystem chargingParticles;
    [SerializeField] private ParticleSystem chargingFullParticles;

    private int hashParamMovSpeed;
    private int hashStatusAttack01;
    private int hashStatusAttack02;

    private float movSpeed;

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
        GameObject manager = GameObject.Find("Manager");
        spellManager = manager.GetComponent<SpellManager>();
        magickManager = manager.GetComponent<MagickManager>();
        stageManager = manager.GetComponent<StageManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameUIManager = GameObject.Find("GameUI").GetComponent<GameUIManager>();

        hashParamMovSpeed = Animator.StringToHash("MovSpeed");
        hashStatusAttack01 = Animator.StringToHash("attack01");
        hashStatusAttack02 = Animator.StringToHash("attack02");

        selfCastParticles.gameObject.SetActive(true);
        chargingParticles.gameObject.SetActive(true);
        chargingFullParticles.gameObject.SetActive(true);

        loadedElements = new List<string>();
        isBeamActive = false;
        isLightningActive = false;
        isSprayActive = false;
        isChargingSpell = false;
        chargedSpellForce = 0;
        chargedSpellDmgMultiplier = 0;
        chargedSpellAngle = 0;
        chargedSpellElements = new Dictionary<string, int>();

        SetControlMode();
        rotationInput = new Vector3();
        moveInput = false;
        elementInput = "";
        startCastInput = "";
        stopCastInput = false;
    }

    private void SetControlMode()
    {
        if (!gameManager.isMobile)
            controlsPC.enabled = true;
    }

    private void Update()
    {
        if (characterStats.isDead)
        {
            DestroyCurrentSpells();
            enabled = false;
        }

        movSpeed = 0;
        RotatePlayerTowards();
        if (moveInput)
        {
            if (isChargingSpell)
                CastChargingSpell();
            else
                DestroyCurrentSpells();
            MovePlayerForward();
            moveInput = false;
        }

        if (elementInput != "")
        {
            LoadElement(elementInput);
            gameUIManager.ShowLoadedElements(loadedElements);
            elementInput = "";
        }

        CastInput(startCastInput, stopCastInput);

        animator.SetFloat(hashParamMovSpeed, movSpeed);
    }

    private void RotatePlayerTowards()
    {
        transform.LookAt(rotationInput);
    }

    private void MovePlayerForward()
    {
        movSpeed = characterStats.movSpeed;
        transform.Translate(transform.forward * (movSpeed * Time.deltaTime), Space.World);
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

    private void ClearElements()
    {
        loadedElements.Clear();
        gameUIManager.ShowLoadedElements(loadedElements);
    }

    private void CastInput(string castType, bool stopCast)
    {
        // Si levanto el botón, desactivo los hechizos que esté lanzando
        if (stopCast)
        {
            if (isChargingSpell)
                CastChargingSpell();
            else
                DestroyCurrentSpells();

            if (isLightningActive)
                DestroyLightning();
            stopCastInput = false;
        }

        switch (castType)
        {
            case "":
                return;
            case "WEA" when loadedElements.Count == 0:
                // Ataque de arma
                print("Ataque de arma");
                return;
            case "MAG":
                string magickName = magickManager.WrittenMagick(loadedElements);
                print("Magick: " + magickName);
                if (magickName != "")
                {
                    magickManager.CastMagick(magickName, gameObject);
                    stageManager.MagickCasted(magickName);
                    PlayAttackAnimation(castType);
                    ClearElements();
                }

                return;
        }

        List<int> priorityList = GetPriorityList();
        OrderLoadedElements(priorityList);
        string spellType = GetSpellType(castType);

        /*
        // Si intento hacer lanzamiento propio sin cargar ningún elemento, no hace nada
        if (castType == "SEL" && spellType == "force")
            return;
            */

        // Si lanzar un hechizo sin cargar ningún elemento, no hace nada
        if (spellType == "force")
            return;

        // Si estoy congelado, sólo puedo realizar el tipo de lanzamiento SelfCast
        if (characterStats.isFrozen && castType != "SEL")
            return;

        // Si estoy aturdido, no puedo lanzar hechizos
        if (characterStats.isStunned)
            return;

        Dictionary<string, int> elements = GetElementDictionary();
        CastSpell(elements, castType, spellType);
        ClearElements();
        stageManager.SpellCasted();
        PlayAttackAnimation(castType);
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
                spellManager.CastBarrier(castType, transform);
                break;

            case "wall":
                StartCoroutine(spellManager.CastWalls(elements, castType, transform));
                break;

            case "mines":
                StartCoroutine(spellManager.CastMines(elements, castType, transform));
                break;

            case "storm":
                StartCoroutine(spellManager.CastStorms(elements, castType, transform));
                break;

            case "rock":
                switch (castType)
                {
                    case "FOC":
                        StartChargingSpell(elements, spellType);
                        break;
                    case "SEL":
                        spellManager.CastRock(elements, castType, 1000, 1, transform, shootPoint);
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
                        spellManager.CastIcicles(elements, castType, 1000, 1, chargedSpellAngle, transform, shootPoint);
                        break;
                }

                break;

            case "beam":
                activeBeam = spellManager.CastBeam(elements, shootPoint);
                isBeamActive = true;
                break;

            case "lightning":
                activeLightning = spellManager.CastLightning(elements, castType, shootPoint, gameObject);
                isLightningActive = true;
                break;

            case "spray":
                activeSpray = spellManager.CastSpray(elements, shootPoint);
                isSprayActive = true;
                break;

            case "nova":
                spellManager.CastNova(elements, transform, "character");
                break;

            /*
            case "force":

                break;
                */

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

            /*
            case "imbuedVerticalSwing":

                break;

            case "imbuedHorizontalSwing":

                break;

            case "imbuedStab":

                break;
                */
        }
    }

    private void PlayAttackAnimation(string castType)
    {
        switch (castType)
        {
            case "FOC":
                animator.Play(hashStatusAttack01);
                break;
            default:
                animator.Play(hashStatusAttack02);
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
                spellManager.CastRock(chargedSpellElements, "FOC", chargedSpellForce, chargedSpellDmgMultiplier,
                    transform,
                    shootPoint);
                break;
            case "icicles":
                spellManager.CastIcicles(chargedSpellElements, "FOC", chargedSpellForce, chargedSpellDmgMultiplier,
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

    public void GetStunnedOrFrozen()
    {
        if (isChargingSpell)
        {
            isChargingSpell = false;
            StopCoroutine(chargingSpellCoroutine);
        }
        else
            DestroyCurrentSpells();
    }

    private void DestroyCurrentSpells()
    {
        if (isBeamActive)
            DestroyBeam();
        if (isLightningActive)
            DestroyLightning();
        else if (isSprayActive)
            DestroySpray();
        chargingParticles.Stop();
        chargingFullParticles.Stop();
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
        Color startColor = spellManager.GetColorByElement(elements.ElementAt(0).Key);
        Color endColor = elements.Count > 1 ? spellManager.GetColorByElement(elements.ElementAt(1).Key) : startColor;

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

    public void Die()
    {
        stageManager.GameEnd(false);
    }
}