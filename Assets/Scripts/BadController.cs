using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class BadController : MonoBehaviour
{
    public BoomerangController BoomerangPrefab;

    public float ThrowSpeed = 30f;
    public float ReturnAcc = 50f;
    public float ReturnJerk = 200f;
    public float Bouncy = 0.4f;
    public float BoomDrag = 0.04f;
    public float BoomStay = 1;
    public float BoomStayEffect = 0.2f;

    public float Acceleration = 24f;
    public float Drag = 0.04f;

    private Vector2 velocity;
    public Vector2 Position2D => new Vector2(transform.position.x, transform.position.z);

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit mouseHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out mouseHit))
            {
                Vector3 toMouse = (mouseHit.point - transform.position);
                Vector2 throwDirection = new Vector2(toMouse.x, toMouse.z).normalized;
                Vector3 throwDir = new Vector3(throwDirection.x, 0, throwDirection.y);
                BoomerangController boomerang = Instantiate(BoomerangPrefab);
                //boomerang.Owner = gameObject;
                boomerang.transform.position = transform.position + throwDir * 1.6f;

                // Let it inherit some velocity to feel good
                boomerang.velocity = velocity * 0.5f + throwDirection * ThrowSpeed;
                /* DEBUG
                boomerang.ReturnAcceleration = ReturnAcc;
                boomerang.ReturnJerk = ReturnJerk;
                boomerang.Bouncyness = Bouncy;
                boomerang.ReturnDrag = BoomDrag;
                boomerang.StayTime = BoomStay;
                boomerang.StayEffect = BoomStayEffect;
                */
            }
        }

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

        Vector2 normal = (actual - target).normalized;
        Vector2 projection = normal * Vector2.Dot(velocity, normal);
        velocity -= projection;

        transform.position = new Vector3(actual.x, y, actual.y);
    }
}
