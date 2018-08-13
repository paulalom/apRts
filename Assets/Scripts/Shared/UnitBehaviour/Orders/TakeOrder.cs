using UnityEngine;
using System.Collections;

public class TakeOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, orderData.target.transform.position, orderData.orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(orderData.target.transform.position.x, orderData.target.transform.position.z), dt);
            return false;
        }
    }

    public override bool FinishChannel(RTSGameObject performingUnit)
    {
        rtsGameObjectManager.TakeItems(performingUnit, orderData.target, orderData.items);
        return true;
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
