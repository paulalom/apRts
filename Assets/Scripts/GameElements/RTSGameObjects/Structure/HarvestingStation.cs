using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(Harvester))]
[RequireComponent(typeof(Storage))]
public class HarvestingStation : RTSGameObject {

    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone) };
    Consumer consumer;
    Harvester harvester;
    bool isActive = false;

    void Awake()
    {
        storage = GetComponent<Storage>();
        consumer = GetComponent<Consumer>();
        harvester = GetComponent<Harvester>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        unitType = UnitType.Structure;

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
    }

    void Start()
    {
        int layerMask = LayerMask.NameToLayer("Resource");
        harvester.harvestTarget = (ResourceDeposit)(rtsGameObjectManager.GetNearestComponentInRange(GetComponent<Collider>(), transform.position, harvester.harvestingRange, 1 << layerMask));
        if (harvester.harvestTarget != null)
        {
            harvester.IsActive = true;
            idle = false;
        }
        else
        {
            idle = true; // AI Manager needs to find the harvester something to do
        }
    }

    void Update()
    {
        if (harvester.IsActive)
        {
            harvester.IsActive = consumer.Operate();
            if (!harvester.Harvest())
            {
                harvester.IsActive = false;
                idle = true;
            }
        }
        else // REMOVE ME (and figure out why harvesters are deactivating randomly)
        {
            harvester.IsActive = true;
        }
    }
}
