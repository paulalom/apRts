using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class Shoot : Ability {

    public Projectile projectile;
    
    void Start()
    {
        projectile = GetComponent<Projectile>();
    }
}
