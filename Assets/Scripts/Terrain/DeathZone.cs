using System.Collections;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterStats characterStats = other.GetComponent<CharacterStats>();
        StartCoroutine(KillIn(1, characterStats));
    }

    private IEnumerator KillIn(float time, CharacterStats characterStats)
    {
        yield return new WaitForSeconds(time);
        if (!characterStats.isDead)
            characterStats.Die();
    }
}