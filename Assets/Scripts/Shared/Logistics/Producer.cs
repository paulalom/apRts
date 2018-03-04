using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Storage))]
//[RequireComponent(typeof(RTSGameObject))]
public class Producer : MyMonoBehaviour {

    private Storage storage;
    // We dont care about random access (our queue shouldn't be that long), 
    // we want to be able to duplicate keys if they are not next to each other in the queue.
    // eg. build unit T1, unit T2, unit T1, unit t3, unit T1
    // We use KVP so that duplications which are next to eacother can be grouped. 
    // Our production queue can be one item with a thousand units queued.
    public MyPair<Type, int> currentProduction;
    public HashSet<Type> canProduce = new HashSet<Type>();
    public Dictionary<Type, float> productionTime = new Dictionary<Type, float>();
    public Dictionary<Type, int> productionQuantity = new Dictionary<Type, int>();
    public Dictionary<Type, Dictionary<Type, int>> productionCost = new Dictionary<Type, Dictionary<Type, int>>();
    RTSGameObjectManager rtsGameObjectManager;

    public class OnProductionBeginEvent : UnityEvent<RTSGameObject> { }
    public OnProductionBeginEvent onProductionBeginEvent = new OnProductionBeginEvent();

    public float timeLeftToProduce = 0;
    private bool _isActive = false;
    // If we turn on, start counting production duration from the correct time
    public bool IsActive {
        get { return _isActive; }
        set
        {
            if (_isActive != value)
            {
                previousTime = Time.time;
                _isActive = value;
            }
        }
    }
    float previousTime;

    // Use this for initialization
    public override void MyStart()
    {
        storage = GetComponent<Storage>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();

        foreach (Type type in canProduce)
        {
            if (!productionCost.ContainsKey(type))
            {
                //producer.productionCost.Add(type, new Dictionary<Type, int>()); // This wont fix it, but it will fail quietly
            }
            if (!productionTime.ContainsKey(type))
            {
                productionTime.Add(type, 30); // default
            }
            if (!productionQuantity.ContainsKey(type))
            {
                productionQuantity.Add(type, 1); // default
            }
        }
    }

    // This is called once per frame
    public override void MyUpdate()
    {
        if (IsActive) {
            timeLeftToProduce -= StepManager.GetDeltaStep();
            if (timeLeftToProduce <= 0)
            {
                if (Produce())
                {
                    StartNextProduction();
                }
            }
        }
    }
    
    bool Produce()
    {
        if (currentProduction == null)
        {
            throw new System.Exception("Y'all be crazy... Aint nothin to produce. ");
        }
        Type typeToProduce = currentProduction.Key;
        int qtyToProduce = 1;
        try {
            qtyToProduce = productionQuantity[currentProduction.Key];
        }
        catch (Exception e)
        {
            Debug.Log("Probably productionQuantity doesnt contain: " + currentProduction.Key);
        }
        if (storage.canContain.Contains(typeToProduce))
        {
            return ProduceToStorage(typeToProduce, qtyToProduce);
        }
        else
        {
            return ProduceToWorld(typeToProduce, qtyToProduce);
        }
    }  

    bool ProduceToStorage(Type typeToProduce, int qtyToProduce)
    {
        if (storage.freeSpace < qtyToProduce) //todo: qty * objectSize
        {
            return false;
        }

        if (storage.AddItem(typeToProduce, qtyToProduce) > 0)
        { 
            PopProductionQueue();
            return true;
        }
        else
        {
            return false;
        }
    }

    bool ProduceToWorld(Type typeToProduce, int qtyToProduce)
    {
        bool success;
        if (typeToProduce.IsSubclassOf(typeof(Structure))) {
            success = rtsGameObjectManager.StartNewStructure(typeToProduce, qtyToProduce, gameObject);
        }
        else
        {
            success = rtsGameObjectManager.SpawnUnitsAround(typeToProduce, qtyToProduce, gameObject);
        }
        if (success){
            PopProductionQueue();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CancelProduction()
    {
        if (currentProduction != null)
        {
            storage.AddItems(productionCost[currentProduction.Key]);
            PopProductionQueue();
            if (currentProduction == null)
            {
                IsActive = false;
            }
        }
    }

    void PopProductionQueue()
    {
        if (currentProduction.Value > 1)
        {
            currentProduction.Value -= 1;
        }
        else
        {
            currentProduction = null;
        }
    }

    void StartNextProduction()
    {
        if (currentProduction == null)
        {
            IsActive = false;
            return;
        }
        try
        {
            StartProduction(currentProduction.Key, currentProduction.Value);
        }
        catch (Exception e)
        {
            Debug.Log("Probably productionTime doesnt contain: " + currentProduction.Key);
        }
    }

    void StartProduction(Type type, int quantity)
    {
        if (type.IsSubclassOf(typeof(Structure)))
        {
            timeLeftToProduce = 0;
        }
        else {
            timeLeftToProduce = productionTime[currentProduction.Key];
        }
    }

    public bool TryQueueItem(Type type, int quantity, bool preValidated = false)
    {
        try
        {
            if (preValidated || ValidateNewProductionRequest(type, quantity))
            {
                Dictionary<Type, int> costs = new Dictionary<Type, int>();
                foreach (KeyValuePair<Type, int> item in productionCost[type])
                {
                    costs.Add(item.Key, item.Value * quantity);
                }
                if (storage.TakeItems(costs))
                {
                    QueueItem(type, quantity);
                    return true;
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log("Exception when queueing item of type: " + type + " to unit: " + gameObject.name);
        }
        return false;
    }

    private void QueueItem(Type type, int quantity)
    {
        bool restartProduction = false;
        if (currentProduction == null)
        {
            currentProduction = new MyPair<Type, int>(type, quantity);
            restartProduction = true;
        }
        else if (currentProduction.Key == type)
        {
            currentProduction.Value += quantity;
        }
        else
        {
            Debug.Log("Warning: Queueing an item which is not the type of current production!");
            currentProduction = new MyPair<Type, int>(type, quantity);
            restartProduction = true;
        }

        //Queueing automatically turns producer on
        if (restartProduction)
        {
            StartNextProduction();
            IsActive = true;
        }
    }

    public bool ValidateNewProductionRequest(Type type, int quantity) {
        if (canProduce.Contains(type))
        {
            return true;
        }
        else {
            return false;
        }
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

    public Type GetFirstProducableItem(List<Type> items)
    {
        foreach (Type item in items)
        {
            if (canProduce.Contains(item) && storage.HasItems(productionCost[item]))
            {
                return item;
            }
        }
        return null;
    }
}
