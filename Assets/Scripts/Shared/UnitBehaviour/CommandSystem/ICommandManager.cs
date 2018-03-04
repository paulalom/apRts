using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICommandManager : MonoBehaviour
{
    public abstract void AddCommand(Command command);
    public abstract void AddCommand(Command command, List<long> unitIds);
    public abstract void AddCommands(List<MyPair<List<long>, Command>> commands);
    public abstract void AddNonNetworkedCommand(Command command);
    public abstract void ProcessCommandsForStep(long step, GameManager gameManager);
    public abstract List<MyPair<List<long>, Command>> GetCommandsForStep(long step);
}
