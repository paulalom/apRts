using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class World {

    public WorldSettings worldSettings;
    public Dictionary<Vector2, GameObject> terrainChunks = new Dictionary<Vector2, GameObject>();
    public List<RTSGameObject> units = new List<RTSGameObject>();
    public List<Vector2> startLocations;
    public int worldSizeX, worldSizeY, worldArea, nextAvailableStartLocation = 0;
    private Vector2 vec2Sentinel = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

    public void BuildWorld(TerrainManager terrainManager)
    {
        if (worldSettings == null)
        {
            Debug.Log("Trying to build world with no settings!");
        }
        int chunkRadiusInWorld = worldSettings.sizeRating;
        
        terrainChunks = terrainManager.GetNewTerrainChunks(chunkRadiusInWorld, this);
        worldSizeX = (int)(worldSettings.sizeRating * TerrainManager.chunkSizeX * TerrainManager.resolutionRatio);
        worldSizeY = (int)(worldSettings.sizeRating * TerrainManager.chunkSizeZ * TerrainManager.resolutionRatio);
        
        worldArea = worldSizeX * worldSizeY;
        float playerStartSafeZoneArea = worldArea / Mathf.Max(1, worldSettings.numStartLocations) * (Mathf.Min(10, worldSettings.startLocationSizeRating) / 10);
        float exclusionBoundaryArea = worldArea - playerStartSafeZoneArea * worldSettings.numStartLocations;
        float maxPercentDeviationInPlayerAreas = 0.10f;
        bool success = false;

        List<MyBitMap> safeZones = null;
        MyBitMap safeZoneWorldMap = new MyBitMap(worldSizeX, worldSizeY);
        while (success == false)
        {
            success = true;
            
            startLocations = GenerateStartLocations(playerStartSafeZoneArea);
            if (startLocations == null)
            {
                success = false;
                continue;
            }
            safeZones = GenerateStartSafeZones(startLocations, playerStartSafeZoneArea, exclusionBoundaryArea, maxPercentDeviationInPlayerAreas);
            if (safeZones == null)
            {
                success = false;
                continue;
            }
        }

        foreach(MyBitMap safeZone in safeZones)
        {
            safeZoneWorldMap = new MyBitMap(worldSizeX, worldSizeY, safeZoneWorldMap.bitMap.Or(safeZone.bitMap));
        }
        terrainManager.SetTerrainHeights(safeZoneWorldMap, .4f, this);
    }

    List<MyBitMap> GenerateStartSafeZones(List<Vector2> startLocations, float playerStartSafeZoneArea, float exclusionBoundaryArea, float maxPercentDeviationInPlayerAreas)
    {
        MyBitMap worldStartLocationBitMap = new MyBitMap(worldSizeX, worldSizeY);
        MyBitMap worldStartLocationPlusExclusionBoundaryBitMap = new MyBitMap(worldSizeX, worldSizeY);
        List<MyBitMap> playerSafeZones = new List<MyBitMap>();

        MyBitMap playerSafeZone = null;
        foreach(Vector2 startLocation in startLocations)
        {
            bool gotSafeZone = false;
            for (int i = 0; i < 3; i++)
            {
                playerSafeZone = GenerateStartSafeZone(startLocation, playerStartSafeZoneArea, worldStartLocationPlusExclusionBoundaryBitMap, (int)(playerStartSafeZoneArea * maxPercentDeviationInPlayerAreas));
                if (playerSafeZone != null)
                {
                    playerSafeZones.Add(playerSafeZone);
                    gotSafeZone = true;
                    break;
                }
            }
            if (!gotSafeZone)
            {
                return null;
            }
            worldStartLocationPlusExclusionBoundaryBitMap = new MyBitMap(worldSizeX, worldSizeY, worldStartLocationPlusExclusionBoundaryBitMap.bitMap.Or(playerSafeZone.bitMap));
            MyBitMap exclusionBoundary = GenerateExclusionBoundary(playerSafeZone, startLocation, playerStartSafeZoneArea, exclusionBoundaryArea / worldSettings.numStartLocations, (int)(worldArea - playerStartSafeZoneArea * worldSettings.numStartLocations) * maxPercentDeviationInPlayerAreas, worldStartLocationPlusExclusionBoundaryBitMap);
            if (exclusionBoundary != null)
            {
                worldStartLocationPlusExclusionBoundaryBitMap = new MyBitMap(worldSizeX, worldSizeY, worldStartLocationPlusExclusionBoundaryBitMap.bitMap.Or(exclusionBoundary.bitMap));
            }
        }
        return playerSafeZones;
    }

    List<Vector2> GenerateStartLocations(float playerStartSafeZoneArea)
    {
        List<Vector2> startLocations = new List<Vector2>();
        float minDistToOtherPlayers = playerStartSafeZoneArea/((worldSizeX + worldSizeY)/2) * .10f;
        
        for (int i = 0; i < worldSettings.numStartLocations; i++)
        {
            Vector2 startLocation = GetNewStartLocation(startLocations, minDistToOtherPlayers);
            if (startLocation == vec2Sentinel)
            {
                return null;
            }
            startLocations.Add(startLocation);
        }
        return startLocations;
    }

    Vector2 GetNewStartLocation(List<Vector2> existingStartLocations, float minDistToOtherPlayers)
    {
        for (int i = 0; i < 100; i++) {
            Vector2 startLocation = new Vector2(Random.value * (worldSizeX - 1), Random.value * (worldSizeY - 1));
            bool tooClose = false;
            foreach (Vector2 otherPlayerLocation in existingStartLocations)
            {
                if ((otherPlayerLocation - startLocation).sqrMagnitude < minDistToOtherPlayers * minDistToOtherPlayers)
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose)
            {
                return startLocation;
            }
        }
        return vec2Sentinel;
    }

    MyBitMap GenerateExclusionBoundary(MyBitMap filledShapeToOutline, Vector2 filledShapeStartLocation, float filledShapeArea, float outlineArea, float maxDifferenceBetweenActualAndDesiredArea, MyBitMap worldMapWithExcludedAreasTrue)
    {
        List<int[]> safeZoneCoordinates = new List<int[]>();
        
        int maxIterations = 100, currentPositionIndex = 0;
        int[] currentPosition = new int[] { (int)filledShapeStartLocation.x, (int)filledShapeStartLocation.y };
        safeZoneCoordinates.Add(currentPosition);

        return GenerateRandomShapeOfArea(filledShapeArea, filledShapeArea + outlineArea, maxIterations, maxDifferenceBetweenActualAndDesiredArea, currentPositionIndex, safeZoneCoordinates, filledShapeToOutline, worldMapWithExcludedAreasTrue);
    }

    MyBitMap GenerateStartSafeZone(Vector2 start, float startSafeZoneArea, MyBitMap worldMapWithExcludedAreasTrue, int maxDifferenceBetweenActualAndDesiredArea)
    {
        MyBitMap safeZoneBitMap = new MyBitMap(worldMapWithExcludedAreasTrue.width, worldMapWithExcludedAreasTrue.height);
        List<int[]> safeZoneCoordinates = new List<int[]>();

        float shapeArea = 1; // start point counted
        int maxIterations = 100, currentPositionIndex = 0;
        int[] currentPosition = new int[] { (int)start.x, (int)start.y };
        safeZoneBitMap[(int)start.x, (int)start.y] = true;
        safeZoneCoordinates.Add(currentPosition);
        Debug.Log("Start Location:" + start);
        return GenerateRandomShapeOfArea(shapeArea, startSafeZoneArea, maxIterations, maxDifferenceBetweenActualAndDesiredArea, currentPositionIndex, safeZoneCoordinates, safeZoneBitMap, worldMapWithExcludedAreasTrue);
    }

    // Input bitmaps must be of the same dimensions
    MyBitMap GenerateRandomShapeOfArea(float currentShapeArea, float desiredShapeArea, int maxIterations, float maxDifferenceBetweenActualAndDesiredArea, int currentPositionIndex, List<int[]> shapeCoords, MyBitMap shapeBitMap, MyBitMap excludedAreasBitMap)
    {
        int totalIterations = 0;
        int[] currentPosition = shapeCoords[currentPositionIndex];
        while (currentShapeArea < desiredShapeArea && totalIterations < maxIterations)
        {
            List<int[]> cardinalSurroundingPoints = new List<int[]>();

            if (currentPosition[0] - 1 >= 0)
            {
                cardinalSurroundingPoints.Add(new int[] { currentPosition[0] - 1, currentPosition[1] });
            }
            if (currentPosition[0] + 1 < shapeBitMap.width)
            {
                cardinalSurroundingPoints.Add(new int[] { currentPosition[0] + 1, currentPosition[1] });
            }
            if (currentPosition[1] - 1 >= 0)
            {
                cardinalSurroundingPoints.Add(new int[] { currentPosition[0], currentPosition[1] - 1 });
            }
            if (currentPosition[1] + 1 < shapeBitMap.width)
            {
                cardinalSurroundingPoints.Add(new int[] { currentPosition[0], currentPosition[1] + 1 });
            }

            foreach (int[] point in cardinalSurroundingPoints)
            {
                if (!shapeBitMap[point[0], point[1]]
                    && !excludedAreasBitMap[point[0], point[1]]
                    && Random.value < (1 - currentShapeArea / desiredShapeArea)
                    )
                {
                    shapeBitMap[point[0], point[1]] = true;
                    shapeCoords.Add(point);
                    currentShapeArea++;
                }
            }

            if (currentPositionIndex == shapeCoords.Count - 1)
            {
                currentPositionIndex = 0;
                totalIterations++;
            }
            else
            {
                currentPositionIndex++;
            }
            currentPosition = shapeCoords[currentPositionIndex];
        }

        if (currentShapeArea >= desiredShapeArea - maxDifferenceBetweenActualAndDesiredArea)
        {
            Debug.Log("shapeCoords 0 (should be start location): " + shapeCoords[0][0] + ", " + shapeCoords[0][1]);
            return shapeBitMap;
        }
        else
        {
            return null;
        }
    }
}
