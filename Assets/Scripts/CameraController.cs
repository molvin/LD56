using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Follow;
    public float Smoothing;

    private Vector3 velocity;

    private void Update()
    {
        Vector3 targetPosition = Follow.transform.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, Smoothing);
    }
}
