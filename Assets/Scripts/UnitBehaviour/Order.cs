using System;
using UnityEngine;
using System.Collections.Generic;

public enum OrderType
{
    Stop,
    Move,
    HoldPosition,
    Patrol,
    Follow,
    Guard,
    Give,
    Take,
    Harvest,
    Construct,
    UseAbillity
}

public class Order {
    
    public OrderType type;
    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public float orderRange;
    public MyKVP<Type, int> item;
    public RTSGameObject target;
    public Ability ability;

    public Order() { }

    public Order(Order o)
    {
        type = o.type;
        targetPosition = o.targetPosition;
        orderIssuedPosition = o.orderIssuedPosition;
        orderRange = o.orderRange;
        item = o.item;
        target = o.target;
        ability = o.ability;
    }
}
