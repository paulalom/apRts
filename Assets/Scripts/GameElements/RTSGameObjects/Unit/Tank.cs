using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(FlowSystem))]
[RequireComponent(typeof(Cannon))]
[RequireComponent(typeof(Shoot))]
public class Tank : RTSGameObject {
    
    public Mover mover;
    public Consumer consumer;
    public FlowSystem power;

    void Start()
    {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        mover = GetComponent<Mover>();
        storage = GetComponent<Storage>();
        consumer = GetComponent<Consumer>();
        power = GetComponent<FlowSystem>();
        
        // No fuel yet, just use coal
        consumer.operationCosts.Add(typeof(Coal), 1);
        consumer.operationInterval = 1f;
        defaultAbility = GetComponent<Shoot>();
        ((Shoot)defaultAbility).projectileType = typeof(BasicCannonProjectile);
        storage.AddItem(typeof(Coal), 30);
    }

    void Update()
    {
        if (orderManager.orders.ContainsKey(this) && orderManager.orders[this].Count > 0 && orderManager.orders[this][0].type == OrderType.Move)
        {
            if (!consumer.Operate())
            {
                mover.isActive = false;
            }
            else if (!mover.isActive)
            {
                mover.isActive = true;
            }
        }
    }
}
