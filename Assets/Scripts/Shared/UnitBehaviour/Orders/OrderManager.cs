using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OrderManager : MyMonoBehaviour {
    
    public Dictionary<RTSGameObject, List<Order>> orders;
    List<RTSGameObject> completedOrders;
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

                switch (order.orderData.phase)
                {
                    case OrderPhase.GetInRange:
                        order.orderData.phase += (order.GetInRange(unit, rtsGameObjectManager, dt) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Activate:
                        order.orderData.phase += (order.Activate(unit, rtsGameObjectManager) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Channel:
                        order.orderData.phase += (order.Channel(unit, rtsGameObjectManager, dt) == true) ? 1 : 0;
                        break;
                    case OrderPhase.FinishChannel:
                        order.orderData.phase += (order.FinishChannel(unit, rtsGameObjectManager) == true) ? 1 : 0;
                        break;
                    default:
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
        orders[unit].RemoveAt(0);
        if (orders[unit].Count == 0)
        {
            orders.Remove(unit);
            unit.Idle = true;
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
                QueueOrder(unit, order, false);
                return true;
            }
            else {
                return false;
            }
        }
        else
        {
            CancelOrders(unit);
            QueueOrder(unit, order, false);
            return true;
        }
    }

    public bool QueueOrder(RTSGameObject unit, Order order, bool validateOrder = true)
    {
        if (!orders.ContainsKey(unit))
        {
            orders.Add(unit, new List<Order>());
            unit.Idle = false;
        }
        if (validateOrder)
        {
            if (ValidateOrder(unit, order))
            {
                orders[unit].Add(order);
                order.OnQueue(unit, rtsGameObjectManager);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            orders[unit].Add(order);
            order.OnQueue(unit, rtsGameObjectManager);
            return true;
        }
    }

    public void QueueOrders(Dictionary<RTSGameObject, Order> orders)
    {
        foreach (KeyValuePair<RTSGameObject, Order> unitOrderPair in orders)
        {
            QueueOrder(unitOrderPair.Key, unitOrderPair.Value);
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
                order.OnCancel(unit, gameManager, rtsGameObjectManager);
            }
            orders[unit].Clear();
        }
    }
}
