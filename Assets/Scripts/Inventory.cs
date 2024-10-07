using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int MaxWeapons = 4;
    private List<Weapon> ownedWeapons = new();
    private Queue<Weapon> weaponQueue = new();
    private Weapon lastAquired;
    public bool AtMax => ownedWeapons.Count == MaxWeapons;

    [Header("Follow")]
    public Vector3[] FollowPoints;
    public float FollowSpacing;
    public Follower FollowerPrefab;
    private List<Follower> followers = new();
    

    private void Start()
    {
        AddNewWeapon(Weapons.Meteor);
        AddNewWeapon(Weapons.Zapper);
        AddNewWeapon(Weapons.Chaining);
        FollowPoints = new Vector3[MaxWeapons];
    }

    private void Update()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            FollowPoints[i] = transform.position - transform.forward * FollowSpacing * (i + 1);
            followers[i].TargetPosition = FollowPoints[i];
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

        Follower follower = followers.First(x => x.Weapon == weapon);
        ObjectPool.Return(follower);
        followers.Remove(follower);

        return weapon;
    }

    public void AddWeapon(Weapon weapon)
    {
        if(ownedWeapons.Contains(weapon))
        {
            weaponQueue.Enqueue(weapon);
            Follower follower = ObjectPool.Get(FollowerPrefab);
            follower.Init(weapon, transform);
            followers.Add(follower);
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

        {
            Follower follower = followers.FirstOrDefault(x => x.Weapon == oldWeapon);
            if(follower)
            {
                ObjectPool.Return(follower);
                followers.Remove(follower);
            }
        }
        {
            Follower follower = ObjectPool.Get(FollowerPrefab);
            follower.Init(newWeapon, transform);
            followers.Add(follower);
        }
    }

    public Weapon GetRandom()
    {
        return ownedWeapons[Random.Range(0, ownedWeapons.Count)];
    }
}
