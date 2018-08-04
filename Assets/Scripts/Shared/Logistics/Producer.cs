using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Storage))]
public class Producer : MyMonoBehaviour {
    
    private Storage storage;
    public int productionLevel;
    public Type currentProductionType;
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

    public void SetProductionTarget(Type type)
    {
        currentProductionType = type;
    }

    public int GetQuantityToProduce()
    {
        int qtyToProduce = 1;

        if (currentProductionType == null)
        {
            throw new System.Exception("Exception trying to produce with nothing queued");
        }       

        try {
            qtyToProduce = productionQuantity[currentProductionType];
        }
        catch (Exception e)
        {
            Debug.Log("Probably productionQuantity doesnt contain: " + currentProductionType);
        }

        return qtyToProduce;
    }

    public bool ProduceToStorage()
    {
        int qtyToProduce = GetQuantityToProduce();
        if (storage.freeSpace >= qtyToProduce && (storage.AddItem(currentProductionType, qtyToProduce) > 0)) //todo: qty * objectSize
        {
            currentProductionType = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<RTSGameObject> ProduceUnitsToWorld()
    {
        int qtyToProduce = GetQuantityToProduce();
        List<RTSGameObject> newUnits = rtsGameObjectManager.SpawnUnitsAround(currentProductionType, qtyToProduce, gameObject);
        if (newUnits.Count > 0)
        {
            currentProductionType = null;
        }
        return newUnits;
    }

    public RTSGameObject ProduceStructureToWorld()
    {
        RTSGameObject newUnit = rtsGameObjectManager.StartNewStructure(currentProductionType, gameObject);
        currentProductionType = null;
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

    public void CancelProduction(float timeSpentOnProduction)
    {
        if (currentProductionType != null)
        {
            Debug.Log("Cancelling production.. time spent: " + timeSpentOnProduction);
            Dictionary<Type, int> refundItems = GetFractionOfProductionCost(currentProductionType, timeSpentOnProduction);
            Debug.Log("Cancelling production.. refundItems = " + refundItems);
            storage.AddItems(refundItems);
        }
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

    public Dictionary<Type, int> GetAvailableResourcesForProduction(Type type)
    {
        Dictionary<Type, int> availableResources = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> cost in productionCost[type])
        {
            if (storage.HasItem(cost.Key, cost.Value))
            {
                Dictionary<Type, int> storageItems = storage.GetItems();
                int availableQty = (storageItems.ContainsKey(cost.Key) ? storageItems[cost.Key] : 0);
                availableQty = Mathf.Min(cost.Value, availableQty);
                availableResources.Add(cost.Key, availableQty);
            }
        }
        return availableResources;
    }

    public List<MyPair<Type, int>> GetMissingResourcesForProduction(Type type)
    {
        List<MyPair<Type, int>> missingResources = new List<MyPair<Type, int>>();
        foreach (KeyValuePair<Type, int> cost in productionCost[type])
        {
            if (!storage.HasItem(cost.Key, cost.Value))
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
}
