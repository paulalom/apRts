using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OrderManager : MyMonoBehaviour {

    List<RTSGameObject> completedOrders;
    public Dictionary<RTSGameObject, List<Order>> orders;

    RTSGameObjectManager rtsGameObjectManager;
    GameManager gameManager;
    UIManager uiManager;

    public override void MyAwake()
    {
        orders = new Dictionary<RTSGameObject, List<Order>>();
        completedOrders = new List<RTSGameObject>(); //max one order completion per frame
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }

    public void CarryOutOrders(List<RTSGameObject> units, int dt)
    {
        foreach (RTSGameObject unit in units)
        {
            if (orders.ContainsKey(unit) && orders[unit].Count > 0)
            {
                Order order = orders[unit][0];

                switch (order.Phase)
                {
                    case OrderPhase.GetInRange:
                        order.Phase += (order.GetInRange(unit, dt) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Activate:
                        order.Phase += (order.Activate(unit) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Channel:
                        order.Phase += (order.Channel(unit, dt) == true) ? 1 : 0;
                        break;
                    case OrderPhase.FinishChannel:
                        order.Phase += (order.FinishChannel(unit) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Complete:
                        order.Complete(unit);
                        // "Completion" event triggers need to fire in the same phase as completedOrders.Add
                        // otherwise potential for single frame exceptions with joining not yet cleaned up orders.
                        if (!(order is JoinOrder))
                        {
                            completedOrders.Add(unit);
                        }
                        break;
                    default: // cleanup phase
                        completedOrders.Add(unit);
                        break;
                }
            }
        }

        // Need to make this better. Specific orders need to be removed, 
        // not just the first one (if someone queues an order as one finishes, there will be problems)
        foreach (RTSGameObject completer in completedOrders)
        {
            List<Order> unitOrders = orders[completer];
            Order completedOrder = orders[completer][0];
            if (completedOrder.orderData.repeatOnComplete)
            {
                unitOrders.Add(completedOrder);
            }
            CompleteOrder(completer);
        }
        completedOrders.Clear();
    }

    public void CompleteOrder(RTSGameObject unit)
    {
        List<Order> unitOrders = orders[unit];
        unitOrders.RemoveAt(0);
        if (unitOrders.Count == 0)
        {
            orders.Remove(unit);
            unit.IsIdle = true;
        }
        else
        {
            // We need to always set the orderPhase for the case when an order was interrupted mid-channel
            unitOrders[0].Phase = OrderPhase.GetInRange;
        }
    }

    // Only give/take need this for now
    internal void CheckOrderCompletionOnCollision(RTSGameObject unit, RTSGameObject target)
    {
        if (orders.ContainsKey(unit) && orders[unit].Count > 0)
        {
            Order order = orders[unit][0];

            if (order.GetType() == typeof(GiveOrder) && target.gameObject == order.orderData.target.gameObject && unit.ownerId == target.ownerId)
            {
                rtsGameObjectManager.GiveItems(unit, order.orderData.target, order.orderData.items);
                CompleteOrder(unit);
            }
            else if (order.GetType() == typeof(TakeOrder) && target.gameObject == order.orderData.target.gameObject && unit.ownerId == target.ownerId)
            {
                rtsGameObjectManager.TakeItems(unit, order.orderData.target, order.orderData.items);
                CompleteOrder(unit);
            }
        }
    }

    bool ValidateOrder(RTSGameObject unit, Order order)
    {
        string errorMessage = "";
        // there is no order or recipient for the order
        if (order == null || unit == null)
        {
            errorMessage = "No order or Unit";
            uiManager.CreateText(errorMessage, unit.transform.position);
            return false;
        }

        switch (order.Validate(unit)){
            case OrderValidationResult.Success:
                return true;
            case OrderValidationResult.CantDoThat:
                errorMessage = "Can't do " + order.GetType() + "!";
                uiManager.CreateText(errorMessage, unit.transform.position);
                return false;
            case OrderValidationResult.Failure:
                errorMessage = order.GetType() + " failed!";
                uiManager.CreateText(errorMessage, unit.transform.position);
                return false;
            case OrderValidationResult.InvalidTarget:
                errorMessage = "Can't do " + order.GetType() + " on that target!";
                uiManager.CreateText(errorMessage, unit.transform.position);
                return false;
            case OrderValidationResult.NotOnSelf:
                errorMessage = "Can't do " + order.GetType() + " on self!";
                uiManager.CreateText(errorMessage, unit.transform.position);
                return false;
            default:
                throw new NotImplementedException("Unhandled OrderValudationResult!");
        }
    }

    public bool SetOrder(RTSGameObject unit, Order order, bool validateOrder = true)
    {
        if (validateOrder)
        {
            if (ValidateOrder(unit, order))
            {
                CancelOrders(unit);
                QueueOrder(unit, order, false, false);
                return true;
            }
            else {
                return false;
            }
        }
        else
        {
            CancelOrders(unit);
            QueueOrder(unit, order, false, false);
            return true;
        }
    }

    // NOTE: validate:false should only be used if Initilize is called first (as is the case with SetOrder)
    // If you want a cheat, make a new order that always returns true
    public bool QueueOrder(RTSGameObject unit, Order order, bool insertAtfront = false, bool validateOrder = true)
    {
        order.initiatingUnit = unit;
        if (!orders.ContainsKey(unit))
        {
            orders.Add(unit, new List<Order>());
        }
        if (validateOrder)
        {
            if (ValidateOrder(unit, order))
            {
                if (insertAtfront)
                {
                    if (orders[unit].Count > 0)
                    {
                        orders[unit][0].OnPausedEvent.Invoke(order);
                    }
                    orders[unit].Insert(0, order);
                }
                else
                {
                    orders[unit].Add(order);
                }
                order.Initilize(unit);
                order.OnQueue();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (insertAtfront)
            {
                if (orders[unit].Count > 0)
                {
                    orders[unit][0].OnPausedEvent.Invoke(order);
                }
                orders[unit].Insert(0, order);
            }
            else
            {
                orders[unit].Add(order);
            }
            order.Initilize(unit);
            order.OnQueue();
            return true;
        }
    }

    public void QueueOrders(Dictionary<RTSGameObject, Order> orders, bool insertAtFront)
    {
        foreach (KeyValuePair<RTSGameObject, Order> unitOrderPair in orders)
        {
            QueueOrder(unitOrderPair.Key, unitOrderPair.Value, insertAtFront);
        }
    }

    public void SetOrders(Dictionary<RTSGameObject, Order> orders)
    {
        foreach (KeyValuePair<RTSGameObject, Order> unitOrderPair in orders)
        {
            SetOrder(unitOrderPair.Key, unitOrderPair.Value);
        }
    }

    public void QueueOrders(Dictionary<RTSGameObject, List<Order>> orders)
    {
        foreach (KeyValuePair<RTSGameObject, List<Order>> unitOrderListPair in orders)
        {
            foreach (Order order in unitOrderListPair.Value)
            {
                QueueOrder(unitOrderListPair.Key, order);
            }
        }
    }

    public void CancelOrders(RTSGameObject unit)
    {
        if (orders.ContainsKey(unit) && orders[unit].Count > 0)
        {
            foreach (Order order in orders[unit])
            {
                order.OnCancel(unit, gameManager);
            }
            orders[unit].Clear();
        }
    }
}
