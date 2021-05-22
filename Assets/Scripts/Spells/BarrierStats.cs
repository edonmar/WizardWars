using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BarrierStats : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public int loseRate;

    private void Start()
    {
        health = maxHealth;
        StartCoroutine(LoseHealthOverTime(loseRate));
    }

    private IEnumerator LoseHealthOverTime(int amount)
    {
        while (true)
        {
            ModifyHealth(amount);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void TakeSpell(Dictionary<string, int> dmgTypes)
    {
        int dmg = GetDamageTaken(dmgTypes);
        ModifyHealth(dmg);
    }

    private int GetDamageTaken(Dictionary<string, int> dmgTypes)
    {
        return dmgTypes.Where(dt => dt.Key != "LIF").Aggregate(0, (current, dt) => current - dt.Value);
    }

    private void ModifyHealth(int amount)
    {
        health += amount;
        if (health <= 0)
            DestroyThis();
    }

    public void DestroyThis()
    {
        health = 0;
        if (CompareTag("Barrier"))
            Destroy(gameObject);
        if (CompareTag("Wall"))
            gameObject.GetComponent<Wall>().DestroyThis();
    }
}