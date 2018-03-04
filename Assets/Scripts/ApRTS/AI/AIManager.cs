using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class AIManager
{
    RTSGameObjectManager rtsGameObjectManager;
    PlayerManager playerManager;
    NetworkedCommandManager commandManager;

    // Internal managers
    AITacticsManager tacticsManager;
    AIStrategyManager strategyManager;
    AIEconomyManager economicManager;
    AIMilitaryManager militaryManager;

    // Use this for initialization
    public AIManager(RTSGameObjectManager rtsGameObjectManager, PlayerManager playerManager, NetworkedCommandManager commandManager)
    {
        this.rtsGameObjectManager = rtsGameObjectManager;
        this.playerManager = playerManager;
        this.commandManager = commandManager;
        rtsGameObjectManager.onUnitCreated.AddListener(SubscribeToIdleEvents);
    }

    // Update is called once per frame
    public void MyUpdate()
    {
        UpdateStrategySettings();
        UpdateEconomicSettings();
        UpdateMilitarySettings();
        IssueTacticalOrders();
    }

    void UpdateStrategySettings()
    {
        strategyManager.UpdateStrategySettings();
    }

    void UpdateEconomicSettings()
    {
        economicManager.UpdateEconomicSettings();
    }

    void UpdateMilitarySettings()
    {
        militaryManager.UpdateMilitarySettings();
    }

    void IssueTacticalOrders()
    {
        tacticsManager.IssueTacticalCommands(commandManager);
    }

    public void SetUpPlayerAIManagers(Player player)
    {
        if (!player.isHuman)
        {
            AIStrategyManager strategyManagers = new AIStrategyManager(player, rtsGameObjectManager);
            AIEconomyManager economicManagers = new AIEconomyManager(player);
            AIMilitaryManager militaryManagers = new AIMilitaryManager(player);
            AITacticsManager tacticsManagers = new AITacticsManager(player, economicManager, militaryManager);
            
            commandManager.AddCommands(strategyManagers.GetStartCommands());
        }
    }

    void SubscribeToIdleEvents(RTSGameObject idleUnit)
    {
        idleUnit.onIdle.AddListener(OnIdleChangeEvent);
    }

    void OnIdleChangeEvent(RTSGameObject unit, bool idleStatus)
    {
        if (idleStatus && !playerManager.PlayerSelectedUnits.Contains(unit.uid))
        {
            if (!SetNewPlanForUnit(unit))
            {
                // idleUnits.Add(unit);
            }
        }
        else
        {
            //  idleUnits.Remove(unit);
        }
    }


    public bool SetNewPlanForUnit(RTSGameObject unit)
    {
        if (unit.GetType() == typeof(ConstructionSphere))
        {
            // need to take advantage of unitPlans here to create a repeating plan so we dont need to search every time for the nearest resource
            CollectResourcesPlan collectPlan = new CollectResourcesPlan(this, rtsGameObjectManager);
            return SetNewPlanForUnit(unit, collectPlan);
        }
        else if (unit.GetType() == typeof(Tank) && !playerManager.players[unit.ownerId].isHuman)
        {
            AttackNearestEnemyPlan attackPlan = new AttackNearestEnemyPlan(rtsGameObjectManager);
            return SetNewPlanForUnit(unit, attackPlan);
        }
        return false;
    }

    public bool SetNewPlanForUnit(RTSGameObject unit, Plan plan)
    {
        List<Order> planOrders = plan.GetPlanSteps(unit);
        List<MyPair<List<long>, Command>> commands = new List<MyPair<List<long>, Command>>();

        
        foreach (Order order in planOrders)
        {
            Command command = CommandFactory.GetCommandFromOrder(order);
            List<long> unitIds = new List<long>() { unit.uid };
            MyPair<List<long>, Command> commandUnitPair = new MyPair<List<long>, Command>(unitIds, command);

            commands.Add(commandUnitPair);
        }
        if (commands.Count == 0)
        {
            return false;
        }
        else if(commands[0].Value.getOrder == CommandGetOrderFunction.GetDefaultConstructionOrder)
        {// construction orders queue, not set
            commands[0].Value.clearExistingOrders = false;
        }
        else
        {
            commands[0].Value.clearExistingOrders = true;
        }
        commandManager.AddCommands(commands);
        return true;
    }
}
