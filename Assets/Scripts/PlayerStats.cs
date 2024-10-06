using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int MaxHealth;
    public int CurrentHealth;
    public HUD HUD;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        HUD.SetHealth(1.0f);
    }

    public void TakeDamage(int dmg)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + dmg, 0, MaxHealth);
        HUD.SetHealth(CurrentHealth / (float) MaxHealth);
    }
}
