using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ProductionOrder : Order {

    Producer producer;
    Consumer consumer;

    public override bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        return true; // Production orders are stationary
    }

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        producer = performingUnit.GetComponent<Producer>();
        consumer = performingUnit.GetComponent<Consumer>();
        producer.SetProductionTarget(orderData.items[0].Key);
        orderData.remainingChannelTime = producer.productionTime[orderData.items[0].Key];
        return true;
    }

    // Foreach unit assigned to order
    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        Dictionary<Type, int> productionCosts = producer.GetCostForProductionStep(producer.currentProductionType, orderData.remainingChannelTime);
        if (consumer.Operate(performingUnit.storage, productionCosts))
        {
            ChannelForTime(dt);
            producer.timeLeftToProduce = orderData.remainingChannelTime; // temporary for GUI to work
        }
        return IsFinishedChanneling();
    }

    public override bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        if (performingUnit.storage.canContain.Contains(producer.currentProductionType))
        {
            return producer.ProduceToStorage();
        }
        else
        {
            return producer.ProduceUnitsToWorld().Count > 0;
        }
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        Producer producer = performingUnit.GetComponent<Producer>();
        if (producer.ValidateNewProductionRequest(orderData.items[0].Key, orderData.items[0].Value))
        {
            return OrderValidationResult.Success;
        }
        else {
            return OrderValidationResult.Failure;
        }
    }

    public override void OnQueue(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        base.OnQueue(performingUnit, rtsGameObjectManager);
    }

    public override void OnCancel(RTSGameObject performingUnit, GameManager gameManager, RTSGameObjectManager rtsGameObjectManager)
    {
        float totalChannelTime = producer.productionTime[producer.currentProductionType] - orderData.remainingChannelTime;
        producer.CancelProduction(totalChannelTime);
    }
}
