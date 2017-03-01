using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(FlowSystem))]
public class Tank : RTSGameObject {
    
    public Mover mover;
    public Consumer consumer;
    public FlowSystem power;

    void Start()
    {
        mover = GetComponent<Mover>();
        storage = GetComponent<Storage>();
        consumer = GetComponent<Consumer>();
        power = GetComponent<FlowSystem>();
        
        // No fuel yet, just use coal
        consumer.operationCosts.Add(typeof(Coal), 1);
        consumer.operationInterval = 1f;
    }
}
