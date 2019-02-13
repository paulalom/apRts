using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization;
using Assets.Scripts.Shared;

public class TransportManager : MonoBehaviour
{
    public string serverIpAddress;
    const int maxNumClients = 10;
    const int maxNumServers = 5;
    public int myReiliableChannelId;
    public int myUnreliableChannelId;

    int clientSocketId, serverSocketId;
    int serverSocketPort = 8888, clientSocketPort = 8889;
    int clientConnectionId;

    int messageReceivedConnectionId;
    int messageReceivedSocketId;
    int messageReceivedQOSChannel;
    byte[] messageReceivedBuffer;
    const int messageReceivedBufferLength = 1024;
    int messageReceivedDataLength;
    byte[] messageSendBuffer;
    const int messageSendBufferLength = 1024;
    int messageSendDataLength;
    int messageSendChannelId;
    byte error;
    public bool isConnected = false;
    //public int lifetimeNumPlayersConnected = 1; //includes server

    public List<int> connectedClients = new List<int>();
    ConnectionConfig config;
    HostTopology hostTopology;

    public class UnityEventInt : UnityEvent<int> { }
    public class UnityEventIntStr : UnityEvent<int,string> { }
    public UnityEventIntStr OnMessageReceived = new UnityEventIntStr();
    public UnityEventInt OnClientConnected = new UnityEventInt(); // exists only on server
    public UnityEventInt OnClientDisconnected = new UnityEventInt(); // exists only on server

    public void StartServer()
    {
        isConnected = true;
        
        config = new ConnectionConfig();
        myReiliableChannelId = config.AddChannel(QosType.ReliableSequenced);
        myUnreliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        hostTopology = new HostTopology(config, 10);
        serverSocketId = NetworkTransport.AddHost(hostTopology, serverSocketPort);

        // Connect to ourselves as a client
        clientConnectionId = NetworkTransport.Connect(serverSocketId, serverIpAddress, serverSocketPort, 0, out error);
        //serverConnectionId = NetworkTransport.Connect(socketId, serverIpAddress, socketPort, 0, out error);
    }

    public void StartClient()
    {
        clientSocketPort = Random.Range(9000, 65500);

        config = new ConnectionConfig();
        myReiliableChannelId = config.AddChannel(QosType.ReliableSequenced);
        myUnreliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        hostTopology = new HostTopology(config, 10);
        clientSocketId = NetworkTransport.AddHost(hostTopology, clientSocketPort);
        clientConnectionId = NetworkTransport.Connect(clientSocketId, serverIpAddress, serverSocketPort, 0, out error);
        isConnected = true;
    }

    void NetInit(int numConnections)
    {
        
    }

	void Start()
    {
        NetworkTransport.Init();
        if (GlobalState.isServer)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
        DontDestroyOnLoad(this);
    }
    
    void Update()
    {
        NetworkTransport.Init();
        ListenForSocketMessage();
    }

    void PrepareSocketMessage(string message, int channelId)
    {
        if (!isConnected)
        {
            Debug.Log("Not connected, cant send message!");
            return;
        }
        byte[] messageBytes = ConvertMessageFromStringToByte(message);
        LoadMessageIntoBuffer(messageBytes);
        messageSendChannelId = channelId;
    }

    /*public void SendSocketMessage(bool isServer, string message)
    {
        SendSocketMessage(isServer, message, myReiliableChannelId);
    }
    public void SendSocketMessage(bool isServer, string message, int channelId)
    {
        if (isServer)
        {
            SendSocketMessageToClients(message);
        }
        else
        {
            SendSocketMessageToServer(message);
        }
    }*/

    public void SendSocketMessageToClients(string message)
    {
        SendSocketMessageToClients(message, myReiliableChannelId);
    }
    public void SendSocketMessageToServer(string message)
    {
        SendSocketMessageToServer(message, myReiliableChannelId);
    }
    public void SendSocketMessageToClients(string message, int channelId)
    {
        PrepareSocketMessage(message, channelId);
        SendMessageInBufferToClients();
    }
    public void SendSocketMessageToServer(string message, int channelId)
    {
        PrepareSocketMessage(message, channelId);
        SendMessageInBufferToServer();
    }

    byte[] ConvertMessageFromStringToByte(string message)
    {
        byte[] byteMsg = new byte[messageSendBufferLength];
        using (MemoryStream stream = new MemoryStream(byteMsg))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
        }
        return byteMsg;
    }

    void LoadMessageIntoBuffer(byte[] message)
    {
        if (message.Length <= messageSendBufferLength)
        {
            messageSendBuffer = message;
        }
        else
        {
            Debug.Log("todo netmessage too big");
        }
    }

    // later we can improve this by limiting what info we send on messages which are command request responses
    void SendMessageInBufferToClients()
    {
        List<Thread> threads = new List<Thread>();
        foreach (int clientConnectionId in connectedClients)
        {
            NetworkTransport.Send(serverSocketId, clientConnectionId, messageSendChannelId, messageSendBuffer, messageSendBufferLength, out error);
            if (error != 0)
            {
                Debug.Log("Socket Send Error!: " + (NetworkError)error);
            }
        }
    }
    void SendMessageInBufferToServer()
    {
        Debug.Log("client sending to " + clientConnectionId);
        NetworkTransport.Send(clientSocketId, clientConnectionId, myReiliableChannelId, messageSendBuffer, messageSendBufferLength, out error);
        if (error != 0)
        {
            Debug.Log("Socket Send Error!: " + (NetworkError)error);
        }
    }

    void ListenForSocketMessage()
    {
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[messageReceivedBufferLength];
        int bufferSize = messageReceivedBufferLength;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
        //NetworkEventType dataReceived = NetworkTransport.Receive(out messageReceivedSocketId, out messageReceivedConnectionId, out messageReceivedQOSChannel, messageReceivedBuffer, messageReceivedBufferLength, out messageReceivedDataLength, out error);
        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                
                if (clientSocketId == recHostId && clientConnectionId == recConnectionId)
                {
                    isConnected = true;
                    Debug.Log("Connected to server!");
                }
                else
                {
                    connectedClients.Add(recConnectionId);
                    OnClientConnected.Invoke(recConnectionId);
                    isConnected = true;
                    Debug.Log("ConnectionEvent: HostId:" + recHostId + ", conId:" + recConnectionId);
                }
                break;
            case NetworkEventType.DataEvent:
                //recHostId will define host, connectionId will define connection, channelId will define channel; dataSize will define size of the received data. If recBuffer is big enough to contain data, data will be copied in the buffer. If not, error will contain MessageToLong error and you will need reallocate buffer and call this function again.
                string message;
                using (MemoryStream stream = new MemoryStream(recBuffer))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    message = formatter.Deserialize(stream) as string;
                }
                OnMessageReceived.Invoke(recConnectionId, message);
                break;
            case NetworkEventType.DisconnectEvent:
                break;
        }
    }
    
    void OnDestroy()
    {
        if (isConnected)
        {
            //NetworkTransport.Disconnect(clientsocketId, serverConnectionId, out error);
            //NetworkTransport.Shutdown();
        }
    }

    /*
    // Convert an object to a byte array
    public byte[] ObjectToByteArray(System.Object obj)
    {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    // Convert a byte array to an Object
    public System.Object ByteArrayToObject(byte[] arrBytes)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream memStream = new MemoryStream())
        {
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            System.Object obj = (System.Object)bf.Deserialize(memStream);
            return obj;
        }
    }*/
}
