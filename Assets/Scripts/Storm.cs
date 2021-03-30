using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storm : MonoBehaviour
{
    public List<string> elements; // Después de eliminar SHI

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
        List<string> otherElements = new List<string>();
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
        if (elements.Contains("LIG"))
            elementsThatDestroysThis.Add("WAT");
        if (elements.Contains("WAT") || elements.Contains("COL"))
            elementsThatDestroysThis.Add("FIR");
        if (elements.Contains("FIR"))
        {
            elementsThatDestroysThis.Add("WAT");
            elementsThatDestroysThis.Add("COL");
        }

        // Si la lista de elementos del otro hechizo contiene uno de los elementos que destruyen a este hechizo,
        // devuelvo true
        if (elementsThatDestroysThis.Any(e => otherElements.Contains(e)))
            destroys = true;

        return destroys;
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}