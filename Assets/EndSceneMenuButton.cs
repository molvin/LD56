using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EndSceneMenuButton : MonoBehaviour
{

    public GameObject minionPrefab;
    public int numberOfMinions = 100;
    public Transform minionSpawn;
    void Start()
    {
        Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/bulli_bulli"), this.transform.position, true);

        for (int j = 0; j < numberOfMinions; j++)
        {
            setMinionDestination(gameObject, Instantiate(minionPrefab, RandomNavmeshLocation(20), Quaternion.identity));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = default;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("UI")))
            {
                if(hit.collider.gameObject == this.gameObject) {
                    SceneManager.LoadScene(0);
                }

            }
        }
        
      
    }
    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += minionSpawn.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    public void setMinionDestination(GameObject destination, GameObject agent)
    {

        //agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.GetComponent<Animator>().SetBool("IsRunning",true);

        Vector3 result;
        MeshCollider collider = destination.GetComponent<MeshCollider>();
        float margin = 0.0f;
        int limit = 100;
        do
        {

            result = new Vector3(
                    UnityEngine.Random.Range(collider.bounds.min.x + margin, collider.bounds.max.x - margin),
                    collider.bounds.center.y,
                    UnityEngine.Random.Range(collider.bounds.min.z + margin, collider.bounds.max.z - margin)
                );
            limit = --limit;
        } while (!IsInsideMeshCollider(collider, result) && limit >= 0);
        //  Debug.Log(limit);
        if (IsInsideMeshCollider(collider, result))
        {
            //agent.destination = result;
            StartCoroutine(moveMinion(result, agent));
        }


    }

    IEnumerator moveMinion(Vector3 target, GameObject agent)
    {
        while ((target - agent.transform.position).magnitude > 0.01f)
        {
            agent.transform.position += (target - agent.transform.position).normalized * 15f * Time.deltaTime;
            yield return null;

        }
        yield return null;
    }

    public static bool IsInside(MeshCollider c, Vector3 point)
    {

        var closest = c.ClosestPoint(point);
        // Because closest=point if point is inside - not clear from docs I feel
        Debug.Log(closest == point);
        return closest == point;
    }

    bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
    {
        var temp = Physics.queriesHitBackfaces;
        Ray ray = new Ray(point, Vector3.back);

        bool hitFrontFace = false;
        RaycastHit hit = default;

        Physics.queriesHitBackfaces = true;
        bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
        if (hitFrontOrBackFace)
        {
            Physics.queriesHitBackfaces = false;
            hitFrontFace = col.Raycast(ray, out hit, 100f);
        }
        Physics.queriesHitBackfaces = temp;

        if (!hitFrontOrBackFace)
        {
            return false;
        }
        else if (!hitFrontFace)
        {
            return true;
        }
        else
        {
            // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
            if (hit.distance > hit2.distance)
            {
                return true;
            }
            else
                return false;
        }

    }
}
