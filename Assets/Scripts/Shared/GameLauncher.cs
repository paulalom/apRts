using Assets.Scripts.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLauncher : MonoBehaviour {

    void Awake()
    {
        //Screen.SetResolution(800, 600, false);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    public void LaunchMainServer()
    {
        GlobalState.currentGameSceneNum = 2;
        GlobalState.isServer = true;
        new SceneLoader().LoadSceneNum(2);
    }

    public void LaunchMainClient()
    {
        GlobalState.currentGameSceneNum = 2;
        new SceneLoader().LoadSceneNum(2);
    }

    public void LaunchCTF()
    {
        GlobalState.currentGameSceneNum = 3;
        new SceneLoader().LoadSceneNum(3);
    }

    public void LaunchSimCity()
    {
        GlobalState.currentGameSceneNum = 4;
        new SceneLoader().LoadSceneNum(4);
    }
}
