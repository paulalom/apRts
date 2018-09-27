using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum OrderPhase
{
    GetInRange,
    Activate,
    Channel,
    FinishChannel,
    Complete,
    SelfCleanup // only used by joinOrders
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
    public static OrderManager orderManager; // needed for joinableOrders
    public static RTSGameObjectManager rtsGameObjectManager;
    
    // Event fires with the new order
    public class OrderChangedEvent : UnityEvent<Order> { }
    public OrderChangedEvent OnPausedEvent = new OrderChangedEvent();
    public OrderChangedEvent OnCompletionEvent = new OrderChangedEvent();
    // These are outside of orderData to facilitate joinOrders maintaining part of their state
    // May be a cleaner solution to put them in orderData and just have joinOrders only copy part of the data
    // But I like having all of the joined orders share the same orderData as a reference
    protected OrderPhase _phase = OrderPhase.GetInRange;
    public virtual OrderPhase Phase { get; set; }
    public RTSGameObject initiatingUnit;
    public bool isActivated = false;

    // this happens before validation but is not a part of the normal phases, 
    // since the order needs to be validated before sent over the network as a command
    public virtual void Initilize(RTSGameObject performingUnit)
    {
        if (orderData.target != null) {
            orderData.target.onDestroyed.AddListener(OnTargetDestroyed);
        }
    }

    public virtual OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        return OrderValidationResult.Success;
    }

    public virtual void OnQueue()
    {

    }

    public virtual void OnCancel(RTSGameObject performingUnit, GameManager gameManager)
    {

    }

    public virtual bool GetInRange(RTSGameObject performingUnit, int dt)
    {
        Vector3 targetPos = orderData.target == null ? orderData.targetPosition : orderData.target.transform.position;

        if (rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPos, orderData.orderRange + performingUnit.transform.localScale.magnitude))
        {
            return true;
        }
        else
        {
            rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(targetPos.x, targetPos.z), dt);
            return false;
        }
    }

    public virtual bool Activate(RTSGameObject performingUnit)
    {
        isActivated = true;
        return true;
    }

    // Some orders can be participated in by multiple units. This is called instead of Activate for joining units.
    public virtual void Join(RTSGameObject performingUnit) {}

    public virtual bool Channel(RTSGameObject performingUnit, int dt)
    {
        ChannelForTime(dt);
        return IsFinishedChanneling();
    }

    public virtual bool FinishChannel(RTSGameObject performingUnit)
    {
        return true;
    }
    
    public virtual void Complete(RTSGameObject performingUnit)
    {
        List<Order> orders = orderManager.orders[performingUnit];
        OnCompletionEvent.Invoke(orders.Count > 1 ? orders[1] : null);
    }
        
    // dt should be factored into channeled time
    protected void ChannelForTime(int channeledTime)
    {
        orderData.remainingChannelTime = orderData.remainingChannelTime - channeledTime;
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

    protected virtual void OnTargetDestroyed()
    {
        orderData.target = null;
        _phase = OrderPhase.SelfCleanup;
    }
}
