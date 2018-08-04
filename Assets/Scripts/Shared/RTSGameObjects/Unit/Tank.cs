using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(FlowSystem))]
[RequireComponent(typeof(Cannon))]
[RequireComponent(typeof(Shoot))]
public class Tank : RTSGameObject {
    
    public Consumer consumer;
    public FlowSystem power;

    public override void MyStart()
    {
        DefaultInit();
        consumer = GetComponent<Consumer>();
        power = GetComponent<FlowSystem>();
        
        // No fuel yet, just use coal
        consumer.operationCosts.Add(typeof(Coal), 1);
        consumer.operationInterval = 1000;
        defaultAbility = GetComponent<Shoot>();
        ((Shoot)defaultAbility).projectileType = typeof(BasicCannonProjectile);
        storage.AddItem(typeof(Coal), 30);
    }

    public override void MyUpdate()
    {
        if (orderManager.orders.ContainsKey(this) && orderManager.orders[this].Count > 0 && orderManager.orders[this][0].GetType() == typeof(MoveOrder))
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
        DefaultUpdate();
    }
}
