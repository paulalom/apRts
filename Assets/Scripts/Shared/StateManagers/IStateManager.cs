using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStateManager : MonoBehaviour {

    public abstract void AddCommand(long step, MyPair<List<long>, Command> unitCommands);

    //public abstract StartGame();

    public abstract void Step();

    //public abstract void AddPlayer();
        
}
