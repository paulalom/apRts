using System;
using UnityEngine;
using System.Collections.Generic;

public class OrderFactory {
  
    public static Order NewGiveOrder(RTSGameObject target, List<MyKVP<Type, int>> items)
    {
        return new GiveOrder() { target = target, items = items, orderRange = 0 };
    }

    /// <summary>
    /// Creates a construction order
    /// </summary>
    /// <param name="items">Things to construct</param>
    /// <param name="targetPosition">Pass in Vector3.zero for default</param>
    /// <returns>Order</returns>
    public static Order NewConstructionOrder(List<MyKVP<Type, int>> items, Vector3 targetPosition)
    {
        return new ConstructionOrder() { items = items, targetPosition = targetPosition };
    }
}
