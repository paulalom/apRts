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
    public RTSGameObject target;
    public Ability ability;
}
