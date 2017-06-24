using System;
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

// Orders probably need to be rewritten into many classes (one per type?)
public abstract class Order {
    
    public OrderPhase phase = OrderPhase.GetInRange;
    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public float orderRange = 1f;
    public List<MyPair<Type, int>> items;
    public RTSGameObject target;
    public Ability ability;
    public bool repeatOnComplete = false;
    public float remainingChannelTime;

    public Order() {}

    public Order(Order o)
    {
        phase = o.phase;
        targetPosition = o.targetPosition;
        orderIssuedPosition = o.orderIssuedPosition;
        orderRange = o.orderRange;
        items = o.items;
        target = o.target;
        ability = o.ability;
        remainingChannelTime = o.remainingChannelTime;
    }
    
    public virtual OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }

    public virtual bool GetInRange(RTSGameObject performingUnit, RTSGameObjectManager rtsGameObjectManager, float dt)
    {
        Vector3 targetPos = target == null ? targetPosition : target.transform.position;

        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPos, orderRange))
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
        remainingChannelTime-= dt;
    }
    protected bool IsFinishedChanneling()
    {
        return remainingChannelTime <= 0;
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
}
