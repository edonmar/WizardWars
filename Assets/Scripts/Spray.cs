using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spray : MonoBehaviour
{
    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    private bool canHit;

    private void Start()
    {
        dmgTypes = GetDamageTypesDictionary();
        StartCoroutine(HitTimer(0.25f));
    }

    private Dictionary<string, int> GetDamageTypesDictionary()
    {
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int>();

        int waterCount = 0;
        int coldCount = 0;
        int lightningCount = 0;
        int fireCount = 0;
        int steamCount = 0;

        if (elements.ContainsKey("WAT"))
            waterCount = elements["WAT"];
        if (elements.ContainsKey("COL"))
            coldCount = elements["COL"];
        if (elements.ContainsKey("LIG"))
            lightningCount = elements["LIG"];
        if (elements.ContainsKey("FIR"))
            fireCount = elements["FIR"];
        if (elements.ContainsKey("STE"))
            steamCount = elements["STE"];

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 77 + 24 * (waterCount - 1));
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 6 + 2 * (coldCount - 1));
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 77 + 24 * (lightningCount - 1));
        if (fireCount > 0)
            dmgTypesDict.Add("FIR", 15 + 5 * (fireCount - 1));
        if (steamCount > 0)
            dmgTypesDict.Add("STE", 70 + 22 * (steamCount - 1));

        return dmgTypesDict;
    }

    private void OnTriggerStay(Collider other)
    {
        CheckCharacterHit(other);
    }

    private void CheckCharacterHit(Collider other)
    {
        if (!canHit)
            return;

        string otherTag = other.tag;
        if (otherTag != "Player" && otherTag != "Enemy")
            return;

        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (characterStats.health != 0)
            characterStats.TakeSpell(dmgTypes);
        canHit = false;
    }

    private IEnumerator HitTimer(float hitRate)
    {
        while (true)
        {
            canHit = true;
            yield return new WaitForSeconds(hitRate);
        }
    }
}