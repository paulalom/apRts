using UnityEngine;
using System.Collections;

public class UseAbilityOrder : Order {

    public UseAbilityOrder() { }

    public UseAbilityOrder(Order o)
    {
        orderData.phase = o.orderData.phase;
        orderData.targetPosition = o.orderData.targetPosition;
        orderData.orderIssuedPosition = o.orderData.orderIssuedPosition;
        orderData.orderRange = o.orderData.orderRange;
        orderData.items = o.orderData.items;
        orderData.target = o.orderData.target;
        orderData.ability = o.orderData.ability;
        orderData.remainingChannelTime = o.orderData.remainingChannelTime;
    }
    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.UseAbility(performingUnit, orderData.target, orderData.targetPosition, orderData.ability);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return ValidateAbilityUsage(performingUnit, orderData.ability);
    }
}
