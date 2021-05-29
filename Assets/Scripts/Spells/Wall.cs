using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private SpellManager spellManager;

    public Dictionary<string, int> elements; // Después de eliminar SHI
    public Dictionary<string, int> wallAuraElements;
    public GameObject wallAura;

    private void Start()
    {
        spellManager = GameObject.Find("Manager").GetComponent<SpellManager>();

        wallAura = CreateWallAuraIfNecessary();
        if (wallAura != null)
        {
            wallAuraElements = wallAura.GetComponent<WallAura>().elements;
        }

        spellManager.CheckAndDestroyOverlappingSpells(gameObject, 0.625f);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObj = other.gameObject;

        if (wallAura == null)
            return;

        if (OtherDestroysWallAura(otherGameObj, otherGameObj.tag))
            DestroyWallAura();
    }

    private GameObject CreateWallAuraIfNecessary()
    {
        // Creo el aura que rodea al wall, si es que tiene
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "EAR" && e.Key != "ICE")
                .ToDictionary(e => e.Key, e => e.Value);
        if (subDictElements.Count <= 0)
            return null;

        GameObject newObj = spellManager.InstantiateWallAura(transform, subDictElements);

        // Calculo el efecto que tendrá el WallAura
        int effectMode;
        if (elements.ContainsKey("EAR"))
        {
            if (elements.ContainsKey("LIF") || elements.ContainsKey("ARC"))
                effectMode = 0;
            else
                effectMode = 1;
        }
        else
            effectMode = 2;

        newObj.GetComponent<WallAura>().effectMode = effectMode;

        return newObj;
    }

    // Devuelve true si el aura de este hechizo debe ser destruida porque ha sido golpeado por determinados tipos de
    // hechizos que contengan determinados elementos
    private bool OtherDestroysWallAura(GameObject otherGameObj, string otherGameObjTag)
    {
        bool destroys = false;
        Dictionary<string, int> otherElements = new Dictionary<string, int>();
        List<string> elementsThatDestroysThisWallAura = new List<string>();

        // Obtengo una lista con los elementos del hechizo que ha provocado el trigger
        // Sólo si ese hechizo es Nova o Spray
        // TODO añadir Lightning a esta comprobación
        otherElements = otherGameObjTag switch
        {
            "Nova" => otherGameObj.GetComponent<Nova>().elements,
            "Spray" => otherGameObj.GetComponent<Spray>().elements,
            _ => otherElements
        };

        // Detiene la comprobación si el WallAura contiene LIF o ARC
        // o si el otro hechizo es Force (hechizo todavía no implementado)
        if (otherElements.Count == 0 || wallAuraElements.ContainsKey("LIF") || wallAuraElements.ContainsKey("ARC"))
            return false;

        // Según los elementos del aura este hechizo, obtengo una lista con los elementos que la destruyen
        if (wallAuraElements.ContainsKey("WAT"))
            elementsThatDestroysThisWallAura.Add("FIR");
        if (wallAuraElements.ContainsKey("COL"))
        {
            elementsThatDestroysThisWallAura.Add("FIR");
            elementsThatDestroysThisWallAura.Add("STE");
        }

        if (wallAuraElements.ContainsKey("FIR"))
            elementsThatDestroysThisWallAura.Add("WAT");
        if (wallAuraElements.ContainsKey("STE"))
            elementsThatDestroysThisWallAura.Add("COL");

        // Si la lista de elementos del otro hechizo contiene uno de los elementos que destruyen al aura de este
        // hechizo, devuelvo true
        if (elementsThatDestroysThisWallAura.Any(e => otherElements.ContainsKey(e)))
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

    public void DestroyWallAura()
    {
        wallAura.GetComponent<WallAura>().DestroyThis();
    }

    public void DestroyThis()
    {
        if (wallAura != null)
            DestroyWallAura();
        Destroy(gameObject);
    }
}