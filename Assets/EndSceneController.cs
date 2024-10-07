using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneController : MonoBehaviour
{
    public SpawnNumbers killcounter;
    public SpawnNumbers levelcounter;
    public SpawnNumbers survivecounter;


    public void Start()
    {
        StatStealer  ss = FindAnyObjectByType<StatStealer>();
        killcounter.testNumber = ss.kills;
        killcounter.text = "Kills";

        levelcounter.testNumber = ss.level;
        levelcounter.text = "Level";

        survivecounter.testNumber = Mathf.FloorToInt(ss.surviveTime);
        survivecounter.text = "Time";

        StartCoroutine(killcounter.playAfterWait());
        StartCoroutine(levelcounter.playAfterWait());
        StartCoroutine(survivecounter.playAfterWait());

    }
}
