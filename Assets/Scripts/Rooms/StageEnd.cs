using UnityEngine;

public class StageEnd : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            print("WIN");
    }
}