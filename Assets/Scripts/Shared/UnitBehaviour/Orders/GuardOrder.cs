using UnityEngine;
using System.Collections;

public class GuardOrder : Order {
    
    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        if (false) // There is a unit threatening the target
        {
            // engage!
        }
        else // Follow
        {
            if (!rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, orderData.targetPosition, orderData.orderRange))
            {
                // this order isnt invalid for things that cant move. We may want defensive structures to prioritize the defense of a certain unit
                if (performingUnit.GetComponent<Mover>() != null)
                {
                    rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(orderData.target.transform.position.x, orderData.target.transform.position.z), dt);
                }
                else
                {
                    // If the unit moves out of range and guarding unit can't follow, remove the guard order
                    return true;
                }
            }
        }
        return false;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckTargetExists(orderData.target))
        {
            return OrderValidationResult.InvalidTarget;
        }
        if (performingUnit == orderData.target)
        {
            return OrderValidationResult.NotOnSelf;
        }
        return OrderValidationResult.Success;
    }
}
