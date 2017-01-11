using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Storage))]
public class Factory : MonoBehaviour {
    public FactoryData factoryType;
    // Update is called once per frame
    private Storage storage;
    private ProductionData[] productions;
    public Station localStation;

    void Awake()
    {
        storage = GetComponent<Storage>();
        productions = new ProductionData[factoryType.productions.Length];
        for (int i = 0; i < factoryType.productions.Length; i++)
        {
            productions[i] = factoryType.productions[i];
        }

        if (localStation != null)
        {
            //localStation.RegisterStorageItem(storage);
            storage = localStation.GetComponent<Storage>();
        }

    }

    void Update() {
        for (int i = 0; i < factoryType.productions.Length; i++)
        {
            if (CheckRequirements(ref productions[i]))
            {
                ProduceItem(ref productions[i]);
            }
        }
    }

    bool CheckRequirements(ref ProductionData production)
    {
        if (Time.time < production.lastUpdateTime + production.requiredTime)
        {
            return false;
        }
        for (int i = 0; i < production.requiredItems.Length; i++)
        {
            if (storage.GetItemCount(production.requiredItems[i].type) < production.requiredItems[i].count)
            {
                return false;
            }
        }
        return true;
    }
    void ProduceItem(ref ProductionData production)
    {
        for (int i = 0; i < production.requiredItems.Length; i++)
        {
            storage.RemoveItem(production.requiredItems[i]);
        }
        storage.AddItem(production.targetItem);
        production.lastUpdateTime = Time.time;
    }
}
