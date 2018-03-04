using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CollectResourcesPlan : Plan {

    int shouldDumpCargoThreshold = 50;
    int shouldGetFromHarvestingStationThreshold = 50;
    RTSGameObjectManager rtsGameObjectManager;
    AIManager aiManager;

    public CollectResourcesPlan(AIManager aiManager, RTSGameObjectManager rtsGameObjectManager)
    {
        this.rtsGameObjectManager = rtsGameObjectManager;
        this.aiManager = aiManager;
    }

    public List<Order> GetPlanSteps(RTSGameObject unit)
    {
        List<Order> steps = new List<Order>();

        if (ShouldReturnResourcesToDepot(unit))
        {
            steps.AddRange(FactoryInteraction.DumpCargoAtNearestDepot(unit, unit.transform.position, rtsGameObjectManager));
            steps.AddRange(TakeFromNearestHarvestingStation(unit, steps.Count > 0 ? steps[0].orderData.target.transform.position : unit.transform.position));
        }
        else
        {
            steps.AddRange(TakeFromNearestHarvestingStation(unit, unit.transform.position));
            steps.AddRange(FactoryInteraction.DumpCargoAtNearestDepot(unit, steps.Count > 0 ? steps[0].orderData.target.transform.position : unit.transform.position, rtsGameObjectManager));
        }
        
        return steps;
    }

    // This may want to check the distance to the depot, the distance to the next deposit, the value of the cargo.
    private bool ShouldReturnResourcesToDepot(RTSGameObject unit)
    {
        return unit.storage.freeSpace < shouldDumpCargoThreshold;
    }
    
    private HarvestingStation GetTargetHarvester(RTSGameObject unit, Vector3 searchPosition)
    {
        List<Harvester> harvesters = rtsGameObjectManager.GetAllComponentsInRangeOfTypeOwnedByPlayerInOrder<Harvester>(searchPosition,
                                                                            AITacticsManager.rangeToSearchForResources,
                                                                            unit.ownerId,
                                                                            rtsGameObjectManager.rtsGameObjectLayerMask);
        if (harvesters == null || harvesters.Count == 0)
        {
            return null;
        }

        foreach (Harvester harvester in harvesters)
        {
            Storage storage = harvester.GetComponent<Storage>();
            if (storage != null && storage.usedSpace >= shouldGetFromHarvestingStationThreshold)
            {
                return harvester.GetComponent<HarvestingStation>();
            }
        }
        return null;
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
            List<MyPair<Type, int>> items = harvestingStation.storage.GetItemsMyKVP(-1);
            if (items.Count != 0) {
                collectionOrders.Add(OrderFactory.BuildTakeOrder(harvestingStation, items));
            }
            return collectionOrders;
        }
    }
}
