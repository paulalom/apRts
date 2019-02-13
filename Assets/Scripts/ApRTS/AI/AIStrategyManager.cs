using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class AIStrategyManager {

    RTSGameObjectManager rtsGameObjectManager;
    Player playerToManage;
    Player largestEconomicRival;
    Player largestMilitaryRival;
    float safetyScore;

    Type nextToBuild;

    public AIStrategyManager(Player playerToManage, RTSGameObjectManager rtsGameObjectManager)
    {
        this.playerToManage = playerToManage;
        this.rtsGameObjectManager = rtsGameObjectManager;
    }

    public void UpdateStrategySettings()
    {

    }

    public void CheckForNewOrders()
    {

    }

    public List<MyPair<List<long>, Command>> GetStartCommands()
    {
        List<MyPair<List<long>, Command>> startCommands = new List<MyPair<List<long>, Command>>();
        List<MyPair<Type, int>> thingsToBuild = new List<MyPair<Type, int>>() { MyPairFactory.OneFactory() };
        List<Order> orders = new List<Order>() { OrderFactory.BuildConstructionOrder(thingsToBuild) };

        Vector3 startLocation = playerToManage.commander.transform.position;
        List<ResourceDeposit> startingResources = rtsGameObjectManager
                                                    .GetAllComponentsInRangeOfTypeInOrder<ResourceDeposit>(startLocation,
                                                    AITacticsManager.rangeToSearchForResources,
                                                    1 << LayerMask.NameToLayer("Resource")).Take(3).ToList();

        foreach (ResourceDeposit resource in startingResources)
        {
            thingsToBuild = new List<MyPair<Type, int>>() { MyPairFactory.OneHarvestingStation() };
            ConstructionOrder order = (ConstructionOrder)OrderFactory.BuildConstructionOrder(thingsToBuild);
            order.orderData.targetPosition = resource.transform.position;
            order.orderData.orderRange = 1f;
            orders.Add(order);
        }

        foreach (Order order in orders)
        {
            startCommands.Add(new MyPair<List<long>, Command>(new List<long>() { playerToManage.commander.unitId }, 
                CommandFactory.GetCommandFromOrder(order)));
        }

        return startCommands;
    }

}
