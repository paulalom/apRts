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
    //public Dictionary<Vector2, GameObject> terrainChunks = new Dictionary<Vector2, GameObject>();
    
    //We need to maintain the ability to scale this value
    public const int chunkSizeX = 130, chunkSizeZ = 130;
    public const int resolution = chunkSizeX / 2; // MUST BE POWER OF 2 + 1
    public const float resolutionRatio = resolution / (float)chunkSizeX;
    const int chunkGraphics1dArrayLength = 3; // THIS MUST ALWAYS BE ODD BECAUSE IM LAZY
    
    Vector2 oldCenterChunkGlobalIndex;

    public static float TERRAIN_HEIGHT_WILDCARD = -1;
    SplatPrototype[] terrainTextures;
    public Biome[] biomeTextures;
    public TreeBlueprint[] trees;
    public TreePrototype[] terrainTrees;
    public Transform waterPlanePrefab;

    RTSGameObjectManager rtsGameObjectManager;

    // This is called before any other script's "Start" function
    // Do local inits here
    void Awake()
    {
        projector = GameObject.Find("BrushSizeProjector").transform;
        Camera.main.GetComponent<RTSCamera>().Subscribe(this);
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
    
    void Start () {

        /* this should be removed once world generation is in place
        //This is a little bit of duplicated code from OnCameraMove for setup (which we dont run if we haven't moved).
        Vector2 newCameraPosition = Camera.main.transform.position;
        oldCenterChunkGlobalIndex = GetChunkIndexFromCameraPos(newCameraPosition);
        Vector2 cameraChunkMoveVector = GetCameraChunkMoveVector(newCameraPosition);
        Vector2 newCenterChunkGlobalIndex = GetNewCenterChunkGlobalIndex(cameraChunkMoveVector);
        GenerateNewTerrain(oldCenterChunkGlobalIndex, newCenterChunkGlobalIndex);*/
    }

    #region ModifyTerrain


    // meh buggy/fails around chunk edges.. whatever its not needed right now
    public void ModifyTerrain(Vector3 position, float amount, int diameter, World world)
    {
        Debug.Log("yep");
        Vector2 terrainIndex = GetChunkIndexFromGlobalCoords(position.x, position.z);
        if (!world.terrainChunks.ContainsKey(terrainIndex))
        {
            Debug.Log("yep2");
            return;
        }
        Terrain mainTerrain = world.terrainChunks[terrainIndex].GetComponent<Terrain>();
        Vector2 relativePosition = GetTerrainRelativePosition(position.x, position.z);

        float[,] heights = mainTerrain.terrainData.GetHeights(0, 0, resolution, resolution);
        int terrainPosX = (int)((relativePosition.x / mainTerrain.terrainData.size.x) * resolution);
        int terrainPosY = (int)((relativePosition.y / mainTerrain.terrainData.size.z) * resolution);
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

    public void GenerateNewTerrainOnCameraMove(Vector2 cameraChunkMoveVector, Vector2 newCenterChunkGlobalIndex, World world)
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
                if (!world.terrainChunks.ContainsKey(chunkIndex))
                {
                    world.terrainChunks[chunkIndex] = GenerateChunk(chunkIndex, world, world.terrainChunks);
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
                    if (!world.terrainChunks.ContainsKey(chunkIndex))
                    {
                        world.terrainChunks[chunkIndex] = GenerateChunk(chunkIndex, world, world.terrainChunks);
                    }
                }
            }
        }
    }

    public void OnCameraMove(Vector3 newCameraPosition, World world)
    {
        Vector2 cameraChunkMoveVector = GetCameraChunkMoveVector(newCameraPosition);

        if (cameraChunkMoveVector.x != 0 || cameraChunkMoveVector.y != 0)
        {
            Vector2 newCenterChunkGlobalIndex = GetNewCenterChunkGlobalIndex(cameraChunkMoveVector);

            //GenerateNewTerrainOnCameraMove(cameraChunkMoveVector, newCenterChunkGlobalIndex, world);

            //We have generated any missing terrain so now we just have to populate the visible chunks 
            //and set the graphics objects/fade out the leaving chunks
            //SetVisibleTerrain(cameraChunkMoveVector, newCenterChunkGlobalIndex);
        }
        oldCenterChunkGlobalIndex += cameraChunkMoveVector;
    }
    
    public Dictionary<Vector2, GameObject> GetNewTerrainChunks(int terrainRadiusInChunks, World world)
    {
        UnityEngine.Random.InitState(world.worldSettings.randomSeed);
        
        Dictionary<Vector2, GameObject> chunks = new Dictionary<Vector2, GameObject>();
        for (int y = 0; y < terrainRadiusInChunks; y++)
        {
            for (int x = 0; x < terrainRadiusInChunks; x++)
            {
                Vector2 chunkIndex = new Vector2(x, y);
                chunks[chunkIndex] = (GenerateChunk(chunkIndex, world, chunks, "Chunk " + x + ", " + y));
            }
        }

        return chunks;
    }

    GameObject GenerateChunk(Vector2 worldSpaceChunkIndex, World world, Dictionary<Vector2, GameObject> terrainChunks, string chunkName = null)
    {
        Debug.Log("Generating Chunk world index (" + worldSpaceChunkIndex.x + "," + worldSpaceChunkIndex.y + "))");
        GameObject terrainGO = new GameObject();
        Terrain terrain;
        terrainGO.name = (chunkName == null ? "Chunk_" + terrainChunks.Count : chunkName);
        terrainGO.layer = LayerMask.NameToLayer("Terrain");
        terrain = terrainGO.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        terrainGO.AddComponent<TerrainCollider>().terrainData = terrain.terrainData;
        terrain.terrainData.heightmapResolution = resolution;
        terrain.terrainData.size = new Vector3(chunkSizeX, 600, chunkSizeZ);
        terrain.terrainData.splatPrototypes = terrainTextures;
        terrain.terrainData.treePrototypes = terrainTrees;
        terrain.transform.position = new Vector3(chunkSizeX * worldSpaceChunkIndex.x, 0, chunkSizeZ * worldSpaceChunkIndex.y);

        /* Water commented out because it lags older machines. Need to find a replacement.
        Transform waterPlane = GameObject.Instantiate(waterPlanePrefab, terrainGO.transform.position, Quaternion.identity) as Transform;
        waterPlane.transform.position = new Vector3(waterPlane.position.x + chunkSizeX / 2, waterThreshold, waterPlane.position.z + chunkSizeZ / 2);
        waterPlane.localScale = new Vector3(5.1f, 1, 5.1f); // Yay magic
        waterPlane.SetParent(terrainGO.transform);
        */

        SetTerrainHeightMap(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex, terrainChunks);
        SetTerrainTextures(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex);
        SetTerrainTrees(terrainGO.GetComponent<Terrain>());
        SetTerrainResources(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex, world);
        return terrainGO;
    }

    void SetTerrainHeightMap(Terrain terrain, Vector2 worldSpaceChunkIndex, Dictionary<Vector2, GameObject> terrainChunks)
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
        float[,] hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x - 1, worldSpaceChunkIndex.y), terrainChunks);
        if (hm != null)
        {
            left = true;
            for(int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, 0] = hm[i, hm.GetLength(1) - 1];
            }
        }

        //right of new terrain = left of old terrain
        hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x + 1, worldSpaceChunkIndex.y), terrainChunks);
        if (hm != null)
        {
            right = true;
            for (int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, heights.GetLength(1) - 1] = hm[i, 0];
            }
        }

        //top of new terrain = bottom of old terrain
        hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x, worldSpaceChunkIndex.y - 1), terrainChunks);
        if (hm != null)
        {
            top = true;
            for (int i = 0; i < hm.GetLength(1); i++)
            {
                heights[0, i] = hm[hm.GetLength(0) - 1, i];
            }
        }

        //bottom of new terrain = top of old terrain
        hm = GetTerrainHeightMap(new Vector2(worldSpaceChunkIndex.x, worldSpaceChunkIndex.y + 1), terrainChunks);
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
        for (int x = 0; x < resolution; x+= UnityEngine.Random.Range(5, 30))
        {
            for (int y = 0; y < resolution; y+= UnityEngine.Random.Range(5,30))
            {
                float height = terrain.terrainData.GetHeight(x,y);
                float steepness = terrain.terrainData.GetSteepness(x/(float)resolution, y/(float)resolution);

                if (UnityEngine.Random.value > 0.3 + (steepness/30) && height >= waterThreshold - 10)
                {
                    TreeInstance instance = new TreeInstance();
                    instance.prototypeIndex = 0;
                    instance.position = new Vector3(x /(float)resolution, 0, y/ (float)resolution);
                    instance.widthScale = 1f;
                    instance.heightScale = 1f;
                    instance.color = Color.white;
                    instance.rotation = 0f;
                    terrain.AddTreeInstance(instance);
                }
            }
        }
    }

    void SetTerrainResources(Terrain terrain, Vector2 terrainGlobalCoords, World world)
    {
        //Use terrain to check criteria for generation
        //consider biomes in the future
        //consider merging tree generation, as trees should be a resource
        //consider a less naive approach for which resource we want to generate (do we already have a million of these?)
        //consider using fewer magic numbers
        
        // if x or y is 0 we may be placing objects at the very edge of a terrain, which can cause out of bounds exceptions
        for (int x = UnityEngine.Random.Range(1, 60); x < resolution; x += UnityEngine.Random.Range(0, 60))
        {
            for (int y = UnityEngine.Random.Range(1, 60); y < resolution; y += UnityEngine.Random.Range(0, 60))
            {
                float height = terrain.terrainData.GetHeight(x, y);
                float steepness = terrain.terrainData.GetSteepness(x / (float)resolution, y / (float)resolution);
                float resourceRandom;
                //Generate a resource?
                if (UnityEngine.Random.value > 0.6)
                {
                    //+res.graphicObject.transform.lossyScale.y
                    //Pick a resource
                    resourceRandom = UnityEngine.Random.value;
                    Dictionary<Type, int> items = new Dictionary<Type, int>();
                    if (resourceRandom > 0.4 + (steepness / 30) && height >= waterThreshold - 10)
                    {
                        items.Add(typeof(Wood), 20000);
                        rtsGameObjectManager.NewDeposit(DepositType.Forest,
                                                        items, 
                                                        new Vector3(terrain.transform.position.x + x * (chunkSizeX / resolution), 
                                                                    height, 
                                                                    terrain.transform.position.z + y * (chunkSizeZ / resolution)),
                                                        world
                                                        );
                    }
                    else if(resourceRandom > 0.4)
                    {
                        items.Add(typeof(Iron), 8000);
                        items.Add(typeof(Stone), 16000);
                        rtsGameObjectManager.NewDeposit(DepositType.Iron,
                                                        items,
                                                        new Vector3(terrain.transform.position.x + x * (chunkSizeX / resolution),
                                                                    height,
                                                                    terrain.transform.position.z + y * (chunkSizeZ / resolution)),
                                                        world
                                                        );
                    }
                    else if (resourceRandom > 0.3)
                    {
                        items.Add(typeof(Coal), 8000);
                        items.Add(typeof(Stone), 16000);
                        rtsGameObjectManager.NewDeposit(DepositType.Coal,
                                                        items,
                                                        new Vector3(terrain.transform.position.x + x * (chunkSizeX / resolution),
                                                                    height,
                                                                    terrain.transform.position.z + y * (chunkSizeZ / resolution)),
                                                        world
                                                        );
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

    float[,] GetTerrainHeightMap(Vector2 worldSpaceChunkIndex, Dictionary<Vector2, GameObject> terrainChunks)
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

    public float GetHeightFromGlobalCoords(float xPos, float zPos, World world)
    {
        try {
            Vector2 chunkCoords = GetChunkIndexFromGlobalCoords(xPos, zPos);
            Terrain terrain = world.terrainChunks[chunkCoords].GetComponent<Terrain>();
            Vector2 terrainRelativePosition = GetTerrainRelativePosition(xPos, zPos);
            return terrain.terrainData.GetHeight((int)(terrainRelativePosition.x * resolutionRatio), (int)(terrainRelativePosition.y * resolutionRatio));
        }
        catch(Exception e)
        {
            Debug.Log("Exception: Unit likely outside of world. Coords: " + xPos + ", " + zPos);
            return 0;
        }
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

    public bool DoesTerrainExistForPoint(Vector3 point, World world)
    {
        return world.terrainChunks.ContainsKey(GetChunkIndexFromGlobalCoords(point.x, point.z));
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
        return 0;// UnityEngine.Random.Range(-0.13f, 0.13f) / Mathf.Pow(2, depth);
    }

    // Only x,z coords are used. This way we can pass in 3d objects without converting
    // Assumes rectangular selection of terrains
    // position is center
    public MultiTerrain GetTerrainsInArea(Vector3 position, Vector3 area)
    {
        MultiTerrain mt = new MultiTerrain();
        float minX = position.x - area.x / 2f,
            maxX = position.x + (area.x - 1) / 2f,
            minY = position.z - area.z / 2f,
            maxY = position.z + (area.z - 1) / 2f;

        Vector2 minTerrainIndex = GetChunkIndexFromGlobalCoords(minX, minY);
        Vector2 maxTerrainIndex = GetChunkIndexFromGlobalCoords(maxX, maxY);
        Vector2 minLocalTerrainStartPos = GetTerrainRelativePosition(minX, minY);
        Vector2 maxLocalTerrainEndPos = GetTerrainRelativePosition(maxX, maxY);

        // Terrain resolution, not actual coordinates
        minLocalTerrainStartPos.x = (int)(minLocalTerrainStartPos.x * resolutionRatio);
        minLocalTerrainStartPos.y = (int)(minLocalTerrainStartPos.y * resolutionRatio);
        maxLocalTerrainEndPos.x = (int)(maxLocalTerrainEndPos.x * resolutionRatio);
        maxLocalTerrainEndPos.y = (int)(maxLocalTerrainEndPos.y * resolutionRatio);

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
                    localTerrainEndPos[x, y] = new Vector2(maxLocalTerrainEndPos.x, resolution);
                }
                else if (y == numTerrainsY - 1)
                {
                    localTerrainEndPos[x, y] = new Vector2(resolution, maxLocalTerrainEndPos.y);
                }
                else
                {
                    localTerrainEndPos[x, y] = new Vector2(resolution, resolution);
                }
            }
        }

        mt.terrainCoords = terrainCoords;
        mt.localTerrainStartPos = localTerrainStartPos;
        mt.localTerrainEndPos = localTerrainEndPos;
        return mt;
    }

    public void FlattenTerrainUnderObject(RTSGameObject obj, World world)
    {
        float newHeight = GetHeightFromGlobalCoords(obj.transform.position.x, obj.transform.position.z, world) / maxTerrainHeight;
        MultiTerrain objectArea = GetTerrainsInArea(obj.transform.position, obj.transform.localScale);
        SetTerrainHeights(objectArea, newHeight, world);
    }

    public void SetTerrainHeights(MyBitMap terrainToSetWorldMap, float percentToRaise, World world)
    {
        MultiTerrain terrains = GetTerrainsInArea(new Vector3(world.worldSizeX, 0, world.worldSizeY), new Vector3(world.worldSizeX*2, 0, world.worldSizeY*2));
        SetTerrainHeights(terrains, percentToRaise, world, terrainToSetWorldMap);
    }

    public void SetTerrainHeights(MultiTerrain terrains, float heightValue, World world, MyBitMap heightsToChange = null)
    {
        for (int j = 0; j < terrains.terrainCoords.GetLength(1); j++)
        {
            for (int i = 0; i < terrains.terrainCoords.GetLength(0); i++)
            {
                TerrainData data;
                try
                {
                    data = world.terrainChunks[terrains.terrainCoords[i, j]].GetComponent<Terrain>().terrainData;
                }
                catch (Exception e)
                {
                    Debug.Log("ERROR GETTING TERRAIN at coords: " + terrains.terrainCoords[i, j]);
                    continue;
                }

                int dx = (int)(terrains.localTerrainEndPos[i, j].x - terrains.localTerrainStartPos[i, j].x);
                int dy = (int)(terrains.localTerrainEndPos[i, j].y - terrains.localTerrainStartPos[i, j].y);
                float[,] newHeights = new float[dy, dx];
                float[,] oldHeights = data.GetHeights(0,0, resolution, resolution);
                for (int y = 0; y < dy; y++)
                {
                    for (int x = 0; x < dx; x++)
                    {
                        if (heightsToChange == null || // Origin of map is top left, origin in unity is bottom right, so we need to invert the indecies.
                            heightsToChange[(int)(x + terrains.terrainCoords[i, j].x * resolution),
                                            (int)(y + terrains.terrainCoords[i, j].y * resolution)] == true)
                        { // y,x because unity height maps have inverted indecies (probably for the reason mentioned above)
                            newHeights[y, x] = heightValue;
                        }
                        else
                        {
                            newHeights[y, x] = oldHeights[y, x];
                        }
                    }
                }

                data.SetHeights((int)terrains.localTerrainStartPos[i, j].x, (int)terrains.localTerrainStartPos[i, j].y, newHeights);
            }
        }
    }
    
    public void GenerateChunkAtPositionIfMissing(Vector3 position, World world)
    {
        if (!DoesTerrainExistForPoint(position, world))
        {
            Vector2 chunkIndex = GetChunkIndexFromGlobalCoords(position.x, position.z);
            world.terrainChunks[chunkIndex] = GenerateChunk(chunkIndex, world, world.terrainChunks);
        }
    }
    
    public void RaiseTerrain(MyBitMap terrainToRaiseWorldMap, float percentToRaise, World world)
    {

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
