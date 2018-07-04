using UnityEngine;
using System.Collections.Generic;

public class CommandQueue
{
    // Map of step->List of (unitIds->Command)
    public Dictionary<long, List<MyPair<List<long>, Command>>> commands = new Dictionary<long, List<MyPair<List<long>, Command>>>();
    
    public List<MyPair<List<long>, Command>> GetCommandsForStep(long step)
    {
        if (commands.ContainsKey(step))
        {
            return commands[step];
        }
        else
        {
            return new List<MyPair<List<long>, Command>>();
        }
    }

    public void AddCommand(long step, MyPair<List<long>, Command> unitCommands)
    {
        if (commands.ContainsKey(step))
        {
            commands[step].Add(unitCommands);
        }
        else
        {
            commands.Add(step, new List<MyPair<List<long>, Command>>() { unitCommands });
        }
    }
}
