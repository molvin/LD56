using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public float Radius;
    public Transform CameraPos;
    public Transform PlayerPos;
    public Follower[] Followers;
    public WeaponCard WillReplace;
    public Follower WillReplaceFollower;

    public void Init(Weapon[] weapons, Weapon replace)
    {
        for(int i = 0; i < 3; i++)
        {
            Followers[i].Init(weapons[i], null, false);
        }
        if(replace != null)
        {
            WillReplaceFollower.gameObject.SetActive(true);
            WillReplace.GetComponentInParent<Canvas>().enabled = true;
            WillReplace.Init(replace, null, true);
            WillReplaceFollower.Init(replace, null, false);
        }
        else
        {
            WillReplace.GetComponentInParent<Canvas>().enabled = false;
            WillReplaceFollower.gameObject.SetActive(false);
        }
    }
}
