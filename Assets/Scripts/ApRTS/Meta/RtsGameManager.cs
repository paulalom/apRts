using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Assets.Scripts.ApRTS.Meta
{
    class RtsGameManager : GameManager
    {
        public override void Awake()
        {
            base.Awake();
            debug = true;
            Debug.Log("Start time: " + DateTime.Now);

            WorldSettings worldSettings = worldManager.GetWorldSettings(worldManager.numWorlds);
            playerManager.numAIPlayers = worldSettings.aiPresenceRating;
            playerManager.gameManager = this;
            netStateManager = GameObject.Find("NetworkStateManager").GetComponent<NetworkStateManager>();
            commandManager.netStateManager = netStateManager;
            commandManager.playerManager = playerManager;

            LoadingScreenManager.SetLoadingProgress(0.05f);

            //QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 120;
        }

        // Input happens on a unity timeframe
        protected void Update()
        {
            // too far behind, disable input.
            if (StepManager.CurrentStep < netStateManager.serverStep - 7)
            {
                return;
            }
            uiManager.HandleInput();
        }


        protected override IEnumerator StartGame()
        {
            yield return netStateManager.InitilizeLocalGame(this, playerManager);
            yield return worldManager.SetUpWorld(terrainManager, mainCamera);
            yield return MainGameLoop();
        }

        protected override IEnumerator MainGameLoop()
        {
            while (true)
            {
                realTimeSinceLastStep += Time.deltaTime;
                float stepDt = StepManager.GetDeltaStep();

                while (stepDt < realTimeSinceLastStep || StepManager.CurrentStep < netStateManager.serverStep - 1)
                {
                    if (StepManager.CurrentStep >= netStateManager.serverStep)
                    {
                        yield return null;
                    }
                    StepGame(stepDt);
                    realTimeSinceLastStep -= stepDt;
                    netStateManager.Step();
                    StepManager.Step();
                    yield return new WaitForEndOfFrame();
                }
                yield return null;
            }
        }
    }
}
