using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxManager : MonoBehaviour
{
    private static FxManager instance;
    private Dictionary<string, GameObject> loadedResources = new();
    private Dictionary<object, Dictionary<string, GameObject>> keyedResources = new();

    private void Awake()
    {
        instance = this;
    }

    public static GameObject Get(string effect)
    {
        if (!instance.loadedResources.ContainsKey(effect))
        {
            GameObject fx = Resources.Load<GameObject>($"Effects/{effect}");
            instance.loadedResources.Add(effect, fx);
        }

        GameObject vfx = instance.loadedResources[effect];
        return ObjectPool.Get(vfx);
    }

    public static GameObject GetWithKey(string effect, object key)
    {
        if (!instance.keyedResources.ContainsKey(key))
        {
            instance.keyedResources.Add(key, new());
        }

        var store = instance.keyedResources[key];
        if (!store.ContainsKey(effect))
        {
            store[effect] = Get(effect);
        }

        return store[effect];
    }

    public static void ClearKey(object key)
    {
        instance.keyedResources.Remove(key);
    }
}
