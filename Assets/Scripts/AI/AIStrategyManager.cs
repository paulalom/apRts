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

    public Dictionary<RTSGameObject, List<Order>> GetStartOrders()
    {
        Dictionary<RTSGameObject, List<Order>> startOrders = new Dictionary<RTSGameObject, List<Order>>();
        List<MyPair<Type, int>> thingsToBuild = new List<MyPair<Type, int>>() { MyPairFactory.OneFactory() };
        List<Order> orders = new List<Order>() { OrderFactory.BuildConstructionOrder(thingsToBuild) };

        Vector3 startLocation = playerToManage.commander.transform.position;
        List<ResourceDeposit> startingResources = rtsGameObjectManager.GetAllComponentsInRangeOfTypeInOrder<ResourceDeposit>(startLocation, AITacticsManager.rangeToSearchForResources, 1 << LayerMask.NameToLayer("Resource"));
        
        foreach(ResourceDeposit resource in startingResources)
        {
            MoveOrder moveOrder = new MoveOrder();
            moveOrder.orderData.targetPosition = resource.transform.position;
            orders.Add(moveOrder);
            thingsToBuild = new List<MyPair<Type, int>>() { MyPairFactory.OneHarvestingStation() };
            orders.Add(OrderFactory.BuildConstructionOrder(thingsToBuild));
        }

        startOrders.Add(playerToManage.commander, orders);

        return startOrders;
    }

}
