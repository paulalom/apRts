using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CollectResources : Plan {

    int shouldDumpCargoThreshold = 50;
    int shouldGetFromHarvestingStationThreshold = 50;
    int shouldDepositAtFactoryThreshold = 50;
    float rangeToSearchForResources = 50;
    RTSGameObjectManager rtsGameObjectManager;

    public CollectResources()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
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

    private Factory GetTargetDepot(RTSGameObject unit, Vector3 searchPosition)
    {
        List<Factory> factories = rtsGameObjectManager.GetAllComponentsInRangeOfType<Factory>(searchPosition,
                                                                            rangeToSearchForResources,
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
                                                                            rangeToSearchForResources,
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

        Factory depot = null;

        if (unit.target == null || !unit.target.GetComponent<Factory>())
        {
            depot = GetTargetDepot(unit, searchPosition);
        }
        else
        {
            depot = unit.target.GetComponent<Factory>();
        }
        
        // No station meets criteria, unit should remain idle
        if (depot == null)
        {
            return dropOffOrders;
        }
        else
        {
            foreach (KeyValuePair<Type, int> item in unit.storage.GetItems())
            {
                dropOffOrders.Add(new Order() { type = OrderType.Give, target = depot, orderRange = 3f, item = new MyKVP<Type, int>(item) });
            }
            return dropOffOrders;
        }
    }

    private List<Order> TakeFromNearestHarvestingStation(RTSGameObject unit, Vector3 searchPosition)
    {
        HarvestingStation station = null;
        List<Order> collectionOrders = new List<Order>(); // This should be a single order, but order does not yet support take of multiple items.

        if (unit.storage.freeSpace <= 0)
        {
           //return collectionOrders;
        }

        if (unit.target == null || !unit.target.GetComponent<HarvestingStation>())
        {
            station = GetTargetHarvester(unit, searchPosition);
        }
        else
        {
            station = unit.target.GetComponent<HarvestingStation>();
        }
        
        // No station meets criteria, unit should remain idle
        if (station == null)
        {
            return collectionOrders;
        }
        else
        {
            foreach (KeyValuePair<Type, int> item in station.storage.GetItems())
            {
                collectionOrders.Add(new Order() { type = OrderType.Take, target = station, orderRange = 3f, item = new MyKVP<Type, int>(item) });
            }
            return collectionOrders;
        }
    }
}
