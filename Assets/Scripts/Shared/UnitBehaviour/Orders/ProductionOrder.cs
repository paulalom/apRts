using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ProductionOrder : Order {

    Producer producer;
    Consumer consumer;
    Type currentProductionType;

    public override void Initilize(RTSGameObject performingUnit)
    {
        base.Initilize(performingUnit);
        orderData.target = initiatingUnit;
        currentProductionType = orderData.items[0].Key;
    }

    public override bool Activate(RTSGameObject performingUnit)
    {
        if (isActivated) { return true; }
        base.Activate(performingUnit);
        producer = performingUnit.GetComponent<Producer>();
        consumer = performingUnit.GetComponent<Consumer>();
        orderData.remainingChannelTime = producer.productionTime[currentProductionType];

        orderData.isJoinable = true;
        return true;
    }

    // take necessary resources from joining units
    public override void Join(RTSGameObject performingUnit)
    {
        base.Join(performingUnit);
    }

    // Foreach unit assigned to order
    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        Dictionary<Type, int> productionCosts = producer.GetCostForProductionStep(currentProductionType, orderData.remainingChannelTime);
        if (consumer.Operate(performingUnit.storage, productionCosts))
        {
            ChannelForTime(dt);
            producer.timeLeftToProduce = orderData.remainingChannelTime; // temporary for GUI to work
        }
        return IsFinishedChanneling();
    }

    public override bool FinishChannel(RTSGameObject performingUnit)
    {
        base.FinishChannel(performingUnit);

        if (performingUnit.storage.canContain.Contains(currentProductionType))
        {
            return producer.ProduceToStorage(currentProductionType);
        }
        else
        {
            return producer.ProduceUnitsToWorld(currentProductionType).Count > 0;
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

    public override void OnCancel(RTSGameObject performingUnit, GameManager gameManager)
    {
        float totalChannelTime = producer.productionTime[currentProductionType] - orderData.remainingChannelTime;
        producer.CancelProduction(currentProductionType, totalChannelTime);
    }
}
