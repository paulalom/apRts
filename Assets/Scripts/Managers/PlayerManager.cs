﻿using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

// Things specific to this player?
public class PlayerManager : MonoBehaviour {

    public List<Player> players;
    
    public List<RTSGameObject> Units { get { return players[0].units; } }
    public List<RTSGameObject> SelectedUnits { get { return players[0].selectedUnits; } }
    public UnityEvent OnSelectionChange { get { return players[0].onSelectionChange; } set { players[0].onSelectionChange = value; } }

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

    public int GetNumUnits(Type type)
    {
        return Units.Count(i => i.GetType() == type);
    }

    public int GetNumUnits()
    {
        return Units.Count;
    }

    public void AddUnit(RTSGameObject unit)
    {
        Units.Add(unit);
    }
}