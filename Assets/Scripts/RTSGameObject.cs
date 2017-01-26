using UnityEngine;
using System.Collections.Generic;
using System;

public enum RTSGameObjectType
{
    //Materials
    None,
    Stone,
    Wood,
    Iron,
    Coal,
    Paper,
    Power,

    //Component objects
    Tool,
    Car,

    //Game units
    Factory,
    HarvestingStation,
    Worker
}

[System.Serializable]
public class RTSGameObject : MonoBehaviour
{
    public static Dictionary<RTSGameObjectType, Dictionary<RTSGameObjectType, int>> productionCosts;
    public static Dictionary<RTSGameObjectType, int> productionTime;
    public static Dictionary<RTSGameObjectType, int> productionQuantity;
    public static Dictionary<RTSGameObjectType, Texture2D> menuIcon;

    static RTSGameObject()
    {
        productionCosts = new Dictionary<RTSGameObjectType, Dictionary<RTSGameObjectType, int>>();
        productionTime = new Dictionary<RTSGameObjectType, int>();
        productionQuantity = new Dictionary<RTSGameObjectType, int>();

        foreach (RTSGameObjectType type in Enum.GetValues(typeof(RTSGameObjectType)))
        {
            productionCosts[type] = new Dictionary<RTSGameObjectType, int>();
            productionTime[type] = 30; //Default time is 30s (minimum, base quantities around production time)
            productionQuantity[type] = 1;
        }

        productionCosts[RTSGameObjectType.Paper].Add(RTSGameObjectType.Wood, 1);
        productionCosts[RTSGameObjectType.Power].Add(RTSGameObjectType.Coal, 1);
        productionCosts[RTSGameObjectType.Factory].Add(RTSGameObjectType.Stone, 1000);
        productionCosts[RTSGameObjectType.Factory].Add(RTSGameObjectType.Wood, 1000);
        productionCosts[RTSGameObjectType.Factory].Add(RTSGameObjectType.Tool, 300);
        productionCosts[RTSGameObjectType.Factory].Add(RTSGameObjectType.Coal, 500);
        productionCosts[RTSGameObjectType.Factory].Add(RTSGameObjectType.Paper, 200);
        productionCosts[RTSGameObjectType.Tool].Add(RTSGameObjectType.Wood, 1);
        productionCosts[RTSGameObjectType.Tool].Add(RTSGameObjectType.Iron, 1);
        productionCosts[RTSGameObjectType.Worker].Add(RTSGameObjectType.Tool, 3);
        productionCosts[RTSGameObjectType.HarvestingStation].Add(RTSGameObjectType.Stone, 100);
        productionCosts[RTSGameObjectType.HarvestingStation].Add(RTSGameObjectType.Wood, 100);
        productionCosts[RTSGameObjectType.HarvestingStation].Add(RTSGameObjectType.Iron, 50);

        productionTime[RTSGameObjectType.Factory] = 300;
        productionTime[RTSGameObjectType.HarvestingStation] = 60;

        productionQuantity[RTSGameObjectType.Wood] = 30;
        productionQuantity[RTSGameObjectType.Paper] = 10;
        productionQuantity[RTSGameObjectType.Coal] = 20;
        productionQuantity[RTSGameObjectType.Iron] = 10;
        productionQuantity[RTSGameObjectType.Power] = 10;
        
    }

    //Non-static stuff
    public RTSGameObjectType type;
    public bool selected = false;
    public Camera mainCamera;
    public Renderer flagRenderer; // the part of the object which contains the flag
    public GameManager gm;

    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        flagRenderer = GetComponentInChildren<Renderer>(); // just get any part of the object
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (flagRenderer.isVisible)
            {
                Vector3 camPos = mainCamera.WorldToScreenPoint(transform.position);
                camPos.y = RTSCamera.InvertMouseY(camPos.y);
                selected = GameManager.selectionBox.Contains(camPos);
            }
            if (selected)
            {
                flagRenderer.material.color = Color.red;
                if (!gm.selectedUnits.Contains(this))
                {
                    gm.Select(this, true);
                }
            }
            else
            {
                flagRenderer.material.color = Color.white;
                gm.Select(this, false);
            }
        }
    }
}