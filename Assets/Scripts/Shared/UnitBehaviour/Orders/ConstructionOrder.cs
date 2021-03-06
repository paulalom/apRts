﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionOrder : Order {

    Producer producer;
    Consumer consumer;
    Structure newStructure;
    ConstructionInfo conInfo; // convenience reference

    public override bool GetInRange(RTSGameObject performingUnit, int dt)
    {
        return base.GetInRange(performingUnit, dt);
    }

    public override void Initilize(RTSGameObject performingUnit)
    {
        base.Initilize(performingUnit);
        if (orderData.targetPosition == Vector3.zero)
        {
            orderData.targetPosition = performingUnit.transform.position;
        }
    }

    public override bool Activate(RTSGameObject performingUnit)
    {
        if (isActivated) { return true; }
        base.Activate(performingUnit);
        Type typeToBuild = orderData.items[0].Key;
        producer = performingUnit.GetComponent<Producer>();
        consumer = performingUnit.GetComponent<Consumer>();
        newStructure = (Structure)producer.ProduceStructureToWorld(orderData.items[0].Key);
        conInfo = newStructure.constructionInfo;
        conInfo.constructionTimeRemaining = producer.productionTime[typeToBuild];

        conInfo.totalRequiredItems = new Dictionary<Type, int>(producer.productionCost[typeToBuild]);
        conInfo.itemsUsedInConstruction = new Dictionary<Type, int>();
        foreach (Type type in conInfo.totalRequiredItems.Keys)
        {
            conInfo.itemsUsedInConstruction.Add(type, 0);
        }
        producer.GiveItems(conInfo.storage, conInfo.GetRemainingItemsNeeded());
        orderData.isJoinable = true;
        return true;
    }

    public override void Join(RTSGameObject performingUnit)
    {
        Producer joiningProducer = performingUnit.GetComponent<Producer>();
        joiningProducer.GiveItems(conInfo.storage, conInfo.GetRemainingItemsNeeded());
    }

    // Foreach unit assigned to order
    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        // may be many builders
        if (conInfo.constructionTimeRemaining <= 0)
        {
            return true;
        }

        Dictionary<Type, int> productionCosts = producer.GetCostForProductionStep(newStructure.GetType(), conInfo.constructionTimeRemaining);
        if (performingUnit.GetComponent<Consumer>().Operate(newStructure.storage, productionCosts))
        {
            conInfo.constructionTimeRemaining -= dt;
            conInfo.RecordItemsUsedInConstruction(productionCosts);
        }
        return conInfo.constructionTimeRemaining <= 0;
    }

    public override bool FinishChannel(RTSGameObject performingUnit)
    {
        base.FinishChannel(performingUnit);
        newStructure.CompleteConstruction(rtsGameObjectManager);
        return true;
    }

    // magic performingUnit.transform.position for construction placement validation until we get some targeted structure placement
    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        Producer producer = performingUnit.GetComponent<Producer>();
        Type newStructureType = orderData.items[0].Key;
        Structure newStructurePrefab = rtsGameObjectManager.unitPrefabs[newStructureType.ToString()].GetComponent<Structure>();
        Vector3 orderTargetPosition = orderData.targetPosition == Vector3.zero ? performingUnit.transform.position : orderData.targetPosition;

        if (producer.ValidateNewProductionRequest(newStructureType, 1)
            && newStructurePrefab.ValidatePlacement(rtsGameObjectManager, orderTargetPosition))
        {
            return OrderValidationResult.Success;
        }
        else
        {
            return OrderValidationResult.Failure;
        }
    }
}
