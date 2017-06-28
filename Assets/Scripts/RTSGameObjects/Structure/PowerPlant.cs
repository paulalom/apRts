using System;
using UnityEngine;
using System.Collections;

public class PowerPlant : Structure
{
    public override void MyAwake()
    {
        storage = GetComponent<Storage>();
        storage.canContain.Add(typeof(Coal));
        storage.canContain.Add(typeof(Power));
    }

}
