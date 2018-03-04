using System;
using UnityEngine;
using System.Collections.Generic;

// Consumer is a passive consumption required to operate, like power or fuel
// Active consumptions access the storage directly
[RequireComponent(typeof(Storage))]
public class Consumer : MyMonoBehaviour {

    private Storage storage;

    public Dictionary<Type, int> operationCosts;
    public float operationInterval = 5;
    float lastConsume;

    // Use this for initialization
    public override void MyAwake() {
        storage = GetComponent<Storage>();
        lastConsume = Time.time;
        operationCosts = new Dictionary<Type, int>();
	}

    // this should probably just be in update and fire an event when the state changes

    /// <summary>
    /// Takes operating costs from storage
    /// </summary>
    /// <returns>Whether or not we could operate</returns>
    public bool Operate()
    {
        if (Time.time > lastConsume + operationInterval)
        {
            lastConsume = Time.time;
            return storage.TakeItems(operationCosts);
        }
        else // We are operating because we recently consumed
        {
            return true;
        }
    }
}
