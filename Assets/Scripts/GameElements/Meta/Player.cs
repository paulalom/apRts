using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class Player {

    public string name;
    public UnityEvent onSelectionChange;
    public HashSet<RTSGameObject> selectedUnits;
    public HashSet<RTSGameObject> units;
    public Dictionary<Type,int> resources;
    public UnityEvent onResourceChange;

    public void AddResources(Dictionary<Type, int> items)
    {
        foreach (KeyValuePair<Type, int> item in items)
        {
            if (resources.ContainsKey(item.Key))
            {
                resources[item.Key] += item.Value;
            }
            else
            {
                resources[item.Key] = item.Value;
            }
        }
        onResourceChange.Invoke();
    }

    public void TakeResources(Dictionary<Type,int> items)
    {
        foreach (KeyValuePair<Type, int> item in items)
        {
            if (resources.ContainsKey(item.Key))
            {
                resources[item.Key] -= item.Value;
            }
            else
            {
                resources[item.Key] = -item.Value;
            }
        }
        onResourceChange.Invoke();
    }
}
