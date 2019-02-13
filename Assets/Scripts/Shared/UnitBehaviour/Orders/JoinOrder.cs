
// There are a lot of scenarios here ... note Interrupt means to add to front of queue (alt+click)
// Examples:
//     Worker joins Factory A currently producing a queue of units, then
//         1) Factory A interrupts a production with a new order.
//            Worker needs to join the new order instead of continuing old.
//         2) Factory A completes a unit
//            Worker needs to join the next production
//         3) Factory A completes all units
//            If worker has no queued orders, it will wait for next factory production
//            otherwise it will complete the join order and continue on it's own queue
//         4) Factory A is destroyed during production
//            Worker needs to complete join order and continue with its queue
//         5) Factory A can move or perform non-production orders
//            Worker needs to follow if Factory A moves, or wait if factory A is doing something else.
//            Worker needs to resume join when factory A starts a joinable order.
//         6) Factory A interrupts production with a join order
//            If worker can join factory A's join target, worker does so, replacing the join on factory A.
//            Otherwise, Worker behaves as if (5) occurred.
//         7) Worker interrupts Factory A join (join order 1) with a join order on Factory B (join order 2)
//            Worker needs to join factory B, and when join order 2 completed as described above, resume join order 1 on Factory A
//             i) Factory A dies or completes all orders before worker rejoins
//                Worker needs to complete join order 1
//             ii) Factory A interrupts with another order before worker rejoins
//                 Worker behaves as in 5/6
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Any unit that can move can join any target in their current or future actions (eg. help construct, summon elder god)
/// Other default behaviours should override this one, such as a worker joining a structure under construction is a "Resume construction" order.
/// If you join unit a which is already joined to unit b, you will instead join unit b.
/// </summary>
public class JoinOrder : Order {

    public Order joinedOrder;
    bool waitingForOrder = false;

    public override OrderPhase Phase {
        get {
            if (isActivated)
            {
                return joinedOrder.Phase;
            }
            else
            {
                return _phase;
            }
        }
        set
        {
            if (isActivated)
            {
                joinedOrder.Phase = value;
            }
            else
            {
                _phase = value;
            }
        }
    }

    public override bool GetInRange(RTSGameObject performingUnit, int dt)
    {
        if (waitingForOrder)
        {
            Vector3 targetPos = orderData.target == null ? orderData.targetPosition : orderData.target.transform.position;

            if (!rtsGameObjectManager.lazyWithinDist(performingUnit.transform.position, targetPos, orderData.orderRange + performingUnit.transform.localScale.x))
            {
                rtsGameObjectManager.SetUnitMoveTarget(performingUnit, new Vector2(targetPos.x, targetPos.z), dt);
            }
            return false;
        }
        else
        {
            return joinedOrder.GetInRange(performingUnit, dt);
        }
    }

    public override bool Activate(RTSGameObject performingUnit)
    {
        // Don't activate/join until the joined order is active.
        if (!joinedOrder.isActivated)
        {
            return false;
        }
        // Never call source activation, only join once per order. (otherwise re-init)
        if (isActivated) { return true; }
        base.Activate(performingUnit);
        joinedOrder.Join(performingUnit);
        return true;
    }

    public override bool Channel(RTSGameObject performingUnit, int dt)
    {
        if (joinedOrder.Channel(performingUnit, dt))
        {
            return true;
        }
        else
        {
            return false;
        }        
    }

    public override bool FinishChannel(RTSGameObject performingUnit)
    {
        return false;
    }

    public override OrderValidationResult Validate(RTSGameObject performingUnit)
    {
        // we can always join a unit (other than ourselves), but it might not do anything until the unit we're joining does something
        return orderData.target != null && !(orderData.target is ResourceDeposit) && orderData.target != performingUnit ? OrderValidationResult.Success : OrderValidationResult.InvalidTarget; 
    }

    public override void Initilize(RTSGameObject performingUnit)
    {
        Order targetOrder = null;
        if (orderManager.orders.ContainsKey(orderData.target)){
            targetOrder = orderManager.orders[orderData.target][0];
        }
        Initilize(performingUnit, targetOrder);
    }

    public void Initilize(RTSGameObject performingUnit, Order targetOrder)
    {
        isActivated = false;
        base.Initilize(performingUnit);
        if (targetOrder != null) // orders never contains an empty list
        {
            joinedOrder = targetOrder;
            joinedOrder.OnPausedEvent.AddListener(OnJoinedOrderPaused);
            joinedOrder.OnCompletionEvent.AddListener(OnJoinedOrderCompleted);

            if (ValidateJoinProduction(performingUnit) || joinedOrder.Validate(performingUnit) == OrderValidationResult.Success)
            {
                orderData = joinedOrder.orderData;
                _phase = OrderPhase.GetInRange;
                waitingForOrder = false;
            }
            else
            {
                waitingForOrder = true;
                WaitForNextOrder();
            }
        }
        else if (orderManager.orders[performingUnit].Count > 1)
        {
            // I've got other shit to do!
            _phase = OrderPhase.SelfCleanup;
        }
        else
        {
            // I'll just follow you around ...
            orderData.target.onIdleStatusChange.AddListener(OnTargetIdleStatusChange);
            waitingForOrder = true;
            WaitForNextOrder();
        }
    }

    public void OnTargetIdleStatusChange(RTSGameObject target, bool isIdle)
    {
        if (!isIdle)
        {
            orderData.target.onIdleStatusChange.RemoveListener(OnTargetIdleStatusChange);
            Initilize(initiatingUnit);
        }
    }

    // Skip the current order since it's not joinable
    void WaitForNextOrder()
    {
        orderData = new OrderData() { target = orderData.target, orderRange = 15f };
        _phase = OrderPhase.GetInRange;
    }

    private void OnJoinedOrderPaused(Order newOrder)
    {
        UnjoinCurrentOrder();
        Initilize(initiatingUnit, newOrder);
    }

    private void OnJoinedOrderCompleted(Order nextOrder)
    {
        UnjoinCurrentOrder();
        Initilize(initiatingUnit, nextOrder);
    }

    void UnjoinCurrentOrder()
    {
        joinedOrder.OnPausedEvent.RemoveListener(OnJoinedOrderPaused);
        if (orderData.target != null) // fixme, unjoin shouldnt be called if there is no target.. thats the unit we're joined to
        {
            orderData.target.onDestroyed.RemoveListener(OnTargetDestroyed);
        }
        // Turn off production animation if next order isnt a production order
        if (joinedOrder is ProductionOrder)
        {
            Order nextOrder = orderManager.GetOrderForUnit(orderData.target, 1);
            if (nextOrder == null || !(nextOrder is ProductionOrder && nextOrder.orderData.targetPosition == Vector3.zero))
            {
                ((ProductionOrder)joinedOrder).SetConstructionHardpointActivity(initiatingUnit.GetComponent<Producer>(), false);
            }
        }
        joinedOrder.OnCompletionEvent.RemoveListener(OnJoinedOrderCompleted);
    }

    public override void OnCancel(RTSGameObject performingUnit, GameManager gameManager)
    {
        base.OnCancel(performingUnit, gameManager);
        orderData.target.onIdleStatusChange.RemoveListener(OnTargetIdleStatusChange);
    }

    // Can always assist productions, later will add skills at producing things
    bool ValidateJoinProduction(RTSGameObject performingUnit)
    {
        if (performingUnit.GetComponent<Producer>() != null 
            && (joinedOrder is ProductionOrder 
            || joinedOrder is ConstructionOrder 
            || joinedOrder is ResumeConstructionOrder))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
