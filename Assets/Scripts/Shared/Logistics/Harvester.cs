using System;
using UnityEngine;
using System.Collections.Generic;

//Harvester facilitates the collection of raw resources such as wood, iron etc. from natural deposits or enemy structures
//Special harvesters can "harvest" other peoples buildings, units, inventories etc like an attack
//These are not required for collecting items which drop on the ground, only for collecting raw resources and scrapping things
[RequireComponent(typeof(Storage))]
[RequireComponent(typeof(Consumer))]
public class Harvester : Transporter
{ 
    public int harvesterLevel = 1;
    public float operationInterval = 5000;
    private float lastHarvest;
    public ResourceDeposit harvestTarget;
    private Consumer consumer;
    public float harvestingRange = 20;

    public override void MyAwake()
    {
        storage = GetComponent<Storage>();
        consumer = GetComponent<Consumer>();
        lastHarvest = StepManager.gameTime;
        RTSGameObjectManager rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        harvestTarget = GetHarvestingTarget(rtsGameObjectManager, GetComponent<Collider>(), transform.position, harvestingRange);
    }

    public override void MyUpdate()
    {
        if (consumer.Operate())
        {
            Harvest();
        }
    }

    public bool Harvest()
    {
        if (StepManager.gameTime - lastHarvest > operationInterval 
            && Take(harvestTarget.harvestItems, harvestTarget.storage, false))
        {
            lastHarvest = StepManager.gameTime;
            return true;
        }
        else 
        {
            return false;
        }
    }
    
    public static ResourceDeposit GetHarvestingTarget(RTSGameObjectManager rtsGameObjectManager, Collider harvestingStationCollider, Vector3 position, float harvestingRange)
    {
        int layerMask = LayerMask.NameToLayer("Resource");
        return (ResourceDeposit)(rtsGameObjectManager.GetNearestComponentInRange(harvestingStationCollider, position, harvestingRange, 1 << layerMask));
    }
}
