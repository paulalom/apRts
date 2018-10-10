using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class Shield : Defense, IActivatable
{
    public ShieldGenerator generator;
    new Collider collider;
    new Renderer renderer;
    float shieldAlpha;

    public override void MyAwake()
    {
        collider = GetComponentsInChildren<Collider>().Where(x => x.name == "Shield").FirstOrDefault();
        renderer = GetComponentsInChildren<Renderer>().Where(x => x.name == "Shield").FirstOrDefault();
        shieldAlpha = renderer.material.color.a;
    }
    
    public void PowerShield(float powerRatio)
    {
        absorptionRatio = powerRatio;
        AdjustShieldAlpha(powerRatio);
        currentHitPoints = powerRatio * maxHitPoints;
        if (powerRatio == 0)
        {
            Deactivate();
        }
    }
    
    public override void TakeDamage(float amount)
    {
        generator.Generate(amount * generator.damageToEnergyBurnRatio);
    }

    public void Activate()
    {
        collider.enabled = true;
        renderer.enabled = true;
        isActive = true;
    }

    public void Deactivate()
    {
        collider.enabled = false;
        renderer.enabled = false;
        isActive = false;
    }

    private void AdjustShieldAlpha(float powerRatio)
    {
        Color shieldColor = renderer.material.color;
        shieldColor.a = shieldAlpha * powerRatio;
        renderer.material.color = shieldColor;
    }
}
