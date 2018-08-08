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

    public override void MyAwake()
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

    public override void MyStart()
    {
        DefaultInit();
        int layerMask = LayerMask.NameToLayer("Resource");
        harvester.harvestTarget = (ResourceDeposit)(rtsGameObjectManager.GetNearestComponentInRange(GetComponent<Collider>(), transform.position, harvester.harvestingRange, 1 << layerMask));
        if (harvester.harvestTarget == null)
        {
            DemolishStructure("Invalid Construction Location: No Resource within " + harvester.harvestingRange + " units", gameManager, rtsGameObjectManager);
        }
    }

    public override void MyUpdate()
    {
        if (harvester.IsActive)
        {
            harvester.IsActive = consumer.Operate();
            if (!harvester.Harvest())
            {
                harvester.IsActive = false;
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
