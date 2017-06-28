using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AttackNearestEnemyPlan : Plan
{
    RTSGameObjectManager rtsGameObjectManager;

    public AttackNearestEnemyPlan()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
    }

    public List<Order> GetPlanSteps(RTSGameObject unit)
    {
        List<Order> planSteps = new List<Order>();

        RTSGameObject nearestEnemy = rtsGameObjectManager.GetNearestEnemy(unit);

        if (nearestEnemy != null)
        {
            planSteps.Add(OrderFactory.BuildAbilityOrder(nearestEnemy, unit.defaultAbility));
        }

        return planSteps;
    }
}
