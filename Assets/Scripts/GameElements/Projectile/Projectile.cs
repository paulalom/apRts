using UnityEngine;
using System.Collections;

// Eventually these probably shouldn't inherit from RTSGameObject
[RequireComponent(typeof(Mover))]
public class Projectile : RTSGameObject {

    Mover mover;
    public RTSGameObject parent;

	// Use this for initialization
	void Start () {
        mover = GetComponent<Mover>();
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
