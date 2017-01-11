using UnityEngine;
using System.Collections.Generic;

public class Route {
    //Is this item needed?
    public bool CanTransportItem(Item item)
    {
        return true;
    }

    //Where is this item needed?
    public List<Station> GetStationsForItemType(ItemType type)
    {
        return new List<Station>();
    }
}
