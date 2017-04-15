using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class ConstructionPlan : Plan {

    public List<MyKVP<Type,int>> thingsToBuild;
    RTSGameObjectManager rtsGameObjectManager;
    AIManager aiManager;

    public ConstructionPlan()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        aiManager = GameObject.FindGameObjectWithTag("AIManager").GetComponent<AIManager>();
    }

    // Can i build them all?
    // if yes, go build
    // if no, get resources for as many as possible in order
    // if i got at least enough for one, build it and continue
    // 
    public List<Order> GetPlanSteps(RTSGameObject unit)
    {
        List<Order> planSteps = new List<Order>();
        List<MyKVP<Type, int>> missingResources;
        List<MyKVP<Type, int>> costs;
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
        else if (missingResources.Count != costs.Count) // we have some resources, so try to get more otherwise build
        {
            planSteps.AddRange(GetResources(unit, missingResources));
            planSteps.AddRange(ConstructThings(unit, producer));
        }
        else
        {
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

    List<MyKVP<Type, int>> GetConstructionCosts(Producer producer)
    {
        List<MyKVP<Type, int>> costs = new List<MyKVP<Type, int>>();
        foreach (MyKVP<Type, int> item in thingsToBuild)
        {
            foreach (KeyValuePair<Type, int> cost in producer.productionCost[item.Key])
            {
                costs.Add(new MyKVP<Type, int>(cost.Key, item.Value * cost.Value));
            }
        }
        return costs;
    }

    List<MyKVP<Type, int>> ValidateConstructions(RTSGameObject unit, Producer producer)
    {
        List<MyKVP<Type, int>> validatedThingsToBuild = new List<MyKVP<Type, int>>();
        
        foreach (MyKVP<Type,int> thingToBuild in thingsToBuild)
        {
            if (producer.ValidateNewProductionRequest(thingToBuild.Key, thingToBuild.Value))
            {
                validatedThingsToBuild.Add(thingToBuild);
            }
        }

        return validatedThingsToBuild;
    }

    List<MyKVP<Type, int>> CheckForMissingResources(RTSGameObject unit, List<MyKVP<Type, int>> costs)
    {
        foreach (var item in unit.storage.CheckForItemsInOrder(costs))
        {
            costs.Remove(item);
        }

        return costs;
    }

    private Factory GetTargetDepot(RTSGameObject unit, Vector3 searchPosition, List<MyKVP<Type, int>> missingItems)
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
            if (storage != null && storage.HasItems(missingItems))
            {
                return factory.GetComponent<Factory>();
            }
        }
        return null;
    }

    List<Order> GetResources(RTSGameObject unit, List<MyKVP<Type, int>> missingResources)
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
            foreach (MyKVP<Type, int> missingItem in missingResources)
            {
                collectionOrders.Add(new Order() { type = OrderType.Take, target = depot, orderRange = 3f, item = missingItem });
            }
            // After we collect, we move back
            collectionOrders.Add(new Order() { type = OrderType.Move, targetPosition = unit.transform.position, orderRange = 1f });
            return collectionOrders;
        }
    }

    List<Order> ConstructThings(RTSGameObject unit, Producer producer)
    {
        List<Order> constructionOrders = new List<Order>();

        foreach (MyKVP<Type, int> thingToBuild in thingsToBuild)
        {
            for(int i = 0; i < thingToBuild.Value; i++)
            {
                constructionOrders.Add(new Order() { type = OrderType.Construct, item = thingToBuild, orderRange = 3f, waitTimeAfterOrder = producer.productionTime[thingToBuild.Key] });
            }
        }
        return constructionOrders;
    }
    
}
