using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerManager : MyMonoBehaviour {

    public List<Player> players = new List<Player>();
    public GameManager gameManager;
    public World activeWorld;
    public int numAIPlayers;
    public int numHumanPlayers = 0;
    
    // Neutral is player 0
    public Dictionary<long, RTSGameObject> PlayerUnits { get { return ActivePlayer.units; } }
    public List<long> PlayerSelectedUnits { get { return ActivePlayer.selectedUnits; } }
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
    public Text statusBarText;

    public void InitNeutralPlayer()
    {
        Player player = new Player(this, false);
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
        Player player = new Player(this, true);
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
        
        ActivePlayerId = players.Count - 1;
    }

    public override void MyUpdate()
    {
        foreach (Player player in players)
        {
            player.UpdatePlayer();
        }
    }

    public List<RTSGameObject> GetPlayerSelectedUnits()
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (long id in _activePlayer.selectedUnits)
        {
            units.Add(RTSGameObjectManager.allUnits[id]);
        }
        return units;
    }

    public RTSGameObject GetUnit(long unitId)
    {
        if (RTSGameObjectManager.allUnits.ContainsKey(unitId))
        {
            return RTSGameObjectManager.allUnits[unitId];
        }
        else
        {
            return null;
        }
    }

    // no null return, send a new RTSGameObject instead
    public RTSGameObject GetUnitOrDefault(long unitId)
    {

        if (RTSGameObjectManager.allUnits.ContainsKey(unitId))
        {
            return RTSGameObjectManager.allUnits[unitId];
        }
        else
        {
            return new RTSGameObject() { ownerId = -1 };
        }
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
        Player player = players[playerId];
        player.units.Add(unit.uid, unit);
    }
    
    // We should store these lists (nonneutral, enemyUnits etc..) and maintain them rather than building them each time.
    public List<RTSGameObject> GetNonNeutralUnits()
    {
        List <RTSGameObject> units = new List<RTSGameObject>();
        foreach(Player player in players)
        {
            if (player.name != "Neutral")
            {
                units.AddRange(player.units.Values);
            }
        }
        return units;
    }


    public HashSet<RTSGameObject> GetAllUnits()
    {
        HashSet<RTSGameObject> units = new HashSet<RTSGameObject>();
        foreach (Player player in players)
        {
            foreach (RTSGameObject unit in player.units.Values)
            {
                units.Add(unit);
            }
        }
        return units;
    }

    public List<long> GetOrderableSelectedUnitIds()
    {
        List<long> unitIds = new List<long>();
        foreach (long id in PlayerSelectedUnits.Where(x => GetUnitOrDefault(x).ownerId == ActivePlayerId))
        {
            unitIds.Add(id);
        }
        return unitIds;
    }

    public List<RTSGameObject> GetOrderableSelectedUnits()
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (RTSGameObject unit in GetPlayerSelectedUnits().Where(x => x.ownerId == ActivePlayerId))
        {
            units.Add(unit);
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
                foreach (RTSGameObject unit in player.units.Values)
                {
                    units.Add(unit);
                }
            }
        }
        return units;
    }

    public Dictionary<long, RTSGameObject> GetNeutralUnits()
    {
        return players[0].units;
    }
}
