using System;
using UnityEngine;
using System.Collections.Generic;

public enum DepositType
{
    None,
    Forest,
    Iron,
    Coal
}
[RequireComponent(typeof(Storage))]
public class ResourceDeposit : RTSGameObject
{
    public DepositType type;
    public Dictionary<Type, int> harvestItems;
    public override void MyAwake()
    {
        storage = GetComponent<Storage>();
        harvestItems = new Dictionary<Type, int>();
        storage.requiredAccessComponents.Add(typeof(Harvester));
    }

    public override void MyUpdate()
    {
        // override defaultUpdate because resources dont need to onIdle
    }
}
