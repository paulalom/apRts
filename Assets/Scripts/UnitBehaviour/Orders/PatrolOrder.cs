using UnityEngine;
using System.Collections;

public class PatrolOrder : Order {

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

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Vector3 tempOrderIssuePosition = targetPosition;
        targetPosition = orderIssuedPosition;
        orderIssuedPosition = tempOrderIssuePosition;
        phase = OrderPhase.Activate;
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckCanMove(performingUnit))
        {
            return OrderValidationResult.CantDoThat;
        }
        if (!CheckTargetExists(target))
        {
            return OrderValidationResult.InvalidTarget;
        }
        if (performingUnit == target)
        {
            return OrderValidationResult.NotOnSelf;
        }
        return OrderValidationResult.Success;
    }
}
