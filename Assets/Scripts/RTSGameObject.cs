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
    Worker,
    PowerPlant
}

public enum RTSGameObjectGroup
{
    Resource,
    Unit,
    Structure
}

[System.Serializable]
public class RTSGameObject : MonoBehaviour
{
    public static Dictionary<RTSGameObjectType, Dictionary<RTSGameObjectType, int>> productionCosts;
    public static Dictionary<RTSGameObjectType, List<RTSGameObjectType>> canProduce; // this should probably be dictionary<type, HashSet<Type>>
    public static Dictionary<RTSGameObjectType, List<RTSGameObjectType>> canContain; // this should probably be dictionary<type, HashSet<Type>>
    public static Dictionary<RTSGameObjectGroup, List<RTSGameObjectType>> objectGroup;
    public static Dictionary<RTSGameObjectType, int> productionTime;
    public static Dictionary<RTSGameObjectType, int> productionQuantity;
    public static Dictionary<RTSGameObjectType, int> HarvestQuantity;
    public static Dictionary<RTSGameObjectType, Texture2D> menuIcon;

    static RTSGameObject()
    {
        productionCosts = new Dictionary<RTSGameObjectType, Dictionary<RTSGameObjectType, int>>();
        canProduce = new Dictionary<RTSGameObjectType, List<RTSGameObjectType>>();
        canContain = new Dictionary<RTSGameObjectType, List<RTSGameObjectType>>();
        objectGroup = new Dictionary<RTSGameObjectGroup, List<RTSGameObjectType>>();
        productionTime = new Dictionary<RTSGameObjectType, int>();
        productionQuantity = new Dictionary<RTSGameObjectType, int>();
        HarvestQuantity = new Dictionary<RTSGameObjectType, int>();

        foreach (RTSGameObjectGroup group in Enum.GetValues(typeof(RTSGameObjectGroup)))
        {
            objectGroup[group] = new List<RTSGameObjectType>();
        }
        foreach (RTSGameObjectType type in Enum.GetValues(typeof(RTSGameObjectType)))
        {
            productionCosts[type] = new Dictionary<RTSGameObjectType, int>();
            canProduce[type] = new List<RTSGameObjectType>();
            canContain[type] = new List<RTSGameObjectType>();
            
            //productionTime[type] = 30; //Default time is 30s (minimum, base quantities around production time)
            productionTime[type] = 5; //Testing
            productionQuantity[type] = 1;
            HarvestQuantity[type] = 10;
        }

        objectGroup[RTSGameObjectGroup.Structure].Add(RTSGameObjectType.Factory);
        objectGroup[RTSGameObjectGroup.Structure].Add(RTSGameObjectType.HarvestingStation);
        objectGroup[RTSGameObjectGroup.Structure].Add(RTSGameObjectType.PowerPlant);

        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Iron);
        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Wood);
        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Paper);
        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Stone);
        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Coal);
        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Tool);
        objectGroup[RTSGameObjectGroup.Resource].Add(RTSGameObjectType.Power);

        objectGroup[RTSGameObjectGroup.Unit].Add(RTSGameObjectType.Worker);

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
        productionCosts[RTSGameObjectType.HarvestingStation].Add(RTSGameObjectType.Tool, 20);
        productionCosts[RTSGameObjectType.PowerPlant].Add(RTSGameObjectType.Stone, 200);
        productionCosts[RTSGameObjectType.PowerPlant].Add(RTSGameObjectType.Tool, 20);
        productionCosts[RTSGameObjectType.PowerPlant].Add(RTSGameObjectType.Iron, 150);
        
        /* Testing, speed it up
        productionTime[RTSGameObjectType.Factory] = 300;
        productionTime[RTSGameObjectType.PowerPlant] = 240;
        productionTime[RTSGameObjectType.HarvestingStation] = 60;
        */
        productionQuantity[RTSGameObjectType.Wood] = 30;
        productionQuantity[RTSGameObjectType.Paper] = 10;
        productionQuantity[RTSGameObjectType.Coal] = 20;
        productionQuantity[RTSGameObjectType.Iron] = 10;
        productionQuantity[RTSGameObjectType.Power] = 10;

        canProduce[RTSGameObjectType.Worker].Add(RTSGameObjectType.HarvestingStation);
        canProduce[RTSGameObjectType.Worker].Add(RTSGameObjectType.Factory);

        canProduce[RTSGameObjectType.Factory].Add(RTSGameObjectType.Worker);
        canProduce[RTSGameObjectType.Factory].Add(RTSGameObjectType.Paper);
        canProduce[RTSGameObjectType.Factory].Add(RTSGameObjectType.Tool);
        canProduce[RTSGameObjectType.Factory].Add(RTSGameObjectType.Car);

        canProduce[RTSGameObjectType.PowerPlant].Add(RTSGameObjectType.Power);

        foreach (RTSGameObjectType type in objectGroup[RTSGameObjectGroup.Resource])
        {
            canContain[RTSGameObjectType.Factory].Add(type);
            canContain[RTSGameObjectType.Worker].Add(type);
            canContain[RTSGameObjectType.HarvestingStation].Add(type);
        }

        canContain[RTSGameObjectType.PowerPlant].Add(RTSGameObjectType.Coal);
        canContain[RTSGameObjectType.PowerPlant].Add(RTSGameObjectType.Power);
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
        CheckSelected();
    }

    void CheckSelected()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (gm.newSelectedUnit == null)
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
}