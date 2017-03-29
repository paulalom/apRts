using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class AIManager : MonoBehaviour {

    List <Plan> plans = new List<Plan>();
    RTSGameObjectManager rtsGameObjectManager;
    OrderManager orderManager;
    float lastTime;

    // Use this for initialization
    void Start () {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        rtsGameObjectManager.onUnitCreated.AddListener(SubscribeToIdleEvents);
        lastTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
	}

    void SubscribeToIdleEvents(RTSGameObject idleUnit)
    {
        idleUnit.onIdle.AddListener(OnIdleChangeEvent);
    }

    void OnIdleChangeEvent(RTSGameObject unit, bool idleStatus)
    {
        if (idleStatus)
        {
            if (!SetNewPlanForUnit(unit))
            {
               // idleUnits.Add(unit);
            }
        }
        else
        {
          //  idleUnits.Remove(unit);
        }
    }
   

    bool SetNewPlanForUnit(RTSGameObject unit)
    {
        if (unit.GetType() == typeof(Worker))
        {
            CollectResources collectPlan = new CollectResources();
            List<Order> planOrders = collectPlan.GetPlanSteps(unit);
            foreach (Order order in planOrders)
            {
                orderManager.QueueOrder(unit, order);
            }
            if (planOrders.Count == 0)
            {
                return false;
            }
        }
        return true;
    }
}
