using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatStealer : MonoBehaviour
{
    public int kills = 0;
    public int level = 0;
    public float surviveTime = 0;
    private static StatStealer instance;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        var b = FindAnyObjectByType<Boids>();
        this.kills = b != null ? b.killCount : this.kills;
        var p = FindAnyObjectByType<Player>();
        this.level = p != null ? p.Level : this.level;
        this.surviveTime = p != null ?  p.dieTime - p.startTime : surviveTime;
    }
}
