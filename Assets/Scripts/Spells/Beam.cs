using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    private int layerMask;
    private LineRenderer lineRenderer;
    private bool canHit;

    private void Start()
    {
        // Las capas con las que chocará el Beam, ignorando las demás capas
        layerMask = LayerMask.GetMask("TerrainWall", "SolidSpells", "Characters", "Barrier");
        lineRenderer = GetComponent<LineRenderer>();

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
        int earthCount = elements.ContainsKey("EAR") ? elements["EAR"] : 0;
        int fireCount = elements.ContainsKey("FIR") ? elements["FIR"] : 0;
        int iceCount = elements.ContainsKey("ICE") ? elements["ICE"] : 0;
        int steamCount = elements.ContainsKey("STE") ? elements["STE"] : 0;

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 51 + 13 * (waterCount - 1));
        if (lifeCount > 0)
            dmgTypesDict.Add("LIF", 89);
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 13 + 3 * (coldCount - 1));
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 51 + 13 * (lightningCount - 1));
        if (arcaneCount > 0)
            dmgTypesDict.Add("ARC", 106);
        if (fireCount > 0)
            dmgTypesDict.Add("FIR", 30 + 8 * (fireCount - 1));
        if (steamCount > 0)
            dmgTypesDict.Add("STE", 90 + 23 * (steamCount - 1));

        return dmgTypesDict;
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position);
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 5000,
            layerMask))
        {
            if (!hit.collider)
                return;
            lineRenderer.SetPosition(1, hit.point);
            OnHit(hit.collider);
        }
        else
            lineRenderer.SetPosition(1, transform.forward * 5000);
    }

    private void OnHit(Collider other)
    {
        CheckDestroyWallAura(other);

        if (!canHit)
            return;

        if (CanHitCharacter(other))
        {
            HitCharacter(other);
            canHit = false;
        }
        else if (CanHitBarrier(other))
        {
            HitBarrier(other);
            canHit = false;
        }
    }

    // Si el Beam choca con un Wall, comprueba si debe eliminar su WallAura y lo elimina si se cumplen los elementos
    private void CheckDestroyWallAura(Collider other)
    {
        if (!other.CompareTag("Wall"))
            return;

        Wall wallScript = other.GetComponent<Wall>();

        if (wallScript.wallAura == null)
            return;

        if (BeamDestroysWallAura(wallScript.wallAuraElements))
            wallScript.DestroyWallAura();
    }

    // Devuelve true si el aura del Wall con el que el Beam ha chocado debe ser destruida porque el beam contiene
    // determinados elementos
    private bool BeamDestroysWallAura(Dictionary<string, int> wallAuraElements)
    {
        bool destroys = false;
        List<string> elementsThatDestroysWallAura = new List<string>();

        // Según los elementos del wallAura hechizo, obtengo una lista con los elementos que la destruyen
        if (wallAuraElements.ContainsKey("WAT"))
            elementsThatDestroysWallAura.Add("FIR");
        if (wallAuraElements.ContainsKey("COL"))
        {
            elementsThatDestroysWallAura.Add("FIR");
            elementsThatDestroysWallAura.Add("STE");
        }

        if (wallAuraElements.ContainsKey("FIR"))
            elementsThatDestroysWallAura.Add("WAT");
        if (wallAuraElements.ContainsKey("STE"))
            elementsThatDestroysWallAura.Add("COL");

        // Si la lista de elementos del beam contiene uno de los elementos que destruyen al wallAura, devuelvo true
        if (elementsThatDestroysWallAura.Any(e => elements.ContainsKey(e)))
            destroys = true;

        return destroys;
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