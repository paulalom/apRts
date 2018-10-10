using UnityEngine;
using System.Collections;
using System;

public class Health : Defense, IDamagable
{
    RTSGameObjectManager rtsGameObjectManager;

    public override void MyAwake()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
    }

    public override void TakeDamage(float amount)
    {
        currentHitPoints -= amount;
        if (currentHitPoints <= 0)
        {
            rtsGameObjectManager.DestroyUnit(owner);
        }
    }
}
