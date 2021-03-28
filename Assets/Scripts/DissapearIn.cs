using System.Collections;
using UnityEngine;

public class DissapearIn : MonoBehaviour
{
    public float duration;
    
    private void Start()
    {
        StartCoroutine(DissapearInMethod());
    }
    
    private IEnumerator DissapearInMethod()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}