using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {

    public Dictionary<RTSGameObject, List<Order>> orders;
    public float moveSpeed = 0.3f;
    public OrderType nextOrderType;
    List<RTSGameObject> completedOrders;
    

    void Start()
    {
        orders = new Dictionary<RTSGameObject, List<Order>>();
        completedOrders = new List<RTSGameObject>(); //max one order completion per frame
        nextOrderType = OrderType.Move;
    }

    public void CarryOutOrders(List<RTSGameObject> units)
    {
        MoveUnits(units);

        foreach (RTSGameObject completer in completedOrders)
        {
            orders[completer].RemoveAt(0);
            if (orders[completer].Count == 0)
            {
                orders.Remove(completer);
            }
        }
        completedOrders.Clear();
    }

    public void SetNextOrderType(OrderType type)
    {
        nextOrderType = type;
    }

    public void ClearNextOrderType()
    {
        nextOrderType = OrderType.Move;
    }

    void MoveUnits(List<RTSGameObject> units)
    {
        Vector2 newPos;
        foreach (RTSGameObject unit in units)
        {
            if (orders.ContainsKey(unit))
            {
                foreach (Order order in orders[unit])
                {
                    newPos = Vector2.MoveTowards(new Vector2(unit.transform.position.x, unit.transform.position.z), new Vector2(order.targetPosition.x, order.targetPosition.z), moveSpeed);
                    /*if (Math.Abs(newPos.x - order.owner.transform.position.x) < 0.5f && Math.Abs(newPos.y - order.owner.transform.position.z) < 0.5f)
                    {
                        completedOrders.Add(order);
                    }*/
                    //else {
                    unit.transform.position = new Vector3(newPos.x, unit.transform.position.y, newPos.y);
                    //}
                }
            }
        }
    }

    bool ValidateOrder(RTSGameObject unit, Order order)
    {
        return true;
    }

    public bool SetOrder(RTSGameObject unit, Vector3 targetPosition, RTSGameObject target, Ability ability, bool validateOrder = true)
    {
        if (orders.ContainsKey(unit))
        {
            orders[unit].Clear();
        }
        return QueueOrder(unit, targetPosition, target, ability, validateOrder);
    }

    public bool QueueOrder(RTSGameObject unit, Vector3 targetPosition, RTSGameObject target, Ability ability, bool validateOrder = true)
    {
        Order order = new Order();
        order.type = nextOrderType;
        order.orderIssuedPosition = unit.transform.position;
        order.targetPosition = targetPosition;
        order.ability = ability;
        order.target = target;
        return QueueOrder(unit, order, validateOrder);
    }

    public bool QueueOrder(RTSGameObject unit, Order order, bool validateOrder = true)
    {
        if (!orders.ContainsKey(unit))
        {
            orders.Add(unit, new List<Order>());
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
}
