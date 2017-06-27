using UnityEngine;
using System.Collections;

public class UseAbilityOrder : Order {

    public UseAbilityOrder() { }

    public UseAbilityOrder(Order o)
    {
        phase = o.phase;
        targetPosition = o.targetPosition;
        orderIssuedPosition = o.orderIssuedPosition;
        orderRange = o.orderRange;
        items = o.items;
        target = o.target;
        ability = o.ability;
        remainingChannelTime = o.remainingChannelTime;
    }
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
