using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    private Dictionary<GameObject, List<GameObject>> pool = new();
    private Dictionary<GameObject, GameObject> handles = new();

    private void Awake()
    {
        instance = this;
    }

    public static T Get<T>(T template) where T : MonoBehaviour
    {
        return Get(template.gameObject).GetComponent<T>();
    }
    public static GameObject Get(GameObject gameObject)
    {
        if (!instance.pool.ContainsKey(gameObject))
        {
            instance.pool[gameObject] = new();
        }

        List<GameObject> available = instance.pool[gameObject];
        if (available.Count == 0)
        {
            GameObject obj = Instantiate(gameObject, instance.transform);
            available.Add(obj);
        }

        GameObject active = available[available.Count - 1];
        active.SetActive(true);
        available.RemoveAt(available.Count - 1);

        instance.handles[active] = gameObject;
        return active;
    }

    public static void Return<T>(T obj) where T : MonoBehaviour
    {
        Return(obj.gameObject);
    }
    public static void Return(GameObject obj)
    {
        obj.SetActive(false);

        var handle = instance.handles[obj];
        instance.pool[handle].Add(obj);
    }
}
