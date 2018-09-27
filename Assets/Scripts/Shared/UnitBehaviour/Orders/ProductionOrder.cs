using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ProductionOrder : Order {

    Producer producer;
    Consumer consumer;
    Storage storage; // Joiners shouldnt use their own storage
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
        storage = performingUnit.GetComponent<Storage>();
        orderData.remainingChannelTime = producer.productionTime[currentProductionType];
        SetConstructionHardpointActivity(producer, true);

        orderData.isJoinable = true;
        return true;
    }

    // take necessary resources from joining units
    public override void Join(RTSGameObject performingUnit)
    {
        Producer joiningProducer = performingUnit.GetComponent<Producer>();
        joiningProducer.GiveItems(storage, producer.productionCost[currentProductionType]);
        joiningProducer.GiveItems(storage, consumer.GetOperatingCostsForTimespan(orderData.remainingChannelTime));
        SetConstructionHardpointActivity(joiningProducer, true);
    }

    // Foreach unit assigned to order
    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        // may be many builders
        if (IsFinishedChanneling())
        {
            return true;
        }
        Consumer performingConsumer = performingUnit.GetComponent<Consumer>();
        Producer performingProducer = performingUnit.GetComponent<Producer>();

        Dictionary<Type, int> productionCosts = producer.GetCostForProductionStep(currentProductionType, orderData.remainingChannelTime);
        if (performingConsumer.Operate(storage, productionCosts))
        {
            ChannelForTime(dt);
            producer.timeLeftToProduce = orderData.remainingChannelTime; // temporary for GUI to work
        }
        return IsFinishedChanneling();
    }

    public void SetConstructionHardpointActivity(Producer producer, bool active)
    {
        List<GameObject> hardPoints;
        if (producer == this.producer)
        {
            hardPoints = producer.constructionSubsystem.internalConstructionHardpoints;
        }
        else
        {
            hardPoints = producer.constructionSubsystem.externalConstructionHardpoints;
        }

        if (active)
        {
            foreach(GameObject hardPoint in hardPoints)
            {
                var ps = hardPoint.GetComponentInChildren<ParticleSystem>();
                if (!ps.isPlaying){
                    ps.Play();
                }
            }
        }
        else
        {
            hardPoints.ForEach(x => x.GetComponentInChildren<ParticleSystem>().Stop());
        }
    }

    public override bool FinishChannel(RTSGameObject performingUnit)
    {
        base.FinishChannel(performingUnit);

        // turn off production animation if next order isn't also production
        Order nextOrder = orderManager.GetOrderForUnit(performingUnit, 1);
        if (nextOrder == null || !(nextOrder is ProductionOrder && nextOrder.orderData.targetPosition == Vector3.zero))
        {
            SetConstructionHardpointActivity(producer, false);
        }

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
        // turn off production animation if next order isn't also production
        Order nextOrder = orderManager.GetOrderForUnit(performingUnit, 1);
        if (nextOrder == null || !(nextOrder is ProductionOrder && nextOrder.orderData.targetPosition == Vector3.zero))
        {
            SetConstructionHardpointActivity(producer, false);
        }
    }
}
