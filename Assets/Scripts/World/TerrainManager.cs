using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour, ICameraObserver  {

    [System.Serializable]
    public struct TreeBlueprint
    {
        public GameObject prefab;
        public float bendFactor;
    }

    [RequireComponent(typeof(Storage))]
    public class ResourceZone
    {
        
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

    [System.Serializable]
    public struct TerrainTextureBlueprint
    {
        public Texture2D albedo;
        public Texture2D normal;
        public int metallic;
        public Vector2 size, offset;
    }
    Transform projector;
    
    List<Vector2> loadedChunks = new List<Vector2>();
    Vector2[] visibleChunks = null;
    Terrain[] chunkGraphics = new Terrain[9];
    const int chunkSizeX = 512, chunkSizeY = 512;

    Vector2 currentChunkIndex = new Vector2(-9999,-9999);

    static Vector2 FADING_CHUNK = new Vector2(-9999, -9999);
    public static float TERRAIN_HEIGHT_WILDCARD = -1;
    SplatPrototype[] terrainTextures;
    public Biome[] biomeTextures;
    public TreeBlueprint[] trees;
    public TreePrototype[] terrainTrees;
    public Transform waterPlanePrefab;

    public int Resolution { get { return chunkSizeX / 2; } }

    // Use this for initialization
    void Start () {
        projector = GameObject.Find("BrushSizeProjector").transform;
        Camera.main.GetComponent<RTSCamera>().Subscribe(this);

        terrainTextures = new SplatPrototype[biomeTextures.Length * 5];
        terrainTrees = new TreePrototype[trees.Length];

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

        for (int i = 0; i < chunkGraphics.Length; i++)
        {
            GameObject go = new GameObject();
            go.name = "Chunk_" + i;
            chunkGraphics[i] = go.AddComponent<Terrain>();
            chunkGraphics[i].terrainData = new TerrainData();
            go.AddComponent<TerrainCollider>().terrainData = chunkGraphics[i].terrainData;
            chunkGraphics[i].terrainData.size = new Vector3(chunkSizeX/8, 600,chunkSizeY/8);
            chunkGraphics[i].terrainData.heightmapResolution = chunkSizeX / 2;
            chunkGraphics[i].terrainData.splatPrototypes = terrainTextures;
            chunkGraphics[i].terrainData.treePrototypes = terrainTrees;
            Transform waterPlane = GameObject.Instantiate(waterPlanePrefab, go.transform.position, Quaternion.identity) as Transform;
            waterPlane.transform.position = new Vector3(waterPlane.position.x + chunkSizeX/2, waterThreshold, waterPlane.position.z + chunkSizeY/2);
            waterPlane.localScale = new Vector3(5.1f, 1, 5.1f);
            waterPlane.SetParent(go.transform);
        }
        
        OnCameraMove(Camera.main.transform.position);

    }

    // Update is called once per frame
    void Update () {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButton(0))
            {
                ModifyTerrain(hit.point, .001f, 20);
            }

            projector.position = new Vector3(hit.point.x, projector.position.y, hit.point.z);
        }
	}

    void ModifyTerrain(Vector3 position, float amount, int diameter)
    {
        int terrainIndex = GetTerrainFromPos(position);
        Terrain mainTerrain = chunkGraphics[terrainIndex];

        position = GetRelativePosition(position);

        float[,] heights = mainTerrain.terrainData.GetHeights(0,0,Resolution,Resolution);
        int terrainPosX = (int)((position.x / mainTerrain.terrainData.size.x) * Resolution);
        int terrainPosY = (int)((position.z / mainTerrain.terrainData.size.z) * Resolution);
        int radius = diameter / 2;
        float[,] heightChange = new float[diameter, diameter];
        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                int x2 = x - radius;
                int y2 = y - radius;

                if (terrainPosY + y2 < 0 || terrainPosY + y2 >= Resolution)
                {
                    continue;
                }
                else if (terrainPosX + x2 < 0 || terrainPosX + x2 >= Resolution)
                {

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

    int GetTerrainFromPos(Vector3 position)
    {
        int xVal = (int)position.x / chunkSizeX, yVal = (int)position.y/chunkSizeY;

        for (int i = 0; i < visibleChunks.Length; i++)
        {
            if (visibleChunks[i].x == xVal && visibleChunks[i].y == yVal)
            {
                return i;
            }
        }
        //Shouldn't happen
        throw new System.Exception("Couldn't find terrain at x,y,z: " + position.x + ", " + position.y + ", " + position.z);
    }

    Vector3 GetRelativePosition(Vector3 position)
    {
        return new Vector3(Mod(position.x, chunkSizeX), position.y, Mod(position.z, chunkSizeY));
    }

    //This is needed because of mod behaviour for negative numbers
    float Mod(float f1, float f2)
    {
        return f1 - f2 * (int)(f1 / f2);
    }

    public void OnCameraMove(Vector3 newCameraPosition)
    {
        int chunkIndexX = (int)newCameraPosition.x / chunkSizeX;
        int chunkIndexY = (int)newCameraPosition.z / chunkSizeY;
        if (currentChunkIndex.x == chunkIndexX && currentChunkIndex.y == chunkIndexY)
        {
            return;
        }
        currentChunkIndex.x = chunkIndexX;
        currentChunkIndex.y = chunkIndexY;
        Vector2[] newVisibleChunks = new Vector2[9];
        List<int> leavingChunks = new List<int>();



        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                newVisibleChunks[(x + 1) + 3 * (y + 1)] = new Vector2(chunkIndexX + x, chunkIndexY + y);
            }
        }

        Terrain[] newChunkGraphics = new Terrain[9];
        List<int> freeTerrain = new List<int>();
        List<int> loadingIndexes = new List<int>();
        List<int> newIndex = new List<int>();
        // Get new chunks to be used and store chunks as they leave visibility
        for (int i = 0; i < 9; i++)
        {
            bool found = false;
            for (int j = 0; j < 9; j++)
            {
                if (visibleChunks == null)
                {
                    break;
                }
                if (newVisibleChunks[i].Equals(visibleChunks[j]))
                {
                    //leavingChunks.Add(i);
                    visibleChunks[j] = FADING_CHUNK;
                    newChunkGraphics[i] = chunkGraphics[j];
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                loadingIndexes.Add(i);
            }
        }
        if (visibleChunks != null)
        {
            for (int i = 0; i < 9; i++)
            {
                if (visibleChunks[i] != FADING_CHUNK)
                {
                    freeTerrain.Add(i);
                    SaveChunkToMemory(chunkGraphics[i], visibleChunks[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                freeTerrain.Add(i);
            }
        }
        /*
        foreach (int i in leavingChunks)
        {
            SaveChunkToMemory(chunkGraphics[i], visibleChunks[i]);
        }
        */

        visibleChunks = newVisibleChunks;
        for (int i = 0; i < loadingIndexes.Count; i++)
        {
            newChunkGraphics[loadingIndexes[i]] = chunkGraphics[freeTerrain[i]];
        }
        chunkGraphics = newChunkGraphics;

        for (int i = 0; i < loadingIndexes.Count; i++) {
            // LoadChunkFromMemory(newVisibleChunks[loadingIndexes[i]], freeTerrain[i]);
            LoadChunkFromMemory(visibleChunks[loadingIndexes[i]], loadingIndexes[i]);
        }
    }

    void LoadChunkFromMemory(Vector2 coordIndex, int graphicIndex)
    {
        GameObject terrainGO;

        if (loadedChunks.Contains(coordIndex))
        {
            terrainGO = chunkGraphics[graphicIndex].gameObject;
        }
        else
        {
            terrainGO = GenerateChunk(coordIndex, graphicIndex);
        }

        terrainGO.transform.position = new Vector3(chunkSizeX * coordIndex.x, 0, chunkSizeY * coordIndex.y);
    }

    GameObject GenerateChunk(Vector2 coordIndex, int graphicIndex)
    {
        Debug.Log("Generating Chunk (" + coordIndex.x + "," + coordIndex.y + "))");
        GameObject terrainGO = chunkGraphics[graphicIndex].gameObject;
        loadedChunks.Add(coordIndex);
        SetTerrainHeightMap(terrainGO.GetComponent<Terrain>(), coordIndex);
        SetTerrainTextures(terrainGO.GetComponent<Terrain>(), coordIndex);
        SetTerrainTrees(terrainGO.GetComponent<Terrain>());

        return terrainGO;
    }

    void SetTerrainHeightMap(Terrain terrain, Vector2 coordIndex)
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
        float[,] hm = GetTerrainHeightMap(new Vector2(coordIndex.x - 1, coordIndex.y));
        if (hm != null)
        {
            left = true;
            for(int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, 0] = hm[i, hm.GetLength(1) - 1];
            }
        }

        //right of new terrain = left of old terrain
        hm = GetTerrainHeightMap(new Vector2(coordIndex.x + 1, coordIndex.y));
        if (hm != null)
        {
            right = true;
            for (int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, heights.GetLength(1) - 1] = hm[i, 0];
            }
        }

        //top of new terrain = bottom of old terrain
        hm = GetTerrainHeightMap(new Vector2(coordIndex.x, coordIndex.y - 1));
        if (hm != null)
        {
            top = true;
            for (int i = 0; i < hm.GetLength(1); i++)
            {
                heights[0, i] = hm[hm.GetLength(0) - 1, i];
            }
        }

        //bottom of new terrain = top of old terrain
        hm = GetTerrainHeightMap(new Vector2(coordIndex.x, coordIndex.y + 1));
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
        for (int x = 0; x < Resolution; x+= Random.Range(0, 30))
        {
            for (int y = 0; y < Resolution; y+= Random.Range(0,30))
            {
                float height = terrain.terrainData.GetHeight(x,y);
                float steepness = terrain.terrainData.GetSteepness(x/(float)Resolution, y/(float)Resolution);

                if (Random.value > 0.3 + (steepness/30) && height >= waterThreshold - 10)
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

    void SetTerrainTextures(Terrain terrain, Vector2 coordIndex)
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

                alphaMap[y, x, currentBiome + 0] = 1 - isRocky - isCliff - isSnow - isRiverbank;
                alphaMap[y, x, currentBiome + 1] = isRocky - isCliff;
                alphaMap[y, x, currentBiome + 2] = isCliff;
                alphaMap[y, x, currentBiome + 3] = isRiverbank - isCliff - isRocky;
                alphaMap[y, x, currentBiome + 4] = isSnow - isRocky - isCliff - isRiverbank;
                
                
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    float[,] GetTerrainHeightMap(Vector2 coordIndex)
    {
        if (loadedChunks.Contains(coordIndex))
        {
            for (int i = 0; i< visibleChunks.Length; i++)
            {
                if (visibleChunks[i].x == coordIndex.x && visibleChunks[i].y == coordIndex.y)
                {
                    //Chunk is visible
                    return chunkGraphics[i].terrainData.GetHeights(0, 0, chunkGraphics[i].terrainData.heightmapWidth, chunkGraphics[i].terrainData.heightmapHeight);
                }
            }

            return LoadHeightmapFromMemory(coordIndex);
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

    float AveragePoints(float p1, float p2, float p3, float p4)
    {
        return (p1 + p2 + p3 + p4) * 0.25f;
    }

    float AveragePoints(float p1, float p2, float p3)
    {
        return (p1 + p2 + p3) * 0.33f;
    }

    float GetRandHeight(int depth)
    {
        return Random.Range(-0.2f, 0.2f) / Mathf.Pow(2, depth);
    }

    float[,] LoadHeightmapFromMemory(Vector2 coordIndex)
    {
        return null;
    }

    void SaveChunkToMemory(Terrain chunk, Vector2 index) {

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
