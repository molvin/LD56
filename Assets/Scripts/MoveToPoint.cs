using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPoint : MonoBehaviour
{
    public Vector3 desiredPos;
    public float Speed = 0.3f;
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPos, Speed);
    }
}
