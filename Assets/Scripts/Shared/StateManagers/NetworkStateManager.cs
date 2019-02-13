using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using System;
using Assets.Scripts.Shared;

public enum MessageIds
{
    CommandMessage,
    StepMessage,
    PlayerIdsMessage
}

public class NetworkStateManager : IStateManager{

    public GameManager gameManager;
    public PlayerManager playerManager;
    public TransportManager transportManager;
    public Dictionary<int, List<string>> recievedMessages = new Dictionary<int, List<string>>();
    public CommandQueue localCommands = new CommandQueue();

    public bool isServer = false;
    public long serverStep;
    public long clientStep { get { return StepManager.CurrentStep; } }
    bool isLocalInitilized = false, hasInitPlayerCount = false;
    const char messageSeparatorChar = '-';
    const char messageVariableSeparatorChar = '!';
    public string messageRequestSuffixCharacter = "?";

    void Start()
    {
        isServer = GlobalState.isServer;
        transportManager.OnMessageReceived.AddListener(OnMessageReceived);
    }

    public override void AddCommand(long step, MyPair<List<long>, Command> unitCommands)
    {
        SendCommandRequest(step, unitCommands.Key, unitCommands.Value);
    }

    // Server only
    public void OnClientConnected(int clientId)
    {
        if (isLocalInitilized)
        {
            playerManager.InitPlayer(clientId, true);
            gameManager.SetUpPlayer(playerManager.players.Count - 1, playerManager.ActiveWorld);
        }
    }

    // Server only
    public void OnClientDisconnected(int clientId)
    {

    }
    void OnMessageReceived(int clientId, string message)
    {
        bool isRequest = false;
        int messageTypeId = int.Parse(message[0].ToString());
        if (message[message.Length - 1] == '?')
        {
            isRequest = true;
            message = message.TrimEnd('?');
        }
        message = message.Substring(1);
        if (isRequest)
        {
            switch (messageTypeId)
            {
                case (int)MessageIds.CommandMessage:
                    ParseCommandRequest(message);
                    break;
                case (int)MessageIds.StepMessage:
                    throw new NotImplementedException("StepCountRequest");
                case (int)MessageIds.PlayerIdsMessage:
                    ParsePlayerIdsRequest();
                    break;
            }
        }
        else
        {
            switch (messageTypeId)
            {
                case (int)MessageIds.CommandMessage:
                    StoreReceivedCommand(clientId, message); // memory leak for now, will use this later for short term resync
                    ParseCommand(message);
                    break;
                case (int)MessageIds.StepMessage:
                    ParseStepMessage(message);
                    break;
                case (int)MessageIds.PlayerIdsMessage:
                    ParsePlayerIdsMessage(message);
                    break;
            }
        }
    }

    public override void Step()
    {
        if (isServer)
        {
            serverStep++;
            SendStep(transportManager.myUnreliableChannelId);
        }
    }

    // Server sends step to clients
    void SendStep(int channelId)
    {
        transportManager.SendSocketMessageToClients(((int)MessageIds.StepMessage).ToString() + serverStep, channelId);
    }

    // Clients receive step message
    void ParseStepMessage(string message)
    {
        if (!isServer) // this hides a problem... server sends step too frequently, client falls behind
        {               // going to want some assumption of server continuing at expected rate with only periodic (250-500ms) step# sync
                        // command requests should still be validated and correctly executed even if serverStep is out of sync
            serverStep = long.Parse(message);
        }
    }

    // Client requests
    void SendPlayerIdsRequest()
    {
        if (isServer)
        {
            return; // Server doesn't need the count
        }
        transportManager.SendSocketMessageToServer(((int)MessageIds.PlayerIdsMessage).ToString() + messageRequestSuffixCharacter);
    }
    
    // Server receives
    void ParsePlayerIdsRequest()
    {
        SendPlayerIds();
    }

    // Server Responds
    void SendPlayerIds()
    {
        string[] playerIds = new string[transportManager.connectedClients.Count];
        for (int i = 0; i < transportManager.connectedClients.Count; i++)
        {
            playerIds[i] = transportManager.connectedClients[i].ToString();
        }
        string message = string.Join(messageSeparatorChar.ToString(), playerIds);
        transportManager.SendSocketMessageToClients(((int)MessageIds.PlayerIdsMessage).ToString() + message);
    }

