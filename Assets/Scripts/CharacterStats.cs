using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public float physicPercentageTaken;
    public float waterPercentageTaken;
    public float lifePercentageTaken;
    public float coldPercentageTaken;
    public float lightningPercentageTaken;
    public float arcanePercentageTaken;
    public float firePercentageTaken;
    public float icePercentageTaken;
    public float steamPercentageTaken;

    private Dictionary<string, float> resistances;

    private void Start()
    {
        currentHealth = maxHealth;
        resistances = GetResistances();
    }

    private Dictionary<string, float> GetResistances()
    {
        Dictionary<string, float> resistanceDict = new Dictionary<string, float>
        {
            {"PHY", physicPercentageTaken},
            {"WAT", waterPercentageTaken},
            {"LIF", lifePercentageTaken},
            {"COL", coldPercentageTaken},
            {"LIG", lightningPercentageTaken},
            {"ARC", arcanePercentageTaken},
            {"FIR", firePercentageTaken},
            {"ICE", icePercentageTaken},
            {"STE", steamPercentageTaken}
        };

        return resistanceDict;
    }

    public void TakeSpell(Dictionary<string, int> dmgTypes)
    {
        // TODO efectos de estado

        int amount = GetAmountTaken(dmgTypes);
        ModifyHealth(amount);
    }

    private int GetAmountTaken(Dictionary<string, int> dmgTypes)
    {
        return dmgTypes.Aggregate(0, (current, dt) => current - (int) (dt.Value * resistances[dt.Key] / 100));
    }

    private void ModifyHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        else if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        currentHealth = 0;
        Destroy(gameObject);
    }
}