using UnityEngine;
using System.Collections;

public class CancelOrder : Order {

    public override bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        return true;
    }
}
