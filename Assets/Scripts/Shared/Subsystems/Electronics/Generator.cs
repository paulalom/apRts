using UnityEngine;
using System.Collections.Generic;

public abstract class Generator : Subsystem, IActivatable {

    public List<FlowSystem> InOrderOutlets;
    public float generationAmountPerStep;
    public float activationCost;
    public bool isActive = true;

    public override void MyUpdate()
    {
        if (isActive)
        {
            float dt = StepManager.GetDeltaStep()/1000f;
            Generate(generationAmountPerStep * dt);
        }
    }

    public abstract bool Generate(float amount);

    public abstract void Activate();

    public abstract void Deactivate();
}
