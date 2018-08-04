using UnityEngine;
using System.Collections;

public class GiveOrder : Order {
    
    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, orderData.target.transform.position, orderData.orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, orderData.target.Position2D, dt);
            return false;
        }
    }

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.GiveItems(performingUnit, orderData.target, orderData.items);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (performingUnit == orderData.target)
        {
            return OrderValidationResult.NotOnSelf;
        }
        if (!ValidateStorageAccess(performingUnit, orderData.target)){
            return OrderValidationResult.Failure;
        }
        return OrderValidationResult.Success;
    }
}
