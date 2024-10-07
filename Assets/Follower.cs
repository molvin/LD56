using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public HatSelector HatSelector;
    public Vector3 TargetPosition;

    public void Init(Weapon weapon)
    {
        HatSelector.SetHat(weapon);
    }

    private void Update()
    {
        transform.position = TargetPosition;
    }
}
