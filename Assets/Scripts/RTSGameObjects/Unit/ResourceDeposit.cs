using System;
using UnityEngine;
using System.Collections;

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
    void Awake()
    {
        storage = GetComponent<Storage>();
    }
}
