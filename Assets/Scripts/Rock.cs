using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    private GameManager manager;

    public Dictionary<string, int> elements; // Después de eliminar EAR e ICE

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (elements.Count <= 0)
            return;
        manager.HandleInstantiateNova(elements, transform);
        Destroy(gameObject);
    }
}