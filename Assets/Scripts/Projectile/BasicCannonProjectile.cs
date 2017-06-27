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
        if ((otherRtsGo == null || otherRtsGo.ownerId != rtsGo.ownerId) 
            && orderManager.orders.ContainsKey(rtsGo))
        {
            orderManager.orders[rtsGo][0].targetPosition = transform.position;
        }
    }

    // Activate on collision
    void OnTriggerEnter(Collider other)
    {
        CheckExplode(other);
    }

    void OnTriggerStay(Collider other) { } // override
}
