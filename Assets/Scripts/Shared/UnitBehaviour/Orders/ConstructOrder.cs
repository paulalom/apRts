using UnityEngine;
using System.Collections;

public class ConstructionOrder : Order {

    public override bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        if (orderData.targetPosition == Vector3.zero)
        {
            orderData.targetPosition = performingUnit.transform.position;
        }
        return base.GetInRange(performingUnit, rtsGameObjectManager, dt);
    }

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Producer producer = performingUnit.GetComponent<Producer>();
        if (producer.TryQueueItem(orderData.items[0].Key, orderData.items[0].Value))
        {
            orderData.remainingChannelTime = producer.productionTime[orderData.items[0].Key];
            return true;
        }
        else
        {
            orderData.remainingChannelTime = 0;
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
        if (worker != null && worker.unitUnderConstruction != null)
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
