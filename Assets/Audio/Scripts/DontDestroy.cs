using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    private static DontDestroy instance;
    void Awake()
    {
        Debug.Log("awake");
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
        
    }
}
