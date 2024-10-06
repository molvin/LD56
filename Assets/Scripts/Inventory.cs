using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public HUD HUD;
    private Queue<Weapon> weaponQueue = new();
    
    private void Start()
    {
        weaponQueue.Enqueue(Weapons.Default);
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
    }
}
