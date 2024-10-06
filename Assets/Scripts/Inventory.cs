using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<int> Equipment;
    public float FollowSpacing;
    public float FollowSmoothing;

    public GameObject Follower;
    private Queue<(int, GameObject)> followers = new Queue<(int, GameObject)>();
    private Dictionary<int, GameObject> inUse = new Dictionary<int, GameObject>();

    private void Start()
    {
        foreach(int i in Equipment)
        {
            SpawnFollower(i);
        }
    }

    private void Update()
    {
        Vector3 targetPosition = transform.position - transform.forward * FollowSpacing;
        foreach((int index, GameObject follower) in followers)
        {
            Vector3 velo = Vector3.zero;
            follower.transform.position = Vector3.SmoothDamp(follower.transform.position, targetPosition, ref velo, FollowSmoothing);
            targetPosition -= transform.forward * FollowSpacing;
        }
    }

    public void SpawnFollower(int i)
    {
        GameObject follower = Instantiate(Follower, transform.position, transform.rotation);
        followers.Enqueue((i, follower));
    }

    public void UseNextEquipment()
    {
        (int index, GameObject follower) = followers.Dequeue();
        follower.SetActive(false);
        inUse.Add(index, follower);
    }

    public void ReturnEquipment(int index)
    {
        GameObject follower = inUse[index];
        follower.SetActive(true);
        followers.Enqueue((index, follower));
    }

}
