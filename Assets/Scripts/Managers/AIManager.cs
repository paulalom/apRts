using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class AIManager : MonoBehaviour {

    List <Plan> plans = new List<Plan>();
    RTSGameObjectManager rtsGameObjectManager;
    OrderManager orderManager;

    // Use this for initialization
    void Start () {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        rtsGameObjectManager.onUnitCreated.AddListener(SubscribeToIdleEvents);
    }
	
	// Update is called once per frame
	void Update () {
	    
	}

    void SubscribeToIdleEvents(RTSGameObject idleUnit)
    {
        idleUnit.onIdle.AddListener(SetNewPlanForUnit);
    }

    void SetNewPlanForUnit(RTSGameObject unit)
    {
        if (unit.GetType() == typeof(Worker))
        {
            CollectResources collectPlan = new CollectResources();
            foreach(Order order in collectPlan.GetPlanSteps(unit))
            {
                orderManager.QueueOrder(unit, order);
            }
        }
    }
}
