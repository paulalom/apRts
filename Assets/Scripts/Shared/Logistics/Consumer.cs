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
            if (storage.TakeItems(operationCosts))
            {
                lastUpkeepTaken = StepManager.gameTime;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Run a tick of production, consuming external costs from productionStorage and upkeepCosts from internal storage.
    /// </summary>
    /// <param name="productionStorage"></param>
    /// <param name="externalConsumptionCosts"></param>
    /// <returns></returns>
    public bool Operate(Storage productionStorage, Dictionary<Type, int> externalConsumptionCosts)
    {
        // need to check so we dont take operating costs unless we're going to succeed in taking production costs
        if (productionStorage.HasItems(externalConsumptionCosts))
        {
            return Operate() && productionStorage.TakeItems(externalConsumptionCosts);
        }
        else
        {
            return false;
        }        
    }

    public Dictionary<Type, int> GetOperatingCostsForTimespan (int timeInMs)
    {
        Dictionary<Type, int> costs = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> cost in operationCosts)
        {
            costs.Add(cost.Key, (int)(cost.Value * timeInMs / (float)upkeepInterval) + 1);
        }
        return costs;
    }
}
