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
        CheckCharacterHit(other);
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
        Destroy(gameObject);
    }
}