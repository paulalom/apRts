﻿using UnityEngine;
using System.Collections;

public class Cannon : AttackSubsystem {

    // Use this for initialization
    void Start () {
        attack = GetComponent<Shoot>();
        damageType = DamageType.explosive;
        attackType = AttackType.projectile;
        powerDraw = 100;
        basedamage = 15;
	}
}
