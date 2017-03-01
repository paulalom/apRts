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

public class ResourceDeposit : RTSGameObject
{
    public DepositType type;
    public Dictionary<Type, int> harvestItems;
    void Awake()
    {
        storage = GetComponent<Storage>();
        harvestItems = new Dictionary<Type, int>();
        unitType = UnitType.Resource;
    }
}
