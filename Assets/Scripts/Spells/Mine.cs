using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mine : MonoBehaviour
{
    private SpellManager spellManager;

    public Dictionary<string, int> elements; // Después de eliminar SHI
    public bool destroyed;

    private void Start()
    {
        spellManager = GameObject.Find("Manager").GetComponent<SpellManager>();
        destroyed = false;
        spellManager.CheckAndDestroyOverlappingSpells(gameObject, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destroyed)
            return;

        GameObject otherGameObj = other.gameObject;

        if (OtherDestroysThis(otherGameObj, otherGameObj.tag))
            DestroyThis();
    }

    // Devuelve true si este hechizo debe ser destruido porque ha sido golpeado por un personaje, un proyectil o por
    // determinados tipos de hechizos que contengan determinados elementos
    private bool OtherDestroysThis(GameObject otherGameObj, string otherGameObjTag)
    {
        bool destroys = false;
        Dictionary<string, int> otherElements = new Dictionary<string, int>();
        List<string> elementsThatDestroysThis = new List<string>();
        int otherGameLayer = otherGameObj.layer;

        // Si el otro objeto no es un hechizo sino un personaje o un proyectil, devuelvo true
        if (otherGameLayer == LayerMask.NameToLayer("Characters") ||
            otherGameLayer == LayerMask.NameToLayer("Projectiles"))
            return true;

        // Obtengo una lista con los elementos del hechizo que ha provocado el trigger
        // Sólo si ese hechizo es Nova o Spray
        otherElements = otherGameObjTag switch
        {
            "Nova" => otherGameObj.GetComponent<Nova>().elements,
            "Spray" => otherGameObj.GetComponent<Spray>().elements,
            _ => otherElements
        };

        // Según el tipo del otro hechizo, obtengo una lista con los elementos que necesita para destruir a este hechizo
        switch (otherGameObjTag)
        {
            case "Nova":
                elementsThatDestroysThis.Add("LIF");
                elementsThatDestroysThis.Add("COL");
                elementsThatDestroysThis.Add("ARC");
                elementsThatDestroysThis.Add("FIR");
                elementsThatDestroysThis.Add("ICE");
                elementsThatDestroysThis.Add("STE");
                break;

            case "Spray":
                elementsThatDestroysThis.Add("COL");
                elementsThatDestroysThis.Add("FIR");
                elementsThatDestroysThis.Add("STE");
                break;
        }

        if (otherElements.Count == 0)
            return false;

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

    private void Explode()
    {
        spellManager.InstantiateNova(elements, transform, "mine", 1);
    }

    public void DestroyThis()
    {
        destroyed = true;
        Explode();
        Destroy(gameObject);
    }
}