using System;
using UnityEngine;
using System.Collections.Generic;

public static class OrderFactory {
  
    public static Order NewGiveOrder(RTSGameObject target, List<MyPair<Type, int>> items)
    {
        return new GiveOrder() { target = target, items = items, orderRange = 0 };
    }

    public static Order NewConstructionOrder(List<MyPair<Type, int>> items)
    {
        return new ConstructionOrder() { items = items, targetPosition = Vector3.zero };
    }

    public static Order NewConstructionOrder(List<MyPair<Type, int>> items, Vector3 targetPosition)
    {
        return new ConstructionOrder() { items = items, targetPosition = targetPosition };
    }

    public static Order GetDefaultUseAbilityOrder()
    {
        return new UseAbilityOrder();
    }
    public static Order GetDefaultCancelOrder()
    {
        return new CancelOrder() { orderRange = 1f };
    }
    public static Order GetDefaultHarvestOrder()
    {
        return new HarvestOrder() { orderRange = 15f };
    }
    public static Order GetDefaultPatrolOrder()
    {
        return new PatrolOrder() { orderRange = 1f };
    }
    public static Order GetDefaultGuardOrder()
    {
        return new GuardOrder() { orderRange = 6f };
    }
    public static Order GetDefaultFollowOrder()
    {
        return new FollowOrder() { orderRange = 6f };
    }
}
