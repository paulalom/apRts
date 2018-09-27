using UnityEngine;
using System.Collections;

public class StepManager : MyMonoBehaviour {
    
    // Fixme: need to adjust order system and whatnot to scale movement etc with fixedTimeStep
    // Movement should be movement per gameSecond, changing the step size should not change movement speed
    public const int fixedStepTimeSize = 25; // 25ms per frame, 40 fps
    public const int fixedStepTimeSizeX1000 = fixedStepTimeSize * 1000;
    public const int stepTimeSizeNumDigits = 3; // used for rounding
    public const int numStepsToDelayInputProcessing = 4;
    public static long gameStep = 0;
    public static long gameTime = 0;
    public static long CurrentStep { get { return gameStep; }
        set // This should only be called when the game needs to sync state with the server
        { gameStep = value; } }

    public static int GetDeltaStep()
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
        gameTime += fixedStepTimeSize;
    }
}
