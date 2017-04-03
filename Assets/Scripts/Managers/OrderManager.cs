using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {

    public Dictionary<RTSGameObject, List<Order>> orders;
    public float moveSpeed = 0.3f; // this wont be here later
    List<RTSGameObject> completedOrders;
    RTSGameObjectManager rtsGameObjectManager;
    GameManager gameManager;

    void Awake()
    {
        orders = new Dictionary<RTSGameObject, List<Order>>();
        completedOrders = new List<RTSGameObject>(); //max one order completion per frame
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void CarryOutOrders(List<RTSGameObject> units)
    {
        /*
        MoveUnits(units);
        TakeItems(units);
        GiveItems(units);
        etc..
        */
        foreach (RTSGameObject unit in units)
        {
            if (orders.ContainsKey(unit) && orders[unit].Count > 0)
            {
                Order order = orders[unit][0];

                if (order.type == OrderType.Construct)
                {
                    Producer producer = unit.GetComponent<Producer>();
                    if (producer != null)
                    {
                        producer.TryQueueItem(order.item.Key, order.item.Value);
                    } 
                    completedOrders.Add(unit);
                }
                else if (order.type == OrderType.Follow)
                {
                    if (!rtsGameObjectManager.lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        rtsGameObjectManager.MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                    }
                }
                else if (order.type == OrderType.Give)
                {
                    if (rtsGameObjectManager.lazyWithinDist(unit.transform.position, order.target.transform.position, order.orderRange))
                    {
                        rtsGameObjectManager.GiveItem(unit, order.target, order.item);
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        rtsGameObjectManager.MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                    }
                }
                else if (order.type == OrderType.Guard)
                {
                    if (false) // There is a unit threatening the target
                    {
                        // engage!
                    }
                    else // Follow
                    {
                        // this order isnt invalid for things that cant move. We may want defensive structures to prioritize the defense of a certain unit
                        if (unit.GetComponent<Mover>() != null)
                        {
                            rtsGameObjectManager.MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                        }
                        else
                        {
                            // If the unit moves out of range, remove the guard order
                        }
                    }
                }
                else if (order.type == OrderType.Harvest)
                {
                    if (rtsGameObjectManager.lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        rtsGameObjectManager.Harvest(unit, (ResourceDeposit)order.target);
                    }
                    else
                    {
                        // this order isnt invalid for things that cant move. Harvesting stations can't move, but this might be an action workers can take in the future
                        if (unit.GetComponent<Mover>() != null)
                        {
                            rtsGameObjectManager.MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
                        }
                    }
                }
                else if (order.type == OrderType.HoldPosition)
                {
                    // Unnecessary for now, but later!
                }
                else if (order.type == OrderType.Move)
                {
                    if (rtsGameObjectManager.lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        rtsGameObjectManager.MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
                    }
                }
                else if (order.type == OrderType.Patrol)
                {
                    if (rtsGameObjectManager.lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        Vector3 tempVariablesMakeThingsEasierToUnderstand = order.targetPosition;
                        order.targetPosition = order.orderIssuedPosition;
                        order.orderIssuedPosition = tempVariablesMakeThingsEasierToUnderstand;
                        SetOrder(unit, order);
                    }
                    else
                    {
                        rtsGameObjectManager.MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
                    }
                }
                else if (order.type == OrderType.Stop)
                {
                    completedOrders.Add(unit);
                    // Is this really an order? Setting to stop clears the queue. What does it mean to queue a stop order?
                }
                else if (order.type == OrderType.Take)
                {
                    if (rtsGameObjectManager.lazyWithinDist(unit.transform.position, order.target.transform.position, order.orderRange))
                    {
                        rtsGameObjectManager.TakeItem(unit, order.target, order.item);
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        rtsGameObjectManager.MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                    }
                }
                else if (order.type == OrderType.UseAbillity)
                {
                    Vector3 targetPos = order.target == null ? order.targetPosition : order.target.transform.position;
                    if (rtsGameObjectManager.lazyWithinDist(unit.transform.position, targetPos, order.orderRange))
                    {
                        rtsGameObjectManager.UseAbility(unit, order.target, order.targetPosition, order.ability);
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        rtsGameObjectManager.MoveUnit(unit, new Vector2(targetPos.x, targetPos.z));
                    }
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
            unitOrders.RemoveAt(0);
            if (unitOrders.Count == 0)
            {
                orders.Remove(completer);
                completer.Idle = true;
            }
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

    
    /*
        bool lazyWithinDist(Vector2 o1, Vector2 o2, float dist)
        {
            return Math.Abs(o1.x - o2.x) < dist && Math.Abs(o1.y - o2.y) < dist;
        }*/

    bool ValidateOrder(RTSGameObject unit, Order order)
    {
        string errorMessage = "";
        // there is no order or recipient for the order
        if (order == null || unit == null)
        {
            errorMessage = "No order or Unit";
            gameManager.CreateText(errorMessage, unit.transform.position);
            return false;
        }
        if (order.type == OrderType.Move)
        {
            if (!CheckCanMove(unit))
            {
                errorMessage = "Can't Move!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }
        if (order.type == OrderType.Follow)
        {
            if (!CheckTargetExists(order.target) || !CheckCanMove(unit))
            {
                errorMessage = "Can't follow!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }
        if (order.type == OrderType.Harvest)
        {
            if (!ValidateStorageAccess(unit, order.target))
            {
                errorMessage = "Can't Harvest!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }
        // For give and take, there must be a target, and either it or the unit must be able to move.
        if (order.type == OrderType.Give)
        {
            if (!ValidateStorageAccess(unit, order.target))
            {
                errorMessage = "Can't Give!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }
        if (order.type == OrderType.Take)
        {
            if (!ValidateStorageAccess(unit, order.target))
            {
                errorMessage = "Can't Take!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }

        if (order.type == OrderType.Take || order.type == OrderType.Give || order.type == OrderType.Harvest || order.type == OrderType.Follow)
        {
            if (unit == order.target)
            {
                errorMessage = "Can't " + order.type.ToString() + " self!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }

        if (order.type == OrderType.UseAbillity)
        {
            if (!ValidateAbilities(unit, order))
            {
                return false;
            }
        }

        return true;
    }

    private bool ValidateAbilities(RTSGameObject unit, Order order)
    {
        bool hasAbility = false;
        foreach (Ability ability in unit.GetComponents<Ability>())
        {
            if (order.ability.GetType() == ability.GetType())
            {
                hasAbility = true;
                break;
            }
        }
        if (!hasAbility)
        {
            return false;
        }
        
        return true;
    }

    private bool ValidateStorageAccess(RTSGameObject accessor, RTSGameObject target)
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

    private bool CheckHasComponent<T>(RTSGameObject unit)
    {
        return unit.GetComponent<T>() != null;
    }

    private bool CheckHasComponent(RTSGameObject unit, Type type)
    {
        return unit.GetComponent(type) != null;
    }

    private bool CheckTargetExists(RTSGameObject target)
    {
            return target != null;
    }

    private bool CheckCanMove(RTSGameObject unit)
    {
        return (unit.GetComponent<Mover>() != null);
    }

    public bool SetOrder(RTSGameObject unit, Order order, bool validateOrder = true)
    {
        if (orders.ContainsKey(unit))
        {
            orders[unit].Clear();
        }
        return QueueOrder(unit, order, validateOrder);
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
                orders[unit].Add(new Order(order)); // clone because reference stuff is referential?
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            orders[unit].Add(new Order(order));
            return true;
        }
    }
}
