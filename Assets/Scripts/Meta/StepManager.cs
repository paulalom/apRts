using UnityEngine;
using System.Collections;

// Step size is currently 1 second
public class StepManager : MonoBehaviour {
    
    public static float GetDeltaStep()
    {
        return Time.deltaTime;
    }
}
