using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAura : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar SHI, EAR e ICE
    private Dictionary<string, int> dmgTypes;
    private bool canHit;

    public int effectMode;
    // Si un personaje entra en el trigger de un wallAura, recibirá estos efectos según el valor de effectMode:
    // 0 - Nada
    // 1 - Todos los elementos harán efecto menos LIF y ARC
    // 2 - Todos los elementos harán efecto

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();

        if (effectMode == 0)
            return;
        dmgTypes = GetDamageTypesDictionary();
        StartCoroutine(HitTimer(0.25f));
    }

    private Dictionary<string, int> GetDamageTypesDictionary()
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
            dmgTypesDict.Add("WAT", 32 + 14 * (waterCount - 1));
        if (lifeCount > 0 && effectMode == 2)
            dmgTypesDict.Add("LIF", 149);
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 0);
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 32 + 14 * (lightningCount - 1));
        if (arcaneCount > 0 && effectMode == 2)
            dmgTypesDict.Add("ARC", 56);
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
        if (characterStats.currentHealth != 0)
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
            manager.InstantiateNova(elements, transform, "wallAura", 1);

        Destroy(gameObject);
    }
}