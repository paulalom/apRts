using UnityEngine;
using System.Collections.Generic;
using System;

public class PowerGenerator : Generator
{
    public override bool Generate(float amount)
    {
        foreach (FlowSystem outlet in InOrderOutlets)
        {
            amount = outlet.AddCharge(amount);
        }
        return amount == 0;
    }

    public override void Activate()
    {
        isActive = true;
    }

    public override void Deactivate()
    {
        isActive = false;
    }
}
