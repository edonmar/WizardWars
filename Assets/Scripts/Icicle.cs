using System.Collections.Generic;
using UnityEngine;

public class Icicle : MonoBehaviour
{
    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    public float dmgMultiplier;

    private void Start()
    {
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

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 0);
        if (lifeCount > 0)
            dmgTypesDict.Add("LIF", 60 + 15 * (lifeCount - 1));
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 0);
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 40 + 10 * (lightningCount - 1));
        if (arcaneCount > 0)
            dmgTypesDict.Add("ARC", 40 + 10 * (arcaneCount - 1));
        dmgTypesDict.Add("ICE", 60 * (int) dmgMultiplier);

        return dmgTypesDict;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CanHit(other))
            Hit(other);
        DestroyThis();
    }

    private bool CanHit(Collider other)
    {
        string otherTag = other.tag;
        if (otherTag != "Player" && otherTag != "Enemy")
            return false;

        return true;
    }

    private void Hit(Collider other)
    {
        string otherTag = other.tag;
        if (otherTag != "Player" && otherTag != "Enemy")
            return;

        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (characterStats.currentHealth != 0)
            characterStats.TakeSpell(dmgTypes);
    }

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}