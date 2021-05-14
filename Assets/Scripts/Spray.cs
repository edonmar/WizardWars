using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spray : MonoBehaviour
{
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private ParticleSystem thisParticleSystem;

    [HideInInspector] public Transform originTransform;
    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    private int layerMask;

    // Listas con los personajes y barreras que están actualmente dentro del collider de Spray
    private HashSet<Collider> charactersColliding;
    private HashSet<Collider> barriersColliding;

    private void Start()
    {
        layerMask = LayerMask.GetMask("TerrainWall", "Barrier");
        dmgTypes = GetDamageTypesDictionary();

        charactersColliding = new HashSet<Collider>();
        barriersColliding = new HashSet<Collider>();

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

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if (IsCharacter(otherGameObject))
            charactersColliding.Add(other);
        else if (IsBarrier(otherGameObject))
            barriersColliding.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if (IsCharacter(otherGameObject))
            charactersColliding.Remove(other);
        else if (IsBarrier(otherGameObject))
            barriersColliding.Remove(other);
    }

    private void RemoveDestroyedElements()
    {
        charactersColliding.RemoveWhere(item => item == null);
        barriersColliding.RemoveWhere(item => item == null);
    }

    private void HitCollidingObjects()
    {
        foreach (Collider c in charactersColliding.Where(c => CanHit(c)))
            HitCharacter(c);

        foreach (Collider c in barriersColliding.Where(c => CanHit(c)))
            HitBarrier(c);
    }

    private bool IsCharacter(GameObject other)
    {
        return other.CompareTag("Player") || other.CompareTag("Enemy");
    }

    private bool IsBarrier(GameObject other)
    {
        return other.CompareTag("Barrier") || other.CompareTag("Wall");
    }

    private bool CanHit(Collider other)
    {
        // Si las capas de layerMask están entre el punto de lanzamiento y el objeto que provoca el trigger,
        // el Spray no golpeará al objeto
        return !Physics.Linecast(originTransform.position, other.gameObject.transform.position, layerMask);
    }

    private void HitCharacter(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (characterStats.health == 0)
            return;
        characterStats.TakeSpell(dmgTypes);

        // Si el Spray contiene Water y el personaje no es resistente al efecto Wet, es empujado
        if (elements.ContainsKey("WAT") && !characterStats.statusEffectResistances["wet"])
            other.GetComponent<Rigidbody>().velocity = originTransform.forward * 5;
    }

    private void HitBarrier(Collider other)
    {
        BarrierStats barrierStats = other.GetComponent<BarrierStats>();
        if (barrierStats.health != 0)
            barrierStats.TakeSpell(dmgTypes);
    }

    private IEnumerator HitTimer(float hitRate)
    {
        while (true)
        {
            RemoveDestroyedElements();
            HitCollidingObjects();
            yield return new WaitForSeconds(hitRate);
        }
    }

    public void DestroyThis()
    {
        boxCollider.enabled = false;
        charactersColliding = new HashSet<Collider>();
        barriersColliding = new HashSet<Collider>();

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