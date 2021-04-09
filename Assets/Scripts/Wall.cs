using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar SHI
    public Dictionary<string, int> wallAuraElements;
    public GameObject wallAura;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();

        wallAura = CreateWallAuraIfNecessary();
        if (wallAura != null)
            wallAuraElements = wallAura.GetComponent<WallAura>().elements;

        manager.CheckAndDestroyOverlappingSpells(gameObject, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObj = other.gameObject;
        string otherGameObjTag = otherGameObj.tag;

        if (wallAura == null)
            return;

        if (OtherDestroysWallAura(otherGameObj, otherGameObjTag))
            DestroyWallAura();
    }

    private GameObject CreateWallAuraIfNecessary()
    {
        GameObject newObj = null;

        // Creo el aura que rodea al wall, si es que tiene
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "EAR" && e.Key != "ICE")
                .ToDictionary(e => e.Key, e => e.Value);
        if (subDictElements.Count > 0)
            newObj = manager.InstantiateWallAura(transform, subDictElements);

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

        if (otherElements.Count == 0)
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