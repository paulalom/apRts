using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICommandManager : MonoBehaviour
{
    public abstract void AddCommand(Command command, List<long> unitIds);
    public abstract void AddCommands(List<MyPair<List<long>, Command>> commands);
    public abstract void AddNonNetworkedCommand(Command command, List<long> unitIds);
    public abstract List<MyPair<List<long>, Command>> GetCommandsForStep(long step);
}
