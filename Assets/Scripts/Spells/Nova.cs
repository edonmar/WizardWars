using System.Collections.Generic;
using UnityEngine;

public class Nova : MonoBehaviour
{
    private SpellManager spellManager;

    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    public string originType;
    public GameObject caster;

    private void Start()
    {
        spellManager = GameObject.Find("Manager").GetComponent<SpellManager>();
        dmgTypes = GetDamageTypesDictionary();
    }

    private Dictionary<string, int> GetDamageTypesDictionary()
    {
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int>();

        int waterCount = elements.ContainsKey("WAT") ? elements["WAT"] : 0;
        int lifeCount = elements.ContainsKey("LIF") ? elements["LIF"] : 0;
        int coldCount = elements.ContainsKey("COL") ? elements["COL"] : 0;
        int lightningCount = elements.ContainsKey("LIG") ? elements["LIG"] : 0;
        int arcaneCount = elements.ContainsKey("ARC") ? elements["ARC"] : 0;
        int earthCount = elements.ContainsKey("EAR") ? elements["EAR"] : 0;
        int fireCount = elements.ContainsKey("FIR") ? elements["FIR"] : 0;
        int iceCount = elements.ContainsKey("ICE") ? elements["ICE"] : 0;
        int steamCount = elements.ContainsKey("STE") ? elements["STE"] : 0;

        switch (originType)
        {
            case "character":
                if (waterCount > 0)
                    dmgTypesDict.Add("WAT", 250 + 77 * (waterCount - 1));
                if (lifeCount > 0)
                    dmgTypesDict.Add("LIF", 179);
                if (coldCount > 0)
                    dmgTypesDict.Add("COL", 24 + 8 * (coldCount - 1));
                if (lightningCount > 0)
                    dmgTypesDict.Add("LIG", 250 + 77 * (lightningCount - 1));
                if (arcaneCount > 0)
                    dmgTypesDict.Add("ARC", 225);
                if (earthCount > 0)
                    dmgTypesDict.Add("PHY", 0);
                if (fireCount > 0)
                    dmgTypesDict.Add("FIR", 60 + 19 * (fireCount - 1));
                if (iceCount > 0)
                    dmgTypesDict.Add("ICE", 275 + 85 * (iceCount - 1));
                if (steamCount > 0)
                    dmgTypesDict.Add("STE", 280 + 87 * (steamCount - 1));
                break;

            case "rock":
                if (waterCount > 0)
                    dmgTypesDict.Add("WAT", 250 + 96 * (waterCount - 1));
                if (lifeCount > 0)
                    dmgTypesDict.Add("LIF", 179);
                if (coldCount > 0)
                    dmgTypesDict.Add("COL", 25 + 6 * (coldCount - 1));
                if (arcaneCount > 0)
                    dmgTypesDict.Add("ARC", 225);
                if (fireCount > 0)
                    dmgTypesDict.Add("FIR", 60 + 15 * (fireCount - 1));
                if (steamCount > 0)
                    dmgTypesDict.Add("STE", 280 + 70 * (steamCount - 1));
                break;

            case "mine":
            case "wallAura":
                if (waterCount > 0)
                    dmgTypesDict.Add("WAT", 130 + 48 * (waterCount - 1));
                if (lifeCount > 0)
                    dmgTypesDict.Add("LIF", 599);
                if (coldCount > 0)
                    dmgTypesDict.Add("COL", 0);
                if (lightningCount > 0)
                    dmgTypesDict.Add("LIG", 130 + 48 * (lightningCount - 1));
                if (arcaneCount > 0)
                    dmgTypesDict.Add("ARC", 225);
                if (fireCount > 0)
                    dmgTypesDict.Add("FIR", 0);
                if (steamCount > 0)
                    dmgTypesDict.Add("STE", 0);
                break;
        }

        return dmgTypesDict;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CanHitCharacter(other))
            HitCharacter(other);
        else if (CanHitBarrier(other))
            HitBarrier(other);
    }

    private bool CanHitCharacter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
            return false;

        // Si el personaje es el mismo que ha lanzado la Nova (y si la Nova se ha lanzado directamente en modo Area),
        // no le afecta
        if (caster == other.gameObject)
            return false;

        // Si las capas de layerMask están entre el centro de la nova y el objeto que provoca el trigger,
        // el Spray no golpeará al objeto
        if (Physics.Linecast(transform.position, other.gameObject.transform.position, spellManager.layerMaskBarriers))
            return false;

        return true;
    }

    private bool CanHitBarrier(Collider other)
    {
        if (!other.CompareTag("Barrier") && !other.CompareTag("Wall"))
            return false;
        if (Physics.Linecast(transform.position, other.gameObject.transform.position, spellManager.layerMaskBarriers))
            return false;
        return true;
    }

    private void HitCharacter(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (characterStats.isDead)
            return;
        characterStats.TakeSpell(dmgTypes);

        // Si la Nova contiene Earth, aplica el efecto de estado Stunned
        if (elements.ContainsKey("EAR"))
            characterStats.TryApplyStunningEffect();

        // Si la Nova contiene Water y el personaje no es resistente al efecto Wet, es empujado
        if (elements.ContainsKey("WAT") && !characterStats.statusEffectResistances["wet"])
        {
            other.GetComponent<Rigidbody>().velocity =
                (other.transform.position - transform.position).normalized * 6;
            Enemy enemyScript = other.GetComponent<Enemy>();
            if (enemyScript != null)
                enemyScript.ApplyPush();
        }
    }

    private void HitBarrier(Collider other)
    {
        BarrierStats barrierStats = other.GetComponent<BarrierStats>();
        if (barrierStats.health != 0)
            barrierStats.TakeSpell(dmgTypes);
    }

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}