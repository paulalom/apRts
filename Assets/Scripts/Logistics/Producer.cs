using UnityEngine;
using System.Collections.Generic;
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
    public List<KeyValuePair<RTSGameObjectType, int>> productionQueue;
    GameManager gameManager;

    float timeLeftToProduce = 0;
    private bool _isActive = false;
    public bool IsActive {
        get { return _isActive; }
        set
        {
            previousTime = Time.time;
            _isActive = value;
        }
    }
    bool producing = false;
    float previousTime;
    public UnityEvent onStorageChangedEvent;

    // Use this for initialization
    void Start()
    {
        storage = GetComponent<Storage>();
        productionQueue = new List<KeyValuePair<RTSGameObjectType, int>>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
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
        RTSGameObjectType typeToProduce = productionQueue[0].Key;
        int qtyToProduce = RTSGameObject.productionQuantity[productionQueue[0].Key];
        if (RTSGameObject.canContain[GetComponent<RTSGameObject>().type].Contains(typeToProduce))
        {
            return ProduceToStorage(typeToProduce, qtyToProduce);
        }
        else
        {
            return ProduceToWorld(typeToProduce, qtyToProduce);
        }
    }  

    bool ProduceToStorage(RTSGameObjectType typeToProduce, int qtyToProduce)
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

    bool ProduceToWorld(RTSGameObjectType typeToProduce, int qtyToProduce)
    {
        if(gameManager.SpawnUnitsAround(typeToProduce, qtyToProduce, gameObject)){
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
            productionQueue[0] = new KeyValuePair<RTSGameObjectType, int>(productionQueue[0].Key, productionQueue[0].Value - 1);
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
        timeLeftToProduce = RTSGameObject.productionTime[productionQueue[0].Key];
    }

    public void QueueItem(RTSGameObjectType type, int quantity)
    {
        if (productionQueue.Count == 0 || productionQueue[productionQueue.Count - 1].Key != type)
        {
            productionQueue.Add(new KeyValuePair<RTSGameObjectType, int>(type, quantity));
        }
        else { // Stupid immutable KVP
            productionQueue[productionQueue.Count - 1] = new KeyValuePair<RTSGameObjectType, int>
                                (type, productionQueue[productionQueue.Count - 1].Value + quantity);
        }

        //Queueing automatically turns producer on
        if (productionQueue.Count == 1)
        {
            StartNextProduction();
            IsActive = true;
        }
    }
}
