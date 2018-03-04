using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Transporter))]
[RequireComponent(typeof(Producer))]
[RequireComponent(typeof(FlowSystem))]
[RequireComponent(typeof(Cannon))]
[RequireComponent(typeof(Worker))]
public class Commander : RTSGameObject {

    public Producer producer;
    public Consumer consumer;
    public FlowSystem power;
    Worker worker;

    public override void MyAwake()
    {
        producer = GetComponent<Producer>();
        consumer = GetComponent<Consumer>();
        power = GetComponent<FlowSystem>();
        worker = GetComponent<Worker>();

        defaultAbility = GetComponent<Shoot>();
        ((Shoot)defaultAbility).projectileType = typeof(BasicCannonProjectile);
    }

    public override void MyStart()
    {
        DefaultInit();
        worker.SetDefaultProductionSettings(producer);
        worker.SetDefaultStorageSettings(storage);
    }
}
