using System;
using UnityEngine;
using System.Collections.Generic;

// This will be used for Power, Energy, and Magic flow, but it could be used for generally anything... 
// just imagine, batteries filled with tanks!
public class FlowSystem : Subsystem {

    public float maxPowerInPerSecond;
    public float maxPowerOutPerSecond;
    public float capacity;
    public float currentCharge; // READ ONLY
    private float availableChargeIn = 0;
    private float availableChargeOut = 0;
    public float AvailableChargeIn { get { return availableChargeIn; } }
    public float AvailableChargeOut { get { return availableChargeOut; } }
    public float CurrentCharge { get { return currentCharge; } }
    public Type flowTypeIn; // Energy, Magic, Power etc..
    public Type flowTypeOut;
    public List<FlowSystem> inlets;
    

    public override void MyAwake()
    {
        availableChargeOut = Math.Min(maxPowerOutPerSecond, currentCharge);
        availableChargeIn = Math.Max(maxPowerInPerSecond, capacity - currentCharge);
    }

    public override void MyUpdate()
    {
        float dt = StepManager.GetDeltaStep();

        availableChargeOut += maxPowerOutPerSecond * dt;
        availableChargeOut = Math.Min(maxPowerOutPerSecond, availableChargeOut);
        availableChargeOut = Math.Min(currentCharge, availableChargeOut);

        availableChargeIn += maxPowerInPerSecond * dt;
        availableChargeIn = Math.Min(maxPowerInPerSecond, availableChargeIn);
        availableChargeIn = Math.Min(capacity - currentCharge, availableChargeIn);

        float chargeTransferAmount = Drain(availableChargeIn);
        AddCharge(chargeTransferAmount);
    }

    /// <summary>
    /// Adds as much charge as there is room for
    /// </summary>
    /// <param name="amount">Charge quantity</param>
    /// <returns>Amount of charge left over</returns>
    public float AddCharge(float amount)
    {
        float amountToAdd = Math.Min(amount, availableChargeIn);
        currentCharge += amountToAdd;
        availableChargeIn -= amountToAdd;
        return amountToAdd;
    }

    public float TakeCharge(float amount)
    {
        float amountToTake = Math.Min(amount, availableChargeOut);
        currentCharge -= amountToTake;
        availableChargeOut -= amountToTake;
        return amountToTake;
    }

    /// <summary>
    /// Drains as much as possible from the inlets, up to drainAmount
    /// </summary>
    /// <param name="drainAmount"></param>
    /// <returns>Amount drained</returns>
    public float Drain(float drainAmount)
    {
        float remainingDrainAmount = drainAmount;

        foreach (FlowSystem inlet in inlets)
        {
            remainingDrainAmount -= Drain(inlet, remainingDrainAmount);

            if (remainingDrainAmount == 0)
            {
                break;
            }
            else if (remainingDrainAmount < 0)
            {
                throw new Exception("Unexpected drainAmountRemaining: " + remainingDrainAmount);
            }
        }
        return drainAmount - remainingDrainAmount;
    }

    /// <summary>
    /// Drains as much as possible from the inlet, up to drainAmount
    /// </summary>
    /// <param name="drainAmount"></param>
    /// <returns>Amount drained</returns>
    protected float Drain(FlowSystem inlet, float drainAmount)
    {
        return inlet.TakeCharge(drainAmount);
    }
}
