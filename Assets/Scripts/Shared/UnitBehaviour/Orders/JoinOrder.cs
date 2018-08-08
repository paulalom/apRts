using UnityEngine;
using System.Collections;

/// <summary>
/// Join TargetUnit in their current order, join order by target order target (eg. help construct, summon elder god)
/// </summary>
public class JoinOrder : Order {

    public override bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return base.Validate(performingUnit);
    }
}
