using UnityEngine;
using System.Collections;

// Step size is currently 1 second
public class StepManager : MyMonoBehaviour {
    
    // Fixme: need to adjust order system and whatnot to scale movement etc with fixedTimeStep
    // Movement should be movement per gameSecond, changing the step size should not change movement speed
    public const float fixedStepTimeSize = .03f; // 30ms per frame
    public const int numStepsToDelayInputProcessing = 5;
    public static long gameStep = 0;
    public static long CurrentStep { get { return gameStep; }
        set // This should only be called when the game needs to sync state with the server
        { gameStep = value; } }

    public static float GetDeltaStep()
    {
        return fixedStepTimeSize;
    }

    public static float GetEndOfFrameDelay()
    {
        return 0;
    }

    public static void Step()
    {
        gameStep++;
    }
}
