using UnityEngine;
using System.Collections;

public class CheatSpawnFactoryOrder : Order {

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.CheatSpawnFactory(orderData.targetPosition, performingUnit.ownerId);
        return true;
    }
    
    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }
}
