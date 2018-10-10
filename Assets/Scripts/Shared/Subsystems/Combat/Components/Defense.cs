using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class Defense : MyMonoBehaviour, IDamagable {

    public RTSGameObject owner;
    public bool isActive = true;
    public float absorptionRatio = 1;
    public float currentHitPoints, maxHitPoints;

    public abstract void TakeDamage(float amount);
}

public class DefenseOwnerEqualityComparer : IEqualityComparer<Defense>
{
    public bool Equals(Defense x, Defense y)
    {
        return x.owner = y.owner;
    }

    public int GetHashCode(Defense obj)
    {
        throw new NotImplementedException();
    }
}
