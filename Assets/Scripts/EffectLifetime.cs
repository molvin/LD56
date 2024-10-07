using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLifetime : MonoBehaviour
{
    public float Lifetime = 2f;
    private float start;
    private void Awake()
    {
        start = Time.time;
    }
    private void OnEnable()
    {
        start = Time.time;
    }
    private void Update()
    {
        if (Time.time - start > Lifetime)
        {
            ObjectPool.Return(this);
        }
    }
}
