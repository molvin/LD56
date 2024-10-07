using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatStealer : MonoBehaviour
{
    public int kills = 0;

    // Update is called once per frame
    void Update()
    {
        var b = FindAnyObjectByType<Boids>();
        this.kills = b != null ? b.killCount : this.kills;
    }
}
