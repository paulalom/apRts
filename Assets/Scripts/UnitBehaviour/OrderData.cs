using System;
using UnityEngine;
using System.Collections.Generic;

public struct OrderData {
    public OrderPhase phase;
    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public float orderRange;
    public List<MyPair<Type, int>> items;
    public RTSGameObject target;
    public Ability ability;
    public bool repeatOnComplete;
    public float remainingChannelTime;
}
