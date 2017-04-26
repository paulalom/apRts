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
    UseAbillity,
    Wait
}

public enum OrderPhase
{
    Active,
    Wait
}

// Orders probably need to be rewritten into many classes (one per type?)
public class Order {
    
    public OrderType type;
    public OrderPhase phase;
    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public float orderRange;
    public List<MyKVP<Type, int>> items;
    public RTSGameObject target;
    public Ability ability;
    public bool repeatOnComplete = false;
    public float waitTimeAfterOrder;

    public Order() { }

    public Order(Order o)
    {
        type = o.type;
        phase = o.phase;
        targetPosition = o.targetPosition;
        orderIssuedPosition = o.orderIssuedPosition;
        orderRange = o.orderRange;
        items = o.items;
        target = o.target;
        ability = o.ability;
        waitTimeAfterOrder = o.waitTimeAfterOrder;
    }
}
