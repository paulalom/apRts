using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SimianCity
{
    class SCGameManager : GameManager
    {
        public SCTerrainManager terrainManager;

        protected override IEnumerator StartGame()
        {
            terrainManager = GameObject.Find("TerrainManager").GetComponent<SCTerrainManager>();

            LoadingScreenManager.SetLoadingProgress(0.05f);
            terrainManager.GenerateTerrain(13);
            LoadingScreenManager.SetLoadingProgress(1f);
            //worldmanager.setupworld
            yield return MainGameLoop();
        }

        protected override IEnumerator MainGameLoop()
        {
            yield return null;
        }

        public override void SetUpPlayer(int playerId, World world)
        {
            throw new NotImplementedException();
        }

        public override void ProduceFromMenu(Type type)
        {
            throw new NotImplementedException();
        }

        public override void ProduceFromMenu(Type type, int quantity)
        {
            throw new NotImplementedException();
        }
    }
}
