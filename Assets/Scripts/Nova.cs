using System.Collections.Generic;
using UnityEngine;

public class Nova : MonoBehaviour
{
    public Dictionary<string, int> elements;

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}