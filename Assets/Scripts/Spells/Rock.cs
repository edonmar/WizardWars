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
        if (CanHitCharacter(other))
            HitCharacter(other);
        else if (CanHitBarrier(other))
            HitBarrier(other);
        if (other.gameObject.layer != LayerMask.NameToLayer("NonSolidSpells"))
            DestroyThis();
    }

    private bool CanHitCharacter(Collider other)
    {
        return other.CompareTag("Player") || other.CompareTag("Enemy");
    }

    private bool CanHitBarrier(Collider other)
    {
        return other.CompareTag("Barrier") || other.CompareTag("Wall");
    }

    private void HitCharacter(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (!characterStats.isDead)
            characterStats.TakeSpell(dmgTypes);
    }

    private void HitBarrier(Collider other)
    {
        BarrierStats barrierStats = other.GetComponent<BarrierStats>();
        if (barrierStats.health != 0)
            barrierStats.TakeSpell(dmgTypes);
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