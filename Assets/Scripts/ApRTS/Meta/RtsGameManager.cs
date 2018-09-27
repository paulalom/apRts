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
            Debug.Log("Start time: " + DateTime.Now);

            WorldSettings worldSettings = worldManager.GetWorldSettings(worldManager.numWorlds);
            playerManager.numAIPlayers = worldSettings.aiPresenceRating;
            playerManager.gameManager = this;
            netStateManager = GameObject.Find("NetworkStateManager").GetComponent<NetworkStateManager>();
            commandManager.netStateManager = netStateManager;
            commandManager.playerManager = playerManager;

            LoadingScreenManager.SetLoadingProgress(0.05f);
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
                stepTimer.Stop();
                realTimeSinceLastStep += (int)stepTimer.ElapsedMilliseconds;
                stepTimer.Reset();
                stepTimer.Start();

                int stepDt = StepManager.fixedStepTimeSize;
                while (stepDt < realTimeSinceLastStep && (netStateManager.isServer || StepManager.CurrentStep < netStateManager.serverStep))
                {
                    StepGame(stepDt);
                    realTimeSinceLastStep -= stepDt;
                    netStateManager.Step();
                    StepManager.Step();
                    yield return new WaitForEndOfFrame();
                }
                /*
                if (StepManager.CurrentStep == netStateManager.serverStep)
                {
                    //realTimeSinceLastStep = 0; // Don't let the client accumulate time ahead of the server.. may be unnecessary
                }
                if (StepManager.CurrentStep % 50 == 0)
                {
                    Debug.Log("Time: " + DateTime.Now.TimeOfDay + ", Step: " + StepManager.CurrentStep + ", SStep: " + netStateManager.serverStep + ", rtsls: " + realTimeSinceLastStep);
                }*/
                yield return null;
            }
        }
    }
}
