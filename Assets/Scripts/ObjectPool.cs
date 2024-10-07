using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    private Dictionary<System.Type, List<GameObject>> pool = new();

    private void Awake()
    {
        instance = this;
    }

    public static T Get<T>(T template) where T : MonoBehaviour
    {
        if (!instance.pool.ContainsKey(typeof(T)))
        {
            instance.pool[typeof(T)] = new();
        }

        List<GameObject> available = instance.pool[typeof(T)];
        if (available.Count == 0)
        {
            GameObject obj = Instantiate(template.gameObject, instance.transform);
            available.Add(obj);
        }

        T active = available[available.Count - 1].GetComponent<T>();
        active.gameObject.SetActive(true);
        available.RemoveAt(available.Count - 1);
        return active;
    }

    public static void Return<T>(T obj) where T : MonoBehaviour
    {
        obj.gameObject.SetActive(false);

        instance.pool[typeof(T)].Add(obj.gameObject);
    }
}
