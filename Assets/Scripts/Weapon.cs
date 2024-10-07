using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;

public delegate void Trigger(Weapon self, BoomerangController controller);
public delegate void TargetTrigger(Weapon self, BoomerangController controller, Boid target);
public delegate int DisplayDamage(Weapon self);

public class Weapon
{
    public bool NonBuyable = false;

    public string Name;

    public int BaseDamage = 15;
    public float SpeedModifier = 1.0f;
    public float InitialSpeedBoost = 1.0f;
    public float StayModifier = 1.0f;
    public float Bouncyness = 0.24f;
    public float PeriodTime = 10.0f;
    public float ProcCooldown = 0.15f;
    public float SizeModifier = 1.0f;

    public DisplayDamage DisplayDamageFunc;

    public Trigger OnSpawn;
    public TargetTrigger OnHit;
    public TargetTrigger OnProc;
    public Trigger OnPeriod;
    public Trigger OnApex;
    public Trigger OnAnimationDone;

    public int activationCount = 0;

    public int BaseLevel = 0;
    public int Level = 0;
    public int GetLevel() => Level + BaseLevel;

    public float LevelModifier => Mathf.Pow(1.5f, Level);
    public int GetDamage() => Mathf.RoundToInt(BaseDamage * LevelModifier);
    public int GetDisplayDamage() => DisplayDamageFunc != null ? DisplayDamageFunc.Invoke(this) : GetDamage();

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
        BaseDamage = 10,
        NonBuyable = true,
        SpeedModifier = 1.6f,
        SizeModifier = 0.7f,
        OnApex = (self, c) =>
        {
            c.Delete();
        },
    };

    public static Weapon QuickDraw => new()
    {
        Name = "Quick Draw",
        BaseDamage = 17,
        SpeedModifier = 2.1f,
    };

    public static Weapon SlowBoy => new()
    {
        Name = "Slow Boy",
        BaseDamage = 15,
        SpeedModifier = 0.3f,
    };

    public static Weapon Bouncer => new()
    {
        Name = "Bouncer",
        BaseDamage = 16,
        SpeedModifier = 1.2f,
        Bouncyness = 1.6f,
    };
    public static Weapon Returner => new()
    {
        Name = "Returner",
        BaseDamage = 40,
        InitialSpeedBoost = 1.4f,
        SpeedModifier = 0.4f,
        Bouncyness = 0.6f,
        OnHit = (self, c, target) =>
        {
            Vector2 toOwner = (c.Owner.Position2D - c.Position2D).normalized;
            c.velocity = toOwner * c.velocity.magnitude * 0.8f;
        }
    };

    public static Weapon Chaining => new()
    {
        // Level 3
        Name = "Chaining",
        BaseDamage = 35,
        SpeedModifier = 1.1f,
        OnHit = (self, c, target) =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 4, c.velocity.magnitude * 0.5f);

            foreach (Boid b in boids)
            {
                if (b == null || b == target || c.IsInternalBoidCooldown(b))
                    continue;

                Vector2 dir = (b.Position2D - c.Position2D);
                // Chain max 0.5 sec away
                if (dir.magnitude > c.velocity.magnitude * 0.5f)
                    continue;

                c.velocity = dir.normalized * c.velocity.magnitude * self.SpeedModifier;
                break;
            }
        },
    };

    public static Weapon Randomancer => new()
    {
        // Level 2
        Name = "Randomancer",
        BaseDamage = 34,
        SpeedModifier = 0.8f,
        PeriodTime = 0.4f,
        OnPeriod = (self, c) =>
        {
            float magnitude = c.velocity.magnitude * 1.2f;
            Vector2 dir = Random.insideUnitCircle.normalized;
            c.velocity = dir * magnitude;
        },
    };

    public static Weapon Zapper => new()
    {
        // level 3
        Name = "Zapper",
        BaseDamage = 24,
        SpeedModifier = 1.1f,
        DisplayDamageFunc = w => Mathf.RoundToInt(24 * w.LevelModifier),
        OnProc = (self, c, target) =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 6, 4.0f);

            foreach (Boid b in boids)
            {
                if (b == null || b == target) continue;

                float dist = Vector2.Distance(b.Position2D, c.Position2D);
                if (dist < 4.0f)
                {
                    int damage = Mathf.RoundToInt(24 * self.LevelModifier);
                    Boids.Instance.DamageBoid(b, damage);
                }
            }
        },
    };

    public static Weapon Forker => new()
    {
        // level 2
        Name = "Forker",
        SpeedModifier = 1.4f,
        OnHit = (self, c, target) =>
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

            Weapon weapon = Weapons.Temporary;
            weapon.BaseDamage = self.BaseDamage;
            weapon.Level = self.Level;

            BoomerangController boomerang = BoomerangController.New(c.Owner.BoomerangPrefab);
            boomerang.Init(c.Owner, weapon, c.transform.position, dir2, Vector2.zero);
            boomerang.UpdateHitCooldown(target);

            self.activationCount++;
        }
    };
    public static Weapon Multiballer => new()
    {
        // Level 2
        Name = "Multiballer",
        BaseDamage = 12,
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


            Weapon w1 = Weapons.Temporary;
            w1.Level = self.Level;
            BoomerangController b1 = BoomerangController.New(c.Owner.BoomerangPrefab);
            b1.Init(c.Owner, w1, c.transform.position, dir1, Vector2.zero);

            Weapon w2 = Weapons.Temporary;
            w2.Level = self.Level;
            BoomerangController b2 = BoomerangController.New(c.Owner.BoomerangPrefab);
            b2.Init(c.Owner, w2, c.transform.position, dir2, Vector2.zero);
        }
    };
    public static Weapon ExtremeBalls => new()
    {
        // level 3
        Name = "ExtremeBalls",
        BaseDamage = 19,
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

                Weapon weapon = Weapons.Temporary;
                weapon.BaseDamage = 13;
                weapon.Level = self.Level;
                
                BoomerangController b = BoomerangController.New(c.Owner.BoomerangPrefab);
                b.Init(c.Owner, weapon, c.transform.position, dir1, Vector2.zero);
            }
        }
    };
    public static Weapon TheUltimate => new()
    {
        // Level 4
        Name = "The Ultimate",
        BaseDamage = 42,
        SpeedModifier = 1.1f,
        ProcCooldown = 0.3f,
        OnProc = (self, c, target) =>
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

                Weapon weapon = Weapons.TheRecurer;
                weapon.BaseDamage = Mathf.RoundToInt(self.BaseDamage * 0.6f);
                weapon.Level = self.Level;

                BoomerangController b = BoomerangController.New(c.Owner.BoomerangPrefab);
                b.Init(c.Owner, weapon, c.transform.position, dir1, Vector2.zero);
                b.UpdateHitCooldown(target);
            }

            self.activationCount++;
        }
    };
    public static Weapon TheRecurer => new()
    {
        Name = "The Recurer",
        NonBuyable = true,
        SpeedModifier = 1.4f,
        SizeModifier = 0.7f,
        ProcCooldown = 0.3f,
        OnProc = (self, c, target) =>
        {
            bool recur = self.SizeModifier > .5f;

            Vector2 dir = c.velocity.normalized;

            for (int i = 0; i < 3; i++)
            {
                int a = -1 + i;
                if (a == 0) continue;

                float rad = Mathf.Deg2Rad * 40 * a;
                Vector2 dir1 = new(
                    dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                    dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

                BoomerangController b = BoomerangController.New(c.Owner.BoomerangPrefab);
                Weapon weapon = recur ? Weapons.TheRecurer : Weapons.Temporary;
                weapon.SizeModifier = self.SizeModifier * 0.7f;
                weapon.BaseDamage = Mathf.RoundToInt(self.BaseDamage * 0.6f);
                weapon.Level = self.Level;

                b.Init(c.Owner, weapon, c.transform.position, dir1, Vector2.zero);
                b.UpdateHitCooldown(target);
            }

            self.activationCount++;
        },
        OnApex = (self, c) =>
        {
            c.Delete();
        },
    };
    public static Weapon TheOrb => new()
    {
        // Level 4
        Name = "The Orb",
        BaseDamage = 15,
        InitialSpeedBoost = 1.1f,
        SpeedModifier = 0.9f,
        PeriodTime = 1.0f / 8.0f,
        OnPeriod = (self, c) =>
        {
            float rad = Mathf.Deg2Rad * 360f / 8f * (self.activationCount % 8);
            Vector2 dir = new(1, 0);
            dir = new(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

            Weapon weapon = Weapons.Temporary;
            weapon.BaseDamage = 15;
            weapon.Level = self.Level;

            BoomerangController boomerang = BoomerangController.New(c.Owner.BoomerangPrefab);
            boomerang.Init(c.Owner, weapon, c.transform.position, dir, Vector2.zero);

            self.activationCount++;
        }
    };
    public static Weapon GravityPull => new()
    {
        // level 2
        Name = "Gravity Pull",
        BaseDamage = 4,
        PeriodTime = 0.1f,
        InitialSpeedBoost = 1.4f,
        SpeedModifier = 0.5f,
        StayModifier = 0.3f,
        OnPeriod = (self, c) =>
        {
            float aoe = 8f * self.LevelModifier;
            float force = 3.5f * self.LevelModifier;

            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 12, aoe);

            foreach (Boid b in boids)
            {
                if (b == null)
                    continue;

                Vector2 dir = (c.Position2D - b.Position2D);

                if (dir.magnitude < aoe)
                {
                    Vector3 vec = new(dir.x, 0, dir.y);
                    b.velocity += vec.normalized * force;
                }
            }
        }
    };
    public static Weapon Meteor => new()
    {
        // level 3
        Name = "Meteor",
        BaseDamage = 0,
        DisplayDamageFunc = w => Mathf.RoundToInt(120 * w.LevelModifier),
        OnHit = (self, c, target) =>
        {
            if (self.activationCount == 0)
            {
                Vector2 rand = Random.insideUnitCircle;
                List<Vector3> points = new()
                {
                    c.transform.position + Vector3.up * 15f,
                    c.transform.position + new Vector3(target.velocity.x, 0, target.velocity.z),
                };
                c.Animate(points);
            }

            self.activationCount++;
        },
        OnAnimationDone = (self, c) =>
        {
            float aoe = 4f * self.LevelModifier;

            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 8, aoe);

            foreach (Boid b in boids)
            {
                if (b == null)
                    continue;

                Vector2 dir = (c.Position2D - b.Position2D);
                if (dir.magnitude < aoe)
                {
                    int damage = Mathf.RoundToInt(120 * self.LevelModifier);
                    Boids.Instance.DamageBoid(b, damage);
                }
            }
        }
    };
    public static IEnumerable<Weapon> GetShop(int count, int level)
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
                int maxLevel = level - weapon.BaseLevel;
                if (!weapon.NonBuyable && maxLevel >= 0)
                {
                    weapon.Level = maxLevel;
                    weapons.Add(weapon);
                }
            }
        }

        return weapons.OrderBy(_ => Random.value).ToArray()[0..count];
    }
}
