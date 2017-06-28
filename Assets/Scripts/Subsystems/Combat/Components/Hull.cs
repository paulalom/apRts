using UnityEngine;
using System.Collections;
using System;

public class Hull : Defense, IDamagable
{
    public float hullPoints;
    RTSGameObjectManager rtsGameObjectManager;

    public override void MyAwake()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
    }

    public override void TakeDamage(float amount)
    {
        hullPoints -= amount;
        if (hullPoints <= 0)
        {
            rtsGameObjectManager.DestroyUnit(owner);
        }
    }
}
