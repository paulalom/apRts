using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Producer))]
[RequireComponent(typeof(Storage))]
[RequireComponent(typeof(Worker))]
public class ConstructionSphere : RTSGameObject {

    public Producer producer;
    Worker worker;

    void Awake()
    {
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        mover = GetComponent<Mover>();
        producer = GetComponent<Producer>();
        storage = GetComponent<Storage>();
        worker = GetComponent<Worker>();
        
        worker.SetDefaultProductionSettings(producer);
        worker.SetDefaultStorageSettings(storage);
    }
}
