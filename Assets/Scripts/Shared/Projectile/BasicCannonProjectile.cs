using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Explode))]
public class BasicCannonProjectile : Projectile
{
    public float baseDamage;
    public DamageType damageType;
    
    void CheckExplode(Collider other)
    {
        RTSGameObject otherRtsGo = other.GetComponent<RTSGameObject>();
        if (otherRtsGo == null)
        {
            otherRtsGo = other.GetComponentInParent<RTSGameObject>();
        }
        if ((otherRtsGo == null || otherRtsGo.ownerId != ownerId) 
            && orderManager.orders.ContainsKey(this))
        {
            orderManager.orders[this][0].orderData.targetPosition = transform.position;
        }
    }

    // Activate on collision
    void OnTriggerEnter(Collider other)
    {
        CheckExplode(other);
    }

    void OnTriggerStay(Collider other) { } // override
}
