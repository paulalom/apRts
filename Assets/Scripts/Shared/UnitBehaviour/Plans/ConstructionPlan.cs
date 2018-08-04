using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class ConstructionPlan : Plan {

    public List<MyPair<Type,int>> thingsToBuild;
    RTSGameObjectManager rtsGameObjectManager;
    AIManager aiManager;

    public ConstructionPlan(AIManager aiManager, RTSGameObjectManager rtsGameObjectManager)
    {
        this.rtsGameObjectManager = rtsGameObjectManager;
        this.aiManager = aiManager;
    }

    // Can i build them all?
    // if yes, go build
    // if no, get resources for as many as possible in order
    // if i got at least enough for one, build it and continue
    // 
    public List<Order> GetPlanSteps(RTSGameObject unit)
    {
        List<Order> planSteps = new List<Order>();
        List<MyPair<Type, int>> missingResources;
        List<MyPair<Type, int>> costs;
        Producer producer = unit.GetComponent<Producer>();
        if (producer == null)
        {
            return planSteps;
        }

        thingsToBuild = ValidateConstructions(unit, producer);
        if (thingsToBuild.Count == 0)
        {
            return planSteps;
        }
        
        costs = GetConstructionCosts(producer);
        missingResources = CheckForMissingResources(unit, costs);

        if (missingResources.Count == 0)
        {
            planSteps.AddRange(ConstructThings(unit, producer));
        }
        else
        {
            if (missingResources.Count != costs.Count)
            {
                planSteps.AddRange(FactoryInteraction.DumpCargoAtNearestDepot(unit, unit.transform.position, rtsGameObjectManager));
            }
            planSteps.AddRange(GetResources(unit, missingResources));
            if (planSteps.Count == 0)
            {
                return planSteps; // Couldn't find resources or didnt have room
            }
            else
            {
                planSteps.AddRange(ConstructThings(unit, producer));
            }
        }
        
        return planSteps;
    }

    List<MyPair<Type, int>> GetConstructionCosts(Producer producer)
    {
        List<MyPair<Type, int>> costs = new List<MyPair<Type, int>>();
        foreach (MyPair<Type, int> item in thingsToBuild)
        {
            foreach (KeyValuePair<Type, int> cost in producer.productionCost[item.Key])
            {
                costs.Add(new MyPair<Type, int>(cost.Key, item.Value * cost.Value));
            }
        }
        return costs;
    }

    List<MyPair<Type, int>> ValidateConstructions(RTSGameObject unit, Producer producer)
    {
        List<MyPair<Type, int>> validatedThingsToBuild = new List<MyPair<Type, int>>();
        
        foreach (MyPair<Type,int> thingToBuild in thingsToBuild)
        {
            if (producer.ValidateNewProductionRequest(thingToBuild.Key, thingToBuild.Value))
            {
                validatedThingsToBuild.Add(thingToBuild);
            }
        }

        return validatedThingsToBuild;
    }

    List<MyPair<Type, int>> CheckForMissingResources(RTSGameObject unit, List<MyPair<Type, int>> costs)
    {
        foreach (var item in unit.storage.GetItemsInInventoryInOrder(costs))
        {
            costs.Remove(item);
        }

        return costs;
    }

    private Factory GetTargetDepot(RTSGameObject unit, Vector3 searchPosition, List<MyPair<Type, int>> missingItems)
    {
        List<Factory> factories = rtsGameObjectManager.GetAllComponentsInRangeOfTypeOwnedByPlayerInOrder<Factory>(searchPosition,
                                                                            AITacticsManager.rangeToSearchForResources,
                                                                            unit.ownerId,
                                                                            rtsGameObjectManager.rtsGameObjectLayerMask);
        if (factories == null || factories.Count == 0)
        {
            return null;
        }

        foreach (Factory factory in factories)
        {
            Storage storage = factory.GetComponent<Storage>();
            if (storage != null && storage.HasItems(missingItems))
            {
                return factory.GetComponent<Factory>();
            }
        }
        return null;
    }

    List<Order> GetResources(RTSGameObject unit, List<MyPair<Type, int>> missingResources)
    {
        Factory depot = null;
        List<Order> collectionOrders = new List<Order>(); // This should be a single order, but order does not yet support take of multiple items.

        if (unit.storage.freeSpace <= 0)
        {
            //return collectionOrders;
        }
        depot = GetTargetDepot(unit, unit.transform.position, missingResources);

        // No station meets criteria, unit should remain idle
        if (depot == null)
        {
            return collectionOrders;
        }
        else
        {
            collectionOrders.Add(OrderFactory.BuildTakeOrder(depot, missingResources));
            // After we collect, we move back
            collectionOrders.Add(OrderFactory.BuildMoveOrder(new Vector3(unit.transform.position.x, unit.transform.position.y, unit.transform.position.z)));
            return collectionOrders;
        }
    }

    List<Order> ConstructThings(RTSGameObject unit, Producer producer)
    {
        List<Order> constructionOrders = new List<Order>();

        foreach (MyPair<Type, int> thingToBuild in thingsToBuild)
        {
            for(int i = 0; i < thingToBuild.Value; i++)
            {
                constructionOrders.Add(OrderFactory.BuildConstructionOrder(new List<MyPair<Type, int>>() { thingToBuild }));
            }
        }
        return constructionOrders;
    }
    
}
