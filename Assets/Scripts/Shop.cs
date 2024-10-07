using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public float Radius;
    public Transform CameraPos;
    public Transform PlayerPos;
    public Follower[] Followers;

    public void Init(Weapon[] weapons)
    {
        for(int i = 0; i < 3; i++)
        {
            Followers[i].Init(weapons[i], null, false);
        }
    }
}
