using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public enum UnitType
{
    Unit,
    Structure,
    Resource
}

[System.Serializable]
public class RTSGameObject : MonoBehaviour
{
    public bool selected = false;
    public Renderer flagRenderer; // the part of the object which contains the flag
    protected GameManager gameManager;
    protected OrderManager orderManager;
    protected RTSGameObjectManager rtsGameObjectManager;
    public GameObject graphicObject; // should this be a thing?
    public Storage storage; // SHOULD ONLY BE ACCESSED THROUGH rtsGameObjectManager.GetStorage?
    public UnitType unitType;
    public Ability defaultAbility;
    public int ownerId;
    public int kills = 0;

    void Awake()
    {
        storage = GetComponent<Storage>();
        flagRenderer = GetComponent<Renderer>(); // just get any part of the object
        unitType = UnitType.Unit;
    }
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
    }

    void Update()
    {
        
    }

    // Temporary solution to prevent units from entering buildings until pathing is set up
    void OnTriggerEnter(Collider other)
    {
        if (GetComponent<Mover>() != null && rtsGameObjectManager != null)
        {
            if (orderManager.orders.ContainsKey(this) && orderManager.orders[this].Count > 0)
            {
                // Give and take could potentially be an event driven system like this, 
                // Move towards could be simplified (it wont need to check if we're in range, this trigger will do that)
                Order order = orderManager.orders[this][0];
                if (order.type == OrderType.Give)
                {
                    rtsGameObjectManager.GiveItem(this, order.target, order.item);
                }
                else if (order.type == OrderType.Take)
                {
                    rtsGameObjectManager.TakeItem(this, order.target, order.item);
                }
                orderManager.orders[this].Clear();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        Mover mover = GetComponent<Mover>();
        if (mover != null && rtsGameObjectManager != null)
        {
            Vector3 targetPos = transform.position + (transform.position - other.transform.position) * 1000;
            rtsGameObjectManager.MoveUnit(this, new Vector2(targetPos.x, targetPos.z), mover.moveSpeed * 2);
        }
    }
}