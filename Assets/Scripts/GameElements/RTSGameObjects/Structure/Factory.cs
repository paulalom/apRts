using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(Producer))]
public class Factory : Structure
{
    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car) };
    static Type[] defaultCanProduce = new Type[] { typeof(ConstructionSphere), typeof(Tank), typeof(Paper), typeof(Tool), typeof(Car) };
    Producer producer;
    Consumer consumer;

    void Awake()
    {
        storage = GetComponent<Storage>();
        producer = GetComponent<Producer>();
        consumer = GetComponent<Consumer>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
        foreach (Type t in defaultCanProduce)
        {
            producer.canProduce.Add(t);
        }
        producer.productionCost.Add(typeof(ConstructionSphere), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Paper), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Tool), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Car), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Tank), new Dictionary<Type, int>());

        producer.productionCost[typeof(ConstructionSphere)].Add(typeof(Tool), 5);
        producer.productionCost[typeof(ConstructionSphere)].Add(typeof(Paper), 5);
        producer.productionCost[typeof(Tank)].Add(typeof(Iron), 50);
        producer.productionCost[typeof(Tank)].Add(typeof(Tool), 50);
        producer.productionCost[typeof(Tank)].Add(typeof(Coal), 50);
        producer.productionCost[typeof(Paper)].Add(typeof(Wood), 5);
        producer.productionCost[typeof(Tool)].Add(typeof(Wood), 5);
        producer.productionCost[typeof(Tool)].Add(typeof(Iron), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Tool), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Iron), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Coal), 5);

        producer.productionTime[typeof(ConstructionSphere)] = 2;
        producer.productionTime[typeof(Tank)] = 2;
        producer.productionTime[typeof(Paper)] = 2;
        producer.productionTime[typeof(Tool)] = 2;

        producer.productionQuantity[typeof(Paper)] = 10;
        producer.productionQuantity[typeof(Tool)] = 10;

        consumer.operationCosts[typeof(Coal)] = 3;
        consumer.operationInterval = .3f;
        producer.IsActive = false;
    }

    void Start()
    {
        storage.onStorageAddEvent.AddListener(CheckActivate);
    }

    void Update()
    {
        if (!underConstruction && producer.IsActive)
        {
            producer.IsActive = consumer.Operate();
        }
    }
    
    void CheckActivate(Dictionary<Type, int> items)
    {
        if (!underConstruction && producer.IsActive == false && producer.productionQueue.Count > 0)
        {
            if (storage.HasItems(consumer.operationCosts))
            {
                producer.IsActive = true;
            }
        }
    }
}
