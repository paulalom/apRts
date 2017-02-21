using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {

    public Dictionary<RTSGameObject, List<Order>> orders;
    public float moveSpeed = 0.3f; // this wont be here later
    List<RTSGameObject> completedOrders;
    RTSGameObjectManager rtsGameObjectManager;

    void Awake()
    {
        orders = new Dictionary<RTSGameObject, List<Order>>();
        completedOrders = new List<RTSGameObject>(); //max one order completion per frame
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
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
                        MoveUnit(unit, new Vector2(order.target.transform.position.x, order.target.transform.position.z));
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
                        MoveUnit(unit, new Vector2(order.targetPosition.x, order.targetPosition.z));
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

        int levelQuantityMultiplier = 1; // todo
        float harvesterLevel = harvester.harvesterLevel;
        
        Dictionary<Type, int> resourcesToCollect = new Dictionary<Type, int>();
        if (target.type == DepositType.Coal)
        {
            resourcesToCollect.Add(typeof(Coal), (int)(producer.productionQuantity[typeof(Coal)] * harvesterLevel * levelQuantityMultiplier));
            resourcesToCollect.Add(typeof(Stone), (int)(producer.productionQuantity[typeof(Stone)] * harvesterLevel * levelQuantityMultiplier));
        }
        else if (target.type == DepositType.Forest)
        {
            resourcesToCollect.Add(typeof(Wood), (int)(producer.productionQuantity[typeof(Wood)] * harvesterLevel * levelQuantityMultiplier));
        }
        else if (target.type == DepositType.Iron)
        {
            resourcesToCollect.Add(typeof(Iron), (int)(producer.productionQuantity[typeof(Iron)] * harvesterLevel * levelQuantityMultiplier));
            resourcesToCollect.Add(typeof(Stone), (int)(producer.productionQuantity[typeof(Stone)] * harvesterLevel * levelQuantityMultiplier));
        }
        return rtsGameObjectManager.TakeFromStorage(taker, target, resourcesToCollect);
        
    }

    void TakeItem(RTSGameObject taker, RTSGameObject target, MyKVP<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage takerStorage = taker.GetComponent<Storage>();
        if (targetStorage != null && takerStorage != null && taker.GetComponent<Transporter>() != null)
        {
            int taken = targetStorage.TakeItem(item.Key, item.Value, false);
            takerStorage.AddItem(item.Key, taken);
        }
    }

    void GiveItem(RTSGameObject giver, RTSGameObject target, MyKVP<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage giverStorage = giver.GetComponent<Storage>();
        if (targetStorage != null && giverStorage != null && giver.GetComponent<Transporter>() != null)
        {
            int given = giverStorage.TakeItem(item.Key, item.Value, false);
            targetStorage.AddItem(item.Key, given);
        }
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
        // there is no order or recipient for the order
        if (order == null || unit == null)
        {
            return false;
        }
        if (order.type == OrderType.Follow)
        {
            if (order.target == null)
            {
                return false;
            }
        }
        if (order.type == OrderType.Harvest)
        {
            if (order.target == null)
            {
                return false;
            }
        }
        if (order.type == OrderType.Give)
        {
            if (order.target == null)
            {
                return false;
            }
        }
        if (order.type == OrderType.Take)
        {
            if (order.target == null)
            {
                return false;
            }
        }

        return true;
        /* PENDING Type Heirarchy rewrite
        Mover mover =  unit.GetComponent<Mover>();
        Ability[] abilities = unit.GetComponents<Ability>();

        if (order.type)
        */
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
