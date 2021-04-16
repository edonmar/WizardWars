using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth;
    public int health;
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
        health = maxHealth;
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

        int qty = GetQuantytyTaken(dmgTypes);
        if (qty < 0)
            TakeDamage(-qty);
        if (qty >= 0)
            TakeHealing(qty);
    }

    private int GetQuantytyTaken(Dictionary<string, int> dmgTypes)
    {
        return dmgTypes.Aggregate(0, (current, dt) => current - (int) (dt.Value * resistances[dt.Key] / 100));
    }

    private void TakeDamage(int qty)
    {
        health -= qty;
        if (health > 0)
            return;

        Die();
    }

    private void TakeHealing(int qty)
    {
        health += qty;
        if (health > maxHealth)
            health = maxHealth;
    }

    private void Die()
    {
        health = 0;
        Destroy(gameObject);
    }
}