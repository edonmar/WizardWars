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

        int waterCount = 0;
        int lifeCount = 0;
        int coldCount = 0;
        int lightningCount = 0;
        int arcaneCount = 0;

        if (elements.ContainsKey("WAT"))
            waterCount = elements["WAT"];
        if (elements.ContainsKey("LIF"))
            lifeCount = elements["LIF"];
        if (elements.ContainsKey("COL"))
            coldCount = elements["COL"];
        if (elements.ContainsKey("LIG"))
            lightningCount = elements["LIG"];
        if (elements.ContainsKey("ARC"))
            arcaneCount = elements["ARC"];

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

    private void OnCollisionEnter(Collision other)
    {
        CheckCharacterHit(other.collider);
        DestroyThis();
    }

    private void CheckCharacterHit(Collider other)
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