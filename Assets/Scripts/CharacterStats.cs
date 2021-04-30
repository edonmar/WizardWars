using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar;

    public int maxHealth;
    public int health;
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
        health = maxHealth;
        movSpeed = baseMovSpeed;
        percDmgTypes = GetPercDmgTypes();
        isWet = false;
        isChilled = false;
        isFrozen = false;
        isBurning = false;

        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);

        chillTime = 10;
        freezeTime = 5;
        burningTime = 5;
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
        int amount = GetAmountTaken(dmgTypes);
        ModifyHealth(amount);
    }

    private void TakeBurningDamage()
    {
        int amount = -(int) (35 * percDmgTypes["FIR"] / 100);
        ModifyHealth(amount);
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
    }

    private void DispelWet()
    {
        if (!isWet)
            return;

        isWet = false;
        SetPercDmgType("COL", coldPercTaken);
        SetPercDmgType("LIG", lightningPercTaken);
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
        }
    }

    private void DispelChill()
    {
        if (!isChilled)
            return;

        isChilled = false;
        StopCoroutine(chillCoroutine);
        movSpeed = baseMovSpeed;
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
        }
    }

    private void DispelBurning()
    {
        if (!isBurning)
            return;

        isBurning = false;
        StopCoroutine(burningCoroutine);
    }

    private int GetAmountTaken(Dictionary<string, int> dmgTypes)
    {
        return dmgTypes.Aggregate(0, (current, dt) => current - (int) (dt.Value * percDmgTypes[dt.Key] / 100));
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

    private void Die()
    {
        health = 0;
        Destroy(gameObject);
    }
}