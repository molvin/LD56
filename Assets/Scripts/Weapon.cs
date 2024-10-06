using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void Trigger(Weapon self, BoomerangController controller);

public class Weapon
{
    public bool NonBuyable = false;

    public string Name;
    public int Damage = 60;
    public float SpeedModifier = 1.0f;
    public float InitialSpeedBoost = 1.0f;
    public float StayModifier = 1.0f;
    public float Bouncyness = 0.24f;
    public float PeriodTime = 10.0f;
    public Trigger OnSpawn;
    public Trigger OnHit;
    public Trigger OnProc;
    public Trigger OnPeriod;
    public Trigger OnApex;
    public Trigger OnAnimationDone;

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
        NonBuyable = true,
        SpeedModifier = 1.6f,
        OnApex = (self, c) =>
        {
            GameObject.Destroy(c.gameObject);
        },
    };

    public static Weapon QuickDraw => new()
    {
        Name = "Quick Draw",
        SpeedModifier = 2,
    };

    public static Weapon SlowBoy => new()
    {
        Name = "Slow Boy",
        SpeedModifier = 0.3f,
    };

    public static Weapon Bouncer => new()
    {
        Name = "Bouncer",
        SpeedModifier = 1.2f,
        Bouncyness = 1.2f,
    };
    public static Weapon Returner => new()
    {
        Name = "Returner",
        InitialSpeedBoost = 1.4f,
        SpeedModifier = 0.4f,
        Bouncyness = 0.6f,
        OnHit = (self, c) =>
        {
            Vector2 toOwner = (c.Owner.Position2D - c.Position2D).normalized;
            c.velocity = toOwner * c.velocity.magnitude * 0.8f;
        }
    };

    public static Weapon Chaining => new()
    {
        Name = "Chaining",
        OnHit = (self, c) =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 3);

            foreach (Boid b in boids)
            {
                if (b == null || c.IsInternalBoidCooldown(b))
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
        SpeedModifier = 0.8f,
        PeriodTime = 0.5f,
        OnPeriod = (self, c) =>
        {
            float magnitude = c.velocity.magnitude * 1.2f;
            Vector2 dir = Random.insideUnitCircle.normalized;
            c.velocity = dir * magnitude;
        },
    };

    public static Weapon Zapper => new()
    {
        Name = "Zapper",
        SpeedModifier = 1.3f,
        Damage = 10,
        OnProc = (self, c) =>
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
        SpeedModifier = 1.4f,
        OnHit = (self, c) =>
        {
            float rad = Mathf.Deg2Rad * 45;
            if ((self.activationCount & 1) == 0)
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

            self.activationCount++;
        }
    };
    public static Weapon Multiballer => new()
    {
        Name = "Multiballer",
        OnSpawn = (self, c) =>
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
    public static Weapon ExtremeBalls => new()
    {
        Name = "ExtremeBalls",
        OnSpawn = (self, c) =>
        {
            Vector2 dir = c.velocity.normalized;

            for (int i = 0; i < 5; i++)
            {
                int a = -2 + i;
                if (a == 0) continue;

                float rad = Mathf.Deg2Rad * 40 * a;
                Vector2 dir1 = new(
                    dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                    dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

                BoomerangController b = GameObject.Instantiate(c.Owner.BoomerangPrefab);
                b.Init(c.Owner, Weapons.Temporary, c.transform.position, dir1, Vector2.zero);
                b.Temporary = true;
                b.transform.localScale *= 0.7f;
            }
        }
    };
    public static Weapon TheUltimate => new()
    {
        Name = "The Ultimate",
        Damage = 30,
        SpeedModifier = 1.1f,
        OnProc = (self, c) =>
        {
            Vector2 dir = c.velocity.normalized;

            for (int i = 0; i < 3; i++)
            {
                int a = -1 + i;
                if (a == 0) continue;

                float rad = Mathf.Deg2Rad * 40 * a;
                Vector2 dir1 = new(
                    dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                    dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

                BoomerangController b = GameObject.Instantiate(c.Owner.BoomerangPrefab);
                b.Init(c.Owner, Weapons.TheRecurer, c.transform.position, dir1, Vector2.zero);
                b.Temporary = true;
                b.transform.localScale *= 0.7f;
                b.Weapon.Damage = Mathf.RoundToInt(self.Damage * 0.6f);
            }

            self.activationCount++;
        }
    };
    public static Weapon TheRecurer => new()
    {
        Name = "The Recurer",
        NonBuyable = true,
        SpeedModifier = 1.4f,
        OnProc = (self, c) =>
        {
            bool recur = c.transform.localScale.x > .35f;

            Vector2 dir = c.velocity.normalized;

            for (int i = 0; i < 3; i++)
            {
                int a = -1 + i;
                if (a == 0) continue;

                float rad = Mathf.Deg2Rad * 40 * a;
                Vector2 dir1 = new(
                    dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                    dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

                BoomerangController b = GameObject.Instantiate(c.Owner.BoomerangPrefab);
                b.Init(c.Owner, recur ? Weapons.TheRecurer : Weapons.Temporary, c.transform.position, dir1, Vector2.zero);
                b.Temporary = true;
                b.transform.localScale = c.transform.localScale * 0.7f;
                b.Weapon.Damage = Mathf.RoundToInt(self.Damage * 0.6f);
            }

            self.activationCount++;
        },
        OnApex = (self, c) =>
        {
            GameObject.Destroy(c.gameObject);
        },
    };
    public static Weapon TheOrb => new()
    {
        Name = "The Orb",
        PeriodTime = 1.0f / 8.0f,
        OnPeriod = (self, c) =>
        {
            float rad = Mathf.Deg2Rad * 360f / 8f * (self.activationCount % 8);
            Vector2 dir = new(1, 0);
            dir = new(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

            BoomerangController boomerang = GameObject.Instantiate(c.Owner.BoomerangPrefab);
            boomerang.Init(c.Owner, Weapons.Temporary, c.transform.position, dir, Vector2.zero);
            boomerang.Temporary = true;
            boomerang.transform.localScale *= 0.7f;

            self.activationCount++;
        }
    };
    public static Weapon GravityPull => new()
    {
        Name = "Gravity Pull",
        Damage = 1,
        PeriodTime = 0.1f,
        InitialSpeedBoost = 1.4f,
        SpeedModifier = 0.5f,
        StayModifier = 0.3f,
        OnPeriod = (self, c) =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 12);

            foreach (Boid b in boids)
            {
                if (b == null)
                    continue;

                Vector2 dir = (c.Position2D - b.Position2D);
                if (dir.magnitude < 12f)
                {
                    Vector3 vec = new(dir.x, 0, dir.y);
                    b.velocity += vec.normalized * 5f;
                }
            }
        }
    };
    public static Weapon Meteor => new()
    {
        Name = "Meteor",
        Damage = 0,
        OnHit = (self, c) =>
        {
            if (self.activationCount == 0)
            {
                Vector2 rand = Random.insideUnitCircle;
                List<Vector3> points = new()
                {
                    c.transform.position + Vector3.up * 15f,
                    c.transform.position + new Vector3(rand.x, 0, rand.y) * 6f,
                };
                c.Animate(points);
            }

            self.activationCount++;
        },
        OnAnimationDone = (self, c) =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 8);

            foreach (Boid b in boids)
            {
                if (b == null)
                    continue;

                Vector2 dir = (c.Position2D - b.Position2D);
                if (dir.magnitude < 5f)
                {
                    Boids.Instance.DamageBoid(b, 180);
                }
            }
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
                    if (!weapon.NonBuyable)
                    {
                        weapons.Add(weapon);
                    }
                }
            }
        }

        return weapons.OrderBy(_ => Random.value).ToArray()[0..count];
    }
}
