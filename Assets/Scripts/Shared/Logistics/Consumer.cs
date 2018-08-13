using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

// Consumer is a passive consumption required to operate, like power or fuel
// Active consumptions access the storage directly
[RequireComponent(typeof(Storage))]
public class Consumer : MyMonoBehaviour {

    private Storage storage;

    public Dictionary<Type, int> operationCosts; // costs to operate structure per second of active time
    public long upkeepInterval = 1000; // 1s
    long lastUpkeepTaken;

    // Use this for initialization
    public override void MyAwake() {
        storage = GetComponent<Storage>();
        operationCosts = new Dictionary<Type, int>();
        lastUpkeepTaken = StepManager.gameTime;
	}
    
    public bool Operate()
    {
        if (StepManager.gameTime - lastUpkeepTaken > upkeepInterval)
        {
            return storage.TakeItems(operationCosts);
        }
        else
        {
            return true;
        }
    }

    public bool Operate(Storage storage, Dictionary<Type, int> externalConsumptionCosts)
    {
        Dictionary<Type, int> costs = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> cost in externalConsumptionCosts)
        {
            costs.Add(cost.Key, cost.Value);
        }
        if (StepManager.gameTime - lastUpkeepTaken > upkeepInterval)
        {
            foreach (KeyValuePair<Type, int> cost in operationCosts)
            {
                costs.Add(cost.Key, cost.Value);
            }
            if (storage.HasItems(costs))
            {
                lastUpkeepTaken = StepManager.gameTime;
            }
            else
            {
                return false;
            }
        }
        return storage.TakeItems(costs);
    }
}
