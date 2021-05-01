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

    private ParticleSystem psWard;
    private ParticleSystem psFlames;
    private ParticleSystem psFrozen;
    ParticleSystem.MainModule psMainWard;
    ParticleSystem.MainModule psMainFlames;

    public int maxHealth;
    public int health;
    private int shield;
    public float baseMovSpeed;
    public float movSpeed;

    public float physicPercTaken;
    public float waterPercTaken;
    public float lifePercTaken;
    public float coldPercTaken;
    public float lightningPercTaken;
    public float arcanePercTaken;
    public float firePercTaken;
    public float icePercTaken;
    public float steamPercTaken;

    // Diccionario con todos los tipos de daño y el porcentaje que recibirá de cada uno
    private Dictionary<string, float> percDmgTypes;

    private bool isWet;
    private bool isChilled;
    private bool isFrozen;
    private bool isBurning;
    private bool isWetAndChilled;

    private float chillTime;
    private float freezeTime;
    private float burningTime;
    private float chillTimeRemaining;
    private float freezeTimeRemaining;
    private float burningTimeRemaining;

    private IEnumerator chillCoroutine;
    private IEnumerator freezeCoroutine;
    private IEnumerator burningCoroutine;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();

        health = maxHealth;
        shield = 0;
        movSpeed = baseMovSpeed;
        percDmgTypes = GetPercDmgTypes();
        isWet = false;
        isChilled = false;
        isFrozen = false;
        isBurning = false;
        isWetAndChilled = false;

        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);

        chillTime = 10;
        freezeTime = 5;
        burningTime = 5;

        psFlames = flamesParticles.GetComponent<ParticleSystem>();
        psFrozen = frozenParticles.GetComponent<ParticleSystem>();
        psMainFlames = psFlames.main;
        flamesParticles.SetActive(true);
        frozenParticles.SetActive(true);

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

    private void SetPercDmgType(string dmgType, float num)
    {
        percDmgTypes[dmgType] = num;
    }

    public void TakeSpell(Dictionary<string, int> dmgTypes)
    {
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
        {
            healthBar.DeactivateShield();
            StopWardParticles();
        }
        else
            healthBar.SetShield(shield);
    }

    private void Die()
    {
        health = 0;
        Destroy(gameObject);
    }

    public void CastWard(Dictionary<string, int> elements)
    {
        if (elements.Count != 1)
            return;

        shield = (int) (maxHealth * (2f / 3f));

        healthBar.SetMaxShield(shield);
        healthBar.SetShield(shield);
        healthBar.ActivateShield();

        psMainWard.startColor = manager.matShield.color;
        PlayWardParticles();
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
                else
                    ApplyWet();
            }
        }
        else if (dmgTypes.ContainsKey("WAT"))
        {
            if (isBurning)
                DispelBurning();
            else
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
                ApplyFreeze();
            }
            else if (isBurning)
                DispelBurning();
            else
                ApplyChill();
        }
        else if (dmgTypes.ContainsKey("FIR"))
        {
            bool canBurn = true;

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

    private IEnumerator BurningCoroutine()
    {
        float hitTimer = 0.25f;
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

    private void PlayWardParticles()
    {
        psWard.Play();
    }

    private void StopWardParticles()
    {
        psWard.Stop();
    }
}