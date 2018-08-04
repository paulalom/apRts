using UnityEngine;
using System.Collections;

public class CheatRaiseTerrainOrder : Order {

    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        rtsGameObjectManager.CheatRaiseTerrain(orderData.targetPosition);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }
}
