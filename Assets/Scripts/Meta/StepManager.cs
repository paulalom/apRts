using UnityEngine;
using System.Collections;

// Step size is currently 1 second
public class StepManager : MyMonoBehaviour {

    // All game time related features are in "game time". 
    // For example, movement speed of 30 is 30 distance units per game second.
    // Real time will not always sync up with game time.
    // For example, if gameTimePerStep is 10ms, then 10 steps is 100ms game time,
    // but we may actually compute 8 steps per 100ms real time, making the game appear to run slower.
    // The game time per step should be at a realistic value, 
    // so that most of the time we run faster and cap the simulation to real time.
    // this ensures smooth gameplay most of the time.
    public const float gameTimeInSecondsPerStep = .02f; // 20ms per frame

    public static float GetDeltaStep()
    {
        return Time.deltaTime;//gameTimeInSecondsPerStep;
    }

    public static float GetEndOfFrameDelay()
    {
        return 0;
    }
}
