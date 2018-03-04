using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLauncher : MonoBehaviour {

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
}
