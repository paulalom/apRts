using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLauncher : MonoBehaviour {

    public TransportManager transportManager;
    public NetworkStateManager netStateManager;
    
    public void Start()
    {
        Application.runInBackground = true;
    }

    public void StartServer()
    {
        transportManager.StartServer();
        transportManager.OnClientConnected.AddListener(netStateManager.OnClientConnected);
        transportManager.OnClientDisconnected.AddListener(netStateManager.OnClientDisconnected);
        new SceneLoader().LoadSceneNum(1);
    }

    public void StartClient()
    {
        transportManager.StartClient();
        new SceneLoader().LoadSceneNum(1);
    }

    public void StartSinglePlayer()
    {
        Destroy(transportManager);
        Destroy(netStateManager);
        new SceneLoader().LoadSceneNum(1);
    }
}
