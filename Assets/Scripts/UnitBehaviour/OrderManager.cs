using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {

    public Dictionary<RTSGameObject, List<Order>> orders;
    public float moveSpeed = 0.3f; // this wont be here later
    List<RTSGameObject> completedOrders;
    RTSGameObjectManager rtsGameObjectManager;
    GameManager gameManager;
    UIManager uiManager;

    void Awake()
    {
        orders = new Dictionary<RTSGameObject, List<Order>>();
        completedOrders = new List<RTSGameObject>(); //max one order completion per frame
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }

    public void CarryOutOrders(List<RTSGameObject> units, float dt)
    {
        foreach (RTSGameObject unit in units)
        {
            if (orders.ContainsKey(unit) && orders[unit].Count > 0)
            {
                Order order = orders[unit][0];

                switch (order.phase)
                {
                    case OrderPhase.GetInRange:
                        order.phase += (order.GetInRange(unit, rtsGameObjectManager, dt) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Activate:
                        order.phase += (order.Activate(unit, rtsGameObjectManager) == true) ? 1 : 0;
                        break;
                    case OrderPhase.Channel:
                        order.phase += (order.Channel(unit, rtsGameObjectManager, dt) == true) ? 1 : 0;
                        break;
                    case OrderPhase.FinishChannel:
                        order.phase += (order.FinishChannel(unit, rtsGameObjectManager) == true) ? 1 : 0;
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
            if (completedOrder.repeatOnComplete)
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
            // Dequeue productions
            Producer producer = unit.GetComponent<Producer>();
            foreach (Order order in orders[unit])
            {
                if (order.GetType() == typeof(ConstructionOrder))
                {
                    Worker worker = unit.GetComponent<Worker>();
                    if (worker != null && worker.unitUnderConstruction != null)
                    {
                        ((Structure)worker.unitUnderConstruction).DemolishStructure("Construction cancelled!", gameManager, rtsGameObjectManager);
                        worker.unitUnderConstruction = null;
                    }
                    else {
                        producer.CancelProduction();
                    }
                }
            }
            orders[unit].Clear();
        }
    }

    public void CancelOrder(RTSGameObject unit, Order order)
    {

    }
}
