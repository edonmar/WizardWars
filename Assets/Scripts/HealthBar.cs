using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject shieldBar;
    [SerializeField] private Slider shieldSlider;

    public void SetMaxHealth(int health)
    {
        healthSlider.maxValue = health;
    }

    public void SetHealth(int health)
    {
        healthSlider.value = health;
    }

    public void SetMaxShield(int shield)
    {
        shieldSlider.maxValue = shield;
    }

    public void SetShield(int shield)
    {
        shieldSlider.value = shield;
    }

    public void ActivateShield()
    {
        shieldBar.SetActive(true);
    }

    public void DeactivateShield()
    {
        shieldBar.SetActive(false);
    }
}