using UnityEngine;
using System.Collections;

public class GiveOrder : Order {
    
    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, target.transform.position, orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.MoveUnit(performingUnit, new Vector2(target.transform.position.x, target.transform.position.z), dt);
            return false;
        }
    }

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.GiveItems(performingUnit, target, items);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (performingUnit == target)
        {
            return OrderValidationResult.NotOnSelf;
        }
        if (!ValidateStorageAccess(performingUnit, target)){
            return OrderValidationResult.Failure;
        }
        return OrderValidationResult.Success;
    }
}
