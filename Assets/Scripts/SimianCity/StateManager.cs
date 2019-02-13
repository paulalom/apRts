using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : IStateManager
{
    long currentStep = 0;
    Dictionary<long, List<MyPair<List<long>, Command>>> stepCommands = new Dictionary<long, List<MyPair<List<long>, Command>>>();

    public override void AddCommand(long step, MyPair<List<long>, Command> unitCommands)
    {
        if (stepCommands.ContainsKey(step))
        {
            stepCommands[step].Add(unitCommands);
        }
        else
        {
            var newStepCommands = new List<MyPair<List<long>, Command>>();
            newStepCommands.Add(unitCommands);
            stepCommands.Add(step, newStepCommands);
        }
    }

    public override void Step()
    {
        currentStep++;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
