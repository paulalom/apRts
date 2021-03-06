﻿using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Storage))]
public class Transporter : MyMonoBehaviour {

    protected Storage storage;
    
	public override void MyAwake() {
        storage = GetComponent<Storage>();
	}

    public bool Take(Dictionary<Type, int> items, Storage target, bool allOrNone = true)
    {
        if (target.TakeItems(items)) //Do they have the items?
        {
            if (!storage.AddItems(items)) // Do we have room?
            {
                target.AddItems(items);
                return false;
            }
        }
        return true;
    }

    public bool Give(Dictionary<Type, int> items, Storage target, bool allOrNone = true)
    {
        if (storage.TakeItems(items)) // Do we have the items?
        {
            if (!target.AddItems(items)) // Do they have room?
            {
                target.TakeItems(items);
                return false;
            }
        }
        return true;
    }
}
