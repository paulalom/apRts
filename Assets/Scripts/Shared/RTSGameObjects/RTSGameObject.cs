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
    public long unitId;
    public int ownerId;
    public int kills = 0;
    public float flyHeight = 0;
    float lastIdleTime;
    float updateIdleInterval = 5000;
    bool _isIdle = false;
    public bool IsIdle { get { return _isIdle; } set
        {
            if (value != _isIdle)
            {
                _isIdle = value;
                onIdleStatusChange.Invoke(this, _isIdle);
            }
        }
    }
    
    public class OnIdleStatusChangeEvent : UnityEvent<RTSGameObject, bool> { }
    public OnIdleStatusChangeEvent onIdleStatusChange = new OnIdleStatusChangeEvent();

    public class OnUnitChangeEvent : UnityEvent { }
    public OnUnitChangeEvent onDestroyed = new OnUnitChangeEvent();
    
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
        lastIdleTime = StepManager.gameTime;
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
        if (IsIdle && lastIdleTime + updateIdleInterval < StepManager.gameTime)
        {
            onIdleStatusChange.Invoke(this, IsIdle);
            lastIdleTime = StepManager.gameTime;
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

    void OnTriggerEnter(Collider other)
    {
        RTSGameObject otherRtsGo = other.GetComponent<RTSGameObject>();
        if (mover != null && rtsGameObjectManager != null && otherRtsGo != null)
        {
            orderManager.CheckOrderCompletionOnCollision(this, otherRtsGo);
        }
    }

    /* // cant use this because unity collision detection is nondeterministic... will need to implement my own
    void OnTriggerStay(Collider other)
    {
        if (mover != null && rtsGameObjectManager != null
            && other.transform.GetComponent<RTSGameObject>() != null // Don't collide with our own shields/child colliders
            && !(other.GetComponent<RTSGameObject>() is Projectile)
            && other.gameObject.name != "Shield")
        {
            Mover otherMover = other.GetComponent<Mover>();
            Vector2 dpos = new Vector2(transform.position.x, transform.position.z)
                - new Vector2(other.transform.position.x, other.transform.position.z);

            Vector2 size = (new Vector2(transform.localScale.x, transform.localScale.z)
                + new Vector2(other.transform.localScale.x, other.transform.localScale.z)) / 2;

            float distToMove = size.x * (otherMover != null ? 0.5f : 1);
            if (dpos.sqrMagnitude == 0) { dpos = new Vector3(0.1f, 0, 0); }
            Vector2 newDPos = distToMove * dpos.normalized;
            Vector3 newDPos3 = new Vector3(newDPos.x, 0, newDPos.y);
            Vector3 targetPos = other.transform.position + newDPos3;

            Debug.Log(name + " collided");
            transform.position = targetPos;
            if (otherMover != null)
            {
                other.gameObject.transform.position -= newDPos3;
            }
            //rtsGameObjectManager.SetUnitMoveTarget(this, new Vector2(targetPos.x, targetPos.z), StepManager.fixedStepTimeSize);
            //transform.position += mover.velocity;
            //mover.SetVelocity2D(Vector2.zero);
        }
    }
    */
}