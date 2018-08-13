using UnityEngine;
using System.Collections;

public class WaitOrder : Order {

    // Follow and Wait
    public override bool GetInRange(RTSGameObject performingUnit, int dt)
    {
        Vector3 targetPos = orderData.target == null ? orderData.targetPosition : orderData.target.transform.position;

        if (!rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPos, orderData.orderRange))
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(targetPos.x, targetPos.z), dt);
        }
        return false;
    }
    // Stand and wait
    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        return false;
    }
    
    public void OnWaitConditionFulfilled()
    {

    }
}
