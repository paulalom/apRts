using System;
using UnityEngine;
using System.Collections.Generic;

public class NetworkedCommandManager : ICommandManager{

    public PlayerManager playerManager;
    public NetworkStateManager netStateManager;
    CommandQueue nonNetworkedCommandQueue = new CommandQueue(); // non game affecting ui commands
    
    public override void AddCommand(Command command, List<long> unitIds)
    {
        MyPair<List<long>, Command> unitCommandPair = BuildUnitCommandPair(command, unitIds);
        long stepToRunCommands = StepManager.CurrentStep + StepManager.numStepsToDelayInputProcessing;

        netStateManager.AddCommand(stepToRunCommands, unitCommandPair);
    }

    public override void AddCommands(List<MyPair<List<long>, Command>> commands)
    {
        long stepToRunCommands = StepManager.CurrentStep + StepManager.numStepsToDelayInputProcessing;
        foreach (MyPair<List<long>, Command> command in commands)
        {
            netStateManager.AddCommand(stepToRunCommands, command);
        }
    }

    public override void AddNonNetworkedCommand(Command command, List<long> unitIds)
    {
        MyPair<List<long>, Command> unitCommandPair = BuildUnitCommandPair(command, unitIds);
        nonNetworkedCommandQueue.AddCommand(StepManager.CurrentStep, unitCommandPair);
    }
    
    MyPair<List<long>, Command> BuildUnitCommandPair(Command command, List<long> unitIds)
    {
        MyPair<List<long>, Command> unitCommandPair;

        unitCommandPair = new MyPair<List<long>, Command>(unitIds, command);

        return unitCommandPair;
    }
    
    public override List<MyPair<List<long>, Command>> GetCommandsForStep(long step)
    {
        return netStateManager.localCommands.GetCommandsForStep(step);
    }
}
