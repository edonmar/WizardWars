using System.Collections;
using UnityEngine;

public class DestroyIn : MonoBehaviour
{
    public float duration;

    private void Start()
    {
        StartCoroutine(DestroyInMethod());
    }

    private IEnumerator DestroyInMethod()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}