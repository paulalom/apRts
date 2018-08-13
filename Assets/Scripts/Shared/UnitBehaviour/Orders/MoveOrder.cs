using UnityEngine;
using System.Collections;

public class MoveOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, orderData.targetPosition, orderData.orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(orderData.targetPosition.x, orderData.targetPosition.z), dt);
            return false;
        }
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckCanMove(performingUnit))
        {
            return OrderValidationResult.CantDoThat;
        }
        return OrderValidationResult.Success;
    }
}
