using System;
using UnityEngine;
using System.Collections;

public class PowerPlant : RTSGameObject
{
    void Awake()
    {
        storage = GetComponent<Storage>();
        storage.canContain.Add(typeof(Coal));
        storage.canContain.Add(typeof(Power));
        unitType = UnitType.Structure;
    }

}