    // Client receives
    void ParsePlayerIdsMessage(string message)
    {
        Debug.Log("PlayerIds received: " + message);
        if (!isServer) // Server doesnt need
        {
            int prevPlayerCount = transportManager.connectedClients.Count;
            string[] playerIds = message.Split(messageSeparatorChar);
            transportManager.connectedClients.Clear();
            foreach (string playerId in playerIds)
            {
                transportManager.connectedClients.Add(int.Parse(playerId));
            }

            // Add new players to game
            int currentPlayerCount = transportManager.connectedClients.Count;
            if (hasInitPlayerCount && currentPlayerCount > prevPlayerCount)
            {
                for (int i = prevPlayerCount; i < currentPlayerCount; i++)
                {
                    playerManager.InitPlayer(transportManager.connectedClients[i], true);
                    gameManager.SetUpPlayer(i + 1, playerManager.ActiveWorld); // players are 1 index based (neutral player is 0)
                }
            }
            else {
                hasInitPlayerCount = true;
            }
        }
    }
    
    // Client requests to send command
    public void SendCommandRequest(long step, List<long> units, Command command)
    {
        string commandString = command.ToNetString();
        List<string> unitStrings = new List<string>();
        foreach (long unitId in units)
        {
            unitStrings.Add(unitId.ToString());
        }
        string unitIdsString = string.Join(messageSeparatorChar.ToString(), unitStrings.ToArray());
        string message = string.Join(messageVariableSeparatorChar.ToString(), new string[] { unitIdsString, commandString, step.ToString() });
        message = ((int)MessageIds.CommandMessage) + message + messageRequestSuffixCharacter;
        transportManager.SendSocketMessageToServer(message);
    }

    // Server receives client command request
    void ParseCommandRequest(string message)
    {
        //todo validate here
        string[] messageComponents = message.Split(messageVariableSeparatorChar);
        long clientStep = long.Parse(messageComponents[2]);
        if (clientStep <= serverStep)
        {
            messageComponents[2] = serverStep.ToString() + StepManager.numStepsToDelayInputProcessing;
            Debug.Log("server step: " + serverStep + ", requested step: " + clientStep + " response step: " + (serverStep + StepManager.numStepsToDelayInputProcessing));
        }
        else
        {
            Debug.Log("server step: " + serverStep + ", requested step: " + clientStep + " response step: " + clientStep);
        }
        SendCommand(string.Join(messageVariableSeparatorChar.ToString(), messageComponents));
    }

    // Server responds to client command request
    void SendCommand(string message)
    {
        transportManager.SendSocketMessageToClients((int)MessageIds.CommandMessage + message);
    }

    // Client received command from server
    void ParseCommand(string message)
    {
        string[] commandComponents = message.Split(messageVariableSeparatorChar);
        string unitIdsString = commandComponents[0];
        string commandString = commandComponents[1];
        string step = commandComponents[2];
        List<long> unitIds = new List<long>();
        Command command = Command.FromNetString(commandString);
        if (unitIdsString != "")
        {
            foreach (string unitId in unitIdsString.Split(messageSeparatorChar))
            {
                unitIds.Add(long.Parse(unitId));
            }
            localCommands.AddCommand(long.Parse(step), new MyPair<List<long>, Command>(unitIds, command));
        }
        Debug.Log("Command received: " + command.ToString());
    }

    public IEnumerator InitilizeLocalGame(GameManager gm, PlayerManager pm)
    {
        SendPlayerIdsRequest();
        yield return new WaitForSeconds(3);
        gameManager = gm;
        playerManager = pm;
        StepManager.CurrentStep = serverStep;
        playerManager.InitAllPlayers(transportManager.connectedClients.ToArray(), transportManager.connectedClients.Count);
        isLocalInitilized = true;
    }
        
    void StoreReceivedCommand(int clientId, string command)
    {
        if (recievedMessages.ContainsKey(clientId))
        {
            recievedMessages[clientId].Add(command);
        }
        else
        {
            recievedMessages.Add(clientId, new List<string>() { command });
        }
    }
}
