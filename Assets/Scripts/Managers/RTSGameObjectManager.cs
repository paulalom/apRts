using System;
using UnityEngine;
using System.Collections.Generic;


//All of the manager classes could probaby be static
public class RTSGameObjectManager : MonoBehaviour {
    
    //Instantiating prefabs with resources.load is slow so here we are
    //Maybe this should be in an assets class or something like that
    //We cant expose a dictionary to the inspector so we expose an array then populate the dictionary
    //if we need to do this too often ill just make a component for this
    public string[] InspectorPrefabNames;
    public GameObject[] InspectorPrefabTypes;
    public Dictionary<string, GameObject> prefabs;
    GameManager gameManager;
    TerrainManager terrainManager;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        prefabs = new Dictionary<string, GameObject>();
        
        if (InspectorPrefabNames.Length != InspectorPrefabTypes.Length)
        {
            throw new System.Exception("fix the prefabs arrays in the rts game object manager");
        }
        if (InspectorPrefabNames.Length <= 0)
        {
            throw new System.Exception("Populate the prefabs arrays in the rts game object manager");
        }
        for (int i = 0; i < InspectorPrefabTypes.Length; i++)
        {
            Debug.Log(InspectorPrefabNames[i] + ", " + InspectorPrefabTypes[i].ToString());
            prefabs.Add(InspectorPrefabNames[i], InspectorPrefabTypes[i]);
        }
        if (InspectorPrefabNames.Length != prefabs.Count)
        {
            throw new System.Exception("No duplicate prefab names in the rts game object manager");
        }
    }

    void Update()
    {
        
    }

    public void SnapToTerrainHeight(RTSGameObject obj)
    {
        TerrainManager tMan = gameManager.terrainManager;
        Vector3 position = obj.transform.position;
        position.y = tMan.GetHeightFromGlobalCoords(position.x, position.z) + obj.transform.localScale.y/2;
        obj.transform.position = position;
    }
    public void SnapToTerrainHeight(List<RTSGameObject> objs)
    {
        foreach (RTSGameObject obj in objs)
        {
            SnapToTerrainHeight(obj);
        }
    }

    public void SetTargetLocation(RaycastHit hit)
    {

    }

    public GameObject NewDeposit(DepositType type, Dictionary<Type, int> items, Vector3 position)
    {
        Color color = Color.gray;
        string name = "Deposit";
        if (type == DepositType.Iron)
        {
            color = Color.red;
            name = "IronDeposit";
        }
        else if (type == DepositType.Coal)
        {
            color = Color.black;
            name = "CoalDeposit";
        }
        else if (type == DepositType.Forest)
        {
            color = Color.green;
            name = "ForstDeposit";
        }
        name += gameManager.GetNumUnits();
        return NewDeposit(name, color, type, items, position);
    }


    public GameObject NewDeposit(string name, Color color, DepositType type, Dictionary<Type, int> items, Vector3 position)
    {
        GameObject go = SpawnUnit(typeof(ResourceDeposit), position);
        ResourceDeposit deposit = go.GetComponent<ResourceDeposit>();
        deposit.type = type;
        go.name = name;
        try {
            go.GetComponentInChildren<Renderer>().material.color = color;
        }
        catch (Exception e)
        {
            throw new Exception("Dont be lazy next time");
        }
        go.GetComponent<Storage>().AddItems(items);

        if (type == DepositType.Coal)
        {
            deposit.harvestItems.Add(typeof(Coal), 50);
            deposit.harvestItems.Add(typeof(Stone), 50);
        }
        else if (type == DepositType.Forest)
        {
            deposit.harvestItems.Add(typeof(Wood), 50);
        }
        else if (type == DepositType.Iron)
        {
            deposit.harvestItems.Add(typeof(Iron), 25);
            deposit.harvestItems.Add(typeof(Stone), 50);
        }

        return go;
    }

    public Storage GetStorage(RTSGameObject accessor, RTSGameObject obj)
    {
        if (accessor is ResourceDeposit)
        {
            if (accessor.GetComponent<Harvester>() != null)
            {
                return obj.storage;
            }
            else
            {
                throw new MissingComponentException("Non-Harvester attempting to harvest" + accessor.name);
            }
        }
        else
        {
            return obj.storage;
        }
    }

    public bool TakeFromStorage(RTSGameObject taker, RTSGameObject target, Dictionary<Type, int> items)
    {
        if (target.storage.TakeItems(items)) // Do they have the items?
        {
            if (!taker.storage.AddItems(items)) // Do we have room?
            {
                target.storage.AddItems(items); // Nope, put em back
                return false;
            }
        }
        return true; // Success!
    }


    //The "around" bit is todo
    public bool SpawnUnitsAround(Type type, int quantity, GameObject producer)
    {
        if (!prefabs.ContainsKey(type.ToString()))
        {
            throw new ArgumentException("Attempting to spawn type: " + type + " which does not exist in prefab list");
        }
        for (int i = 0; i < quantity; i++)
        {
            SpawnUnit(type, new Vector3(producer.transform.position.x + producer.transform.localScale.x/2 + prefabs[type.ToString()].transform.localScale.x/2 + 1, 
                producer.transform.position.y, producer.transform.position.z));
        }
        return true;
    }

    public GameObject SpawnUnit(Type type, Vector3 position)
    {
        Debug.Log(type.ToString());
        GameObject go = Instantiate(prefabs[type.ToString()],
            position,
            Quaternion.identity) as GameObject;
        go.name = type.ToString() + gameManager.GetNumUnits(type);
        RTSGameObject rtsGo = go.GetComponent<RTSGameObject>();
        rtsGo.flagRenderer = go.GetComponent<Renderer>();
        if (rtsGo.flagRenderer == null)
        {
            rtsGo.flagRenderer = go.GetComponentInChildren<Renderer>();
        }
        gameManager.AddUnit(rtsGo);

        if (type == typeof(Factory))
        {
            Dictionary<Type, int> items = new Dictionary<Type, int>();
            items.Add(typeof(Coal), 2000);
            items.Add(typeof(Iron), 2000);
            items.Add(typeof(Wood), 2000);
            items.Add(typeof(Stone), 2000);
            items.Add(typeof(Paper), 200);
            items.Add(typeof(Tool), 100);
            rtsGo.storage.AddItems(items);
        }
        if (rtsGo.unitType == UnitType.Structure)
        {
            terrainManager.FlattenTerrainUnderObject(rtsGo);
        }

        return go;
    }

    public bool Harvest(RTSGameObject taker, ResourceDeposit target)
    {
        Harvester harvester = taker.GetComponent<Harvester>();
        Producer producer = taker.GetComponent<Producer>();
        if (target == null || harvester == null)
        {
            return false; // some weird joojoo here
        }
        harvester.harvestTarget = target;
        harvester.IsActive = true;
        return true;
    }

    public void TakeItem(RTSGameObject taker, RTSGameObject target, MyKVP<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage takerStorage = taker.GetComponent<Storage>();
        int taken = targetStorage.TakeItem(item.Key, item.Value, false);
        takerStorage.AddItem(item.Key, taken);
    }

    public void GiveItem(RTSGameObject giver, RTSGameObject target, MyKVP<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage giverStorage = giver.GetComponent<Storage>();
        int given = giverStorage.TakeItem(item.Key, item.Value, false);
        targetStorage.AddItem(item.Key, given);
    }

    public void MoveUnit(RTSGameObject unit, Vector2 targetPos, float moveSpeed)
    {
        Vector2 newPos = Vector2.MoveTowards(new Vector2(unit.transform.position.x, unit.transform.position.z), targetPos, moveSpeed);
        unit.transform.position = new Vector3(newPos.x, unit.transform.position.y, newPos.y);
    }

    public void MoveUnit(RTSGameObject unit, Vector2 targetPos)
    {
        MoveUnit(unit, targetPos, unit.GetComponent<Mover>().moveSpeed);
    }

    public bool lazyWithinDist(Vector3 o1, Vector3 o2, float dist)
    {
        return Math.Abs(o1.x - o2.x) < dist && Math.Abs(o1.z - o2.z) < dist;
    }
}
