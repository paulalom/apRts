using UnityEngine;
using System.Collections;

public class ConstructOrder : Order {

    public override bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        if (targetPosition == Vector3.zero)
        {
            targetPosition = performingUnit.transform.position;
        }
        return base.GetInRange(performingUnit, rtsGameObjectManager, dt);
    }

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Producer producer = performingUnit.GetComponent<Producer>();
        if (producer.TryQueueItem(items[0].Key, items[0].Value))
        {
            remainingChannelTime = producer.productionTime[items[0].Key];
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

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Worker worker = performingUnit.GetComponent<Worker>();
        if (worker.unitUnderConstruction != null)
        {
            ((Structure)worker.unitUnderConstruction).CompleteConstruction(rtsGameObjectManager);
            worker.unitUnderConstruction = null;
        }
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }
}
