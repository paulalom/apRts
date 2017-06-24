using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Explode))]
public class BasicCannonProjectile : Projectile
{
    public float baseDamage;
    public DamageType damageType;

    void Start()
    {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
    }

    // Activate on collision
    void OnTriggerEnter(Collider col)
    {
        RTSGameObject rtsGo = GetComponent<RTSGameObject>();
        if (col != parent.GetComponent<Collider>() && orderManager.orders.ContainsKey(rtsGo))
        {
            orderManager.orders[rtsGo][0].targetPosition = transform.position;
        }
    }
}
