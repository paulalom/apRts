using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Explode))]
public class BasicCannonProjectile : Projectile {

    public float baseDamage;
    public DamageType damageType;
}
