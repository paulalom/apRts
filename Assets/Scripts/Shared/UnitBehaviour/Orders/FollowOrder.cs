using UnityEngine;
using System.Collections;

public class FollowOrder : Order {
    
    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        if (!rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, orderData.targetPosition, orderData.orderRange))
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(orderData.target.transform.position.x, orderData.target.transform.position.z), dt);
        }
        return false;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckCanMove(performingUnit))
        {
            return OrderValidationResult.CantDoThat;
        }
        if (!CheckTargetExists(orderData.target)){
            return OrderValidationResult.InvalidTarget;
        }
        if (performingUnit == orderData.target)
        {
            return OrderValidationResult.NotOnSelf;
        }
        return OrderValidationResult.Success;
    }
}
