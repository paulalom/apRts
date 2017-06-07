using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class AIManager : MonoBehaviour
{
    RTSGameObjectManager rtsGameObjectManager;
    PlayerManager playerManager;
    OrderManager orderManager;
    List<AITacticsManager> tacticsManagers;
    List<AIStrategyManager> strategyManagers;
    List<AIEconomicManager> economicManagers;
    public float rangeToSearchForResources = 100;

    // Use this for initialization
    void Start()
    {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        rtsGameObjectManager.onUnitCreated.AddListener(SubscribeToIdleEvents);

        tacticsManagers = new List<AITacticsManager>();
        strategyManagers = new List<AIStrategyManager>();
        economicManagers = new List<AIEconomicManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetUpPlayerAIManagers(Player player)
    {
        if (!player.isHuman)
        {
            AIStrategyManager stratMan = new AIStrategyManager(player, rtsGameObjectManager);
            strategyManagers.Add(stratMan);
            tacticsManagers.Add(new AITacticsManager(player));
            economicManagers.Add(new AIEconomicManager(player));

            orderManager.QueueOrders(stratMan.GetStartOrders());
        }
    }

    void SubscribeToIdleEvents(RTSGameObject idleUnit)
    {
        idleUnit.onIdle.AddListener(OnIdleChangeEvent);
    }

    void OnIdleChangeEvent(RTSGameObject unit, bool idleStatus)
    {
        if (idleStatus && !playerManager.PlayerSelectedUnits.Contains(unit))
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
            if (orderManager.orders.ContainsKey(unit))
            {
                orderManager.orders[unit].Clear();
            }
            // need to take advantage of unitPlans here to create a repeating plan so we dont need to search every time for the nearest resource
            CollectResourcesPlan collectPlan = new CollectResourcesPlan();
            List<Order> planOrders = collectPlan.GetPlanSteps(unit);
            foreach (Order order in planOrders)
            {
                orderManager.QueueOrder(unit, order);
            }
            if (planOrders.Count == 0)
            {
                return false;
            }
        }
        else if (unit.GetType() == typeof(Tank) && unit.ownerId == 2)
        {
            if (orderManager.orders.ContainsKey(unit))
            {
                orderManager.orders[unit].Clear();
            }
            // need to take advantage of unitPlans here to create a repeating plan so we dont need to search every time for the nearest resource
            AttackNearestEnemyPlan attackPlan = new AttackNearestEnemyPlan();
            List<Order> planOrders = attackPlan.GetPlanSteps(unit);
            foreach (Order order in planOrders)
            {
                orderManager.QueueOrder(unit, order);
            }
            if (planOrders.Count == 0)
            {
                return false;
            }
        }
        return true;
    }

    public bool SetNewPlanForUnit(RTSGameObject unit, Plan plan)
    {
        List<Order> planOrders = plan.GetPlanSteps(unit);
        /*if (orderManager.orders.ContainsKey(unit) && orderManager.orders[unit].Count > 0)
        {
            orderManager.orders[unit].Clear();
        }*/
        foreach (Order order in planOrders)
        {
            orderManager.QueueOrder(unit, order);
        }
        if (planOrders.Count == 0)
        {
            return false;
        }
        return true;
    }
}
