using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void Trigger(BoomerangController controller);

public class Weapon
{
    public string Name;
    public int Damage = 60;
    public float ThrowSpeedModifier = 1.0f;
    public float Bouncyness = 0.24f;
    public Trigger OnHit;
    public Trigger OnProc;
    public float PeriodTime = 10.0f;
    public Trigger OnPeriod;
}

public static class Weapons
{
    public static Weapon Default => new()
    {
        Name = "Default",
    };

    public static Weapon QuickDraw => new()
    {
        Name = "Quick Draw",
        ThrowSpeedModifier = 2,
    };

    public static Weapon SlowBoy => new()
    {
        Name = "Slow Boy",
        ThrowSpeedModifier = 0.3f,
    };

    public static Weapon Bouncer => new()
    {
        Name = "Bouncer",
        ThrowSpeedModifier = 1.2f,
        Bouncyness = 1.2f,
    };

    public static Weapon Chaining => new()
    {
        Name = "Chaining",
        OnHit = c =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 3);

            foreach (Boid b in boids)
            {
                if (c.IsInternalBoidCooldown(b))
                    continue;

                Vector2 dir = (b.Position2D - c.Position2D);
                // Chain max 0.5 sec away
                if (dir.magnitude > c.velocity.magnitude * 0.5f)
                    continue;

                c.velocity = dir.normalized * c.velocity.magnitude;
                break;
            }
        },
    };

    public static Weapon Randomancer => new()
    {
        Name = "Randomancer",
        ThrowSpeedModifier = 0.8f,
        PeriodTime = 0.5f,
        OnPeriod = c =>
        {
            float magnitude = c.velocity.magnitude * 1.2f;
            Vector2 dir = Random.insideUnitCircle.normalized;
            c.velocity = dir * magnitude;
        },
    };

    public static IEnumerable<Weapon> GetShop(int count)
    {
        // Returns X random weapons for now
        // TODO: filtering, weapon pool?

        var cardFuncs = typeof(Weapons).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        List<Weapon> weapons = new();
        foreach (var func in cardFuncs)
        {
            if (func.ReturnType == typeof(Weapon))
            {
                Weapon weapon = func.Invoke(null, null) as Weapon;
                {
                    // TODO: filter based on something
                    weapons.Add(weapon);
                }
            }
        }

        return weapons.OrderBy(_ => Random.value).ToArray()[0..count];
    }
}
