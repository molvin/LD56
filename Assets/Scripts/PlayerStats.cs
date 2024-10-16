using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float MaxHealth;
    public float CurrentHealth;

    public int MaxBoidDamageDealers = 10;
    public float BoidDamageRadius = 1.0f;
    public float BoidDamageCooldown = 0.5f;
    public HUD HUD;
    public Player Player;

    private float lastBoidDamage;

    private void Start()
    {
        FullHeal();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Player.Die();
        }
        if (CurrentHealth > 0)
            ApplyBoidDamage();
    }

    private void ApplyBoidDamage()
    {
        var boids = Boids.Instance.GetNearest(transform.position, MaxBoidDamageDealers, BoidDamageRadius);

        float dmg = 0;
        foreach(var boid in boids)
        {
            if (boid == null || boid.IsDead)
                continue;
            float dist = Mathf.Max((Vector3.Distance(boid.transform.position, transform.position) - boid.Radius), 0);
            if (dist < BoidDamageRadius)
            {
                dmg = Mathf.Max(boid.damage, dmg);
            }
        }

        if((int)dmg > 0 && (Time.time - lastBoidDamage) > BoidDamageCooldown)
        {
            lastBoidDamage = Time.time;
            TakeDamage((int)dmg);
        }
    }

    public void TakeDamage(float dmg)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - dmg, 0, MaxHealth);
        HUD.SetHealth(CurrentHealth / MaxHealth);
        Audioman.getInstance().PlaySound("Hurt", transform.position);
        if(CurrentHealth <= 0)
        {
            Player.Die();
        }
    }

    public void FullHeal()
    {
        CurrentHealth = MaxHealth;
        HUD.SetHealth(1.0f);
    }
}
