using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DestroyEffectWhenDone : MonoBehaviour
{

    void Update()
    {
        if(this.GetComponent<ParticleSystem>().isStopped)
        {
            Destroy(this.gameObject);
        }
    }
}
