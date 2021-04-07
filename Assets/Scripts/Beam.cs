using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public Dictionary<string, int> elements;
    private int layerMask;
    private LineRenderer lineRenderer;

    private void Start()
    {
        // Las capas con las que chocará el Beam, ignorando las demás capas
        layerMask = LayerMask.GetMask("Terrain", "SolidSpells", "Characters");
        lineRenderer = GetComponent<LineRenderer>();
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
            OnHit(hit.collider.gameObject);
        }
        else
            lineRenderer.SetPosition(1, transform.forward * 5000);
    }

    private void OnHit(GameObject other)
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
}