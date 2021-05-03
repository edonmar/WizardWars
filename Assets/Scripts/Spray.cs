using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spray : MonoBehaviour
{
    public Transform originTransform;
    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    private int layerMask;
    private bool canHit;

    private void Start()
    {
        layerMask = LayerMask.GetMask("Terrain");
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
        if (!CanHit(other))
            return;

        Hit(other);
        canHit = false;
    }

    private bool CanHit(Collider other)
    {
        if (!canHit)
            return false;

        string otherTag = other.tag;
        if (otherTag != "Player" && otherTag != "Enemy")
            return false;

        // Si las capas de layerMask están entre el punto de lanzamiento y el objeto que provoca el trigger,
        // el Spray no golpeará al objeto
        if (Physics.Linecast(originTransform.position, other.gameObject.transform.position, layerMask))
            return false;

        return true;
    }

    private void Hit(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (characterStats.health == 0)
            return;
        characterStats.TakeSpell(dmgTypes);
        // Si el Spray contiene Water, empuja al personaje
        if (elements.ContainsKey("WAT"))
            other.GetComponent<Rigidbody>().velocity = originTransform.forward * 5;
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