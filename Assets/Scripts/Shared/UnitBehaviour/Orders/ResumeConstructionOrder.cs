﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// This feels super redundant to construction order... but adding this functionality to join order was being a big PITA
// since the orderTarget is the structure and not the unit building it added a reverse order lookup and there was the possibility
// that the original constructing unit had abandoned the project entirely.
// Ill figure out a better way next time I revisit this code. For now this works.
public class ResumeConstructionOrder : Order {

    Producer producer;
    Consumer consumer;
    Structure newStructure;
    ConstructionInfo conInfo; // convenience reference

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
        producer = performingUnit.GetComponent<Producer>();
        consumer = performingUnit.GetComponent<Consumer>();
        newStructure = (Structure)orderData.target;
        conInfo = newStructure.constructionInfo;

        producer.GiveItems(conInfo.storage, conInfo.GetRemainingItemsNeeded());
        orderData.isJoinable = true;
        return true;
    }

    public override void Join(RTSGameObject performingUnit)
    {
        producer.GiveItems(conInfo.storage, conInfo.GetRemainingItemsNeeded());
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
        newStructure.CompleteConstruction(rtsGameObjectManager);
        return true;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        Producer producer = performingUnit.GetComponent<Producer>();
        Type newStructureType = orderData.target.GetType();
        Structure newStructurePrefab = rtsGameObjectManager.unitPrefabs[newStructureType.ToString()].GetComponent<Structure>();

        if (producer.ValidateNewProductionRequest(newStructureType, 1)
            && newStructurePrefab.ValidatePlacement(rtsGameObjectManager, orderData.targetPosition))
        {
            return OrderValidationResult.Success;
        }
        else
        {
            return OrderValidationResult.Failure;
        }
    }
}
