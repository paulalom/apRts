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

                if (order.type == OrderType.Follow)
                {
                    if (!lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                    }
                }
                else if (order.type == OrderType.Give)
                {
                    if (lazyWithinDist(unit.transform.position, order.target.transform.position, order.orderRange))
                    {
                        GiveItem(unit, order.target, order.item);
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
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
                            MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                        }
                        else
                        {
                            // If the unit moves out of range, remove the guard order
                        }
                    }
                }
                else if (order.type == OrderType.Harvest)
                {
                    if (lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        Harvest(unit, (ResourceDeposit)order.target);
                    }
                    else
                    {
                        // this order isnt invalid for things that cant move. Harvesting stations can't move, but this might be an action workers can take in the future
                        if (unit.GetComponent<Mover>() != null)
                        {
                            MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
                        }
                    }
                }
                else if (order.type == OrderType.HoldPosition)
                {
                    // Unnecessary for now, but later!
                }
                else if (order.type == OrderType.Move)
                {
                    if (lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
                    }
                }
                else if (order.type == OrderType.Patrol)
                {
                    if (lazyWithinDist(unit.transform.position, order.targetPosition, order.orderRange))
                    {
                        Vector3 tempVariablesMakeThingsEasierToUnderstand = order.targetPosition;
                        order.targetPosition = order.orderIssuedPosition;
                        order.orderIssuedPosition = tempVariablesMakeThingsEasierToUnderstand;
                        SetOrder(unit, order);
                    }
                    else
                    {
                        MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
                    }
                }
                else if (order.type == OrderType.Stop)
                {
                    completedOrders.Add(unit);
                    // Is this really an order? Setting to stop clears the queue. What does it mean to queue a stop order?
                }
                else if (order.type == OrderType.Take)
                {
                    if (lazyWithinDist(unit.transform.position, order.target.transform.position, order.orderRange))
                    {
                        TakeItem(unit, order.target, order.item);
                        completedOrders.Add(unit);
                    }
                    else
                    {
                        MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
                    }
                }
                else if (order.type == OrderType.UseAbillity)
                {

                }
            }
        }

        // Need to make this better. Specific orders need to be removed, 
        // not just the first one (if someone queues an order as one finishes, there will be problems)
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

    public bool Harvest(RTSGameObject taker, ResourceDeposit target)
    {
        Harvester harvester = taker.GetComponent<Harvester>();
        Producer producer = taker.GetComponent<Producer>();
        if (target == null || harvester == null)
        {
            return false; // some weird joojoo here
        }
        harvester.harvestTarget = target;
        harvester.IsActive = true;
        return true;
    }

    void TakeItem(RTSGameObject taker, RTSGameObject target, MyKVP<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage takerStorage = taker.GetComponent<Storage>();
        int taken = targetStorage.TakeItem(item.Key, item.Value, false);
        takerStorage.AddItem(item.Key, taken);
    }

    void GiveItem(RTSGameObject giver, RTSGameObject target, MyKVP<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage giverStorage = giver.GetComponent<Storage>();
        int given = giverStorage.TakeItem(item.Key, item.Value, false);
        targetStorage.AddItem(item.Key, given);
    }

    void MoveUnit(RTSGameObject unit, Vector2 targetPos)
    {
        Vector2 newPos = Vector2.MoveTowards(new Vector2(unit.transform.position.x, unit.transform.position.z), targetPos, moveSpeed);
        unit.transform.position = new Vector3(newPos.x, unit.transform.position.y, newPos.y);
    }

    bool lazyWithinDist(Vector3 o1, Vector3 o2, float dist)
    {
        return Math.Abs(o1.x - o2.x) < dist && Math.Abs(o1.z - o2.z) < dist;
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
            if (!CheckTargetExists(order.target) || !CheckHasComponent<Storage>(order.target) || !CheckHasComponent<Storage>(unit) || !CheckHasComponent<Harvester>(unit))
            {
                errorMessage = "Can't Harvest!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }
        // For give and take, there must be a target, and either it or the unit must be able to move.
        if (order.type == OrderType.Give)
        {
            if (!CheckTargetExists(order.target) || (!CheckCanMove(unit) && !CheckCanMove(order.target)) || !CheckHasComponent<Storage>(order.target) || !CheckHasComponent<Storage>(unit) || !CheckHasComponent<Transporter>(unit))
            {
                errorMessage = "Cant Give!";
                gameManager.CreateText(errorMessage, unit.transform.position);
                return false;
            }
        }
        if (order.type == OrderType.Take)
        {
            if (!CheckTargetExists(order.target) || (!CheckCanMove(unit) && !CheckCanMove(order.target)) || !CheckHasComponent<Storage>(order.target) || !CheckHasComponent<Storage>(unit) || !CheckHasComponent<Transporter>(unit))
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

        return true;
    }

    private bool CheckHasComponent<T>(RTSGameObject unit)
    {
        return unit.GetComponent<T>() != null;
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
