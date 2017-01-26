using System;
using UnityEngine;
using System.Collections.Generic;

//All of the manager classes could probaby be static
public class RTSGameObjectManager : MonoBehaviour {

    GameManager gameManager;
    
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        RTSGameObject.menuIcon = new Dictionary<RTSGameObjectType, Texture2D>();
        //This part cant be done in a (the) static constructor because unity
        foreach (RTSGameObjectType type in Enum.GetValues(typeof(RTSGameObjectType)))
        {
            Debug.Log(type.ToString());
            RTSGameObject.menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/" + type.ToString() + "Icon");
            if (RTSGameObject.menuIcon[type] == null)
            {
                RTSGameObject.menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/None");
            }
        }
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
}
