using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void Trigger(Weapon self, BoomerangController controller);
public delegate void TargetTrigger(Weapon self, BoomerangController controller, Boid target);
public delegate int DisplayDamage(Weapon self);

public class Weapon
{
    public bool NonBuyable = false;

    public string Name;
    public string Description;
    public int Color;

    public int BaseDamage = 15;
    public float SpeedModifier = 1.0f;
    public float InitialSpeedBoost = 1.0f;
    public float StayModifier = 1.0f;
    public float Bouncyness = 0.24f;
    public float PeriodTime = 10.0f;
    public float ProcCooldown = 0.15f;
    public float SizeModifier = 1.0f;
    public float Knockback = 8.0f;

    public DisplayDamage DisplayDamageFunc;

    public Trigger OnSpawn;
    public TargetTrigger OnHit;
    public TargetTrigger OnProc;
    public Trigger OnPeriod;
    public Trigger OnApex;
    public Trigger OnAnimationDone;
    public Trigger OnEnd;

    public int activationCount = 0;

    public int BaseLevel = 0;
    public int Level = 0;
    public int GetLevel() => Level + BaseLevel;
    public float KnockbackForce => Knockback * LevelModifier;

    public float LevelModifier => Mathf.Pow(1.35f, Level);
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
        Name = "Bob",
        Description = "Returns to you, damaging vermins on the way",
        Color = 1,
    };
    public static Weapon Temporary => new()
    {
        Name = "Temporary",
        Color = 0,
        BaseDamage = 10,
        NonBuyable = true,
        SpeedModifier = 1.6f,
        SizeModifier = 0.7f,
        Knockback = 6,
        OnApex = (self, c) =>
        {
            c.Delete();
        },
    };

    public static Weapon QuickDraw => new()
    {
        Name = "Gonzales",
        Description = "Moves very far and fast",
        Color = 2,
        BaseDamage = 17,
        SpeedModifier = 2.2f,
        Knockback = 10f,
    };

    public static Weapon SlowBoy => new()
    {
        Name = "Slouch",
        Description = "Slowly moves a short distance",
        Color = 3,
        BaseDamage = 15,
        SpeedModifier = 0.3f,
        Knockback = 10f,
    };

    public static Weapon Bouncer => new()
    {
        Name = "Hops",
        Description = "Bounces on any terrain it collides with",
        Color = 4,
        BaseDamage = 16,
        SpeedModifier = 1.2f,
        Bouncyness = 3.2f,
        Knockback = 12f,
    };
    public static Weapon Returner => new()
    {
        Name = "Kenny",
        Description = "Knocks vermins back then instantly returns",
        Color = 5,
        BaseDamage = 40,
        InitialSpeedBoost = 1.4f,
        SpeedModifier = 0.4f,
        Bouncyness = 0.6f,
        Knockback = 32f,
        OnHit = (self, c, target) =>
        {
            Vector2 toOwner = (c.Owner.Position2D - c.Position2D).normalized;
            c.velocity = toOwner * c.velocity.magnitude * 0.8f;
        }
    };

    public static Weapon Chaining => new()
    {
        Name = "Bonaparte",
        Description = "Jumps from vermin to vermin when attacking",
        Color = 6,
        BaseLevel = 3,
        BaseDamage = 35,
        SpeedModifier = 1.1f,
        Knockback = 5f,
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

                c.velocity = dir.normalized * c.velocity.magnitude;
                break;
            }
        },
    };

    public static Weapon Randomancer => new()
    {
        Name = "Klaus",
        Description = "Randomly jerks in random directions",
        Color = 7,
        BaseDamage = 34,
        BaseLevel = 2,
        SpeedModifier = 0.8f,
        PeriodTime = 0.4f,
        Knockback = -10f,
        OnPeriod = (self, c) =>
        {
            float magnitude = c.velocity.magnitude * 1.2f;
            Vector2 dir = Random.insideUnitCircle.normalized;
            c.velocity = dir * magnitude;
        },
    };

    public static Weapon Zapper => new()
    {
        Name = "Uri",
        Description = "When hitting vermin it zaps all nearby vermins",
        Color = 8,
        BaseLevel = 3,
        BaseDamage = 24,
        SpeedModifier = 1.1f,
        DisplayDamageFunc = w => Mathf.RoundToInt(24 * w.LevelModifier),
        Knockback = 4f,
        OnProc = (self, c, target) =>
        {
            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 6, 6.0f);

            foreach (Boid b in boids)
            {
                if (b == null || b == target) continue;

                float dist = Vector2.Distance(b.Position2D, c.Position2D);
                if (dist < 4.0f)
                {
                    int damage = Mathf.RoundToInt(24 * self.LevelModifier);
                    Boids.Instance.DamageBoid(b, damage);

                    GameObject zap = FxManager.Get("LineZap");
                    zap.transform.position = c.transform.position + Vector3.up * 0.3f;
                    zap.GetComponent<MoveToPoint>().desiredPos = b.transform.position + Vector3.up * 0.4f;
                }
            }
        },
    };

    public static Weapon Forker => new()
    {
        Name = "Fork",
        Description = "Spawns a tiny critter when hitting vermins",
        Color = 9,
        BaseLevel = 2,
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
        Name = "Banger",
        Description = "Spawns two additional tinier critters",
        Color = 10,
        BaseLevel = 2,
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
        Name = "Mash",
        Description = "Spawns four additional tinier critters",
        Color = 11,
        BaseLevel = 3,
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
    public static Weapon BulletHell => new()
    {
        Name = "Gravy",
        Description = "Spawns nine additional tinier critters",
        Color = 12,
        BaseLevel = 4,
        BaseDamage = 26,
        OnSpawn = (self, c) =>
        {
            Vector2 dir = c.velocity.normalized;

            for (int i = 0; i < 9; i++)
            {
                float rad = Mathf.Deg2Rad * Random.Range(-70, 70);
                Vector2 dir1 = new(
                    dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                    dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad));

                Weapon weapon = Weapons.Temporary;
                weapon.BaseDamage = 26;
                weapon.InitialSpeedBoost = Random.Range(2.5f, 3.5f);
                weapon.SpeedModifier = 0.7f;
                weapon.Bouncyness = 2f;
                weapon.Level = self.Level;
                weapon.OnPeriod = weapon.OnApex;
                weapon.OnApex = null;
                weapon.PeriodTime = 1.5f;
                
                BoomerangController b = BoomerangController.New(c.Owner.BoomerangPrefab);
                b.Init(c.Owner, weapon, c.transform.position, dir1, Vector2.zero);
            }
        }
    };
    public static Weapon TheUltimate => new()
    {
        Name = "Matt",
        Description = "When hitting vermin it spawns two additional tinier critters that in turn spawns even tinier critters",
        Color = 13,
        BaseLevel = 4,
        BaseDamage = 42,
        SpeedModifier = 1.1f,
        Knockback = 6f,
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
        Color = 0,
        NonBuyable = true,
        SpeedModifier = 1.4f,
        SizeModifier = 0.7f,
        ProcCooldown = 0.3f,
        Knockback = 4f,
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
        Name = "Robin",
        Description = "Spawns tiny critters in a spiral pattern while moving",
        Color = 14,
        BaseLevel = 4,
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
        Name = "Harry",
        Description = "Moves slow and pulls all vermin towards it",
        Color = 15,
        BaseLevel = 2,
        BaseDamage = 4,
        PeriodTime = 0.1f,
        InitialSpeedBoost = 1.4f,
        SpeedModifier = 0.5f,
        StayModifier = 0.3f,
        OnPeriod = (self, c) =>
        {
            GameObject fx = FxManager.GetWithKey("MagnetPull", self);
            fx.transform.position = c.transform.position;

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
        },
        OnEnd = (self, c) =>
        {
            GameObject fx = FxManager.GetWithKey("MagnetPull", self);
            ObjectPool.Return(fx);
            FxManager.ClearKey(self);
        },
    };
    public static Weapon Meteor => new()
    {
        Name = "Bub",
        Description = "When touching vermin it jumps up in the air and comes down slamming hard",
        Color = 16,
        BaseLevel = 3,
        BaseDamage = 0,
        Knockback = 0,
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
            GameObject impactFx = FxManager.Get("ApexExploder");
            impactFx.transform.position = c.transform.position;
            GameObject shockwave = FxManager.Get("MeteorImpact");
            shockwave.transform.position = c.transform.position;

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

                    // Shockwave force
                    float force = 20f * self.LevelModifier;
                    b.velocity += new Vector3(-dir.x, 0, -dir.y) * force / (b.Radius * 2.0f);
                }
            }
        }
    };
    public static Weapon ApexEploder => new()
    {
        Name = "Jack",
        Description = "Creates an explosion when reaching its apex and returning back",
        Color = 17,
        BaseLevel = 2,
        BaseDamage = 0,
        InitialSpeedBoost = 0.7f,
        Knockback = 0,
        DisplayDamageFunc = w => Mathf.RoundToInt(60 * w.LevelModifier),
        OnApex = (self, c) =>
        {
            GameObject impactFx = FxManager.Get("ApexExploder");
            impactFx.transform.position = c.transform.position;

            float aoe = 4f * self.LevelModifier;

            List<Boid> boids = Boids.Instance.GetNearest(c.transform.position, 8, aoe);

            foreach (Boid b in boids)
            {
                if (b == null)
                    continue;

                Vector2 dir = (c.Position2D - b.Position2D);
                if (dir.magnitude < aoe)
                {
                    int damage = Mathf.RoundToInt(60 * self.LevelModifier);
                    Boids.Instance.DamageBoid(b, damage);

                    // Shockwave force
                    float force = 20f * self.LevelModifier;
                    b.velocity += new Vector3(-dir.x, 0, -dir.y) * force / (b.Radius * 2.0f);
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
