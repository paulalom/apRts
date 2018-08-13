using UnityEngine;
using System.Collections;

public class HoldPositionOrder : Order { 

    public override bool Channel(RTSGameObject performingUnit, int dt)
    {

        return false;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        if (!CheckCanMove(performingUnit))
        {
            return OrderValidationResult.CantDoThat;
        }
        return OrderValidationResult.Success;
    }
}
