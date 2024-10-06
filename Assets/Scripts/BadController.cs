using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class BadController : MonoBehaviour
{
    public float Acceleration = 24f;
    public float Drag = 0.04f;

    private Vector2 velocity;

    void Update()
    {
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            input.x -= 1;
        if (Input.GetKey(KeyCode.D))
            input.x += 1;
        if (Input.GetKey(KeyCode.W))
            input.y += 1;
        if (Input.GetKey(KeyCode.S))
            input.y -= 1;

        input = input.normalized;
        velocity *= Mathf.Pow(Drag, Time.deltaTime);
        velocity += input * Acceleration * Time.deltaTime;

        Vector2 actual = new Vector2(transform.position.x, transform.position.z);
        Vector2 target = actual + velocity * Time.deltaTime;
        float y = transform.position.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(new Vector3(target.x, transform.position.y - 1f, target.y), out hit, 2f, NavMesh.AllAreas))
        {
            actual = new Vector2(hit.position.x, hit.position.z);
            y = hit.position.y + 1f;
        }

        if (Vector2.Distance(target, actual) > target.magnitude * 0.1f)
        {
            Vector2 normal = (actual - target).normalized;
            Vector2 projection = normal * Vector2.Dot(velocity, normal);
            velocity -= projection;
        }

        transform.position = new Vector3(actual.x, y, actual.y);
    }
}
