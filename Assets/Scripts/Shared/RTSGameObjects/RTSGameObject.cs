using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RTSGameObject : MyMonoBehaviour, IDamagable
{
    public bool selected = false;
    public Renderer flagRenderer;
    public SpriteRenderer selectionCircle;
    protected GameManager gameManager;
    protected OrderManager orderManager;
    protected RTSGameObjectManager rtsGameObjectManager;
    public GameObject graphicObject;
    public Storage storage;
    public Mover mover = null;
    public Ability defaultAbility;
    public RTSGameObject target = null;
    public World world;
    public Vector3 prevPositionForHeightMapCheck;
    public List<Defense> InOrderDefenses;
    public long uid;
    public int ownerId;
    public int kills = 0;
    public float flyHeight = 0;
    float lastIdleTime;
    float updateIdleInterval = 5;
    public bool idle = false;
    public bool Idle { get { return idle; } set
        {
            if (value != idle)
            {
                idle = value;
                onIdle.Invoke(this, value);
            }
        }
    }
    
    public class OnIdleEvent : UnityEvent<RTSGameObject, bool> { }
    public OnIdleEvent onIdle = new OnIdleEvent();
    
    public override void MyStart()
    {
        DefaultInit();
    }
    public Vector2 Position2D
    {
        get {
            var pos3d = transform.position;
            return new Vector2(pos3d.x, pos3d.z);
        }
    }

    protected void DefaultInit()
    {
        prevPositionForHeightMapCheck = transform.position;
        lastIdleTime = Time.time;
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        mover = GetComponent<Mover>();
        storage = GetComponent<Storage>();
    }

    void Destroy()
    {

    }

    public override void MyUpdate()
    {
        DefaultUpdate();
    }

    protected void DefaultUpdate()
    {
        if (idle && lastIdleTime + updateIdleInterval < Time.time)
        {
            onIdle.Invoke(this, idle);
            lastIdleTime = Time.time;
        }
    }

    public void TakeDamage(float amount)
    {
        foreach (Defense defense in InOrderDefenses)
        {
            float defenseAbsorptionAmount;
            if (defense.absorptionRatio < 1)
            {
                defenseAbsorptionAmount = amount * defense.absorptionRatio;
                defense.TakeDamage(defenseAbsorptionAmount);
                amount -= defenseAbsorptionAmount;
            }
            else
            {
                defense.TakeDamage(amount);
                break;
            }
        }
    }

    // Temporary solution to prevent units from entering buildings until pathing is set up
    void OnTriggerEnter(Collider other)
    {
        RTSGameObject otherRtsGo = other.GetComponent<RTSGameObject>();
        if (mover != null && rtsGameObjectManager != null && otherRtsGo != null)
        {
            orderManager.CheckOrderCompletionOnCollision(this, otherRtsGo);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (mover != null && rtsGameObjectManager != null && !(other.GetComponent<RTSGameObject>() is Projectile))// && other.GetComponent<Mover>() == null)
        {
            Vector3 targetPos = transform.position + (transform.position - other.transform.position) * 1000;
            rtsGameObjectManager.MoveUnit(this, new Vector2(targetPos.x, targetPos.z), mover.moveSpeed, StepManager.GetDeltaStep());
        }
    }
}