using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShopCage : MonoBehaviour
{


    public void Start()
    {
        this.GetComponent<Animator>().SetBool("InShop", true);

    }


}
