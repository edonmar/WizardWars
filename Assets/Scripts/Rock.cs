using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rock : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements;
    private Dictionary<string, int> novaElements; // Después de eliminar EAR e ICE
    private Dictionary<string, int> dmgTypes;
    public float dmgMultiplier;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
        novaElements =
            elements.Where(e => e.Key != "EAR" && e.Key != "ICE")
                .ToDictionary(e => e.Key, e => e.Value);
        dmgTypes = GetDamageTypesDictionary();
    }

    private Dictionary<string, int> GetDamageTypesDictionary()
    {
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int>();

        int earthCount = elements["EAR"];
        int iceCount = elements.ContainsKey("ICE") ? elements["ICE"] : 0;

        dmgTypesDict.Add("PHY", (75 + 263 * (earthCount - 1)) * (int) dmgMultiplier);
        if (iceCount > 0)
            dmgTypesDict.Add("ICE", (138 + 516 * (iceCount - 1)) * (int) dmgMultiplier);

        return dmgTypesDict;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCharacterHit(other);
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

    private void Explode()
    {
        manager.CastNova(novaElements, transform, "rock");
    }

    public void DestroyThis()
    {
        if (novaElements.Count > 0)
            Explode();
        Destroy(gameObject);
    }
}