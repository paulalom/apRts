using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(Harvester))]
[RequireComponent(typeof(Storage))]
public class HarvestingStation : Structure {

    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone) };
    Consumer consumer;
    Harvester harvester;

    void Awake()
    {
        storage = GetComponent<Storage>();
        consumer = GetComponent<Consumer>();
        harvester = GetComponent<Harvester>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
        harvester.IsActive = false;
    }

    void Start()
    {
        DefaultInit();
        int layerMask = LayerMask.NameToLayer("Resource");
        harvester.harvestTarget = (ResourceDeposit)(rtsGameObjectManager.GetNearestComponentInRange(GetComponent<Collider>(), transform.position, harvester.harvestingRange, 1 << layerMask));
        if (harvester.harvestTarget == null)
        {
            DemolishStructure("Invalid Construction Location: No Resource within " + harvester.harvestingRange + " units", gameManager, rtsGameObjectManager);
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
        else
        {
            if (!underConstruction)
            {
                harvester.IsActive = true;
            }
        }
    }
}
