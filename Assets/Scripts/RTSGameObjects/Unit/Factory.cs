using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(Producer))]
public class Factory : RTSGameObject
{
    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car) };
    static Type[] defaultCanProduce = new Type[] { typeof(Worker), typeof(Paper), typeof(Tool), typeof(Car) };
    Producer producer;
    Consumer consumer;
    void Awake()
    {
        storage = GetComponent<Storage>();
        producer = GetComponent<Producer>();
        consumer = GetComponent<Consumer>();

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
        foreach (Type t in defaultCanProduce)
        {
            producer.canProduce.Add(t);
        }
        producer.productionCost.Add(typeof(Worker), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Paper), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Tool), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(Car), new Dictionary<Type, int>());

        producer.productionCost[typeof(Worker)].Add(typeof(Tool), 5);
        producer.productionCost[typeof(Worker)].Add(typeof(Paper), 5);
        producer.productionCost[typeof(Paper)].Add(typeof(Wood), 5);
        producer.productionCost[typeof(Tool)].Add(typeof(Wood), 5);
        producer.productionCost[typeof(Tool)].Add(typeof(Iron), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Tool), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Iron), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Coal), 5);

        producer.productionTime[typeof(Worker)] = 3;

        producer.productionQuantity[typeof(Paper)] = 10;
        producer.productionQuantity[typeof(Tool)] = 10;
        foreach (Type type in producer.canProduce)
        {
            if (!producer.productionCost.ContainsKey(type))
            {
                //producer.productionCost.Add(type, new Dictionary<Type, int>()); // This wont fix it, but it will fail quietly
            }
            if (!producer.productionTime.ContainsKey(type)){
                producer.productionTime.Add(type, 30); // default
            }
            if (!producer.productionQuantity.ContainsKey(type)){
                producer.productionQuantity.Add(type, 1); // default
            }
        }
    }

    void Update()
    {
        if (producer.IsActive)
        {
            producer.IsActive = consumer.Operate();
        }
    }
}
