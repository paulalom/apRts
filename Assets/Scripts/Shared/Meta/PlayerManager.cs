using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

// Neutral is player 0
public class PlayerManager : MyMonoBehaviour {

    public List<Player> players = new List<Player>();
    public GameManager gameManager;
    public World activeWorld;
    public int numAIPlayers;
    public int numHumanPlayers = 0;
    
    private Player _activePlayer = new Player();
    private int _activePlayerId;
    public Player ActivePlayer
    {
        get { return _activePlayer; }
    }
    public int ActivePlayerId
    {
        get { return _activePlayerId; }
    }
    
    // Temp UI display of resource totals
    public Text statusBarText;

    public void InitNeutralPlayer()
    {
        AIManager aiManager = new AIManager(gameManager.rtsGameObjectManager, this, gameManager.selectionManager, gameManager.commandManager);
        Player player = new Player(this, aiManager, true);
        player.name = "Neutral";
        players.Add(player);
    }

    public void InitAIPlayers()
    {
        for (int i = 0; i < numAIPlayers; i++)
        {
            InitPlayer(-1, false);
        }
    }

    public void InitPlayer(int networkClientId, bool isHuman)
    {
        AIManager aiManager = new AIManager(gameManager.rtsGameObjectManager, this, gameManager.selectionManager, gameManager.commandManager);
        Player player = new Player(this, aiManager, true);
        player.name = player + players.Count.ToString();
        player.isHuman = isHuman;
        player.networkClientId = networkClientId;
        players.Add(player);
    }

    // Implies last player is newly joining
    public void InitAllPlayers(int[] networkClientIds, int numPlayers)
    {
        InitNeutralPlayer();
        InitAIPlayers();
        for (int i = numAIPlayers; i < numPlayers + numAIPlayers; i++)
        {
            InitPlayer(networkClientIds[i - numAIPlayers], true);
        }
        
        _activePlayerId = players.Count - 1;
        _activePlayer = players[_activePlayerId];
    }

    public override void MyUpdate()
    {
        foreach (Player player in players)
        {
            player.UpdatePlayer();
        }
    }

    public int GetNumUnitsForPlayer(Type type, int playerId)
    {
        return players[playerId].units.Count(i => i.GetType() == type);
    }

    public int GetNumUnitsForPlayer(string type, int playerId)
    {
        return players[playerId].units.Count(i => i.GetType().ToString() == type);
    }

    public int GetNumUnitsForPlayer(int playerId)
    {
        return players[playerId].units.Count;
    }

    public void AddUnitForPlayer(RTSGameObject unit, int playerId)
    {
        Player player = players[playerId];
        player.units.Add(unit);
    }
    
    // We should store these lists (nonneutral, enemyUnits etc..) and maintain them rather than building them each time.
    public List<RTSGameObject> GetNonNeutralUnits()
    {
        List <RTSGameObject> units = new List<RTSGameObject>();
        foreach(Player player in players)
        {
            if (player.name != "Neutral")
            {
                units.AddRange(player.units);
            }
        }
        return units;
    }


    public HashSet<RTSGameObject> GetAllUnits()
    {
        HashSet<RTSGameObject> units = new HashSet<RTSGameObject>();
        foreach (Player player in players)
        {
            foreach (RTSGameObject unit in player.units)
            {
                units.Add(unit);
            }
        }
        return units;
    }
    
    public HashSet<RTSGameObject> GetEnemyUnits(RTSGameObject unitToSearch)
    {
        HashSet<RTSGameObject> units = new HashSet<RTSGameObject>();
        foreach (Player player in players)
        {
            if (player.name != "Neutral" && players[unitToSearch.ownerId] != player)
            {
                foreach (RTSGameObject unit in player.units)
                {
                    units.Add(unit);
                }
            }
        }
        return units;
    }

    public List<RTSGameObject> GetNeutralUnits()
    {
        return players[0].units;
    }
}
