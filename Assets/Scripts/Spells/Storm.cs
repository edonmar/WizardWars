using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storm : MonoBehaviour
{
    private SpellManager spellManager;

    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private ParticleSystem thisParticleSystem;

    public Dictionary<string, int> elements; // Después de eliminar SHI
    private Dictionary<string, int> dmgTypes;
    private bool canHit;

    public void Start()
    {
        spellManager = GameObject.Find("Manager").GetComponent<SpellManager>();
        spellManager.CheckAndDestroyOverlappingSpells(gameObject, 0.25f);

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
        if (OtherDestroysThis(otherGameObj, otherGameObj.tag))
            DestroyThis();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CanHitCharacter(other))
            return;

        HitCharacter(other);
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

        // Si hay determinadas capas entre el hechizo y este objecto, no se eliminará
        if (otherGameObj.CompareTag("Spray"))
        {
            if (Physics.Linecast(transform.position, otherGameObj.GetComponent<Spray>().originTransform.position,
                spellManager.layerMaskBarriers))
                return false;
        }
        else if (Physics.Linecast(transform.position, otherGameObj.gameObject.transform.position,
            spellManager.layerMaskBarriers))
            return false;

        return destroys;
    }

    private bool CanHitCharacter(Collider other)
    {
        if (!canHit)
            return false;

        return other.CompareTag("Player") || other.CompareTag("Enemy");
    }

    private void HitCharacter(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (!characterStats.isDead)
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
        capsuleCollider.enabled = false;
        thisParticleSystem.Stop();
        ParticleSystem.MainModule particleSystemMain = thisParticleSystem.main;
        float duration = particleSystemMain.startLifetime.constant;
        StartCoroutine(DestroyIn(duration));
    }

    private IEnumerator DestroyIn(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}