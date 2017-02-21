using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(Harvester))]
[RequireComponent(typeof(Storage))]
public class HarvestingStation : RTSGameObject {

    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone) };
    Producer producer;
    Consumer consumer;
    Harvester harvester;
    bool isActive = false;

    void Awake()
    {
        storage = GetComponent<Storage>();
        producer = GetComponent<Producer>();
        consumer = GetComponent<Consumer>();
        harvester = GetComponent<Harvester>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
    }

    void Start()
    {
        harvester.harvestTarget = (ResourceDeposit)(gameManager.GetNearestUnitInRangeOfType(this, harvester.harvestingRange, typeof(ResourceDeposit)));
        if (harvester.harvestTarget != null)
        {
            harvester.IsActive = true;
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

                // todo
                // find next harvest target
            }
        }
        else // REMOVE ME (and figure out why harvesters are deactivating randomly)
        {
            harvester.IsActive = true;
        }
    }
}
