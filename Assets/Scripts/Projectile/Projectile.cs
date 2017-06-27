using UnityEngine;
using System.Collections;

// Eventually these probably shouldn't inherit from RTSGameObject
[RequireComponent(typeof(Mover))]
public class Projectile : RTSGameObject {
    
    public RTSGameObject parent;
    public RTSGameObject rtsGo;
    
	void Start () {
        mover = GetComponent<Mover>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        rtsGo = GetComponent<RTSGameObject>();
    }
	
	void Update () {
	    
	}
}
