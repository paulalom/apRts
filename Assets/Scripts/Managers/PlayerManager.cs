using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    public List<Player> players;
    public World activeWorld;
    
    // Neutral is player 0
    public List<RTSGameObject> PlayerUnits { get { return players[1].units; } }
    public List<RTSGameObject> PlayerSelectedUnits { get { return players[1].selectedUnits; } }
    public UnityEvent OnPlayerSelectionChange { get { return players[1].onSelectionChange; } set { players[1].onSelectionChange = value; } }

    // Temp UI display of resource totals
    public Text resourceCountTextDisplay;

    public void InitPlayers(int numPlayers)
    {
        for (int i = 0; i < numPlayers + 1; i++)
        {
            Player player = new Player();
            player.name = (i == 0) ? "Neutral" : "Player " + i;
            player.selectedUnits = new List<RTSGameObject>();
            player.units = new List<RTSGameObject>();
            player.resources = new Dictionary<Type,int>();
            player.onSelectionChange = new UnityEvent();
            player.onResourceChange = new UnityEvent();
            if (i == 1)
            {
                player.onResourceChange.AddListener(UpdateResourceDisplay);
            }
            players.Add(player);
        }
    }
    
    private void UpdateResourceDisplay()
    {
        string resourceString = "";
        foreach (KeyValuePair<Type, int> resource in players[1].resources)
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
    
    public List<RTSGameObject> GetEnemyUnits(RTSGameObject unit)
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (Player player in players)
        {
            if (player.name != "Neutral" && players[unit.ownerId] != player)
            {
                units.AddRange(player.units);
            }
        }
        return units;
    }

    public List<RTSGameObject> GetNeutralUnits()
    {
        return players[0].units;
    }
}
