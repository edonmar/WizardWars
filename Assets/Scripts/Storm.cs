using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storm : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar SHI

    public void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
        manager.CheckAndDestroyOverlappingSpells(gameObject, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObj = other.gameObject;
        string otherGameObjTag = otherGameObj.tag;

        if (OtherDestroysThis(otherGameObj, otherGameObjTag))
            DestroyThis();
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

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}