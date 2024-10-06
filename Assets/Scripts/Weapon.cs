using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public string Name;
    public int Damage = 60;
    public float ThrowSpeedModifier = 1.0f;
    public float Bouncyness = 0.24f;
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
        ThrowSpeedModifier = 0.5f,
    };

    public static Weapon Bouncer => new()
    {
        Name = "Bouncer",
        Bouncyness = 1.2f,
    };
}
