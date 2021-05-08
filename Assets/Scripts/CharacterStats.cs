using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    private GameManager manager;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject wardParticles;
    [SerializeField] private GameObject flamesParticles;
    [SerializeField] private GameObject frozenParticles;
    [SerializeField] private GameObject stunParticles;

    private ParticleSystem psWard;
    private ParticleSystem psFlames;
    private ParticleSystem psFrozen;
    private ParticleSystem psStun;
    ParticleSystem.MainModule psMainWard;
    ParticleSystem.MainModule psMainFlames;
    ParticleSystem.MainModule psMainStun;

    public int maxHealth;
    public int health;
    private int shield;
    public float baseMovSpeed;
    public float movSpeed;

    // Estos son los valores iniciales, no los valores con los que se harán los cálculos
    public float physicPercTaken;
    public float waterPercTaken;
    public float lifePercTaken;
    public float coldPercTaken;
    public float lightningPercTaken;
    public float arcanePercTaken;
    public float firePercTaken;
    public float icePercTaken;
    public float steamPercTaken;

    // Estos son los valores iniciales, no los valores con los que se harán los cálculos
    public bool wetResistant;
    public bool chillResistant;
    public bool freezeResistant;
    public bool stunResistant;
    public bool burningResistant;

    // Diccionario con todos los tipos de daño y el porcentaje que recibirá de cada uno
    // Los cálculos de daño se harán con este diccionario
    private Dictionary<string, float> percDmgTypes;
    public Dictionary<string, bool> statusEffectResistances;

    // Diccionario con los elementos que tendrá el ward, si es que existe
    private Dictionary<string, int> wardElements;

    private bool isWardActive;
    private bool isWet;
    private bool isChilled;
    [HideInInspector] public bool isFrozen;
    [HideInInspector] public bool isStunned;
    private bool isBurning;
    private bool isWetAndChilled;

    private float wardTime;
    private float chillTime;
    private float freezeTime;
    private float stunTime;
    private float burningTime;
    private float wardTimeRemaining;
    private float chillTimeRemaining;
    private float freezeTimeRemaining;
    private float stunTimeRemaining;
    private float burningTimeRemaining;

    private IEnumerator wardCoroutine;
    private IEnumerator chillCoroutine;
    private IEnumerator freezeCoroutine;
    private IEnumerator stunCoroutine;
    private IEnumerator burningCoroutine;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();

        health = maxHealth;
        shield = 0;
        movSpeed = baseMovSpeed;
        percDmgTypes = GetPercDmgTypes();
        statusEffectResistances = GetStatusEffectsResistances();

        isWardActive = false;
        isWet = false;
        isChilled = false;
        isFrozen = false;
        isStunned = false;
        isBurning = false;
        isWetAndChilled = false;

        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);

        wardTime = 15;
        chillTime = 10;
        freezeTime = 5;
        stunTime = 2;
        burningTime = 5;

        psFlames = flamesParticles.GetComponent<ParticleSystem>();
        psFrozen = frozenParticles.GetComponent<ParticleSystem>();
        psStun = stunParticles.GetComponent<ParticleSystem>();
        psMainFlames = psFlames.main;
        flamesParticles.SetActive(true);
        frozenParticles.SetActive(true);
        stunParticles.SetActive(true);

        if (wardParticles == null)
            return;
        psWard = wardParticles.GetComponent<ParticleSystem>();
        psMainWard = psWard.main;
        wardParticles.SetActive(true);
    }

    private Dictionary<string, float> GetPercDmgTypes()
    {
        Dictionary<string, float> percDmgTypesDict = new Dictionary<string, float>
        {
            {"PHY", physicPercTaken},
            {"WAT", waterPercTaken},
            {"LIF", lifePercTaken},
            {"COL", coldPercTaken},
            {"LIG", lightningPercTaken},
            {"ARC", arcanePercTaken},
            {"FIR", firePercTaken},
            {"ICE", icePercTaken},
            {"STE", steamPercTaken}
        };

        return percDmgTypesDict;
    }

    private Dictionary<string, bool> GetStatusEffectsResistances()
    {
        Dictionary<string, bool> statusEffectsResistancesDict = new Dictionary<string, bool>
        {
            {"wet", wetResistant},
            {"chill", chillResistant},
            {"freeze", freezeResistant},
            {"stun", stunResistant},
            {"burning", burningResistant},
        };

        return statusEffectsResistancesDict;
    }

    private void SetPercDmgType(string dmgType, float num)
    {
        percDmgTypes[dmgType] = num;
    }

    public void TakeSpell(Dictionary<string, int> dmgTypes)
    {
        if (shield == 0)
            ApplyStatusEffects(dmgTypes);

        int dmg = GetDamageTaken(dmgTypes);
        int healing = GetHealingTaken(dmgTypes);

        if (shield > 0)
            ModifyShield(dmg);
        else
            ModifyHealth(dmg + healing);
    }

    private void TakeBurningDamage()
    {
        int amount = -(int) (35 * percDmgTypes["FIR"] / 100);
        if (shield > 0)
        {
            if (amount < 0)
                ModifyShield(amount);
        }
        else
            ModifyHealth(amount);
    }

    private void TakeLifeWardEffect()
    {
        // Para esta sanación en el tiempo aplico la resistencia base a Life, en vez de la resistencia que queda
        // después de aplicar el Ward
        int amount = -(int) (wardElements["LIF"] * 25 * lifePercTaken / 100);
        ModifyHealth(amount);
    }

    private int GetDamageTaken(Dictionary<string, int> dmgTypes)
    {
        return dmgTypes.Where(dt => percDmgTypes[dt.Key] > 0).Sum(dt => -(int) (dt.Value * percDmgTypes[dt.Key] / 100));
    }

    private int GetHealingTaken(Dictionary<string, int> dmgTypes)
    {
        return dmgTypes.Where(dt => percDmgTypes[dt.Key] < 0).Sum(dt => -(int) (dt.Value * percDmgTypes[dt.Key] / 100));
    }

    private void ModifyHealth(int amount)
    {
        health += amount;

        if (health > maxHealth)
            health = maxHealth;
        else if (health <= 0)
            Die();

        healthBar.SetHealth(health);
    }

    private void ModifyShield(int amount)
    {
        shield += amount;

        if (shield <= 0)
            DispelShield();
        else
            healthBar.SetShield(shield);
    }

    private void Die()
    {
        health = 0;
        Destroy(gameObject);
    }

    public void CastShield()
    {
        if (shield > 0)
        {
            DispelShield();
            return;
        }

        if (isWardActive)
            DispelWard();

        shield = (int) (maxHealth * (2f / 3f));

        healthBar.SetMaxShield(shield);
        healthBar.SetShield(shield);
        healthBar.ActivateShield();

        psMainWard.startColor = manager.matShield.color;
        PlayWardParticles();
    }

    private void DispelShield()
    {
        shield = 0;
        healthBar.DeactivateShield();
        StopWardParticles();
    }

    // Aumento la resistencia a cada elemento contenido en el Ward
    // Si un elemento está 1 vez, protege un 50% de su daño / sanación y totalmente de su efecto de estado
    // Si un elemento está 2 veces, protege un 100% de su daño / sanación y totalmente de su efecto de estado
    // Si el Ward contiene vida, cura en el tiempo
    public void CastWard(Dictionary<string, int> elements)
    {
        if (shield > 0)
            DispelShield();
        if (isWardActive)
            DispelWard();

        // Elimino el elemento Shield del diccionario y si un elemento está más de 2 veces, lo reduzco a 2
        elements.Remove("SHI");
        foreach (var e in elements.ToList().Where(e => e.Value > 2))
            elements[e.Key] = 2;
        // Cambio el nombre de clave de EAR a PHY
        if (elements.ContainsKey("EAR"))
        {
            int earthCount = elements["EAR"];
            elements.Remove("EAR");
            elements.Add("PHY", earthCount);
        }

        wardElements = elements;

        // Aplico las inmunidades a efectos de estado y los elimino si están activos
        ApplyWardStatusEffectsResistances();

        // Aplico las resistencias a los tipos de daño / sanación
        ApplyWardDmgResistances();

        isWardActive = true;
        wardCoroutine = WardCoroutine();
        StartCoroutine(wardCoroutine);

        PlayWardParticles();
    }

    private void ApplyWardStatusEffectsResistances()
    {
        if (wardElements.ContainsKey("WAT"))
        {
            statusEffectResistances["wet"] = true;
            if (isWet)
                DispelWet();
        }

        if (wardElements.ContainsKey("COL"))
        {
            statusEffectResistances["chill"] = true;
            statusEffectResistances["freeze"] = true;
            if (isChilled)
                DispelChill();
            if (isFrozen)
                DispelFreeze();
        }

        if (wardElements.ContainsKey("PHY"))
        {
            statusEffectResistances["stun"] = true;
        }

        if (wardElements.ContainsKey("FIR"))
        {
            statusEffectResistances["burning"] = true;
            if (isBurning)
                DispelBurning();
        }
    }

    private void ApplyWardDmgResistances()
    {
        foreach (KeyValuePair<string, int> e in wardElements)
            percDmgTypes[e.Key] -= e.Value * percDmgTypes[e.Key] / 2;
    }

    private void DispelWard()
    {
        isWardActive = false;
        StopCoroutine(wardCoroutine);

        DispelWardStatusEffectsResistances();
        DispelWardDmgResistances();

        StopWardParticles();
        wardElements = new Dictionary<string, int>();
    }

    private void DispelWardStatusEffectsResistances()
    {
        if (wardElements.ContainsKey("WAT"))
            statusEffectResistances["wet"] = wetResistant;

        if (wardElements.ContainsKey("COL"))
            statusEffectResistances["chill"] = chillResistant;

        if (wardElements.ContainsKey("PHY"))
            statusEffectResistances["stun"] = stunResistant;

        if (wardElements.ContainsKey("FIR"))
            statusEffectResistances["burning"] = burningResistant;
    }

    private void DispelWardDmgResistances()
    {
        foreach (KeyValuePair<string, int> e in wardElements)
        {
            percDmgTypes[e.Key] = e.Key switch
            {
                "PHY" => physicPercTaken,
                "WAT" => waterPercTaken,
                "LIF" => lifePercTaken,
                "COL" => isWet ? coldPercTaken * 2 : coldPercTaken,
                "LIG" => isWet ? lightningPercTaken * 2 : lightningPercTaken,
                "ARC" => arcanePercTaken,
                "FIR" => firePercTaken,
                "ICE" => icePercTaken,
                "STE" => steamPercTaken,
                _ => percDmgTypes[e.Key]
            };
        }
    }

    private IEnumerator WardCoroutine()
    {
        // Lista con los colores de todos los elementos presentes en el Ward
        List<Color> wardColors = wardElements.Keys.Select(key => key == "PHY" ? "EAR" : key)
            .Select(element => manager.GetColorByElement(element)).ToList();
        // Asigno el primer elemento como color inicial
        manager.SetParticleSystemColor(psWard, wardColors[0]);
        float colorTimer = 1; // Cada cuánto tiempo cambiará de color, si es que hay más de un elemento
        int colorCount = wardColors.Count;
        int currentColor = 0;

        bool containsLife = wardElements.ContainsKey("LIF");
        float lifeTimer = 0.25f; // Cada cuanto tiempo aplicará Life

        wardTimeRemaining = wardTime;
        while (wardTimeRemaining > 0)
        {
            wardTimeRemaining -= Time.deltaTime;

            if (containsLife)
            {
                lifeTimer -= Time.deltaTime;
                if (lifeTimer <= 0)
                {
                    lifeTimer = 0.25f;
                    TakeLifeWardEffect();
                }
            }

            if (colorCount > 1)
            {
                colorTimer -= Time.deltaTime;
                if (colorTimer <= 0)
                {
                    colorTimer = 1;
                    currentColor++;
                    if (currentColor >= colorCount)
                        currentColor = 0;
                    manager.SetParticleSystemColor(psWard, wardColors[currentColor]);
                }
            }

            yield return null;
        }

        DispelWard();
    }

    public void TryApplyStunningEffect()
    {
        if (!statusEffectResistances["stun"])
            ApplyStun();
    }

    private void ApplyStatusEffects(Dictionary<string, int> dmgTypes)
    {
        if (dmgTypes.ContainsKey("STE"))
        {
            if (dmgTypes.ContainsKey("FIR"))
            {
                if (isWet)
                    DispelWet();
                if (isChilled)
                    DispelChill();
                else if (isFrozen)
                    DispelFreeze();
                else if (isBurning)
                    DispelBurning();
            }
            else
            {
                if (isBurning)
                    DispelBurning();
                else if (!statusEffectResistances["wet"])
                    ApplyWet();
            }
        }
        else if (dmgTypes.ContainsKey("WAT"))
        {
            if (isBurning)
                DispelBurning();
            else if (!statusEffectResistances["wet"])
                ApplyWet();
        }
        else if (dmgTypes.ContainsKey("COL"))
        {
            if (isWet || isFrozen)
            {
                if (isWet)
                    DispelWet();
                if (isChilled)
                    DispelChill();
                if (!statusEffectResistances["freeze"])
                    ApplyFreeze();
            }
            else if (isBurning)
                DispelBurning();
            else if (!statusEffectResistances["chill"])
                ApplyChill();
        }
        else if (dmgTypes.ContainsKey("FIR"))
        {
            bool canBurn = !statusEffectResistances["burning"];

            if (isWet)
            {
                DispelWet();
                canBurn = false;
            }

            if (isChilled)
            {
                DispelChill();
                canBurn = false;
            }

            if (isFrozen)
            {
                DispelFreeze();
                canBurn = false;
            }

            if (canBurn)
                ApplyBurning();
        }
    }

    private IEnumerator ChillCoroutine()
    {
        chillTimeRemaining = chillTime;
        while (chillTimeRemaining > 0)
        {
            chillTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        DispelChill();
    }

    private IEnumerator FreezeCoroutine()
    {
        freezeTimeRemaining = freezeTime;
        while (freezeTimeRemaining > 0)
        {
            freezeTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        DispelFreeze();
    }

    private IEnumerator StunCoroutine()
    {
        stunTimeRemaining = stunTime;
        while (stunTimeRemaining > 0)
        {
            stunTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        DispelStun();
    }

    private IEnumerator BurningCoroutine()
    {
        float hitTimer = 0.25f; // Cada cuánto tiempo aplicará Fire
        burningTimeRemaining = burningTime;
        while (burningTimeRemaining > 0)
        {
            burningTimeRemaining -= Time.deltaTime;
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
            {
                hitTimer = 0.25f;
                TakeBurningDamage();
            }

            yield return null;
        }

        DispelBurning();
    }

    private void ApplyWet()
    {
        bool reapplied = isWet;
        isWet = true;

        if (reapplied)
            return;
        SetPercDmgType("COL", coldPercTaken * 2);
        SetPercDmgType("LIG", lightningPercTaken * 2);
        PlayFlamesParticles("wet");
    }

    private void DispelWet()
    {
        if (!isWet)
            return;

        isWet = false;
        SetPercDmgType("COL", coldPercTaken);
        SetPercDmgType("LIG", lightningPercTaken);
        StopFlamesParticles();
    }

    private void ApplyChill()
    {
        if (isChilled)
            chillTimeRemaining = chillTime;
        else
        {
            isChilled = true;
            chillCoroutine = ChillCoroutine();
            StartCoroutine(chillCoroutine);
            movSpeed /= 4;
            PlayFlamesParticles("chill");
        }
    }

    private void DispelChill()
    {
        if (!isChilled)
            return;

        isChilled = false;
        StopCoroutine(chillCoroutine);
        movSpeed = baseMovSpeed;
        StopFlamesParticles();
    }

    private void ApplyFreeze()
    {
        if (isFrozen)
            freezeTimeRemaining = freezeTime;
        else
        {
            isFrozen = true;
            freezeCoroutine = FreezeCoroutine();
            StartCoroutine(freezeCoroutine);
            movSpeed = 0;
            SetPercDmgType("PHY", physicPercTaken * 3);
            SetPercDmgType("ICE", icePercTaken * 3);
            PlayFreezeParticles();
        }
    }

    private void DispelFreeze()
    {
        if (!isFrozen)
            return;

        isFrozen = false;
        StopCoroutine(freezeCoroutine);
        movSpeed = baseMovSpeed;
        SetPercDmgType("PHY", physicPercTaken);
        SetPercDmgType("ICE", icePercTaken);
        StopFreezeParticles();
    }

    private void ApplyStun()
    {
        if (isStunned)
            stunTimeRemaining = stunTime;
        else
        {
            isStunned = true;
            stunCoroutine = StunCoroutine();
            StartCoroutine(stunCoroutine);
            movSpeed = 0;
            PlayStunParticles();
        }
    }

    private void DispelStun()
    {
        if (!isStunned)
            return;

        isStunned = false;
        StopCoroutine(stunCoroutine);
        movSpeed = baseMovSpeed;
        StopStunParticles();
    }

    private void ApplyBurning()
    {
        if (isBurning)
            burningTimeRemaining = burningTime;
        else
        {
            isBurning = true;
            burningCoroutine = BurningCoroutine();
            StartCoroutine(burningCoroutine);
            PlayFlamesParticles("burning");
        }
    }

    private void DispelBurning()
    {
        if (!isBurning)
            return;

        isBurning = false;
        StopCoroutine(burningCoroutine);
        StopFlamesParticles();
    }

    // FlamesParticles dibuja 3 efectos: Wet, Chilled y Burning
    // Si no hay ninguno de estos tres efecto de estado anteriormente, activo las partículas con el color del nuevo efecto.
    // Si hay ninguno de estos tres efecto de estado anteriormente, mezclo el color del efecto antiguo con el color del efecto nuevo.
    // Este caso sólo se puede dar cuando está Chilled y después se aplica Wet
    private void PlayFlamesParticles(string status)
    {
        Color color = status switch
        {
            "wet" => manager.matWater.color,
            "chill" => manager.matCold.color,
            "burning" => manager.matFire.color,
            _ => default
        };

        if (psFlames.isEmitting)
        {
            Color oldColor = psMainFlames.startColor.color;
            Color mixColor;
            mixColor.r = (oldColor.r + color.r) / 2;
            mixColor.g = (oldColor.g + color.g) / 2;
            mixColor.b = (oldColor.b + color.b) / 2;
            mixColor.a = (oldColor.a + color.a) / 2;

            psMainFlames.startColor = mixColor;
            isWetAndChilled = true;
        }
        else
        {
            psMainFlames.startColor = color;
            psFlames.Play();
        }
    }

    // Si tenía Wet y Chilled y sólo es disipado uno de los dos, el disipado siempre será Chilled, así que le pongo
    // a las partículas el color de Wet
    // Si no tenía Wet y Chilled, es que sólo tenía el efecto que es disipado. En este caso detengo las partículas
    private void StopFlamesParticles()
    {
        if (isWetAndChilled)
        {
            psMainFlames.startColor = manager.matWater.color;
            isWetAndChilled = false;
        }
        else
            psFlames.Stop();
    }

    private void PlayFreezeParticles()
    {
        psFrozen.Play();
    }

    private void StopFreezeParticles()
    {
        psFrozen.Stop();
    }

    private void PlayStunParticles()
    {
        psStun.Play();
    }

    private void StopStunParticles()
    {
        psStun.Stop();
    }

    private void PlayWardParticles()
    {
        psWard.Play();
    }

    private void StopWardParticles()
    {
        psWard.Stop();
    }
}