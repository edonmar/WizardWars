using System.Collections.Generic;
using UnityEngine;

public class WallAura : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar SHI, EAR e ICE

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
    }

    public void DestroyThis()
    {
        if (elements.ContainsKey("LIF") || elements.ContainsKey("ARC"))
            manager.InstantiateNova(elements, transform, 1);

        Destroy(gameObject);
    }
}