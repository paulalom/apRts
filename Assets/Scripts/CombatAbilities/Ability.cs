using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour {

    public RTSGameObject target;
    public Vector3 targetLocation;

    // Override me!
    public virtual bool UseAbillity()
    {
        Debug.Log("Attempting to use undefined ability");
        return false;
    }
}
