using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Transporter))]
[RequireComponent(typeof(Producer))]
[RequireComponent(typeof(FlowSystem))]
[RequireComponent(typeof(Cannon))]
[RequireComponent(typeof(Shoot))]
public class Commander : RTSGameObject {

    static Type[] defaultCanContain = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car) };
    static Type[] defaultCanProduce = new Type[] { typeof(Factory), typeof(HarvestingStation), typeof(PowerPlant) };
    public Producer producer;
    public Consumer consumer;
    public FlowSystem power;

    void Awake()
    {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        mover = GetComponent<Mover>();
        producer = GetComponent<Producer>();
        storage = GetComponent<Storage>();
        consumer = GetComponent<Consumer>();
        power = GetComponent<FlowSystem>();

        defaultAbility = GetComponent<Shoot>();
        ((Shoot)defaultAbility).projectileType = typeof(BasicCannonProjectile);

        foreach (Type t in defaultCanContain)
        {
            storage.canContain.Add(t);
        }
        foreach (Type t in defaultCanProduce)
        {
            producer.canProduce.Add(t);
        }
        producer.productionCost.Add(typeof(Factory), new Dictionary<Type, int>());
        producer.productionCost[typeof(Factory)].Add(typeof(Iron), 500);
        producer.productionCost[typeof(Factory)].Add(typeof(Stone), 2500);
        producer.productionCost[typeof(Factory)].Add(typeof(Wood), 500);
        producer.productionCost[typeof(Factory)].Add(typeof(Tool), 200);


        producer.productionTime[typeof(Factory)] = 3;
        producer.productionTime[typeof(HarvestingStation)] = 3;
        producer.productionTime[typeof(PowerPlant)] = 3;
    }

    void Start()
    {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
