using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

// Consumer is a passive consumption required to operate, like power or fuel
// Active consumptions access the storage directly
[RequireComponent(typeof(Storage))]
public class Consumer : MyMonoBehaviour {

    private Storage storage;

    public Dictionary<Type, int> operationCosts; // Passive resource cost just to stay online
    public int operationInterval = 400; // ms
    long lastConsume;

    // Use this for initialization
    public override void MyAwake() {
        storage = GetComponent<Storage>();
        lastConsume = StepManager.gameTime;
        operationCosts = new Dictionary<Type, int>();
	}
    
    public bool Operate()
    {
        if (StepManager.gameTime > lastConsume + operationInterval)
        {
            lastConsume = StepManager.gameTime;
            return storage.TakeItems(operationCosts);
        }
        else
        {
            return false;
        }
    }

    public bool Operate(Dictionary<Type, int> externalConsumptionCosts)
    {
        Dictionary<Type, int> costs = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> cost in externalConsumptionCosts)
        {
            costs.Add(cost.Key, cost.Value);
        }

        if (StepManager.gameTime > lastConsume + operationInterval)
        {
            lastConsume = StepManager.gameTime;
            foreach (KeyValuePair<Type, int> cost in operationCosts)
            {
                costs.Add(cost.Key, cost.Value);
            }
        }
        return storage.TakeItems(costs);
    }
}
