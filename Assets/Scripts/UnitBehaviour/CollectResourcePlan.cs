﻿using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CollectResources : Plan {

    int shouldDumpCargoThreshold = 50;
    int shouldGetFromHarvestingStationThreshold = 50;
    int shouldDepositAtFactoryThreshold = 50;
    RTSGameObjectManager rtsGameObjectManager;
    AIManager aiManager;

    public CollectResources()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        aiManager = GameObject.FindGameObjectWithTag("AIManager").GetComponent<AIManager>();
    }

    public List<Order> GetPlanSteps(RTSGameObject unit)
    {
        List<Order> steps = new List<Order>();

        if (ShouldReturnResourcesToDepot(unit))
        {
            steps.AddRange(DumpCargoAtNearestDepot(unit, unit.transform.position));
            steps.AddRange(TakeFromNearestHarvestingStation(unit, steps.Count > 0 ? steps[0].target.transform.position : unit.transform.position));
        }
        else
        {
            steps.AddRange(TakeFromNearestHarvestingStation(unit, unit.transform.position));
            steps.AddRange(DumpCargoAtNearestDepot(unit, steps.Count > 0 ? steps[0].target.transform.position : unit.transform.position));
        }
        
        return steps;
    }

    // This may want to check the distance to the depot, the distance to the next deposit, the value of the cargo.
    private bool ShouldReturnResourcesToDepot(RTSGameObject unit)
    {
        return unit.storage.freeSpace < shouldDumpCargoThreshold;
    }

    // should merge the get target functions but ill do that later
    private Factory GetTargetDepot(RTSGameObject unit, Vector3 searchPosition)
    {
        List<Factory> factories = rtsGameObjectManager.GetAllComponentsInRangeOfType<Factory>(searchPosition,
                                                                            aiManager.rangeToSearchForResources,
                                                                            rtsGameObjectManager.rtsGameObjectLayerMask);
        if (factories == null || factories.Count == 0)
        {
            return null;
        }

        List<Factory> sortedFactories = factories
                .OrderBy(
                      x => Vector3.SqrMagnitude(searchPosition - x.transform.position))
                        .ToList();


        foreach (Factory factory in sortedFactories)
        {
            Storage storage = factory.GetComponent<Storage>();
            if (storage != null && storage.freeSpace >= shouldDepositAtFactoryThreshold)
            {
                return factory.GetComponent<Factory>();
            }
        }
        return null;
    }

    private HarvestingStation GetTargetHarvester(RTSGameObject unit, Vector3 searchPosition)
    {
        List<Harvester> harvesters = rtsGameObjectManager.GetAllComponentsInRangeOfType<Harvester>(searchPosition,
                                                                            aiManager.rangeToSearchForResources,
                                                                            rtsGameObjectManager.rtsGameObjectLayerMask);
        if (harvesters == null || harvesters.Count == 0)
        {
            return null;
        }

        List<Harvester> sortedHarvesters = harvesters
                                                .OrderBy(
                                                x => Vector3.SqrMagnitude(searchPosition - x.transform.position))
                                                    .ToList();

        foreach (Harvester harvester in sortedHarvesters)
        {
            Storage storage = harvester.GetComponent<Storage>();
            if (storage != null && storage.usedSpace >= shouldGetFromHarvestingStationThreshold)
            {
                return harvester.GetComponent<HarvestingStation>();
            }
        }
        return null;
    }

    private List<Order> DumpCargoAtNearestDepot(RTSGameObject unit, Vector3 searchPosition)
    {
        List<Order> dropOffOrders = new List<Order>(); // This should be a single order, but order does not yet support take of multiple items.

        Factory depot = GetTargetDepot(unit, searchPosition);
        
        // No station meets criteria, unit should remain idle
        if (depot == null)
        {
            return dropOffOrders;
        }
        else
        {
            foreach (KeyValuePair<Type, int> item in unit.storage.GetItems())
            {
                dropOffOrders.Add(new Order() { type = OrderType.Give, target = depot, orderRange = 3f, item = new MyKVP<Type, int>(item.Key, -1) });
            }
            return dropOffOrders;
        }
    }

    private List<Order> TakeFromNearestHarvestingStation(RTSGameObject unit, Vector3 searchPosition)
    {
        HarvestingStation harvestingStation = null;
        List<Order> collectionOrders = new List<Order>(); // This should be a single order, but order does not yet support take of multiple items.

        if (unit.storage.freeSpace <= 0)
        {
           //return collectionOrders;
        }
        harvestingStation = GetTargetHarvester(unit,searchPosition);
        
        // No station meets criteria, unit should remain idle
        if (harvestingStation == null)
        {
            return collectionOrders;
        }
        else
        {
            foreach (KeyValuePair<Type, int> item in harvestingStation.storage.GetItems())
            {
                collectionOrders.Add(new Order() { type = OrderType.Take, target = harvestingStation, orderRange = 3f, item = new MyKVP<Type, int>(item.Key, -1) });
            }
            return collectionOrders;
        }
    }
}