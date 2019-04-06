using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleMages
{
    public class ArenaManager : TerrainManager
    {
        public GameObject platform;
        public GameObject playerPrefab;
        public List<GameObject> startLocations;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public List<BattleMage> SpawnPlayers(int numPlayers)
        {
            List<BattleMage> players = new List<BattleMage>();
            for (int i = 0; i < numPlayers && i < startLocations.Count; i++)
            {
                GameObject player = GameObject.Instantiate(playerPrefab, startLocations[i].transform.position, startLocations[i].transform.rotation);

                players.Add(player.GetComponent<BattleMage>());
            }
            return players;
        }

        public override float GetHeightFromGlobalCoords(float xPos, float zPos, World world)
        {
            throw new System.NotImplementedException();
        }

        public override bool DoesTerrainExistForPoint(Vector3 point, World world)
        {
            Vector3 min = platform.transform.position - platform.transform.localScale / 2;
            Vector3 max = platform.transform.position + platform.transform.localScale / 2;
            return point.x > min.x && point.x < max.x && point.z > min.z && point.z < max.z;
        }

        public override void ModifyTerrain(Vector3 position, float deltaH, int diameter, World world)
        {
            return;
        }

        public override void GenerateChunkAtPositionIfMissing(Vector3 position, World world)
        {
            return;
        }
    }
}