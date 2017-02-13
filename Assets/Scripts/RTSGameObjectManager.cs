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
    Dictionary<string, GameObject> prefabs;
    GameManager gameManager;
    
    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        RTSGameObject.menuIcon = new Dictionary<RTSGameObjectType, Texture2D>();
        prefabs = new Dictionary<string, GameObject>();

        //This part cant be done in a (the) RTSGameObject static constructor because unity
        foreach (RTSGameObjectType type in Enum.GetValues(typeof(RTSGameObjectType)))
        {
            Debug.Log(type.ToString());
            RTSGameObject.menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/" + type.ToString() + "Icon");
            if (RTSGameObject.menuIcon[type] == null)
            {
                RTSGameObject.menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/None");
            }
        }

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

    void Start()
    {
        
    }

    public void SnapToTerrainHeight(RTSGameObject obj)
    {
        TerrainManager tMan = gameManager.terrainManager;
        Vector3 position = obj.transform.position;
        position.y = tMan.GetHeightFromGlobalCoords(position.x, position.z);
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

    public GameObject NewDeposit(RTSGameObjectType type, Dictionary<RTSGameObjectType, int> items, Vector3 position)
    {
        Color color = Color.gray;
        string name = "Deposit";
        if (type == RTSGameObjectType.IronDeposit)
        {
            color = Color.red;
            name = "IronDeposit";
        }
        else if (type == RTSGameObjectType.CoalDeposit)
        {
            color = Color.black;
            name = "CoalDeposit";
        }
        else if (type == RTSGameObjectType.Forest)
        {
            color = Color.green;
            name = "ForstDeposit";
        }
        name += gameManager.GetNumUnits();
        return NewDeposit(name, color, type, items, position);
    }


    public GameObject NewDeposit(string name, Color color, RTSGameObjectType type, Dictionary<RTSGameObjectType, int> items, Vector3 position)
    {
        GameObject go = SpawnUnit(RTSGameObjectType.Resource, position);
        go.GetComponent<RTSGameObject>().type = type;
        go.name = name;
        try {
            go.GetComponent<Renderer>().material.color = color;
        }
        catch (Exception e)
        {
            throw new Exception("Dont be lazy next time");
        }
        go.GetComponent<Storage>().AddItems(items);

        return go;
    }

    public Storage GetStorage(RTSGameObject accessor, RTSGameObject obj)
    {
        if (RTSGameObject.objectGroup[RTSGameObjectGroup.Deposit].Contains(obj.type))
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

    public bool TakeFromStorage(RTSGameObject taker, RTSGameObject target, Dictionary<RTSGameObjectType, int> items)
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
    public bool SpawnUnitsAround(RTSGameObjectType type, int quantity, GameObject producer)
    {
        for (int i = 0; i < quantity; i++)
        {
            SpawnUnit(type, new Vector3(producer.transform.position.x, producer.transform.position.y, producer.transform.position.z));
        }
        return true;
    }

    public GameObject SpawnUnit(RTSGameObjectType type, Vector3 position)
    {
        Debug.Log(type.ToString());
        GameObject go = Instantiate(prefabs[type.ToString()],
            position,
            Quaternion.identity) as GameObject;
        go.name = type.ToString() + gameManager.GetNumUnits(type);
        RTSGameObject rtsGo = go.GetComponent<RTSGameObject>();
        rtsGo.type = type;
        gameManager.AddUnit(rtsGo);

        return go;
    }
}
