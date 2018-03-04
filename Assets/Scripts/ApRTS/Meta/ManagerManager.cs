using System;
using UnityEngine;
using System.Collections.Generic;

public class ManagerManager : MonoBehaviour {

    public string[] insperctorPrefabNames;
    public GameObject[] inspectorPrefabTypes;
    public Dictionary<string, GameObject> managerPrefabs;

    void Awake()
    {
        DontDestroyOnLoad(this);
        managerPrefabs = new Dictionary<string, GameObject>();

        if (insperctorPrefabNames.Length != inspectorPrefabTypes.Length)
        {
            throw new System.Exception("fix the prefabs arrays in the manager manager");
        }
        if (insperctorPrefabNames.Length <= 0)
        {
            throw new System.Exception("Populate the prefabs arrays in the manager manager");
        }
        for (int i = 0; i < inspectorPrefabTypes.Length; i++)
        {
            managerPrefabs.Add(insperctorPrefabNames[i], inspectorPrefabTypes[i]);
        }
        if (insperctorPrefabNames.Length != managerPrefabs.Count)
        {
            throw new System.Exception("No duplicate prefab names in the manager manager");
        }
    }

    public GameObject SpawnManager(Type managerType)
    {
        return Instantiate(managerPrefabs[managerType.ToString()]);
    }

}
