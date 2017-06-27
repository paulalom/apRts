using UnityEngine;
using System.Collections.Generic;

public abstract class Generator : Subsystem, IActivatable {

    public List<FlowSystem> InOrderOutlets;
    public float generationAmountPerStep;
    public float activationCost;
    public bool isActive = true;

    void Update()
    {
        if (isActive)
        {
            float delta = StepManager.GetDeltaStep();
            Generate(generationAmountPerStep * delta);
        }
    }

    public abstract bool Generate(float amount);

    public abstract void Activate();

    public abstract void Deactivate();
}
