using UnityEngine;
using System.Collections;

// Does nothing, cancellation is handled by the ordermanager's "SetOrder" rather than "QueueOrder"
// So we're setting the current order to nothing
public class StopOrder : Order {

    public override bool GetInRange(RTSGameObject performingUnit, int dt)
    {
        return true;
    }
}
