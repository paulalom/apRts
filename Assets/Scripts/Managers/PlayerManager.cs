using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

// Things specific to this player?
public class PlayerManager : MonoBehaviour {

    public List<Player> players;
    public World activeWorld;
    
    public List<RTSGameObject> PlayerUnits { get { return players[1].units; } }
    public List<RTSGameObject> PlayerSelectedUnits { get { return players[1].selectedUnits; } }
    public UnityEvent OnPlayerSelectionChange { get { return players[1].onSelectionChange; } set { players[1].onSelectionChange = value; } }

    //public GameManager gameManager;

    void Awake()
    {
        //gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        foreach (Player player in players)
        {
            player.selectedUnits = new List<RTSGameObject>();
            player.units = new List<RTSGameObject>();
            player.onSelectionChange = new UnityEvent();
        }
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
            if (player.Name != "Neutral")
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
            if (player.Name != "Neutral" && players[unit.ownerId] != player)
            {
                units.AddRange(player.units);
            }
        }
        return units;
    }
}
