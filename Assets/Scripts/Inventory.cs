using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int MaxWeapons = 4;
    public HUD HUD;
    private List<Weapon> ownedWeapons = new();
    private Queue<Weapon> weaponQueue = new();
    private Weapon lastAquired;
    public bool AtMax => ownedWeapons.Count == MaxWeapons;

    [Header("Follow")]
    public Vector3[] FollowPoints;
    public float FollowSpacing;
    public Follower FollowerPrefab;
    private Dictionary<Weapon, Follower> followers = new();
    

    private void Start()
    {
        AddNewWeapon(Weapons.Bouncer);
        FollowPoints = new Vector3[MaxWeapons];
    }

    private void Update()
    {
        for (int i = 0; i < MaxWeapons; i++)
        {
            FollowPoints[i] = transform.position - transform.forward * FollowSpacing * (i + 1);
        }
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

        // TODO: remove relevant follower
        // followers[weapon].DeleteFollow();
        // followers.Remove(weapon);

        return weapon;
    }

    public void AddWeapon(Weapon weapon)
    {
        if(ownedWeapons.Contains(weapon))
        {
            weaponQueue.Enqueue(weapon);
            HUD.SetWeapons(weaponQueue);
        }
    }

    public void AddNewWeapon(Weapon weapon)
    {
        lastAquired = weapon;
        ownedWeapons.Add(weapon);
        AddWeapon(weapon);
    }

    public void ReplaceWeapon(Weapon oldWeapon, Weapon newWeapon)
    {
        int i = ownedWeapons.IndexOf(oldWeapon);
        ownedWeapons[i] = newWeapon;
        lastAquired = newWeapon;

        var weapons = weaponQueue.ToList();
        weapons.Remove(oldWeapon);
        weapons.Insert(0, newWeapon);
        weaponQueue = new Queue<Weapon>(weapons);
        HUD.SetWeapons(weaponQueue);
    }

    public Weapon GetRandom()
    {
        return ownedWeapons[Random.Range(0, ownedWeapons.Count)];
    }
}
