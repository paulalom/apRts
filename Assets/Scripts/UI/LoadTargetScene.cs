using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MyMonoBehaviour
{
    public void LoadSceneNum(int num)
    {
        if (num < 0 || num >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("Cant Load scene num " + num + ". Scene Manager only has " + SceneManager.sceneCountInBuildSettings + " scenes in build settings!");
            return;
        }

        LoadingScreenManager.LoadScene(num);
    }
}
