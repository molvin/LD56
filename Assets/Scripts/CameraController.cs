using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Follow;
    public float Smoothing;

    private Vector3 velocity;

    private bool _pause = false;

    Vector3 original_position;
    public void Start()
    {
        original_position = transform.position;
    }
    private void Update()
    {
        if(!_pause)
        {
            Vector3 targetPosition = Follow.transform.position;
            

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, Smoothing);
        }

    
    }

    public void pause(bool pause)
    {
        this._pause = pause;
    }
}
