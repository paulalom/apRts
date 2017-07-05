using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class Player {

    public string name;
    public bool isHuman = false;
    public int networkClientId;
    PlayerManager playerManager;
    public UnityEvent onSelectionChange;
    public List<long> selectedUnits;
    public Dictionary<long, RTSGameObject> units;
    public Dictionary<Type,int> resources;
    public List<MyPair<float, MyPair<Type, int>>> incomeEventsLast30Seconds = new List<MyPair<float, MyPair<Type, int>>>(); // this should be linkedList to improve efficiency
    public Dictionary<Type, int> avgResourceIncomesLast30Seconds = new Dictionary<Type, int>(); 
    public RTSGameObject commander;
    public float economicPower;
    public float millitaryPower;
    public UnityEvent onEconomicChange;
    public class OnUnitCountIncrease : UnityEvent<RTSGameObject> { }
    public OnUnitCountIncrease onUnitCountIncrease;
    public class OnUnitCountDecrease : UnityEvent<RTSGameObject> { }
    public OnUnitCountDecrease onUnitCountDecrease;

    // Temp UI display of resource totals
    public string statusBarString = "";
    string statusBarText, resourceText = "", unitCountText = " Units: 0";

    public Player(PlayerManager playerManager, bool isHuman)
    {
        this.playerManager = playerManager;
        this.isHuman = isHuman;
        selectedUnits = new List<long>();
        units = new Dictionary<long, RTSGameObject>();
        resources = new Dictionary<Type, int>();
        onSelectionChange = new UnityEvent();
        onEconomicChange = new UnityEvent();
        onUnitCountIncrease = new OnUnitCountIncrease();
        onUnitCountDecrease = new OnUnitCountDecrease();
        onUnitCountIncrease.AddListener(HandleUnitCountIncrease);
        onUnitCountDecrease.AddListener(HandleUnitCountDecrease);
        onEconomicChange.AddListener(UpdateEconomicScores);
        if (isHuman)
        {
            onEconomicChange.AddListener(UpdateResourceText);
        }
    }

    public void UpdatePlayer()
    {
        while (incomeEventsLast30Seconds.Count > 0 && incomeEventsLast30Seconds[0].Key < Time.time - 2)
        {
            MyPair<Type, int> incomeEvent = incomeEventsLast30Seconds[0].Value;
            avgResourceIncomesLast30Seconds[incomeEvent.Key] -= incomeEvent.Value;
            incomeEventsLast30Seconds.RemoveAt(0);
            UpdateResourceText();
        }
    }

    void HandleUnitCountIncrease(RTSGameObject unitAdded)
    {
        AddMilitaryScore(unitAdded);
        UpdateMilitaryText();
    }

    void HandleUnitCountDecrease(RTSGameObject unitDestroyed)
    {
        SubtractMilitaryScore(unitDestroyed);
        UpdateMilitaryText();
    }

    void UpdateEconomicScores()
    {
        economicPower = 0;
        foreach (KeyValuePair<Type, int> resource in resources)
        {
            economicPower += resource.Value;
        }
    }

    void AddMilitaryScore(RTSGameObject unitAdded)
    {
        millitaryPower += 1;
    }

    void SubtractMilitaryScore(RTSGameObject unitDestroyed)
    {
        millitaryPower -= 1;
    }

    public void UpdateResourceText()
    {
        resourceText = "";
        foreach (KeyValuePair<Type, int> resource in resources)
        {
            string resourceChangeText = avgResourceIncomesLast30Seconds.ContainsKey(resource.Key) ?
                                        avgResourceIncomesLast30Seconds[resource.Key].ToString() :
                                        "0";
            resourceText += resource.Key + ": " + resource.Value + " (+ " + resourceChangeText + "), ";
        }
        statusBarString = resourceText + unitCountText;
        if (playerManager.ActivePlayer == this)
        {
            playerManager.statusBarText.text = statusBarString;
        }
    }

    public void UpdateMilitaryText()
    {
        if (playerManager.ActivePlayer == this)
        {
            unitCountText = " Units: " + units.Count + ", Military Power: " + millitaryPower;
            statusBarString = resourceText + unitCountText;
            playerManager.statusBarText.text = statusBarString;
        }
    }

    public void AddResources(Dictionary<Type, int> items)
    {
        foreach (KeyValuePair<Type, int> item in items)
        {
            UpdateResources(item.Key, item.Value);
            UpdateAvgResourceIncomes(item.Key, item.Value);
            incomeEventsLast30Seconds.Add(new MyPair<float, MyPair<Type, int>>(Time.time, new MyPair<Type, int>(item.Key, item.Value)));
        }
        onEconomicChange.Invoke();
    }

    public void TakeResources(Dictionary<Type,int> items)
    {
        foreach (KeyValuePair<Type, int> item in items)
        {
            UpdateResources(item.Key, -item.Value);
            UpdateAvgResourceIncomes(item.Key, -item.Value);
            incomeEventsLast30Seconds.Add(new MyPair<float, MyPair<Type, int>>(Time.time, new MyPair<Type, int>(item.Key, -item.Value)));
        }
        onEconomicChange.Invoke();
    }

    void UpdateResources(Type type, int qty)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += qty;
        }
        else
        {
            resources.Add(type, qty);
        }
    }

    void UpdateAvgResourceIncomes(Type type, int qty)
    {
        if (avgResourceIncomesLast30Seconds.ContainsKey(type))
        {
            avgResourceIncomesLast30Seconds[type] += qty;
        }
        else
        {
            avgResourceIncomesLast30Seconds.Add(type, qty);
        }
    }
}
