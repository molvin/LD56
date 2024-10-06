using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int MaxWeapons = 4;
    public HUD HUD;
    private Queue<Weapon> weaponQueue = new();
    private Weapon lastAquired;
    public bool AtMax => weaponQueue.Count == MaxWeapons;

    private void Start()
    {
        weaponQueue.Enqueue(Weapons.TheOrb);
        weaponQueue.Enqueue(Weapons.TheOrb);
        weaponQueue.Enqueue(Weapons.TheOrb);
        lastAquired = weaponQueue.Peek();
        HUD.SetWeapons(weaponQueue);
    }

    public Weapon PeekNextWeapon()
    {
        bool found = weaponQueue.TryPeek(out Weapon result);
        return found ? result : null;
    }
    public Weapon UseNextWeapon()
    {
        if (weaponQueue.Count == 0)
            return null;
        Weapon weapon = weaponQueue.Dequeue();
        HUD.SetWeapons(weaponQueue);
        return weapon;
    }

    public void AddWeapon(Weapon weapon)
    {
        weaponQueue.Enqueue(weapon);
        HUD.SetWeapons(weaponQueue);
        lastAquired = weapon;
    }

    public void ReplaceWeapon(Weapon oldWeapon, Weapon newWeapon)
    {
        var weapons = weaponQueue.ToList();
        weapons.Remove(oldWeapon);
        weapons.Insert(0, newWeapon);
        weaponQueue = new Queue<Weapon>(weapons);
    }

    public Weapon GetRandom()
    {
        var weapons = weaponQueue.ToList().Except(new List<Weapon>() { lastAquired}).ToList();
        return weapons[Random.Range(0, weaponQueue.Count)];
    }
}
