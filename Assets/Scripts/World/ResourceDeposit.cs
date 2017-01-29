using UnityEngine;
using System.Collections.Generic;

//Not just using RTSGameObjectType because a deposit may have multiple resources (eg. iron gives iron+stone)
public enum DepositType
{
    None,
    Forest,
    MineralVein,
    Stone
}
[RequireComponent(typeof(Storage))]
public class ResourceDeposit : MonoBehaviour
{
    public GameObject graphicObject;
    public DepositType type;
    
    private Storage storage;
    public long graphicRNGSeed;

    void Awake()
    {
        storage = GetComponent<Storage>();
    }

    public static GameObject NewDeposit(string name, Color color, DepositType type, Dictionary<RTSGameObjectType,int> items, Vector3 position)
    {
        GameObject go = new GameObject();
        go.AddComponent<ResourceDeposit>();
        ResourceDeposit res = go.GetComponent<ResourceDeposit>();
        Storage storage = go.GetComponent<Storage>();

        res.graphicObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        res.graphicObject.transform.localScale = new Vector3(4, 4, 4);
        res.graphicObject.transform.position = position;
        go.transform.position = position;

        res.graphicObject.GetComponent<Renderer>().material.color = color;
        go.name = name;
        res.type = type;
        storage.AddItems(items);

        return go;
    }

    public Storage GetStorage(GameObject accessor)
    {
        if (accessor.GetComponent<Harvester>() != null)
        {
            return storage;
        }
        else
        {
            throw new MissingComponentException("Non-Harvester attempting to harvest" + accessor.name);
        }
    }
}
