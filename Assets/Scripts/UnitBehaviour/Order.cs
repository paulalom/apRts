using System;
using UnityEngine;

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
    public MyKVP<Type, int> item;
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
        item = o.item;
        target = o.target;
        ability = o.ability;
        waitTimeAfterOrder = o.waitTimeAfterOrder;
    }
}
