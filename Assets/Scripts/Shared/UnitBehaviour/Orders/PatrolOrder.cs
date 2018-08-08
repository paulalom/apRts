using UnityEngine;
using System.Collections;

/// <summary>
/// NOTE THIS ORDER DOES NOT WORK WITH MUTLI-UNITS (the target switches with the issuedPos as soon as any unit gets to the target)
/// could fix by instead of swapping local targets, giving the performing unit a new patrol order with reversed targets
/// </summary>
public class PatrolOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
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

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Vector3 tempOrderIssuePosition = orderData.targetPosition;
        orderData.targetPosition = orderData.orderIssuedPosition;
        orderData.orderIssuedPosition = tempOrderIssuePosition;
        performingUnit.currentOrderPhase = OrderPhase.Activate;
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckCanMove(performingUnit))
        {
            return OrderValidationResult.CantDoThat;
        }
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
