using UnityEngine;
using System.Collections;
using RVO;
using System;
using System.Collections.Generic;

public class CollisionAvoidanceManager {
    Dictionary<RTSGameObject, int> gameIds = new Dictionary<RTSGameObject, int>();
    List<int> freedIds = new List<int>();

	// Use this for initialization
	public void MyStart() {

        /* Specify the global time step of the simulation. */
        Simulator.Instance.setTimeStep(StepManager.fixedStepTimeSize/1000f);

        /*
         * Specify the default parameters for agents that are subsequently
         * added.
         */
        Simulator.Instance.setAgentDefaults(30.0f, 6, 10.0f, 10.0f, 1.5f, 2.0f, new RVO.Vector2(0.0f, 0.0f));
    }

    private RVO.Vector2 RvoConv(UnityEngine.Vector2 vec)
    {
        return new RVO.Vector2(vec.x, vec.y);
    }

    private UnityEngine.Vector2 RvoConv(RVO.Vector2 vec)
    {
        return new UnityEngine.Vector2(vec.x(), vec.y());
    }

    // Update is called once per frame
    public void SyncObjectState(RTSGameObject obj, float dt)
    {
        Mover m = obj.GetComponent<Mover>();
        if (m == null || obj is Projectile)
        {
            return;
        }
        if (!gameIds.ContainsKey(obj)) {
            StartObject(obj);
        }
        int id = gameIds[obj];
        Simulator.Instance.setAgentMaxSpeed(id, m.moveSpeed * dt);
        Simulator.Instance.setAgentPrefVelocity(id, RvoConv(m.Velocity2D));
        Simulator.Instance.setAgentVelocity(id, RvoConv(m.Velocity2D));
        float objectRadius = obj.transform.localScale.magnitude/2;        
        Simulator.Instance.setAgentRadius(id, objectRadius);
        Simulator.Instance.setAgentPosition(id, RvoConv(obj.Position2D));
    }

    private void UpdateObjectVelocity(RTSGameObject obj, int id)
    {
        Mover m = obj.GetComponent<Mover>();
        if (m == null)
        {
            return;
        }
        UnityEngine.Vector2 velocity = RvoConv(Simulator.Instance.getAgentVelocity(id));
        m.SetVelocity2D(velocity);
    }

    private void StartObject(RTSGameObject obj)
    {
        int id;
        if (freedIds.Count > 0)
        {
            id = freedIds[freedIds.Count - 1];
           freedIds.RemoveAt(freedIds.Count - 1);
        } else
        {
           id = Simulator.Instance.addAgent(RvoConv(obj.Position2D));
        }
        gameIds[obj] = id;
    }

    public void FreeObject(RTSGameObject obj)
    {
        if (gameIds.ContainsKey(obj))
        {
            int id = gameIds[obj];
            freedIds.Add(id);
            gameIds.Remove(obj);
        }
    }

    public void Update()
    {
        // Make sure freed objects are out of sight
        foreach (int idx in freedIds)
        {
            Simulator.Instance.setAgentPosition(idx, new RVO.Vector2(float.MaxValue, float.MaxValue));
            Simulator.Instance.setAgentMaxSpeed(idx, 0);
        }
        Simulator.Instance.doStep();
        foreach (KeyValuePair<RTSGameObject, int> obj in gameIds)
        {
            UpdateObjectVelocity(obj.Key, obj.Value);
        }
    }
}
