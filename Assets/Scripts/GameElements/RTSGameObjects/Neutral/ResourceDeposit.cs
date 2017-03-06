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
    void Awake()
    {
        storage = GetComponent<Storage>();
        harvestItems = new Dictionary<Type, int>();
        unitType = UnitType.Resource;
        storage.requiredAccessComponents.Add(typeof(Harvester));
    }
}
