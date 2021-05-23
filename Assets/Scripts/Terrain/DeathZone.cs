using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        if (!characterStats.isDead)
            characterStats.Die();
    }
}