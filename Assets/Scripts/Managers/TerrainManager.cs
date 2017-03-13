using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// A large portion of this class was adapted from Nick Pearson's Transport Game In Unity series.
// In the future, I will likely rewrite all of this code, but as this is my first Unity project,
// his series has been helpful in overcoming the barrier to entry. Thanks Nick!
// https://www.youtube.com/channel/UC9UZBI9EuXu9o4xMM3CAg2w
public class TerrainManager : MonoBehaviour, ICameraObserver  {

    [System.Serializable]
    public struct TreeBlueprint
    {
        public GameObject prefab;
        public float bendFactor;
    }
    
    [System.Serializable]
    public struct Biome
    {
        public TerrainTextureBlueprint grassTexture;
        public TerrainTextureBlueprint rockyTexture;
        public TerrainTextureBlueprint cliffTexture;
        public TerrainTextureBlueprint riverbankTexture;
        public TerrainTextureBlueprint snowTexture;
    }


    public float waterThreshold = 30;
    public float snowThreshold = 125;
    // This isnt implemented yet. 
    // If the terrain goes above this we will not be able to select any units there
    public const float maxTerrainHeight = 600;

    [System.Serializable]
    public struct TerrainTextureBlueprint
    {
        public Texture2D albedo;
        public Texture2D normal;
        public int metallic;
        public Vector2 size, offset;
    }
    [HideInInspector]
    public Transform projector;
    
    //Terrains are never deleted
    public Dictionary<Vector2, GameObject> terrainChunks = new Dictionary<Vector2, GameObject>();
    Dictionary<Vector2, List<RTSGameObject>> terrainResources = new Dictionary<Vector2, List<RTSGameObject>>();
    
    //We need to maintain the ability to scale this value
    public const int chunkSizeX = 512, chunkSizeZ = 512;
    public const int Resolution = chunkSizeX / 2;
    public const float resolutionRatio = chunkSizeX / Resolution;
    const int chunkGraphics1dArrayLength = 3; // THIS MUST ALWAYS BE ODD BECAUSE IM LAZY
    
    Vector2 oldCenterChunkGlobalIndex;

    public static float TERRAIN_HEIGHT_WILDCARD = -1;
    SplatPrototype[] terrainTextures;
    public Biome[] biomeTextures;
    public TreeBlueprint[] trees;
    public TreePrototype[] terrainTrees;
    public Transform waterPlanePrefab;

    GameManager gameManager;
    RTSGameObjectManager rtsGameObjectManager;

    // This is called before any other script's "Start" function
    // Do local inits here
    void Awake()
    {
        projector = GameObject.Find("BrushSizeProjector").transform;
        Camera.main.GetComponent<RTSCamera>().Subscribe(this);
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();

        terrainTextures = new SplatPrototype[biomeTextures.Length * 5];
        terrainTrees = new TreePrototype[trees.Length];

        //seems like one biome per chunk at the moment
        for (int i = 0; i < biomeTextures.Length; i++)
        {
            terrainTextures[i] = BlueprintToSplatPrototype(biomeTextures[i].grassTexture);
            terrainTextures[i + 1] = BlueprintToSplatPrototype(biomeTextures[i].rockyTexture);
            terrainTextures[i + 2] = BlueprintToSplatPrototype(biomeTextures[i].cliffTexture);
            terrainTextures[i + 3] = BlueprintToSplatPrototype(biomeTextures[i].riverbankTexture);
            terrainTextures[i + 4] = BlueprintToSplatPrototype(biomeTextures[i].snowTexture);
        }

        for (int i = 0; i < trees.Length; i++)
        {
            terrainTrees[i] = BlueprintToTreePrototype(trees[i]);
        }
    }
    
    // Functionality which may be dependant on other scripts should be in Start
    void Start () {

        //This is a little bit of duplicated code from OnCameraMove for setup (which we dont run if we haven't moved).
        Vector2 newCameraPosition = Camera.main.transform.position;
        oldCenterChunkGlobalIndex = GetChunkIndexFromCameraPos(newCameraPosition);
        Vector2 cameraChunkMoveVector = GetCameraChunkMoveVector(newCameraPosition);
        Vector2 newCenterChunkGlobalIndex = GetNewCenterChunkGlobalIndex(cameraChunkMoveVector);
        GenerateNewTerrain(oldCenterChunkGlobalIndex, newCenterChunkGlobalIndex);
    }

