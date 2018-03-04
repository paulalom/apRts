using System;
using UnityEngine;
using System.Collections;

public class AbilitySubsystem : Subsystem {

    public Type powerType; // the type of power this ability costs/generates
    public float powerDraw; // If we can't draw the power, the attack will wait for the capacitor to fill up.
    public float cooldown; // If we have enough power, this is the max rate of fire/use
    public float minPowerDraw; // We can fire/use at a reduced range/area/power depending on the ability if we use less power

}
