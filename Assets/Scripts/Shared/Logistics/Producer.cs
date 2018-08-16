using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Storage))]
public class Producer : MyMonoBehaviour {
    
    private Storage storage;
    public int productionLevel;
    public HashSet<Type> possibleProductions = new HashSet<Type>();
    public Dictionary<Type, int> productionTime = new Dictionary<Type, int>();
    public Dictionary<Type, int> productionQuantity = new Dictionary<Type, int>();
    public Dictionary<Type, Dictionary<Type, int>> productionCost = new Dictionary<Type, Dictionary<Type, int>>();
    RTSGameObjectManager rtsGameObjectManager;

    public class OnProductionBeginEvent : UnityEvent<RTSGameObject> { }
    public OnProductionBeginEvent onProductionBeginEvent = new OnProductionBeginEvent();

    public float timeLeftToProduce = 0;

    // Use this for initialization
    public override void MyStart()
    {
        storage = GetComponent<Storage>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();

        foreach (Type type in possibleProductions)
        {
            if (!productionTime.ContainsKey(type))
            {
                productionTime.Add(type, 30000); // default
            }
            if (!productionQuantity.ContainsKey(type))
            {
                productionQuantity.Add(type, 1); // default
            }
        }
    }

    public bool ProduceToStorage(Type typeToProduce)
    {
        int qtyToProduce = productionQuantity[typeToProduce];
        if (storage.freeSpace >= qtyToProduce && (storage.AddItem(typeToProduce, qtyToProduce) > 0)) //todo: qty * objectSize
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<RTSGameObject> ProduceUnitsToWorld(Type typeToProduce)
    {
        int qtyToProduce = productionQuantity[typeToProduce];
        List<RTSGameObject> newUnits = rtsGameObjectManager.SpawnUnitsAround(typeToProduce, qtyToProduce, gameObject);
        return newUnits;
    }

    public RTSGameObject ProduceStructureToWorld(Type typeToProduce)
    {
        RTSGameObject newUnit = rtsGameObjectManager.StartNewStructure(typeToProduce, gameObject);
        return newUnit;
    }

    /// <summary>
    /// Gets the amount of resources which should be taken for this particular tick of construction.
    /// WARNING: Resource cost is calculated assuming EXACTLY the fixed step time has passed
    /// </summary>
    /// <param name="productionType"></param>
    /// <param name="timeSpentOnProduction"></param>
    /// <returns></returns>
    public Dictionary<Type, int> GetCostForProductionStep(Type productionType, int timeRemainingOnProduction)
    {
        Dictionary<Type, int> itemCosts = new Dictionary<Type, int>();
        int fullProductionTime = productionTime[productionType];
        int numStepsToBuild = fullProductionTime / StepManager.fixedStepTimeSize;
        int timeSinceStart = fullProductionTime - timeRemainingOnProduction;
        // add a hardcoded numStepsToBuild so we start construction by taking 1 instead of taking 1 at the end
        int stepsSinceStart = numStepsToBuild + timeSinceStart / StepManager.fixedStepTimeSize; // evenly divisible because times moves in steps of this

        foreach (KeyValuePair<Type, int> cost in productionCost[productionType])
        {
            float costPerStep = (float)cost.Value / numStepsToBuild;
            float costPerStepDecimalPortion = costPerStep - (int)costPerStep;
            int itemCost = (int)costPerStep +
                (int)(costPerStepDecimalPortion * (stepsSinceStart)) - (int)(costPerStepDecimalPortion * (stepsSinceStart - 1));

            itemCosts.Add(cost.Key, itemCost);
        }
        return itemCosts;
    }

    // potential issue where percentage time remaining is always very small and so items round to zero
    public Dictionary<Type, int> GetFractionOfProductionCost(Type productionType, float timeSpentOnProduction, float returnInefficiencyMultiplier = 1)
    {
        Dictionary<Type, int> refundItems = new Dictionary<Type, int>();
        float percentageTimeRemaining = timeSpentOnProduction / productionTime[productionType];
        foreach (KeyValuePair<Type, int> cost in productionCost[productionType])
        {
            refundItems.Add(cost.Key, Mathf.RoundToInt(cost.Value * percentageTimeRemaining * returnInefficiencyMultiplier));
        }
        return refundItems;
    }

    public void CancelProduction(Type typeToCancel, float timeSpentOnProduction)
    {
        Debug.Log("Cancelling production.. time spent: " + timeSpentOnProduction);
        Dictionary<Type, int> refundItems = GetFractionOfProductionCost(typeToCancel, timeSpentOnProduction);
        Debug.Log("Cancelling production.. refundItems = " + refundItems);
        storage.AddItems(refundItems);
    }

    public bool HasResourcesForProduction(Type type, int quantity)
    {
        Dictionary<Type, int> costs = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> item in productionCost[type])
        {
            costs.Add(item.Key, item.Value * quantity);
        }
        if (storage.HasItems(costs))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ValidateNewProductionRequest(Type type, int quantity) {
        if (possibleProductions.Contains(type) && quantity == 1)
        {
            return true;
        }
        else if (quantity != 1)
        {
            throw new NotImplementedException("Production request with qty>1 issued... queueing is now done by order instead of in the producer. type requested: " + type + " qty: " + quantity);
        }
        return false;
    }
    
    public List<MyPair<Type, int>> GetMissingResourcesForProduction(Type type)
    {
        List<MyPair<Type, int>> missingResources = new List<MyPair<Type, int>>();
        foreach (KeyValuePair<Type, int> cost in productionCost[type])
        {
            if (storage.GetItemCount(cost.Key) == 0)
            {
                Dictionary<Type, int> storageItems = storage.GetItems();
                int missingQty = cost.Value - (storageItems.ContainsKey(cost.Key) ? storageItems[cost.Key] : 0);
                missingResources.Add(new MyPair<Type, int>(cost.Key, missingQty));
            }
        }
        return missingResources;
    }
    
    public Type GetFirstProducableItemFromList(List<Type> items)
    {
        foreach (Type item in items)
        {
            if (possibleProductions.Contains(item) && storage.HasItems(productionCost[item]))
            {
                return item;
            }
        }
        return null;
    }
    
    public void GiveNeededItems(Type typeToBuild, Storage targetStorage, Dictionary<Type, int> requiredItems)
    {
        List<Type> itemTypes = new List<Type>(requiredItems.Keys);
        foreach (Type type in itemTypes)
        {
            int numItemsAvailable = storage.GetItemCount(type);
            int numItemsToTake = Math.Min(numItemsAvailable, requiredItems[type]);

            // internals handle checking for 0 as well as we would
            int addedItems = targetStorage.AddItem(type, numItemsToTake, false);
            storage.TakeItem(type, addedItems);
        }
    }
}
