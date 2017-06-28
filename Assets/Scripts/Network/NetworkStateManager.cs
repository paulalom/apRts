using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class NetworkStateManager : NetworkBehaviour {

    long serverStep;
    long clientStep;

    Dictionary<long, List<OrderData>> commandHistory;
    
}
