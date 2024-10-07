using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    private Dictionary<System.Type, List<GameObject>> pool = new();

    private void Awake()
    {
        instance = this;
    }

    public T Get<T>(T template) where T : MonoBehaviour
    {
        if (!pool.ContainsKey(typeof(T)))
        {
            pool[typeof(T)] = new();
        }

        List<GameObject> available = pool[typeof(T)];
        if (available.Count == 0)
        {
            GameObject obj = Instantiate(template.gameObject, transform);
            available.Add(obj);
        }

        T active = available[available.Count - 1].GetComponent<T>();
        active.gameObject.SetActive(true);
        available.RemoveAt(available.Count - 1);
        return active;
    }

    public void Return<T>(T obj) where T : MonoBehaviour
    {
        obj.gameObject.SetActive(false);

        pool[typeof(T)].Add(obj.gameObject);
    }
}
