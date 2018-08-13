using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transporter))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Producer))]
[RequireComponent(typeof(Storage))]
public class Worker : MyMonoBehaviour
{
    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car) };
    static Type[] defaultCanProduce = new Type[] { typeof(Factory), typeof(HarvestingStation), typeof(PowerPlant) };
    
    public void SetDefaultStorageSettings(Storage storage)
    {
        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
    }

    public void SetDefaultProductionSettings(Producer producer)
    {
        foreach (Type t in defaultCanProduce)
        {
            producer.possibleProductions.Add(t);
        }
        producer.productionCost.Add(typeof(Factory), new Dictionary<Type, int>());
        producer.productionCost[typeof(Factory)].Add(typeof(Iron), 500);
        producer.productionCost[typeof(Factory)].Add(typeof(Stone), 2500);
        producer.productionCost[typeof(Factory)].Add(typeof(Wood), 500);
        producer.productionCost[typeof(Factory)].Add(typeof(Tool), 200);
        producer.productionCost.Add(typeof(HarvestingStation), new Dictionary<Type, int>());
        producer.productionCost.Add(typeof(PowerPlant), new Dictionary<Type, int>());


        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Wood), 100);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Stone), 100);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Iron), 50);
        producer.productionCost[typeof(HarvestingStation)].Add(typeof(Tool), 10);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Iron), 150);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Stone), 100);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Wood), 100);
        producer.productionCost[typeof(PowerPlant)].Add(typeof(Tool), 15);

        producer.productionTime[typeof(Factory)] = 3000;
        producer.productionTime[typeof(HarvestingStation)] = 3000;
        producer.productionTime[typeof(PowerPlant)] = 3000;
    }
}
