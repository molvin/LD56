using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSpawns : MonoBehaviour
{
    public Transform[] Points;
    public float MinDistanceToPlayer;

    public Vector3 GetRandomPoint(Player player)
    {
        Vector3 result = Vector3.zero;
        if (Points.Length == 0)
            return result;
        for (int i = 0; i < 100; i++)
        {
            Transform point = Points[Random.Range(0, Points.Length)];
            float d = Vector3.Distance(point.position, player.transform.position);
            if (d <= MinDistanceToPlayer)
                continue;

            result = point.position;
        }
        return result;
    }

}
