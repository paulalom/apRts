using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Storage))]
//[RequireComponent(typeof(RTSGameObject))]
public class Producer : MonoBehaviour {

    private Storage storage;
    // We dont care about random access (our queue shouldn't be that long), 
    // we want to be able to duplicate keys if they are not next to each other in the queue.
    // eg. build unit T1, unit T2, unit T1, unit t3, unit T1
    // We use KVP so that duplications which are next to eacother can be grouped. 
    // Our production queue can be one item with a thousand units queued.
    public List<MyKVP<Type, int>> productionQueue;
    public HashSet<Type> canProduce = new HashSet<Type>();
    public Dictionary<Type, float> productionTime = new Dictionary<Type, float>();
    public Dictionary<Type, int> productionQuantity = new Dictionary<Type, int>();
    public Dictionary<Type, Dictionary<Type, int>> productionCost = new Dictionary<Type, Dictionary<Type, int>>();
    RTSGameObjectManager rtsGameObjectManager;

    float timeLeftToProduce = 0;
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
    bool producing = false;
    float previousTime;

    // Use this for initialization
    void Start()
    {
        storage = GetComponent<Storage>();
        productionQueue = new List<MyKVP<Type, int>>();
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
    void Update()
    {
        if (IsActive) {
            timeLeftToProduce -= Time.time - previousTime;
            if (timeLeftToProduce <= 0)
            {
                if (Produce())
                {
                    StartNextProduction();
                }
            }
            previousTime = Time.time;
        }
    }
    
    bool Produce()
    {
        if (productionQueue.Count == 0)
        {
            throw new System.Exception("Y'all be crazy... Aint nothin to produce. ");
        }
        Type typeToProduce = productionQueue[0].Key;
        int qtyToProduce = 1;
        try {
            qtyToProduce = productionQuantity[productionQueue[0].Key];
        }
        catch (Exception e)
        {
            Debug.Log("Probably productionQuantity doesnt contain: " + productionQueue[0].Key);
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
        if(rtsGameObjectManager.SpawnUnitsAround(typeToProduce, qtyToProduce, gameObject)){
            PopProductionQueue();
            return true;
        }
        else
        {
            return false;
        }
    }

    void PopProductionQueue()
    {
        // We done it boys! remove one from the queue
        if (productionQueue[0].Value > 1)
        {
            productionQueue[0] = new MyKVP<Type, int>(productionQueue[0].Key, productionQueue[0].Value - 1);
        }
        else
        {
            //This is O(n)... i dont think the list should be many items long though because of our stacking
            productionQueue.RemoveAt(0);
        }
    }

    void StartNextProduction()
    {
        if (productionQueue.Count == 0)
        {
            IsActive = false;
            return;
        }
        try
        {
            timeLeftToProduce = productionTime[productionQueue[0].Key];
        }
        catch (Exception e)
        {
            Debug.Log("Probably productionTime doesnt contain: " + productionQueue[0].Key);
        }
    }

    public bool TryQueueItem(Type type, int quantity)
    {
        try
        {
            if (ValidateNewProductionRequest(type, quantity))
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
        if (productionQueue.Count == 0 || productionQueue[productionQueue.Count - 1].Key != type)
        {
            productionQueue.Add(new MyKVP<Type, int>(type, quantity));
        }
        else {
            productionQueue[productionQueue.Count - 1].Value += quantity;
        }

        //Queueing automatically turns producer on
        if (productionQueue.Count == 1)
        {
            StartNextProduction();
            IsActive = true;
        }
    }

    private bool ValidateNewProductionRequest(Type type, int quantity) {
        if (canProduce.Contains(type))
        {
            return true;
        }
        else {
            return false;
        }
    }
}
