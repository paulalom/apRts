using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Capacitor))]
[RequireComponent(typeof(Shield))]
public class ShieldGenerator : Generator, IActivatable
{
    public Shield shield;
    public Capacitor capacitor;
    public float damageToEnergyBurnRatio;
    
    public override bool Generate(float amount)
    {
        if (shield.isActive)
        {
            capacitor.TakeCharge(amount);
            float shieldPowerRatio = Math.Min(1, capacitor.CurrentCharge * 2 / capacitor.capacity);
            shield.PowerShield(shieldPowerRatio);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Activate()
    {
        shield.Activate();
        Generate(activationCost);
        isActive = true;
    }

    public override void Deactivate()
    {
        shield.Deactivate();
        isActive = false;
    }
}
