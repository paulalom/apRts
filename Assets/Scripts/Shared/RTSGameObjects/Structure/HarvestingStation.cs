using System;
using UnityEngine;
using System.Collections;

public class HarvestingStation : Structure {

    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone) };
    Consumer consumer;
    Harvester harvester;

    public override void MyAwake()
    {
        storage = GetComponent<Storage>();
        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
        // temp hack for under construction structures
        if (constructionComponent != null)
        {
            return;
        }

        consumer = GetComponent<Consumer>();
        harvester = GetComponent<Harvester>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        DefaultInit();
    }

    public override void MyStart()
    {
    }

    public override void MyUpdate()
    {
        // temp until we get Data orented design style lists of active objects
        if (harvester != null)
        {
            harvester.MyUpdate();
        }
    }

    public override bool ValidatePlacement(RTSGameObjectManager rtsGameObjectManager, Vector3 targetPosition)
    {
        return Harvester.GetHarvestingTarget(rtsGameObjectManager, GetComponent<Collider>(), targetPosition, GetComponent<Harvester>().harvestingRange);
    }
}
