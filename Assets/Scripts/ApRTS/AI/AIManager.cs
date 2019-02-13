using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class AIManager
{
    RTSGameObjectManager rtsGameObjectManager;
    PlayerManager playerManager;
    SelectionManager selectionManager;
    ICommandManager commandManager;

    // Internal managers
    AITacticsManager tacticsManager;
    AIStrategyManager strategyManager;
    AIEconomyManager economicManager;
    AIMilitaryManager militaryManager;

    // Use this for initialization
    public AIManager(RTSGameObjectManager rtsGameObjectManager, PlayerManager playerManager, SelectionManager selectionManager, ICommandManager commandManager)
    {
        this.rtsGameObjectManager = rtsGameObjectManager;
        this.playerManager = playerManager;
        this.selectionManager = selectionManager;
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
            strategyManager = new AIStrategyManager(player, rtsGameObjectManager);
            economicManager = new AIEconomyManager(player);
            militaryManager = new AIMilitaryManager(player);
            tacticsManager = new AITacticsManager(player, economicManager, militaryManager);
            
            commandManager.AddCommands(strategyManager.GetStartCommands());
        }
    }

    void SubscribeToIdleEvents(RTSGameObject idleUnit)
    {
        idleUnit.onIdleStatusChange.AddListener(OnIdleStatusChange);
    }

    void OnIdleStatusChange(RTSGameObject unit, bool isIdle)
    {
        if (isIdle && !selectionManager.selectedUnits.Contains(unit))
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
            List<long> unitIds = new List<long>() { unit.unitId };
            MyPair<List<long>, Command> commandUnitPair = new MyPair<List<long>, Command>(unitIds, command);

            commands.Add(commandUnitPair);
        }
        if (commands.Count == 0)
        {
            return false;
        }
        else if(commands[0].Value.getOrder == OrderBuilderFunction.NewConstructionOrder)
        {// construction orders queue, not set
            commands[0].Value.queueOrderInsteadOfClearing = true;
            commands[0].Value.queueOrderAtFront = Input.GetKey(Setting.addOrderToFrontOfQueue);
        }
        else
        {
            commands[0].Value.queueOrderInsteadOfClearing = false;
            commands[0].Value.queueOrderAtFront = Input.GetKey(Setting.addOrderToFrontOfQueue);
        }
        commandManager.AddCommands(commands);
        return true;
    }
}
