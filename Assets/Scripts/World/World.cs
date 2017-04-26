using UnityEngine;
using System.Collections.Generic;

public class World {

    public WorldSettings worldSettings;
    public Dictionary<Vector2, GameObject> terrainChunks = new Dictionary<Vector2, GameObject>();
    public List<RTSGameObject> units = new List<RTSGameObject>();

    public void BuildWorld(TerrainManager terrainManager)
    {
        if (worldSettings == null)
        {
            Debug.Log("Trying to build world with no settings!");
        }
        int chunkRadiusInWorld = worldSettings.sizeRating;
        
        terrainChunks = terrainManager.GetNewTerrainChunks(3, this);
    }
    
}
