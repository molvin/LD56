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
    public float PeriodTime = 10.0f;
    public Trigger OnSpawn;
    public Trigger OnHit;
    public Trigger OnProc;
    public Trigger OnPeriod;
    public Trigger OnApex;

    public int activationCount = 0;

    public void Reset()
    {
        activationCount = 0;
    }
}

public static class Weapons
{
    public static Weapon Default => new()
    {
        Name = "Default",
    };
    public static Weapon Temporary => new()
    {
        Name = "Temporary",
        ThrowSpeedModifier = 1.6f,
        OnApex = c =>
        {
            GameObject.Destroy(c.gameObject);
        },
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
                if (c == null || c.IsInternalBoidCooldown(b))
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

    public static Weapon Zapper => new()
    {
        Name = "Zapper",
        ThrowSpeedModifier = 1.3f,
        Damage = 10,
        OnProc = c =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 4);

            foreach (Boid b in boids)
            {
                if (b == null) continue;

                float dist = Vector2.Distance(b.Position2D, c.Position2D);
                if (dist < 4.0f)
                {
                    Boids.Instance.DamageBoid(b, 10);
                }
            }
        },
    };

    public static Weapon Forker => new()
    {
        Name = "Forker",
        ThrowSpeedModifier = 1.4f,
        OnHit = c =>
        {
            float rad = Mathf.Deg2Rad * 45;
            if ((c.Weapon.activationCount & 1) == 0)
            {
                rad = -rad;
            }
            Vector2 dir = c.velocity.normalized;
            Vector2 dir1 = new(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

            Vector2 dir2 = new(
                dir.x * Mathf.Cos(-rad) - dir.y * Mathf.Sin(-rad),
                dir.x * Mathf.Sin(-rad) + dir.y * Mathf.Cos(-rad));

            c.velocity = dir1 * c.velocity.magnitude;

            BoomerangController boomerang = GameObject.Instantiate(c.Owner.BoomerangPrefab);
            boomerang.Init(c.Owner, Weapons.Temporary, c.transform.position, dir2, Vector2.zero);
            boomerang.Temporary = true;
            boomerang.transform.localScale *= 0.7f;

            c.Weapon.activationCount++;
        }
    };
    public static Weapon Multiballer => new()
    {
        Name = "Multiballer",
        OnSpawn = c =>
        {
            float rad = Mathf.Deg2Rad * 40;
            Vector2 dir = c.velocity.normalized;
            Vector2 dir1 = new(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

            Vector2 dir2 = new(
                dir.x * Mathf.Cos(-rad) - dir.y * Mathf.Sin(-rad),
                dir.x * Mathf.Sin(-rad) + dir.y * Mathf.Cos(-rad));


            BoomerangController b1 = GameObject.Instantiate(c.Owner.BoomerangPrefab);
            b1.Init(c.Owner, Weapons.Temporary, c.transform.position, dir1, Vector2.zero);
            b1.Temporary = true;
            b1.transform.localScale *= 0.7f;

            BoomerangController b2 = GameObject.Instantiate(c.Owner.BoomerangPrefab);
            b2.Init(c.Owner, Weapons.Temporary, c.transform.position, dir2, Vector2.zero);
            b2.Temporary = true;
            b2.transform.localScale *= 0.7f;
        }
    };
    public static Weapon TheOrb => new()
    {
        Name = "The Orb",
        PeriodTime = 1.0f / 8.0f,
        OnPeriod = c =>
        {
            float rad = Mathf.Deg2Rad * 360f / 8f * (c.Weapon.activationCount % 8);
            Vector2 dir = new(1, 0);
            dir = new(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

            BoomerangController boomerang = GameObject.Instantiate(c.Owner.BoomerangPrefab);
            boomerang.Init(c.Owner, Weapons.Temporary, c.transform.position, dir, Vector2.zero);
            boomerang.Temporary = true;
            boomerang.transform.localScale *= 0.7f;

            c.Weapon.activationCount++;
        }
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
