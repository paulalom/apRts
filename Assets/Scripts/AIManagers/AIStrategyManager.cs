using System;
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

    public void CheckForNewOrders()
    {

    }

    public Dictionary<RTSGameObject, List<Order>> GetStartOrders()
    {
        Dictionary<RTSGameObject, List<Order>> startOrders = new Dictionary<RTSGameObject, List<Order>>();
        List<MyKVP<Type, int>> thingsToBuild = new List<MyKVP<Type, int>>() { MyKVPFactory.OneFactory() };
        List<Order> orders = new List<Order>() { OrderFactory.NewConstructionOrder(thingsToBuild, Vector3.zero) };

        int rangeToSearch = 200;
        List<ResourceDeposit> startingResources = rtsGameObjectManager.GetAllComponentsInRangeOfType<ResourceDeposit>(playerToManage.commander.transform.position, rangeToSearch, 1 << LayerMask.NameToLayer("Resource"));
        for (int i = 0; i < startingResources.Count; i++)
        {
            if (i > 2)
            {
                break;
            }
            orders.Add(new MoveOrder() { targetPosition = startingResources[i].transform.position });
            thingsToBuild = new List<MyKVP<Type, int>>() { MyKVPFactory.OneHarvestingStation() };
            orders.Add(OrderFactory.NewConstructionOrder(thingsToBuild, Vector3.zero));
        }

        startOrders.Add(playerToManage.commander, orders);

        return startOrders;
    }

}
