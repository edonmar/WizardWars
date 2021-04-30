using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storm : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar SHI
    private Dictionary<string, int> dmgTypes;
    private bool canHit;

    public void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
        manager.CheckAndDestroyOverlappingSpells(gameObject, 0.25f);

        dmgTypes = GetDamageTypesDictionary();
        StartCoroutine(HitTimer(0.25f));
    }

    private Dictionary<string, int> GetDamageTypesDictionary()
    {
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int>();

        int waterCount = elements.ContainsKey("WAT") ? elements["WAT"] : 0;
        int coldCount = elements.ContainsKey("COL") ? elements["COL"] : 0;
        int lightningCount = elements.ContainsKey("LIG") ? elements["LIG"] : 0;
        int fireCount = elements.ContainsKey("FIR") ? elements["FIR"] : 0;
        int steamCount = elements.ContainsKey("STE") ? elements["STE"] : 0;

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 130 + 32 * (waterCount - 1));
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 0);
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 130 + 32 * (lightningCount - 1));
        if (fireCount > 0)
            dmgTypesDict.Add("FIR", 0);
        if (steamCount > 0)
            dmgTypesDict.Add("STE", 0);

        return dmgTypesDict;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObj = other.gameObject;
        string otherGameObjTag = otherGameObj.tag;

        if (OtherDestroysThis(otherGameObj, otherGameObjTag))
            DestroyThis();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CanHit(other))
            return;

        Hit(other);
        canHit = false;
    }

    // Devuelve true si este hechizo debe ser destruido porque ha sido golpeado por determinados tipos de hechizos
    // que contengan determinados elementos
    private bool OtherDestroysThis(GameObject otherGameObj, string otherGameObjTag)
    {
        bool destroys = false;
        Dictionary<string, int> otherElements = new Dictionary<string, int>();
        List<string> elementsThatDestroysThis = new List<string>();

        // Obtengo una lista con los elementos del hechizo que ha provocado el trigger
        // Sólo si ese hechizo es Nova o Spray
        otherElements = otherGameObjTag switch
        {
            "Nova" => otherGameObj.GetComponent<Nova>().elements,
            "Spray" => otherGameObj.GetComponent<Spray>().elements,
            _ => otherElements
        };

        if (otherElements.Count == 0)
            return false;

        // Según los elementos de este hechizo, obtengo una lista con los elementos que lo destruyen
        if (elements.ContainsKey("LIG"))
            elementsThatDestroysThis.Add("WAT");
        if (elements.ContainsKey("WAT") || elements.ContainsKey("COL"))
            elementsThatDestroysThis.Add("FIR");
        if (elements.ContainsKey("FIR"))
        {
            elementsThatDestroysThis.Add("WAT");
            elementsThatDestroysThis.Add("COL");
        }

        // Si la lista de elementos del otro hechizo contiene uno de los elementos que destruyen a este hechizo,
        // devuelvo true
        if (elementsThatDestroysThis.Any(e => otherElements.ContainsKey(e)))
            destroys = true;

        return destroys;
    }

    private bool CanHit(Collider other)
    {
        if (!canHit)
            return false;

        string otherTag = other.tag;
        if (otherTag != "Player" && otherTag != "Enemy")
            return false;

        return true;
    }

    private void Hit(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (characterStats.health != 0)
            characterStats.TakeSpell(dmgTypes);
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
        Destroy(gameObject);
    }
}