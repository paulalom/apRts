using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Storage))]
public class Transporter : MonoBehaviour {

    private Storage storage;

	// Use this for initialization
	void Start () {
        	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public bool Take(Dictionary<RTSGameObjectType, int> items, Storage target)
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

    public bool Give(Dictionary<RTSGameObjectType, int> items, Storage target)
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
