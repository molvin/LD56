using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneController : MonoBehaviour
{
    public SpawnNumbers killcounter;

    public void Start()
    {
        StatStealer  ss = FindAnyObjectByType<StatStealer>();
        killcounter.testNumber = ss.kills;
        killcounter.text = "Kills";
    }
}
