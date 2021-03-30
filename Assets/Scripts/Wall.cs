using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private GameManager manager;
    
    public List<string> elements; // Después de eliminar SHI

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
        
        // Creo el aura que rodea al wall, si es que tiene
        List<string> subListElements = elements.Where(e => e != "EAR" && e != "ICE").ToList();
        if (subListElements.Count > 0)
            manager.InstantiateWallAura(transform, subListElements);
    }
}
