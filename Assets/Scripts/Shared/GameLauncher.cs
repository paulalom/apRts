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
        new SceneLoader().LoadSceneNum(3);
    }

    public void LaunchMainClient()
    {
        new SceneLoader().LoadSceneNum(2);
    }

    public void LaunchCTF()
    {
        new SceneLoader().LoadSceneNum(5);
    }

    public void LaunchSimCity()
    {
        new SceneLoader().LoadSceneNum(6);
    }
}
