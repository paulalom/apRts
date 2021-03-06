﻿using UnityEngine;
using System.Collections;

public class WorldManager : MyMonoBehaviour {

    PlayerManager playerManager;
    GameManager gameManager;
    public int numWorlds = 0;

    public override void MyAwake()
    {
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public IEnumerator SetUpWorld(ApRTSTerrainManager terrainManager, RTSCamera mainCamera)
    {
        LoadingScreenManager.SetLoadingProgress(0.10f);
        WorldSettings worldSettings = GetWorldSettings(numWorlds);
        LoadingScreenManager.GetInstance().ReplaceTextTokens(LoadingScreenManager.GetWorldGenerationTextTokens(worldSettings));
        yield return null;
        // Loop to make loading bar look like its doing something
        for (int i = 0; i < 30; i++)
        {
            LoadingScreenManager.SetLoadingProgress(.15f + 0.02f * i);
            yield return null;
        }
        playerManager.ActiveWorld = GenerateWorld(worldSettings, terrainManager);
        LoadingScreenManager.SetLoadingProgress(0.85f);
        yield return null;
        numWorlds++;
        mainCamera.world = playerManager.ActiveWorld;
        for (int i = 1; i < playerManager.players.Count; i++)
        {
            gameManager.SetUpPlayer(i, playerManager.ActiveWorld);
        }
        LoadingScreenManager.SetLoadingProgress(0.99f);
        LoadingScreenManager.CompleteLoadingScreen();
    }

    public WorldSettings GetWorldSettings(int randomSeed)
    {
        return new WorldSettings()
        {
            randomSeed = 8,
            resourceAbundanceRating = WorldSettings.starterWorldResourceAbundance,
            resourceQualityRating = WorldSettings.starterWorldResourceRarity,
            sizeRating = 1, // WorldSettings.starterWorldSizeRating,
            numStartLocations = 4, //WorldSettings.starterWorldNumStartLocations,
            startLocationSizeRating = 4f, // WorldSettings.starterWorldStartLocationSizeRating,
            aiStrengthRating = WorldSettings.starterWorldAIStrengthRating,
            aiPresenceRating = 0 // WorldSettings.starterWorldAIPresenceRating
        };
    }

    World GenerateWorld(WorldSettings worldSettings, ApRTSTerrainManager terrainManager)
    {
        World world = new World() { worldSettings = worldSettings };

        world.BuildWorld(terrainManager);

        return world;
    }
}
