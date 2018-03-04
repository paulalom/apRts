using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLauncher : MonoBehaviour {

    public TransportManager transportManager;
    public NetworkStateManager netStateManager;
    public bool isServer;
    
    public void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(loadScene());
    }

    IEnumerator loadScene()
    {
        yield return null;
        // ew, hack to cleanup extra loading screen canvas
        Destroy(GameObject.Find("LoadingScreenCanvas"));
        if (isServer)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
    }

    public void StartServer()
    {
        transportManager.StartServer();
        transportManager.OnClientConnected.AddListener(netStateManager.OnClientConnected);
        transportManager.OnClientDisconnected.AddListener(netStateManager.OnClientDisconnected);
        new SceneLoader().LoadSceneNum(4);
    }

    public void StartClient()
    {
        transportManager.StartClient();
        new SceneLoader().LoadSceneNum(4);
    }

    public void StartSinglePlayer()
    {
        Destroy(transportManager);
        Destroy(netStateManager);
        new SceneLoader().LoadSceneNum(4);
    }
}
