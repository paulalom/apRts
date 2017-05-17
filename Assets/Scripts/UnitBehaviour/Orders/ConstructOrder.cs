using UnityEngine;
using System.Collections;

public class ConstructOrder : Order {

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Producer producer = performingUnit.GetComponent<Producer>();
        if (producer.TryQueueItem(items[0].Key, items[0].Value))
        {
            return true;
        }
        else
        {
            remainingChannelTime = 0;
            return true;
        }
    }

    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        updateChannelDuration(dt);
        return IsFinishedChanneling();
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }
}
