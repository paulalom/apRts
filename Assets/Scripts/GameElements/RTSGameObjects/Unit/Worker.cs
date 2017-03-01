using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transporter))]
[RequireComponent(typeof(Mover))]
public class Worker : RTSGameObject
{
    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car) };
    static Type[] defaultCanProduce = new Type[] { typeof(Factory), typeof(HarvestingStation), typeof(PowerPlant) };
    Producer producer;
    void Awake()
    {
        storage = GetComponent<Storage>();
        producer = GetComponent<Producer>();
        producer.canProduce.Add(typeof(HarvestingStation));
        producer.canProduce.Add(typeof(Factory));

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
        foreach (Type t in defaultCanProduce)
        {
            producer.canProduce.Add(t);
        }
        producer.productionCost.Add(typeof(Factory), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(HarvestingStation), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(PowerPlant), new Dictionary<Type, int>());

        producer.productionCost[typeof(Factory)].Add(typeof(Iron), 500);
        producer.productionCost[typeof(Factory)].Add(typeof(Stone), 2500);
        producer.productionCost[typeof(Factory)].Add(typeof(Wood), 500);
        producer.productionCost[typeof(Factory)].Add(typeof(Tool), 200);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Wood), 100);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Stone), 100);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Iron), 50);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Tool), 10);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Iron), 150);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Stone), 100);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Wood), 100);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Tool), 15);

        producer.productionTime[typeof(Factory)] = 3;
        producer.productionTime[typeof(HarvestingStation)] = 3;
        producer.productionTime[typeof(PowerPlant)] = 3;

        foreach (Type type in producer.canProduce)
        {
            if (!producer.productionCost.ContainsKey(type))
            {
                //producer.productionCost.Add(type, new Dictionary<Type, int>()); // This wont fix it, but it will fail quietly
            }
            if (!producer.productionTime.ContainsKey(type))
            {
                producer.productionTime.Add(type, 30); // default
            }
            if (!producer.productionQuantity.ContainsKey(type))
            {
                producer.productionQuantity.Add(type, 1); // default
            }
        }
    }
}
