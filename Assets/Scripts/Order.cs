using UnityEngine;
using System.Collections;

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
    UseAbillity
}

public class Order {
    
    public OrderType type;
    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public MyKVP<RTSGameObjectType, int> item;
    public RTSGameObject target;
    public Ability ability;
}
