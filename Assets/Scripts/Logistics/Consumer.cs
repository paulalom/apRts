using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Storage))]
//[RequireComponent(typeof(RTSGameObject))]
public class Consumer : MonoBehaviour {

    private Storage storage;

    public Dictionary<Type, int> operationCosts;
    public float operationInterval = 5;
    float lastConsume;

    // Use this for initialization
    void Awake () {
        storage = GetComponent<Storage>();
        lastConsume = Time.time;
        operationCosts = new Dictionary<Type, int>();
        operationCosts.Add(typeof(Coal), 1);
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

    /* this needs some planning or recursion
    /// <summary>
    /// Tries to take the operational cost of items, with the option to run at less than 100% efficiency
    /// </summary>
    /// <returns>The percentage efficiency at which the consumer is operating</returns>
    int Operate(bool allOrNone)
    {
        Dictionary<Type, int> itemsTaken = new Dictionary<Type, int>();
        bool success = true;
        float efficiency = 1; //100%
        foreach (KeyValuePair<Type, int> kvp in operationCosts)
        {
            int effectiveCost = (int)(kvp.Value * efficiency);
            int qtyTaken = storage.TakeItem(kvp.Key, effectiveCost, allOrNone);
            
            if (qtyTaken == effectiveCost)
            {
                itemsTaken.Add(kvp.Key, effectiveCost);
            }
            else if (qtyTaken > 0)
            {
                if (allOrNone)
                {
                    success = false;
                    break;
                }
                else
                {
                    // put back what we've taken
                    storage.AddItems(itemsTaken);

                    efficiency = qtyTaken / effectiveCost; // Reduced Efficiency
                    
                }
            }
            else
            {

            }
        }
    }
    */
}
