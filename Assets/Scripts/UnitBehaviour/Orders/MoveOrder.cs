using UnityEngine;
using System.Collections;

public class MoveOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPosition, orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.MoveUnit(performingUnit, new Vector2(targetPosition.x, targetPosition.z), dt);
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
