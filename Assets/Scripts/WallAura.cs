using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAura : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar SHI, EAR e ICE
    private Dictionary<string, int> dmgTypes;
    private bool canHit;
    private bool isExplosive;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();

        isExplosive = GetIsExplosive();
        if (isExplosive)
            return;
        dmgTypes = GetDamageTypesDictionary();
        StartCoroutine(HitTimer(0.25f));
    }

    private bool GetIsExplosive()
    {
        return elements.ContainsKey("LIF") || elements.ContainsKey("ARC");
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
            dmgTypesDict.Add("WAT", 130 + 32 * waterCount);
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 0);
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 130 + 32 * lightningCount);
        if (fireCount > 0)
            dmgTypesDict.Add("FIR", 0);
        if (steamCount > 0)
            dmgTypesDict.Add("STE", 0);

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

    public void DestroyThis()
    {
        if (elements.ContainsKey("LIF") || elements.ContainsKey("ARC"))
            manager.InstantiateNova(elements, transform, 1);

        Destroy(gameObject);
    }
}