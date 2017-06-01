using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    public List<Player> players;
    public World activeWorld;
    public int myPlayerId = 1, enemyPlayerId = 2;

    // Neutral is player 0
    public HashSet<RTSGameObject> PlayerUnits { get { return ActivePlayer.units; } }
    public HashSet<RTSGameObject> PlayerSelectedUnits { get { return ActivePlayer.selectedUnits; } }
    public UnityEvent OnPlayerSelectionChange { get { return ActivePlayer.onSelectionChange; } set { ActivePlayer.onSelectionChange = value; } }
    private Player _activePlayer;
    private int _activePlayerId;
    public Player ActivePlayer
    {
        get { return _activePlayer; }
    }
    public int ActivePlayerId
    {
        get { return _activePlayerId; }
        set
        {
            _activePlayerId = value;
            _activePlayer = players[_activePlayerId];
        }
    }

    // Temp UI display of resource totals
    public Text resourceCountTextDisplay;

    public void InitPlayers(int numPlayers)
    {
        for (int i = 0; i < numPlayers + 1; i++)
        {
            Player player = new Player();
            player.name = (i == 0) ? "Neutral" : "Player " + i;
            player.selectedUnits = new HashSet<RTSGameObject>();
            player.units = new HashSet<RTSGameObject>();
            player.resources = new Dictionary<Type,int>();
            player.onSelectionChange = new UnityEvent();
            player.onResourceChange = new UnityEvent();
            players.Add(player);
            if (i == 1)
            {
                ActivePlayerId = i;
                ActivePlayer.onResourceChange.AddListener(UpdateResourceDisplay);
            }
        }
    }
    
    private void UpdateResourceDisplay()
    {
        string resourceString = "";
        foreach (KeyValuePair<Type, int> resource in ActivePlayer.resources)
        {
            resourceString += resource.Key + ": " + resource.Value + ", ";
        }
        resourceString.TrimEnd(new char[] { ' ', ',' });
        resourceCountTextDisplay.text = resourceString;
    }

    public int GetNumUnits(Type type, int playerId)
    {
        return players[playerId].units.Count(i => i.GetType() == type);
    }

    public int GetNumUnits(string type, int playerId)
    {
        return players[playerId].units.Count(i => i.GetType().ToString() == type);
    }

    public int GetNumUnits(int playerId)
    {
        return players[playerId].units.Count;
    }

    public void AddUnit(RTSGameObject unit, int playerId)
    {
        players[playerId].units.Add(unit);
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

    public HashSet<RTSGameObject> GetNeutralUnits()
    {
        return players[0].units;
    }
}
