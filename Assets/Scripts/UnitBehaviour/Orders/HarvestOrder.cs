using UnityEngine;
using System.Collections;

public class HarvestOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, orderData.targetPosition, orderData.orderRange))
        {
            rtsGameObjectManager.Harvest(performingUnit, (ResourceDeposit)orderData.target);
            return true;
        }
        else
        {
            if (performingUnit.GetComponent<Mover>() != null)
            {
                rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(orderData.targetPosition.x, orderData.targetPosition.z), dt);
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!ValidateStorageAccess(performingUnit, orderData.target))
        {
            return OrderValidationResult.Failure;
        }
        return OrderValidationResult.Success;
    }
}
