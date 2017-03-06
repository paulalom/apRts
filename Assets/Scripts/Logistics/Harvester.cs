using System;
using UnityEngine;
using System.Collections.Generic;

//Harvester facilitates the collection of raw resources such as wood, iron etc. from natural deposits or enemy structures
//Special harvesters can "harvest" other peoples buildings, units, inventories etc like an attack
//These are not required for collecting items which drop on the ground, only for collecting raw resources and scrapping things
[RequireComponent(typeof(Storage))]
public class Harvester : Transporter/* : Transporter */  // still not sure on the structure
{ 
    public int harvesterLevel = 1;
    public float operationInterval = 5;
    private float lastHarvest;
    public ResourceDeposit harvestTarget;
    public float harvestingRange = 200;
    private bool _isActive = false;
    //private Storage storage;

    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            if (_isActive != value)
            {
                lastHarvest = Time.time;
                _isActive = value;
            }
        }
    }
    private Dictionary<Type, int> harvestItems = new Dictionary<Type, int>();

    void Awake()
    {
        storage = GetComponent<Storage>();
        lastHarvest = Time.time;
    }

    public bool Harvest()
    {
        if (Time.time > lastHarvest + operationInterval)
        {
            lastHarvest = Time.time;
            return Take(harvestTarget.harvestItems, harvestTarget.storage, false);
        }
        else // We are operating because we recently consumed
        {
            return true;
        }
    }

    /*
    // I'm duplicating the give/take code for now because inheriting from transporter causes GetComponent<Transporter> to return harvesters as well

    public bool Take(Dictionary<Type, int> items, Storage target, bool allOrNone = true)
    {
        if (target.TakeItems(items)) //Do they have the items?
        {
            if (!storage.AddItems(items)) // Do we have room?
            {
                target.AddItems(items);
                return false;
            }
        }
        return true;
    }

    public bool Give(Dictionary<Type, int> items, Storage target, bool allOrNone = true)
    {
        if (storage.TakeItems(items)) // Do we have the items?
        {
            if (!target.AddItems(items)) // Do they have room?
            {
                target.TakeItems(items);
                return false;
            }
        }
        return true;
    }*/
}
