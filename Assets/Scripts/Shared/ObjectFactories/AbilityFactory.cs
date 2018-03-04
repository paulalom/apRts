using System;
using UnityEngine;
using System.Collections;

public class AbilityFactory
{

    public static Ability GetAbilityFromString(string abilityType)
    {
        switch (abilityType)
        {
            case "Explode":
                return new Explode();
            case "Shoot":
                return new Shoot();
            default:
                throw new Exception("Ability type not found: " + abilityType);
        }
    }
}
