using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class AIEconomicManager {

    Player playerToManage;
    List<RTSGameObject> untappedNearbyResourceDeposits;
    List<RTSGameObject> productionStructures;
    
    public class OnProductionStructureCreation : UnityEvent<RTSGameObject> { }
    OnProductionStructureCreation onProductionStructureCreation;
    public class OnProductionStructureDestruction : UnityEvent<RTSGameObject> { }
    OnProductionStructureDestruction onProductionStructureDestruction;

    public AIEconomicManager(Player playerToManage)
    {
        this.playerToManage = playerToManage;
        
    }

}
