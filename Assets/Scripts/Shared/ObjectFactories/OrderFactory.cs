﻿using System;
using UnityEngine;
using System.Collections.Generic;

public static class OrderFactory {
  
    public static Order BuildConstructionOrder(List<MyPair<Type, int>> items)
    {
        return BuildConstructionOrder(items, Vector3.zero);
    }

    public static Order BuildConstructionOrder(List<MyPair<Type, int>> items, float buildTime)
    {
        return BuildConstructionOrder(items, Vector3.zero, buildTime);
    }

    public static Order BuildConstructionOrder(List<MyPair<Type, int>> items, Vector3 targetPosition)
    {
        Order order = new ConstructionOrder();
        order.orderData.targetPosition = targetPosition;
        order.orderData.items = items;
        return order;
    }
    
    public static Order BuildConstructionOrder(List<MyPair<Type, int>> items, Vector3 targetPosition, float buildTime)
    {
        Order order = new ConstructionOrder();
        order.orderData.targetPosition = targetPosition;
        order.orderData.items = items;
        order.orderData.remainingChannelTime = buildTime;
        return order;
    }

    public static Order BuildAbilityOrder(RTSGameObject target, Ability ability)
    {
        Order abilityOrder = new UseAbilityOrder();
        abilityOrder.orderData.target = target;
        abilityOrder.orderData.orderRange = ability.range;
        abilityOrder.orderData.ability = ability;
        abilityOrder.orderData.remainingChannelTime = ability.cooldown;
        return abilityOrder;
    }

    public static Order BuildAbilityOrder(RTSGameObject target, Vector3 targetPosition, float range, Ability ability)
    {
        Order abilityOrder = new UseAbilityOrder();
        abilityOrder.orderData.target = target;
        abilityOrder.orderData.targetPosition = targetPosition;
        abilityOrder.orderData.orderRange = range;
        abilityOrder.orderData.ability = ability;
        return abilityOrder;
    }

    public static Order BuildGiveOrder(RTSGameObject target, List<MyPair<Type, int>> items)
    {
        return BuildGiveOrder(target, 0f, items);
    }

    public static Order BuildGiveOrder(RTSGameObject target, float range, List<MyPair<Type, int>> items)
    {
        Order giveOrder = new GiveOrder();
        giveOrder.orderData.target = target;
        giveOrder.orderData.orderRange = range;
        giveOrder.orderData.items = items;
        return giveOrder;
    }

    public static Order BuildMoveOrder(Vector3 targetPosition)
    {
        Order moveOrder = new MoveOrder();
        moveOrder.orderData.targetPosition = targetPosition;
        moveOrder.orderData.orderRange = 1f;
        return moveOrder;
    }

    public static Order BuildTakeOrder(RTSGameObject target, List<MyPair<Type, int>> items)
    {
        return BuildTakeOrder(target, 3f, items);
    }

    public static Order BuildTakeOrder(RTSGameObject target, float range, List<MyPair<Type, int>> items)
    {
        Order takeOrder = new TakeOrder();
        takeOrder.orderData.target = target;
        takeOrder.orderData.orderRange = range;
        takeOrder.orderData.items = items;
        return takeOrder;
    }

    public static Order GetDefaultUseAbilityOrder()
    {
        return new UseAbilityOrder();
    }
    
    public static Order GetDefaultCancelOrder()
    {
        Order order = new CancelOrder();
        order.orderData.orderRange = 1f;
        return order;
    }
    public static Order GetDefaultConstructionOrder()
    {
        Order order = new ConstructionOrder();
        order.orderData.orderRange = 1f;
        return order;
    }
    public static Order GetDefaultHarvestOrder()
    {
        Order order = new HarvestOrder();
        order.orderData.orderRange = 15f;
        return order;
    }
    public static Order GetDefaultPatrolOrder()
    {
        Order order = new PatrolOrder();
        order.orderData.orderRange = 1f;
        return order;
    }
    public static Order GetDefaultGuardOrder()
    {
        Order order = new GuardOrder();
        order.orderData.orderRange = 6f;
        return order;
    }
    public static Order GetDefaultFollowOrder()
    {
        Order order = new FollowOrder();
        order.orderData.orderRange = 6f;
        return order;
    }
    public static Order GetDefaultMoveOrder()
    {
        Order order = new MoveOrder();
        order.orderData.orderRange = .3f;
        return order;
    }
    public static Order GetDefaultGiveOrder()
    {
        Order order = new GiveOrder();
        order.orderData.orderRange = .3f;
        return order;
    }
    public static Order GetDefaultTakeOrder()
    {
        Order order = new TakeOrder();
        order.orderData.orderRange = .3f;
        return order;
    }

    public static Func<Order> GetDefaultOrderFunction(string orderType)
    {
        switch (orderType)
        {
            case "UseAbilityOrder":
                return GetDefaultUseAbilityOrder;
            case "CancelOrder":
                return GetDefaultCancelOrder;
            case "ConstructionOrder":
                return GetDefaultConstructionOrder;
            case "HarvestOrder":
                return GetDefaultHarvestOrder;
            case "PatrolOrder":
                return GetDefaultPatrolOrder;
            case "GuardOrder":
                return GetDefaultGuardOrder;
            case "FollowOrder":
                return GetDefaultFollowOrder;
            case "MoveOrder":
                return GetDefaultMoveOrder;
            case "GiveOrder":
                return GetDefaultGiveOrder;
            case "TakeOrder":
                return GetDefaultTakeOrder;
            default:
                throw new Exception("order type not found: " + orderType);
        }
    }

    public static Order GetNewOrderOftype(string orderType)
    {
        switch (orderType)
        {
            case "UseAbilityOrder":
                return new UseAbilityOrder();
            case "CancelOrder":
                return new CancelOrder();
            case "ConstructionOrder":
                return new ConstructionOrder();
            case "HarvestOrder":
                return new HarvestOrder();
            case "PatrolOrder":
                return new PatrolOrder();
            case "GuardOrder":
                return new GuardOrder();
            case "FollowOrder":
                return new FollowOrder();
            case "MoveOrder":
                return new MoveOrder();
            case "GiveOrder":
                return new GiveOrder();
            case "TakeOrder":
                return new TakeOrder();
            default:
                throw new Exception("order type not found: " + orderType);
        }
    }
}