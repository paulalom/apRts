using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public enum OrderPhase
{
    GetInRange,
    Activate,
    Channel,
    FinishChannel,
    Complete
}

public enum OrderValidationResult
{
    Success,
    Failure,
    CantDoThat,
    InvalidTarget,
    NotOnSelf,
}

public abstract class Order {

    public OrderData orderData = new OrderData();
    
    public virtual OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }

    public virtual bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        Vector3 targetPos = orderData.target == null ? orderData.targetPosition : orderData.target.transform.position;

        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPos, orderData.orderRange))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(targetPos.x, targetPos.z), dt);
            return false;
        }
    }

    public virtual bool Activate(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        return true;
    }

    public virtual bool Channel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        updateChannelDuration(dt);
        return IsFinishedChanneling();
    }

    public virtual bool FinishChannel(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager)
    {
        return true;
    }

    protected void updateChannelDuration(float dt)
    {
        orderData.remainingChannelTime -= dt;
    }
    protected bool IsFinishedChanneling()
    {
        return orderData.remainingChannelTime <= 0;
    }

    protected static bool CheckHasComponent<T>(RTSGameObject unit)
    {
        return unit.GetComponent<T>() != null;
    }

    protected static bool CheckHasComponent(RTSGameObject unit, Type type)
    {
        return unit.GetComponent(type) != null;
    }

    protected static bool CheckTargetExists(RTSGameObject target)
    {
        return target != null;
    }

    protected static bool CheckCanMove(RTSGameObject unit)
    {
        return (unit.GetComponent<Mover>() != null);
    }

    protected OrderValidationResult ValidateAbilityUsage(RTSGameObject unit, Ability abilityToUse)
    {
        bool hasAbility = false;
        foreach (Ability ability in unit.GetComponents<Ability>())
        {
            if (abilityToUse.GetType() == ability.GetType())
            {
                hasAbility = true;
                break;
            }
        }
        if (!hasAbility)
        {
            return OrderValidationResult.CantDoThat;
        }

        return OrderValidationResult.Success;
    }

    protected bool ValidateStorageAccess(RTSGameObject accessor, RTSGameObject target)
    {
        if (!CheckTargetExists(target) || !CheckCanMove(accessor) || !CheckHasComponent<Storage>(target) || !CheckHasComponent<Storage>(accessor))
        {
            return false;
        }
        else {
            foreach (Type type in target.storage.requiredAccessComponents)
            {
                if (!CheckHasComponent(accessor, type))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public override string ToString()
    {
        return orderData.ToString();
    }
}
