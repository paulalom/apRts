﻿using UnityEngine;
using System.Collections;

public enum AttackType
{
    projectile
}

public enum DamageType
{
    explosive
}

[RequireComponent(typeof(Ability))]
public class AttackSubSystem : AbilitySubsystem {
    public Ability attack;
    public AttackType attackType;
    public DamageType damageType;
    public float basedamage;
}