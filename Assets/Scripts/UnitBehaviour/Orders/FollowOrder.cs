using UnityEngine;
using System.Collections;

public class FollowOrder : Order {
    
    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        if (!rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPosition, orderRange))
        {
            rtsGameObjectManager.MoveUnit(performingUnit, new Vector2(target.transform.position.x, target.transform.position.z), dt);
        }
        return false;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckCanMove(performingUnit))
        {
            return OrderValidationResult.CantDoThat;
        }
        if (!CheckTargetExists(target)){
            return OrderValidationResult.InvalidTarget;
        }
        if (performingUnit == target)
        {
            return OrderValidationResult.NotOnSelf;
        }
        return OrderValidationResult.Success;
    }
}
