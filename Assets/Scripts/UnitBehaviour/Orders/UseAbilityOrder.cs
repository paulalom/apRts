using UnityEngine;
using System.Collections;

public class UseAbilityOrder : Order {

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.UseAbility(performingUnit, target, targetPosition, ability);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return ValidateAbilityUsage(performingUnit,ability);
    }
}
