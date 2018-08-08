using System;
using UnityEngine;
using System.Collections.Generic;

//Harvester facilitates the collection of raw resources such as wood, iron etc. from natural deposits or enemy structures
//Special harvesters can "harvest" other peoples buildings, units, inventories etc like an attack
//These are not required for collecting items which drop on the ground, only for collecting raw resources and scrapping things
[RequireComponent(typeof(Storage))]
public class Harvester : Transporter
{ 
    public int harvesterLevel = 1;
    public float operationInterval = 5000;
    private float lastHarvest;
    public ResourceDeposit harvestTarget;
    public float harvestingRange = 20;
    private bool _isActive = false;

    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            if (_isActive != value)
            {
                lastHarvest = StepManager.gameTime;
                _isActive = value;
            }
        }
    }
    //private Dictionary<Type, int> harvestItems = new Dictionary<Type, int>();

    public override void MyAwake()
    {
        storage = GetComponent<Storage>();
        lastHarvest = StepManager.gameTime;
    }

    public bool Harvest()
    {
        if (StepManager.gameTime > lastHarvest + operationInterval)
        {
            lastHarvest = StepManager.gameTime;
            return Take(harvestTarget.harvestItems, harvestTarget.storage, false);
        }
        else // We are operating because we recently consumed
        {
            return true;
        }
    }
}
