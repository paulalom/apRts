using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionOrder : Order {

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
        producer.GiveNeededItems(typeToBuild, conInfo.storage, conInfo.GetRemainingItemsNeeded());
        orderData.isJoinable = true;
        return true;
    }

    public override void Join(RTSGameObject performingUnit)
    {
        producer.GiveNeededItems(newStructure.GetType(), conInfo.storage, conInfo.GetRemainingItemsNeeded());
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
        Structure newStructurePrefab = rtsGameObjectManager.prefabs[newStructureType.ToString()].GetComponent<Structure>();

        if (producer.ValidateNewProductionRequest(newStructureType, 1)
            && newStructurePrefab.ValidatePlacement(rtsGameObjectManager, performingUnit.transform.position))
        {
            return OrderValidationResult.Success;
        }
        else
        {
            return OrderValidationResult.Failure;
        }
    }
}
