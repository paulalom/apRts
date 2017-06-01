using System;
using UnityEngine;
using System.Collections;

// This will be used for Power, Energy, and Magic flow, but it could be used for generally anything... 
// just imagine, batteries filled with tanks!
public class FlowSystem : Subsystem {

    public float powerIn;
    public float powerInInterval;
    public float powerOut;
    public float powerOutInterval;
    public float capacity;
    public float currentCharge;
    public Type flowTypeIn; // Energy, Magic, Power etc..
    public Type flowTypeOut; 
}
