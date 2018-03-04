using UnityEngine;
using System.Collections;

public enum AttackType
{
    projectile
}

public enum DamageType
{
    explosive
}

public class AttackSubsystem : AbilitySubsystem {
    public Ability attack;
    public AttackType attackType;
    public DamageType damageType;
    public float basedamage;
}
