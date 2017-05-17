using System;
using UnityEngine;
using System.Collections.Generic;

public class OrderFactory : MonoBehaviour {
  
    public Order NewGiveOrder(RTSGameObject target, List<MyKVP<Type, int>> items)
    {
        return new GiveOrder() { target = target, items = items, orderRange = 0 };
    }
}
