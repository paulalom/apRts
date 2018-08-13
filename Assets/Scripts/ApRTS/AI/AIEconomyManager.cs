using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class AIEconomyManager {

    Player playerToManage;
    List<RTSGameObject> untappedNearbyResourceDeposits;
    ProductionFacilityMeta productionFacilities;
    
    public class OnProductionStructureCreation : UnityEvent<RTSGameObject> { }
    OnProductionStructureCreation onProductionStructureCreation;
    public class OnProductionStructureDestruction : UnityEvent<RTSGameObject> { }
    OnProductionStructureDestruction onProductionStructureDestruction;

    public AIEconomyManager(Player playerToManage)
    {
        this.playerToManage = playerToManage;
        productionFacilities = new ProductionFacilityMeta();
        playerToManage.onUnitCountIncrease.AddListener(productionFacilities.AddProductionFacility);
        playerToManage.onUnitCountDecrease.AddListener(productionFacilities.RemoveProductionFacility);
    }

    public void UpdateEconomicSettings()
    {
        productionFacilities.productionTargets.Clear();
        foreach(RTSGameObject productionFacility in productionFacilities.productionFacilities)
        {
            if (!productionFacility.IsIdle)
            {
                continue;
            }
            Producer producer = productionFacility.GetComponent<Producer>();
            List<MyPair<Type, int>> missingResources = producer.GetMissingResourcesForProduction(typeof(ConstructionSphere));
            Type productionTarget;
            if (missingResources.Count > 0) {
                List<Type> missingResourceTypes = new List<Type>();
                missingResources.ForEach(x => missingResourceTypes.Add(x.Key));
                productionTarget = producer.GetFirstProducableItemFromList(missingResourceTypes);
            }
            else
            {
                productionTarget = typeof(ConstructionSphere);
            }

            if (productionTarget != null)
            {
                productionFacilities.productionTargets[productionFacility] = productionTarget;
            }
        }
    }

    public List<MyPair<List<long>, Command>> GetConstructionCommands()
    {
        List<MyPair<List<long>, Command>> constructionCommands = new List<MyPair<List<long>, Command>>();
        foreach (KeyValuePair<RTSGameObject, Type> productionTarget in productionFacilities.productionTargets)
        {
            constructionCommands.Add(new MyPair<List<long>, Command>(
                new List<long>() { productionTarget.Key.unitId },
                CommandFactory.GetCommandFromOrder(OrderFactory.BuildConstructionOrder(new List<MyPair<Type, int>>() {
                    new MyPair<Type, int>(productionTarget.Value, 1) }))));
            
        }

        return constructionCommands;
    }
}
