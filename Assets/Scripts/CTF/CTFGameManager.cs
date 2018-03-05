using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFGameManager : MonoBehaviour {

    UIManager uiManager;
    private float realTimeSinceLastStep;


    // Use this for initialization
    void Start () {
        StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        yield return SetUpWorld(null, null);
        //yield return uiManager.InitUI(null, null, null);
        yield return MainGameLoop();
    }

    public IEnumerator SetUpWorld(TerrainManager terrainManager, RTSCamera mainCamera)
    {
        //SetUpPlayer(i, playerManager.activeWorld);
        yield return null;
        LoadingScreenManager.SetLoadingProgress(.99f);
        LoadingScreenManager.CompleteLoadingScreen();
    }

    IEnumerator MainGameLoop()
    {
        while (true)
        {
            realTimeSinceLastStep += Time.deltaTime;
            float stepDt = StepManager.GetDeltaStep();

            while (stepDt < realTimeSinceLastStep)
            {
                StepGame(stepDt);
                realTimeSinceLastStep -= stepDt;
                StepManager.Step();
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }
    }

    void StepGame(float stepDt)
    {

    }

    void SetUpPlayer()
    {

    }

    // Update is called once per frame
    void Update () {
		
	}
}
