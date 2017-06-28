using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLauncher : MyMonoBehaviour {

    NetworkClient myClient;

    public void StartServer()
    {
        NetworkServer.Listen(4444);

        new SceneLoader().LoadSceneNum(1);
        //NetworkServer.SendToAll();
    }

    public void StartClient()
    {
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.Connect("127.0.0.1", 4444);

        new SceneLoader().LoadSceneNum(1);

        //myClient.Send()
    }

    public void StartLocalClient()
    {
        myClient = ClientScene.ConnectLocalServer();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);

        new SceneLoader().LoadSceneNum(1);
    }

    // client function
    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }
}
