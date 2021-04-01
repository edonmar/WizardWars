using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private GameManager manager;

    public List<string> elements; // Después de eliminar SHI
    public List<string> wallAuraElements;
    public GameObject wallAura;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();

        wallAura = CreateWallAuraIfNecessary();
        if (wallAura != null)
            wallAuraElements = wallAura.GetComponent<WallAura>().elements;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObj = other.gameObject;
        string otherGameObjTag = otherGameObj.tag;

        if (OtherDestroysWallAura(otherGameObj, otherGameObjTag))
            DestroyWallAura();
    }

    private GameObject CreateWallAuraIfNecessary()
    {
        GameObject newObj = null;

        // Creo el aura que rodea al wall, si es que tiene
        List<string> subListElements = elements.Where(e => e != "EAR" && e != "ICE").ToList();
        if (subListElements.Count > 0)
            newObj = manager.InstantiateWallAura(transform, subListElements);

        return newObj;
    }

    // Devuelve true si el aura de este hechizo debe ser destruida porque ha sido golpeado por determinados tipos de
    // hechizos que contengan determinados elementos
    private bool OtherDestroysWallAura(GameObject otherGameObj, string otherGameObjTag)
    {
        bool destroys = false;
        List<string> otherElements = new List<string>();
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

        if (otherElements.Count == 0)
            return false;

        // Según los elementos del aura este hechizo, obtengo una lista con los elementos que la destruyen
        if (wallAuraElements.Contains("WAT"))
            elementsThatDestroysThisWallAura.Add("FIR");
        if (wallAuraElements.Contains("COL"))
        {
            elementsThatDestroysThisWallAura.Add("FIR");
            elementsThatDestroysThisWallAura.Add("STE");
        }

        if (wallAuraElements.Contains("FIR"))
            elementsThatDestroysThisWallAura.Add("WAT");
        if (wallAuraElements.Contains("STE"))
            elementsThatDestroysThisWallAura.Add("COL");

        // Si la lista de elementos del otro hechizo contiene uno de los elementos que destruyen al aura de este
        // hechizo, devuelvo true
        if (elementsThatDestroysThisWallAura.Any(e => otherElements.Contains(e)))
            destroys = true;

        return destroys;
    }

    public void DestroyWallAura()
    {
        Destroy(wallAura);
    }
}