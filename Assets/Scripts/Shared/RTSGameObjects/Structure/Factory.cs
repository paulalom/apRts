using System;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Structure
{
    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car) };
    static Type[] defaultCanProduce = new Type[] { typeof(ConstructionSphere), typeof(Tank), typeof(Paper), typeof(Tool), typeof(Car) };
    Producer producer;
    Consumer consumer;

    public override void MyAwake()
    {
        storage = GetComponent<Storage>();
        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }

        // temp hack for under construction structures
        if (constructionComponent != null)
        {
            return;
        }

        producer = GetComponent<Producer>();
        consumer = GetComponent<Consumer>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();

        foreach (Type t in defaultCanProduce)
        {
            producer.possibleProductions.Add(t);
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
        producer.productionCost[typeof(Tool)].Add(typeof(Wood), 1500);
        producer.productionCost[typeof(Tool)].Add(typeof(Iron), 1000);
        producer.productionCost[typeof(Tool)].Add(typeof(Paper), 1);
        producer.productionCost[typeof(Car)].Add(typeof(Tool), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Iron), 5);
        producer.productionCost[typeof(Car)].Add(typeof(Coal), 5);

        producer.productionTime[typeof(ConstructionSphere)] = 3000;
        producer.productionTime[typeof(Tank)] = 2000;
        producer.productionTime[typeof(Paper)] = 2000;
        producer.productionTime[typeof(Tool)] = 2000;

        producer.productionQuantity[typeof(Paper)] = 10;
        producer.productionQuantity[typeof(Tool)] = 10;

        consumer.operationCosts[typeof(Coal)] = 3;
        consumer.operationInterval = 400;
    }
}
