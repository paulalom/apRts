using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMages
{

    public class BattleMagesGameManager : GameManager
    {
        ArenaManager arenaManager;
        UIManager uiManager;
        List<BattleMage> players;
        protected override IEnumerator StartGame()
        {
            arenaManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<ArenaManager>();
            uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
            players = arenaManager.SpawnPlayers(4);
            LoadingScreenManager.SetLoadingProgress(1f);
            LoadingScreenManager.CompleteLoadingScreen();
            //worldmanager.setupworld
            yield return MainGameLoop();
        }

        // Update is called once per frame
        protected void Update()
        {
            uiManager.HandleInput();
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