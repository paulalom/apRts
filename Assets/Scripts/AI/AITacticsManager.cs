using UnityEngine;
using System.Collections;

public class AITacticsManager {

    Player playerToManage;
    AIEconomyManager economyManager;
    AIMilitaryManager militaryManager;
    public const float rangeToSearchForResources = 200;
    public const int shouldDepositAtFactoryThreshold = 50;

    public AITacticsManager(Player playerToManage, AIEconomyManager economyManager, AIMilitaryManager militaryManager)
    {
        this.playerToManage = playerToManage;
        this.economyManager = economyManager;
        this.militaryManager = militaryManager;
    }

    public void IssueTacticalOrders(OrderManager orderManager)
    {
        orderManager.QueueOrders(economyManager.GetConstructionOrders());
    }
}
