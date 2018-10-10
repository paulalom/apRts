using UnityEngine;
using System.Collections;

// Eventually these probably shouldn't inherit from RTSGameObject
[RequireComponent(typeof(Mover))]
public class Projectile : RTSGameObject {
    
    public RTSGameObject parent;
    
	public override void MyStart() {
        mover = GetComponent<Mover>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
    }
	
	public override void MyUpdate() {
	    
	}
}
