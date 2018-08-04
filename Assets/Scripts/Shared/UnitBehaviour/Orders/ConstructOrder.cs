using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionOrder : Order {

    Producer producer;
    Consumer consumer;
    RTSGameObject newStructure;

    public override bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        if (orderData.targetPosition == Vector3.zero)
        {
            orderData.targetPosition = performingUnit.transform.position;
        }
        return base.GetInRange(performingUnit, rtsGameObjectManager, dt);
    }

    public override bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        Type structureToBuild = orderData.items[0].Key;
        producer = performingUnit.GetComponent<Producer>();
        consumer = performingUnit.GetComponent<Consumer>();
        producer.SetProductionTarget(structureToBuild);
        orderData.remainingChannelTime = producer.productionTime[structureToBuild];
        newStructure = producer.ProduceStructureToWorld();

        StructureConstructionStorage newStructureStorage = (StructureConstructionStorage)newStructure.storage;
        newStructureStorage.totalRequiredItems = producer.productionCost[structureToBuild]; // this might need to change to a deep clone since production costs may eventually not be readonly
        
        Dictionary<Type, int> availableResources = producer.GetAvailableResourcesForProduction(structureToBuild);

        performingUnit.storage.TakeItems(availableResources);
        newStructureStorage.AddItems(availableResources);

        // performingUnit.storage.onStorageAddEvent.AddListener() // check for new required resources 
        return true;
    }

    // Foreach unit assigned to order
    public override bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, int dt)
    {
        StructureConstructionStorage storage = newStructure.GetComponent<StructureConstructionStorage>();
        
        Dictionary<Type, int> productionCosts = producer.GetCostForProductionStep(newStructure.GetType(), orderData.remainingChannelTime);
        if (consumer.Operate(productionCosts))
        {
            ChannelForTime(dt);
        }
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
        Producer producer = performingUnit.GetComponent<Producer>();
        if (producer.ValidateNewProductionRequest(orderData.items[0].Key, orderData.items[0].Value))
        {
            return OrderValidationResult.Success;
        }
        else
        {
            return OrderValidationResult.Failure;
        }
    }

    public override void OnQueue(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        base.OnQueue(performingUnit, rtsGameObjectManager);
    }

    public override void OnCancel(RTSGameObject performingUnit, GameManager gameManager, RTSGameObjectManager rtsGameObjectManager)
    {
        Worker worker = performingUnit.GetComponent<Worker>();
        if (worker != null && worker.unitUnderConstruction != null)
        {            
            ((Structure)worker.unitUnderConstruction).DemolishStructure("Construction cancelled!", gameManager, rtsGameObjectManager);
            worker.unitUnderConstruction = null;
        }
    }
}
