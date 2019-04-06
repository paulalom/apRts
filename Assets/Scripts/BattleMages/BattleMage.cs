using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Transporter))]
[RequireComponent(typeof(FlowSystem))]
[RequireComponent(typeof(Cannon))]
public class BattleMage : RTSGameObject
{
    public Consumer consumer;
    public FlowSystem power;
    Worker worker;

    public override void MyAwake()
    {
        consumer = GetComponent<Consumer>();
        power = GetComponent<FlowSystem>();

        defaultAbility = GetComponent<Shoot>();
        ((Shoot)defaultAbility).projectileType = typeof(BasicCannonProjectile);
    }

    public override void MyStart()
    {
        DefaultInit();
    }
}