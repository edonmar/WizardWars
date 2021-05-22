using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<CharacterStats>().Die();
    }
}