﻿using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using System;

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
    public Dictionary<int, List<string>> receivedMessages = new Dictionary<int, List<string>>();
    public CommandQueue localCommands = new CommandQueue();

    public long serverStep;
    public long clientStep { get { return StepManager.CurrentStep; } }
    public long nextRtsGoUid = 0;
    bool isLocalInitilized = false, hasInitPlayerCount = false;
    const char messageSeparatorChar = '-';
    const char messageVariableSeparatorChar = '!';
    public string messageRequestSuffixCharacter = "?";

    void Start()
    {
        DontDestroyOnLoad(this);
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
            gameManager.SetUpPlayer(playerManager.players.Count - 1, playerManager.activeWorld);
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
                    StoreReceivedCommand(clientId, message);
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
        if (transportManager.isServer)
        {
            serverStep++;
            SendStep(transportManager.myUnreliableChannelId);
        }
    }

    // Server sends step to clients
    void SendStep(int channelId)
    {
        transportManager.SendSocketMessage(((int)MessageIds.StepMessage).ToString() + serverStep, channelId);
    }

    // Clients receive step message
    void ParseStepMessage(string message)
    {
        serverStep = long.Parse(message);
    }

    // Client requests
    void SendPlayerIdsRequest()
    {
        if (transportManager.isServer)
        {
            return; // Server doesn't need the count
        }
        transportManager.SendSocketMessage(((int)MessageIds.PlayerIdsMessage).ToString() + messageRequestSuffixCharacter);
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
        transportManager.SendSocketMessage(((int)MessageIds.PlayerIdsMessage).ToString() + message);
    }

    // Client receives
    void ParsePlayerIdsMessage(string message)
    {
        Debug.Log("PlayerIds received: " + message);
        if (!transportManager.isServer) // Server doesnt need
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
                    gameManager.SetUpPlayer(i + 1, playerManager.activeWorld); // players are 1 index based (neutral player is 0)
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
        transportManager.SendSocketMessage(message);
    }

    // Server receives client command request
    void ParseCommandRequest(string message)
    {
        //todo validate here
        string[] messageComponents = message.Split(messageVariableSeparatorChar);
        long clientStep = long.Parse(messageComponents[2]);
        if (clientStep < serverStep + 1)
        {
            messageComponents[2] = serverStep.ToString() + StepManager.numStepsToDelayInputProcessing;
            Debug.Log("server step: " + serverStep + ", requested step: " + clientStep + " response step: " + serverStep + StepManager.numStepsToDelayInputProcessing);
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
        transportManager.SendSocketMessage((int)MessageIds.CommandMessage + message);
    }

    // Client received command from server
    void ParseCommand(string message)
    {
        string[] commandComponents = message.Split(messageVariableSeparatorChar);
        string unitIdsString = commandComponents[0];
        string commandString = commandComponents[1];
        string step = commandComponents[2];
        List<long> unitIds = new List<long>();
        Command command = Command.FromNetString(commandString, playerManager);
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

    public long GetNextUID()
    {
        return nextRtsGoUid++;
    }
    
    void StoreReceivedCommand(int clientId, string command)
    {
        if (receivedMessages.ContainsKey(clientId))
        {
            receivedMessages[clientId].Add(command);
        }
        else
        {
            receivedMessages.Add(clientId, new List<string>() { command });
        }
    }
}