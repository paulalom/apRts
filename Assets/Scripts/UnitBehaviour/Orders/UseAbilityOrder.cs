using UnityEngine;
using System.Collections;

public class UseAbilityOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        Vector3 targetPos = target == null ? targetPosition : target.transform.position;
        
        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPos, orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.MoveUnit(performingUnit, new Vector2(targetPos.x, targetPos.z), dt);
            return false;
        }
    }

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.UseAbility(performingUnit, target, targetPosition, ability);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return ValidateAbilityUsage(performingUnit,ability);
    }
}