    #region ModifyTerrain


    // meh buggy/fails around chunk edges.. whatever its not needed right now
    public void ModifyTerrain(Vector3 position, float amount, int diameter)
    {
        Vector2 terrainIndex = GetChunkIndexFromGlobalCoords(position.x, position.z);
        if (!terrainChunks.ContainsKey(terrainIndex))
        {
            return;
        }
        Terrain mainTerrain = terrainChunks[terrainIndex].GetComponent<Terrain>();
        Vector2 relativePosition = GetTerrainRelativePosition(position.x, position.z);

        float[,] heights = mainTerrain.terrainData.GetHeights(0, 0, Resolution, Resolution);
        int terrainPosX = (int)((relativePosition.x / mainTerrain.terrainData.size.x) * Resolution);
        int terrainPosY = (int)((relativePosition.y / mainTerrain.terrainData.size.z) * Resolution);
        int radius = diameter / 2;
        float[,] heightChange = new float[diameter, diameter];
        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                int x2 = x - radius;
                int y2 = y - radius;

                if (terrainPosY + y2 < 0 || terrainPosY + y2 >= heights.GetLength(0))
                {
                    continue;
                }
                else if (terrainPosX + x2 < 0 || terrainPosX + x2 >= heights.GetLength(1))
                {
                    continue;
                }
                else if (y > heights.GetLength(0) || x > heights.GetLength(1))
                {
                    continue;
                }

                float dist2 = x2 * x2 + y2 * y2;
                float dist = Mathf.Sqrt(dist2);
                if (dist <= radius)
                {
                    heightChange[y, x] = heights[terrainPosY + y2, terrainPosX + x2] + amount - (amount * dist / ((float)radius));
                    heights[terrainPosY + y2, terrainPosX + x2] = heightChange[y, x];
                }
                else
                {
                    heightChange[y, x] = heights[terrainPosY + y2, terrainPosX + x2];
                }
            }
        }

        //Fixme fails when array larger than current terrain
        mainTerrain.terrainData.SetHeights(terrainPosX - radius, terrainPosY - radius, heightChange);
    }

    //This is needed? because of mod behaviour for negative numbers
    public static float Mod(float f1, float f2)
    {
        return f1 - f2 * (int)(f1 / f2);
    }
    
    #endregion

    public Vector2 GetCameraChunkMoveVector(Vector3 newCameraPosition)
    {
        return GetChunkIndexFromCameraPos(newCameraPosition) - oldCenterChunkGlobalIndex;
    }

    public Vector2 GetNewCenterChunkGlobalIndex(Vector2 cameraChunkMoveVector)
    {
        return oldCenterChunkGlobalIndex + cameraChunkMoveVector;
    }

    public void GenerateNewTerrain(Vector2 cameraChunkMoveVector, Vector2 newCenterChunkGlobalIndex)
    {
        //If we havent teleported but have moved, we only need to check chunks in the direction of movement
        //For simplicty, we wont bother with the case where we have a diagonal moveVector (since its very unlikely)
        if ((cameraChunkMoveVector.x == 0 
            || cameraChunkMoveVector.y == 0)
                && (System.Math.Abs(cameraChunkMoveVector.x) >= 1 
                    || System.Math.Abs(cameraChunkMoveVector.y) >= 1))
        {
            Debug.Log("yep " + -chunkGraphics1dArrayLength / 2 + ", " + chunkGraphics1dArrayLength / 2 + " ..." + newCenterChunkGlobalIndex);
            Debug.Log("OldCenterChunkGlobalIndex: " + oldCenterChunkGlobalIndex + "newCenterChunkGlobalIndex " + newCenterChunkGlobalIndex + ", cameraChunkMoveVector " + cameraChunkMoveVector);
            for (int i = -chunkGraphics1dArrayLength/2; i <= chunkGraphics1dArrayLength/2; i++)
            {
                //i is perpendicular to the direction of movement to draw our front line
                Vector2 chunkIndex = new Vector2(
                        cameraChunkMoveVector.x == 0 ? i + newCenterChunkGlobalIndex.x : newCenterChunkGlobalIndex.x,
                        cameraChunkMoveVector.y == 0 ? i + newCenterChunkGlobalIndex.y : newCenterChunkGlobalIndex.y);
                if (!terrainChunks.ContainsKey(chunkIndex))
                {
                    terrainChunks[chunkIndex] = GenerateChunk(chunkIndex);
                }
            }
        }
        else // We check every tile in the new ChunkGraphics area
        { 
            for (int y = -chunkGraphics1dArrayLength/2; y <= chunkGraphics1dArrayLength/2; y++)
            {
                for (int x = -chunkGraphics1dArrayLength/2; x <= chunkGraphics1dArrayLength/2; x++)
                {
                    Vector2 chunkIndex = new Vector2(newCenterChunkGlobalIndex.x + x, newCenterChunkGlobalIndex.y + y);
                    if (!terrainChunks.ContainsKey(chunkIndex))
                    {
                        terrainChunks[chunkIndex] = GenerateChunk(chunkIndex);
                    }
                }
            }
        }
    }

    public void OnCameraMove(Vector3 newCameraPosition)
    {
        Vector2 cameraChunkMoveVector = GetCameraChunkMoveVector(newCameraPosition);

        if (cameraChunkMoveVector.x != 0 || cameraChunkMoveVector.y != 0)
        {
            Vector2 newCenterChunkGlobalIndex = GetNewCenterChunkGlobalIndex(cameraChunkMoveVector);

            GenerateNewTerrain(cameraChunkMoveVector, newCenterChunkGlobalIndex);

            //We have generated any missing terrain so now we just have to populate the visible chunks 
            //and set the graphics objects/fade out the leaving chunks
            //SetVisibleTerrain(cameraChunkMoveVector, newCenterChunkGlobalIndex);
        }
        oldCenterChunkGlobalIndex += cameraChunkMoveVector;
    }

    GameObject GenerateChunk(Vector2 worldSpaceChunkIndex)
    {
        Debug.Log("Generating Chunk world index (" + worldSpaceChunkIndex.x + "," + worldSpaceChunkIndex.y + "))");
        GameObject terrainGO = new GameObject();
        Terrain terrain;
        terrainGO.name = "Chunk_" + terrainChunks.Count;
        terrain = terrainGO.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        terrainGO.AddComponent<TerrainCollider>().terrainData = terrain.terrainData;
        terrain.terrainData.heightmapResolution = chunkSizeX / 2;
        terrain.terrainData.size = new Vector3(chunkSizeX, 600, chunkSizeZ);
        terrain.terrainData.splatPrototypes = terrainTextures;
        terrain.terrainData.treePrototypes = terrainTrees;
        terrain.transform.position = new Vector3(chunkSizeX * worldSpaceChunkIndex.x, 0, chunkSizeZ * worldSpaceChunkIndex.y);
        Transform waterPlane = GameObject.Instantiate(waterPlanePrefab, terrainGO.transform.position, Quaternion.identity) as Transform;
        waterPlane.transform.position = new Vector3(waterPlane.position.x + chunkSizeX / 2, waterThreshold, waterPlane.position.z + chunkSizeZ / 2);
        waterPlane.localScale = new Vector3(5.1f, 1, 5.1f); // Yay magic
        waterPlane.SetParent(terrainGO.transform);
        
        SetTerrainHeightMap(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex);
        SetTerrainTextures(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex);
        SetTerrainTrees(terrainGO.GetComponent<Terrain>());
        SetTerrainResources(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex);
        
        return terrainGO;
    }

    void SetTerrainHeightMap(Terrain terrain, Vector2 worldSpaceChunkIndex)
    {
        //terrain.terrainData.GetHeights(0,0, , )
        float[,] heights = new float[terrain.terrainData.heightmapHeight, terrain.terrainData.heightmapWidth];
        bool left = false;
        bool right = false;
        bool top = false;
        bool bottom = false;

        for (int x = 0; x < heights.GetLength(1); x++)
        {
            for (int y = 0; y< heights.GetLength(0); y++)
            {
                heights[y, x] = TERRAIN_HEIGHT_WILDCARD;
            }
        }
        
        //left of new terrain = right of old terrain
        float[,] hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x - 1, worldSpaceChunkIndex.y));
        if (hm != null)
        {
            left = true;
            for(int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, 0] = hm[i, hm.GetLength(1) - 1];
            }
        }

        //right of new terrain = left of old terrain
        hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x + 1, worldSpaceChunkIndex.y));
        if (hm != null)
        {
            right = true;
            for (int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, heights.GetLength(1) - 1] = hm[i, 0];
            }
        }

        //top of new terrain = bottom of old terrain
        hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x, worldSpaceChunkIndex.y - 1));
        if (hm != null)
        {
            top = true;
            for (int i = 0; i < hm.GetLength(1); i++)
            {
                heights[0, i] = hm[hm.GetLength(0) - 1, i];
            }
        }

        //bottom of new terrain = top of old terrain
        hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x, worldSpaceChunkIndex.y + 1));
        if (hm != null)
        {
            bottom = true;
            for (int i = 0; i < hm.GetLength(1); i++)
            {
                heights[heights.GetLength(0) - 1, i] = hm[0, i];
            }
        }

        if (!top && !left)
        {
            heights[0, 0] = 0.2f;
        }
        if (!bottom && !left)
        {
            heights[terrain.terrainData.heightmapWidth - 1, 0] = 0.2f;
        }
        if (!top && !right)
        {
            heights[0, terrain.terrainData.heightmapHeight - 1] = 0.2f;
        }
        if (!bottom && !right)
        {
            heights[terrain.terrainData.heightmapWidth - 1, terrain.terrainData.heightmapHeight - 1] = 0.2f;
        }

        heights = DiamondSquare(heights, 0, 0, terrain.terrainData.heightmapWidth - 1, 0);
        terrain.terrainData.SetHeights(0,0,heights);
    }

    void SetTerrainTrees(Terrain terrain)
    {
        terrain.terrainData.treeInstances = new TreeInstance[0];
        for (int x = 0; x < Resolution; x+= UnityEngine.Random.Range(5, 30))
        {
            for (int y = 0; y < Resolution; y+= UnityEngine.Random.Range(5,30))
            {
                float height = terrain.terrainData.GetHeight(x,y);
                float steepness = terrain.terrainData.GetSteepness(x/(float)Resolution, y/(float)Resolution);

                if (UnityEngine.Random.value > 0.3 + (steepness/30) && height >= waterThreshold - 10)
                {
                    TreeInstance instance = new TreeInstance();
                    instance.prototypeIndex = 0;
                    instance.position = new Vector3(x /(float)Resolution, 0, y/ (float)Resolution);
                    instance.widthScale = 1f;
                    instance.heightScale = 1f;
                    instance.color = Color.white;
                    instance.rotation = 0f;
                    terrain.AddTreeInstance(instance);
                }
            }
        }
    }

    void SetTerrainResources(Terrain terrain, Vector2 terrainGlobalCoords)
    {
        //Use terrain to check criteria for generation
        //consider biomes in the future
        //consider merging tree generation, as trees should be a resource
        //consider a less naive approach for which resource we want to generate (do we already have a million of these?)
        //consider using fewer magic numbers

        terrainResources.Add(terrainGlobalCoords, new List<RTSGameObject>());
        
        // if x or y is 0 we may be placing objects at the very edge of a terrain, which can cause out of bounds exceptions
        for (int x = UnityEngine.Random.Range(1, 60); x < Resolution; x += UnityEngine.Random.Range(0, 60))
        {
            for (int y = UnityEngine.Random.Range(1, 60); y < Resolution; y += UnityEngine.Random.Range(0, 60))
            {
                float height = terrain.terrainData.GetHeight(x, y);
                float steepness = terrain.terrainData.GetSteepness(x / (float)Resolution, y / (float)Resolution);
                float resourceRandom;
                //Generate a resource?
                if (UnityEngine.Random.value > 0.6)
                {
                    GameObject go;
                    //+res.graphicObject.transform.lossyScale.y
                    //Pick a resource
                    resourceRandom = UnityEngine.Random.value;
                    Dictionary<Type, int> items = new Dictionary<Type, int>();
                    if (resourceRandom > 0.4 + (steepness / 30) && height >= waterThreshold - 10)
                    {
                        items.Add(typeof(Wood), 20000);
                        go = rtsGameObjectManager.NewDeposit(DepositType.Forest,
                                                        items, 
                                                        new Vector3(terrain.transform.position.x + x * (chunkSizeX / Resolution), 
                                                                    height, 
                                                                    terrain.transform.position.z + y * (chunkSizeZ / Resolution))
                                                       );

                        terrainResources[terrainGlobalCoords].Add(go.GetComponent<RTSGameObject>());
                    }
                    else if(resourceRandom > 0.4)
                    {
                        items.Add(typeof(Iron), 8000);
                        items.Add(typeof(Stone), 16000);
                        go = rtsGameObjectManager.NewDeposit(DepositType.Iron,
                                                        items,
                                                        new Vector3(terrain.transform.position.x + x * (chunkSizeX / Resolution),
                                                                    height,
                                                                    terrain.transform.position.z + y * (chunkSizeZ / Resolution))
                                                        );

                        terrainResources[terrainGlobalCoords].Add(go.GetComponent<RTSGameObject>());
                    }
                    else if (resourceRandom > 0.3)
                    {
                        items.Add(typeof(Coal), 8000);
                        items.Add(typeof(Stone), 16000);
                        go = rtsGameObjectManager.NewDeposit(DepositType.Coal,
                                                        items,
                                                        new Vector3(terrain.transform.position.x + x * (chunkSizeX / Resolution),
                                                                    height,
                                                                    terrain.transform.position.z + y * (chunkSizeZ / Resolution))
                                                        );

                        terrainResources[terrainGlobalCoords].Add(go.GetComponent<RTSGameObject>());
                    }
                    else
                    {
                        //meh no resource
                    }
                }
            }
        }
    }

    void SetTerrainTextures(Terrain terrain, Vector2 worldSpaceChunkIndex)
    {
        int currentBiome = 0;
        float[,,] alphaMap = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrainTextures.Length];

        for (int x = 0; x < alphaMap.GetLength(0); x++)
        {
            for (int y = 0; y < alphaMap.GetLength(1); y++)
            {
                float normX = (float)x / (alphaMap.GetLength(0) - 1);
                float normY = (float)y / (alphaMap.GetLength(1) - 1);

                float steepness = terrain.terrainData.GetSteepness(normX, normY);
                float height = terrain.terrainData.GetHeight((int)(normX * terrain.terrainData.heightmapWidth), (int)(normY * terrain.terrainData.heightmapHeight));

                float isCliff = Mathf.Clamp(steepness - 50, 0, 10) / 10f; // max steepness is 90 degrees
                float isSnow = Mathf.Clamp(height - snowThreshold, 0, 30) / 30f;
                float isRocky = Mathf.Clamp(steepness - 60, 0, 10) / 10f;
                // Add 5 so riverbank starts above water
                float isRiverbank = Mathf.Clamp(waterThreshold - height + 15, 0, 10);

                //Im not sure this currentbiome variable makes sense here, though i know what it's trying to do
                alphaMap[y, x, currentBiome + 0] = 1 - isRocky - isCliff - isSnow - isRiverbank;
                alphaMap[y, x, currentBiome + 1] = isRocky - isCliff;
                alphaMap[y, x, currentBiome + 2] = isCliff;
                alphaMap[y, x, currentBiome + 3] = isRiverbank - isCliff - isRocky;
                alphaMap[y, x, currentBiome + 4] = isSnow - isRocky - isCliff - isRiverbank;
                
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    float[,] GetTerrainHeightMap(Vector2 worldSpaceChunkIndex)
    {
        if (terrainChunks.ContainsKey(worldSpaceChunkIndex))
        {
            TerrainData td = terrainChunks[worldSpaceChunkIndex].GetComponent<Terrain>().terrainData;
            return td.GetHeights(0, 0, td.heightmapWidth, td.heightmapHeight);
        }
        else
        {
            return null;
        }
    }

    float[,] DiamondSquare(float[,] heights, int offsetX, int offsetY, int squareSize, int depth)
    {
        if (squareSize == 1)
        {
            return heights;
        }

        float topLeft = heights[offsetY, offsetX];
        float topRight = heights[offsetY, offsetX + squareSize];
        float bottomLeft = heights[offsetY + squareSize, offsetX];
        float bottomRight = heights[offsetY + squareSize, offsetX + squareSize];

        if (topLeft == TERRAIN_HEIGHT_WILDCARD || topRight == TERRAIN_HEIGHT_WILDCARD || bottomLeft == TERRAIN_HEIGHT_WILDCARD || bottomRight == TERRAIN_HEIGHT_WILDCARD)
        {
            Debug.Log("One or more corner seed values is not set.");
        }

        if (heights[offsetY + (squareSize / 2), offsetX + (squareSize / 2)] == TERRAIN_HEIGHT_WILDCARD)
        {
            heights[offsetY + (squareSize / 2), offsetX + (squareSize / 2)] = GetRandHeight(depth) + AveragePoints(topLeft, topRight, bottomLeft, bottomRight);
        }
        float centerPoint = heights[offsetY + (squareSize / 2), offsetX + (squareSize / 2)];
        
        //left diamond
        float runningAverage = AveragePoints(topLeft, centerPoint, bottomLeft);

        if(offsetX-squareSize/2 > 0 && heights[offsetY+squareSize/2, offsetX - squareSize/2] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(topLeft, centerPoint, bottomLeft, heights[offsetY + squareSize / 2, offsetX - squareSize / 2]);
        }

        if (heights[offsetY + squareSize/2, offsetX] == TERRAIN_HEIGHT_WILDCARD)
        {
            heights[offsetY + squareSize / 2, offsetX] = runningAverage + GetRandHeight(depth);
        }

        //right diamond
        runningAverage = AveragePoints(topRight, centerPoint, bottomRight);

        if (offsetX + squareSize * 1.5f < heights.GetLength(1) && heights[offsetY + squareSize / 2, offsetX + (int)(squareSize * 1.5f)] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(topRight, centerPoint, bottomRight, heights[offsetY + squareSize / 2, offsetX + (int)(squareSize * 1.5f)]);
        }

        if (heights[offsetY + squareSize / 2, offsetX + squareSize] == TERRAIN_HEIGHT_WILDCARD)
        {
            heights[offsetY + squareSize / 2, offsetX + squareSize] = runningAverage + GetRandHeight(depth);
        }

        //top diamond
        runningAverage = AveragePoints(topLeft, centerPoint, topRight);

        if (offsetY - squareSize / 2 > 0 && heights[offsetY - squareSize / 2, offsetX + squareSize / 2] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(topLeft, centerPoint, topRight, heights[offsetY - squareSize / 2, offsetX + squareSize / 2]);
        }

        if (heights[offsetY, offsetX + squareSize / 2] == TERRAIN_HEIGHT_WILDCARD)
        {
            heights[offsetY, offsetX + squareSize / 2] = runningAverage + GetRandHeight(depth);
        }

        //bottom diamond
        runningAverage = AveragePoints(bottomRight, centerPoint, bottomLeft);

        if (offsetY + squareSize * 1.5f < heights.GetLength(0) && heights[offsetY + (int)(squareSize * 1.5f), offsetX + squareSize / 2] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(bottomRight, centerPoint, topRight, heights[offsetY + (int)(squareSize * 1.5f), offsetX + squareSize / 2]);
        }

        if (heights[offsetY + squareSize, offsetX + squareSize/2] == TERRAIN_HEIGHT_WILDCARD)
        {
            heights[offsetY + squareSize, offsetX + squareSize / 2] = runningAverage + GetRandHeight(depth);
        }

        heights = DiamondSquare(heights, offsetX, offsetY, squareSize / 2, depth + 1);
        heights = DiamondSquare(heights, offsetX + squareSize/2, offsetY, squareSize / 2, depth + 1);
        heights = DiamondSquare(heights, offsetX, offsetY + squareSize/2, squareSize / 2, depth + 1);
        heights = DiamondSquare(heights, offsetX + squareSize/2, offsetY + squareSize/2, squareSize / 2, depth + 1);

        return heights;
    }

    public static Vector2 GetChunkIndexFromGlobalCoords(float x, float z)
    {
        Vector2 chunkIndex = new Vector2();
        // Need to account for negative zero, so we offset the negative by 1 chunk
        chunkIndex.x = (int)((x < 0 ? x - chunkSizeX : x) / chunkSizeX);
        chunkIndex.y = (int)((z < 0 ? z - chunkSizeZ : z) / chunkSizeZ);
        return chunkIndex;
    }

    public float GetHeightFromGlobalCoords(float xPos, float zPos)
    {
        Vector2 chunkCoords = GetChunkIndexFromGlobalCoords(xPos, zPos);
        Terrain terrain = terrainChunks[chunkCoords].GetComponent<Terrain>();
        Vector2 terrainRelativePosition = GetTerrainRelativePosition(xPos, zPos);
        return terrain.terrainData.GetHeight((int)(terrainRelativePosition.x / resolutionRatio), (int)(terrainRelativePosition.y / resolutionRatio));
    }

    public static Vector2 GetTerrainRelativePosition(float xPos, float zPos)
    {
        xPos = Mod(xPos, chunkSizeX);
        zPos = Mod(zPos, chunkSizeZ);
        // When we are positive, each terrain origin is 512 from the previous
        // When we are negative, each terrain origin is -512 from the previous
        // So in order to get the height at local x,z 
        // when we are negative we need to subtract the modded position from chunkSize
        if (xPos < 0)
        {
            xPos = chunkSizeX + xPos;
        }
        if (zPos < 0)
        {
            zPos = chunkSizeZ + zPos;
        }
        return new Vector2(xPos, zPos);
    }
    
    public static Vector2 GetChunkIndexFromCameraPos(Vector3 cameraPosition)
    {
        return GetChunkIndexFromGlobalCoords(cameraPosition.x, cameraPosition.z);
    }

    static float AveragePoints(float p1, float p2, float p3, float p4)
    {
        return (p1 + p2 + p3 + p4)/4;
    }

    static float AveragePoints(float p1, float p2, float p3)
    {
        return (p1 + p2 + p3)/3;
    }

    static float GetRandHeight(int depth)
    {
        return UnityEngine.Random.Range(-0.13f, 0.13f) / Mathf.Pow(2, depth);
    }

    // Only x,z coords are used. This way we can pass in 3d objects without converting
    // Assumes rectangular selection of terrains
    // position is center
    public MultiTerrain GetTerrainsInArea(Vector3 position, Vector3 area)
    {
        MultiTerrain mt = new MultiTerrain();
        float minX = position.x - area.x / 2,
            minY = position.z - area.z / 2,
            maxX = position.x + area.x / 2,
            maxY = position.z + area.z / 2;

        Vector2 minTerrainIndex = GetChunkIndexFromGlobalCoords(minX, minY);
        Vector2 maxTerrainIndex = GetChunkIndexFromGlobalCoords(maxX, maxY);
        Vector2 minTerrainCoords = GetTerrainRelativePosition(minX, minY);
        Vector2 maxTerrainCoords = GetTerrainRelativePosition(maxX, maxY);
        Vector2 minLocalTerrainStartPos = GetTerrainRelativePosition(minX, minY);
        Vector2 maxLocalTerrainEndPos = GetTerrainRelativePosition(maxX, maxY);

        // Terrain resolution, not actual coordinates
        minTerrainCoords.x = (int)minTerrainCoords.x/2;
        minTerrainCoords.y = (int)minTerrainCoords.y/2;
        maxTerrainCoords.x = (int)maxTerrainCoords.x/2;
        maxTerrainCoords.y = (int)maxTerrainCoords.y/2;
        minLocalTerrainStartPos.x = (int)(minLocalTerrainStartPos.x/2);
        minLocalTerrainStartPos.y = (int)(minLocalTerrainStartPos.y / 2);
        maxLocalTerrainEndPos.x = (int)(maxLocalTerrainEndPos.x / 2) + 2; // fix me: magic 2 (too much truncation? will this break for other objects?)
        maxLocalTerrainEndPos.y = (int)(maxLocalTerrainEndPos.y / 2) + 2;

        // +1 because numTerrains is one more than the difference in indecies
        int numTerrainsX = (int)(maxTerrainIndex.x - minTerrainIndex.x) + 1;
        int numTerrainsY = (int)(maxTerrainIndex.y - minTerrainIndex.y) + 1;
        Vector2[,] terrainCoords = new Vector2[numTerrainsX, numTerrainsY];
        Vector2[,] localTerrainStartPos = new Vector2[numTerrainsX, numTerrainsY];
        Vector2[,] localTerrainEndPos = new Vector2[numTerrainsX, numTerrainsY];

        // foreach terrain in the affected area
        for (int y = 0; y < numTerrainsY; y++)
        {
            for (int x = 0; x < numTerrainsX; x++)
            {
                terrainCoords[x, y] = new Vector2(minTerrainIndex.x + x, minTerrainIndex.y + y);

                if (x == 0 && y == 0)
                {
                    localTerrainStartPos[x,y] = minLocalTerrainStartPos;
                }
                else if (x == 0)
                {
                    localTerrainStartPos[x, y] = new Vector2(minLocalTerrainStartPos.x, 0);
                }
                else if (y == 0)
                {
                    localTerrainStartPos[x, y] = new Vector2(0, minLocalTerrainStartPos.y);
                }
                else
                {
                    localTerrainStartPos[x,y] = new Vector2(0, 0);
                }

                if (x == numTerrainsX - 1 && y == numTerrainsY - 1)
                {
                    localTerrainEndPos[x, y] = maxLocalTerrainEndPos;
                }
                else if (x == numTerrainsX - 1)
                {
                    localTerrainEndPos[x, y] = new Vector2(maxLocalTerrainEndPos.x, Resolution + 1);
                }
                else if (y == numTerrainsY - 1)
                {
                    localTerrainEndPos[x, y] = new Vector2(Resolution + 1, maxLocalTerrainEndPos.y);
                }
                else
                {
                    localTerrainEndPos[x, y] = new Vector2(Resolution + 1, Resolution + 1);
                }
            }
        }

        mt.terrainCoords = terrainCoords;
        mt.localTerrainStartPos = localTerrainStartPos;
        mt.localTerrainEndPos = localTerrainEndPos;
        return mt;
    }

    public void FlattenTerrainUnderObject(RTSGameObject obj)
    {
        float newHeight = GetHeightFromGlobalCoords(obj.transform.position.x, obj.transform.position.z) / maxTerrainHeight;
        MultiTerrain objectArea = GetTerrainsInArea(obj.transform.position, obj.transform.localScale);
        for (int j = 0; j < objectArea.terrainCoords.GetLength(1); j++)
        {
            for (int i = 0; i < objectArea.terrainCoords.GetLength(0); i++)
            {
                TerrainData data;
                try
                {
                    data = terrainChunks[objectArea.terrainCoords[i, j]].GetComponent<Terrain>().terrainData;
                }
                catch (Exception e)
                {
                    Debug.Log("ERROR GETTING TERRAIN at coords: " + objectArea.terrainCoords[i, j]);
                    continue;
                }

                int dx = (int)(objectArea.localTerrainEndPos[i, j].x - objectArea.localTerrainStartPos[i, j].x);
                int dy = (int)(objectArea.localTerrainEndPos[i, j].y - objectArea.localTerrainStartPos[i, j].y);
                float[,] heights = data.GetHeights(0, 0, Resolution, Resolution);
                float[,] newHeights = new float[dy, dx];
                
                for (int y = 0; y < dy; y++)
                {
                    for (int x = 0; x < dx; x++)
                    {
                        newHeights[y, x] = newHeight;
                    }
                }

                data.SetHeights((int)objectArea.localTerrainStartPos[i, j].x, (int)objectArea.localTerrainStartPos[i, j].y, newHeights);
            }
        }
    }

    
    SplatPrototype BlueprintToSplatPrototype(TerrainTextureBlueprint blueprint)
    {
        SplatPrototype prototype = new SplatPrototype();
        prototype.texture = blueprint.albedo;
        prototype.normalMap = blueprint.normal;
        prototype.metallic = blueprint.metallic;
        prototype.tileSize = blueprint.size;
        prototype.tileOffset = blueprint.offset;
        return prototype;
    }

    TreePrototype BlueprintToTreePrototype(TreeBlueprint blueprint)
    {
        TreePrototype prototype = new TreePrototype();
        prototype.prefab = blueprint.prefab;
        prototype.bendFactor = blueprint.bendFactor;
        return prototype;
    }
}
