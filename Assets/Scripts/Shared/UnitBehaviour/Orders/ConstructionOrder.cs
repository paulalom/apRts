using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionOrder : Order {

    Producer producer;
    Consumer consumer;
    Structure newStructure;

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
        newStructure.constructionComponent.constructionTimeRemaining = producer.productionTime[typeToBuild];
        
        producer.GiveNeededItems(typeToBuild, newStructure.storage);
        orderData.isJoinable = true;
        return true;
    }

    public override void Join(RTSGameObject performingUnit)
    {
        producer.GiveNeededItems(newStructure.GetType(), newStructure.storage);
    }

    // Foreach unit assigned to order
    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        // may be many builders
        if (newStructure.constructionComponent.constructionTimeRemaining <= 0)
        {
            return true;
        }

        Dictionary<Type, int> productionCosts = producer.GetCostForProductionStep(newStructure.GetType(), newStructure.constructionComponent.constructionTimeRemaining);
        if (consumer.Operate(newStructure.storage, productionCosts))
        {
            newStructure.constructionComponent.constructionTimeRemaining -= dt;
        }
        return newStructure.constructionComponent.constructionTimeRemaining <= 0;
    }

    public override bool FinishChannel(RTSGameObject performingUnit)
    {
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
