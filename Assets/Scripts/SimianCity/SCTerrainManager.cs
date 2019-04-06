using System.Collections.Generic;
using UnityEngine;
using Voronoi2;
using System;
using System.Linq;
using System.Collections;

namespace SimianCity
{
    public struct Point : IEquatable<Point> // equatable is supposed to be ridiculously fast (unfortunately it doesnt matter for dictionary checks)
    {
        public int x, y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode() // supposed to implement this because struct reasons (boxing? slow? i forget)
        {
            return x + y; // + not * since we dont want integer overflow, 
            // collisions arent tooo likely to be an issue, and equals should be fast so they wont matter too much
            // if they become an issue we can tweak this
        }
    }

    public class TerrainPolyPoint
    {
        public TerrainPolyPoint(Point p)
        {
            x = p.x;
            y = p.y;
        }
        public TerrainPolyPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x, y;
        public List<TerrainPolyEdge> connectedEdges = new List<TerrainPolyEdge>();
    }

    public class TerrainPolyEdge
    {
        public TerrainPolyEdge(TerrainPolyPoint p1, TerrainPolyPoint p2, MapPoly poly1, MapPoly poly2)
        {
            this.p1 = p1;
            this.p2 = p2;
            adjacentPoly1 = poly1;
            adjacentPoly2 = poly2;
        }

        public int deltaHeight = 0; // positive means p2 is higher than p1
        public TerrainPolyPoint p1, p2;
        public MapPoly adjacentPoly1, adjacentPoly2;
    }

    public class MapPoly
    {
        public int id, height;
        public bool isWater;
        public TerrainPolyPoint center;
        public List<TerrainPolyEdge> edges = new List<TerrainPolyEdge>();
    }

    public class Map
    {
        public MapPoly[] polies;
        public Terrain[] terrainChunks;
        public float[,] mapHeights;
        public int chunkSizeXY; // chunkSize x and y
        public int mapSizeX, mapSizeY; // must be divisible by chunkSize
    }

    public class River
    {
        public List<TerrainPolyEdge> terrainEdgesInOrder = new List<TerrainPolyEdge>();
    }

    public class SCTerrainManager : TerrainManager
    {
        public string loadingState = "";

        #region terrainStuff
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
        #endregion

        public float waterThreshold = 1;
        public float snowThreshold = 4;

        public static float TERRAIN_HEIGHT_WILDCARD = -1;
        SplatPrototype[] terrainTextures;
        public Biome[] biomeTextures;
        public TreeBlueprint[] trees;
        public TreePrototype[] terrainTrees;
        public Transform waterPlanePrefab;

        public Dictionary<Point, GameObject> terrainChunks;

        public const int chunkSizeX = 514, chunkSizeZ = 514;
        public const int resolution = chunkSizeX / 2; // MUST BE POWER OF 2 + 1
        public const float resolutionRatio = resolution / (float)chunkSizeX;
        const int chunkGraphics1dArrayLength = 3; // THIS MUST ALWAYS BE ODD BECAUSE IM LAZY

        RTSGameObjectManager rtsGameObjectManager;

        // This is called before any other script's "Start" function
        // Do local inits here
        public override void MyAwake()
        {
            projector = GameObject.Find("BrushSizeProjector").transform;
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

        public override void MyStart()
        {
            base.MyStart();

        }

        public void GenerateTerrain(int seed)
        {
            loadingState = "Generating Map...";
            const int sizeX = 1024, sizeY = 1024;
            Map map = new Map()
            {
                mapSizeX = sizeX,
                mapSizeY = sizeY,
                mapHeights = new float[sizeX, sizeY]
            };
            loadingState = "Generating Polies...";
            map.polies = GenerateTerrainPolies(map, seed);
            SetHeightsForPoints(map, seed);
            //List<River> rivers = GenerateRivers(polies, 0.1, seed);
            terrainChunks = GenerateTerrainObjectsForMap(map, seed);

            var svg = GetMapAsSvg(map);
            return;
        }

        MapPoly[] GenerateTerrainPolies(Map map, int seed)
        {
            System.Random r = new System.Random(seed);
            Voronoi vPoly = new Voronoi(10);
            int pointCount = 1000;
            int min = 1;
            double[] randomPointsX = new double[pointCount], randomPointsY = new double[pointCount];
            MapPoly[] mapPolies = new MapPoly[pointCount];
            Dictionary<Point, TerrainPolyPoint> polyPoints = new Dictionary<Point, TerrainPolyPoint>();

            for (int i = 0; i < pointCount; i++)
            {
                var x = r.Next(min, map.mapSizeX);
                var y = r.Next(min, map.mapSizeY);
                randomPointsX[i] = x;
                randomPointsY[i] = y;

                mapPolies[i] = new MapPoly();
            }

            // GraphEdges returned have p1, p2 forming edge, as well as site1 and site2 int ids of bordering polygons.
            // The site id is the index of the xy point provided (so poly with site id 0 was generated from randomPointsX[0], randomPointsY[0])
            List<GraphEdge> terrainGraph = vPoly.generateVoronoi(randomPointsX, randomPointsY, min, map.mapSizeX, min, map.mapSizeY);

            foreach (var edge in terrainGraph)
            {
                // casting to int here will probably cause unforseen problems, but for now im going to pretend everything is okay
                // todo think more about it later
                // could use Point(x,y) and sort by distance to nearest point, adjusting the points which are too close (or re-rolling)
                var newPoint1 = new Point((int)edge.x1, (int)edge.y1);
                var newPoint2 = new Point((int)edge.x2, (int)edge.y2);
                TerrainPolyPoint newPolyPoint1, newPolyPoint2;

                // We want to share points between edges
                if (polyPoints.ContainsKey(newPoint1))
                {
                    newPolyPoint1 = polyPoints[newPoint1];
                }
                else
                {
                    newPolyPoint1 = new TerrainPolyPoint(newPoint1);
                    polyPoints.Add(newPoint1, newPolyPoint1);
                }
                if (polyPoints.ContainsKey(newPoint2))
                {
                    newPolyPoint2 = polyPoints[newPoint2];
                }
                else
                {
                    newPolyPoint2 = new TerrainPolyPoint(newPoint2);
                    polyPoints.Add(newPoint2, newPolyPoint2);
                }

                TerrainPolyEdge myEdge = new TerrainPolyEdge(newPolyPoint1,
                                                            newPolyPoint2,
                                                            mapPolies[edge.site1],
                                                            mapPolies[edge.site2]);
                // Track edges by point (node) for graph traversal (eg. river/road generation)
                newPolyPoint1.connectedEdges.Add(myEdge);
                newPolyPoint2.connectedEdges.Add(myEdge);

                // Track edges by polygon for terrain transitions (cliffs, texture transitions, etc.)
                // Might not need this if we just store edges as top level
                mapPolies[edge.site1].edges.Add(myEdge);
                mapPolies[edge.site2].edges.Add(myEdge);
                mapPolies[edge.site1].center = new TerrainPolyPoint((int)randomPointsX[edge.site1], (int)randomPointsY[edge.site1]);
                mapPolies[edge.site2].center = new TerrainPolyPoint((int)randomPointsX[edge.site2], (int)randomPointsY[edge.site2]);
            }
            
            return mapPolies;
        }

        void SetHeightsForPoints(Map map, int seed)
        {
            System.Random r = new System.Random(seed + 2);
            double lakeRate = .1, heightDifferentialBias = 0.1; // HDB -> 0 means none, positive means more highlands, negative means more water
            int maxHeight = 5, nextHeight = r.Next(0, 5); // start height at a random value

            loadingState = "Setting Heights...";
            foreach (MapPoly poly in map.polies)
            {
                poly.height = nextHeight;
                SetHeightsForPoly(map, poly);

                // This will happen twice per edge, but its okay because the second run will be the correct one.
                // Not worth optimizing
                foreach (TerrainPolyEdge edge in poly.edges)
                {
                    edge.deltaHeight = edge.adjacentPoly1.height - edge.adjacentPoly2.height;
                }

                if (r.NextDouble() < 0.5 + heightDifferentialBias)
                {
                    nextHeight = Math.Max(nextHeight + 1, maxHeight);
                }
            }

            loadingState = "Generating Lakes...";
            foreach (MapPoly poly in map.polies)
            {
                if (poly.height == 0)
                {
                    poly.isWater = true;
                }
                else if (r.NextDouble() < lakeRate)
                {
                    poly.isWater = true;
                }
            }
        }

        void SetHeightsForPoly(Map map, MapPoly poly)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;

            foreach (TerrainPolyEdge edge in poly.edges)
            {
                if (edge.p1.x < minX) minX = edge.p1.x;
                if (edge.p1.y < minY) minY = edge.p1.y;
                if (edge.p2.x < minX) minX = edge.p2.x;
                if (edge.p2.y < minY) minY = edge.p2.y;

                if (edge.p1.x > maxX) maxX = edge.p1.x;
                if (edge.p1.y > maxY) maxY = edge.p1.y;
                if (edge.p2.x > maxX) maxX = edge.p2.x;
                if (edge.p2.y > maxY) maxY = edge.p2.y;
            }

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (IsPointInPoly(x, y, poly))
                    {
                        map.mapHeights[x, y] = poly.height;
                    }
                }
            }
        }

        bool IsPointInPoly(int x, int y, MapPoly poly)
        {
            int numIntersectionsFromPolyToZero =
                poly.edges.Count(edge => doLinesIntersect(new Point(x, y), new Point(0, 0),
                    new Point(edge.p1.x, edge.p1.y), new Point(edge.p2.x, edge.p2.y)));

            return numIntersectionsFromPolyToZero == 1;
        }

        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        bool onSegment(Point p, Point q, Point r)
        {
            if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        int Orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            int val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0;  // colinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The main function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        bool doLinesIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        List<River> GenerateRivers(MapPoly[] mapPolies, double riverRate, int seed)
        {
            System.Random r = new System.Random(seed + 3);
            List<River> rivers = new List<River>();

            loadingState = "Generating Rivers ...";
            // Might be useful to track edges separately from polies
            foreach (MapPoly poly in mapPolies)
            {
                if (poly.isWater && r.NextDouble() < riverRate)
                {
                    int firstEdgeIndex = r.Next(0, poly.edges.Count - 1);
                    bool firstEdgePointisSource = r.NextDouble() < 0.5;
                    TerrainPolyEdge firstEdge = poly.edges[firstEdgeIndex];
                    River river = new River { terrainEdgesInOrder = new List<TerrainPolyEdge> { firstEdge } };
                    rivers.Add(river);

                    loadingState = "Generating River ... " + rivers.Count();
                    GenerateRiver(river, firstEdge, firstEdgePointisSource ? firstEdge.p1 : firstEdge.p2, r);
                }
            }

            return rivers;
        }


        void GenerateRiver(River river, TerrainPolyEdge nextEdge, TerrainPolyPoint originatingPoint, System.Random r)
        {
            TerrainPolyPoint nextPoint = nextEdge.p1 != originatingPoint ? nextEdge.p1 : nextEdge.p2;

            if (!CanEdgeBeRiver(nextEdge))
            {
                return;
            }
            else if (nextPoint.connectedEdges.Count == 1) // This is the edge of the map
            {
                river.terrainEdgesInOrder.Add(nextEdge);
                return;
            }
            else
            {
                river.terrainEdgesInOrder.Add(nextEdge);
                int currentEdgeIndex = nextPoint.connectedEdges.FindIndex(edge => edge == nextEdge);
                int nextEdgeIndex = r.Next(0, nextPoint.connectedEdges.Count - 2);
                if (nextEdgeIndex >= currentEdgeIndex) { nextEdgeIndex++; }
                TerrainPolyEdge nextNextEdge = nextPoint.connectedEdges[nextEdgeIndex];

                GenerateRiver(river, nextNextEdge, nextPoint, r);
            }
        }

        bool CanEdgeBeRiver(TerrainPolyEdge edge)
        {
            return edge.adjacentPoly1.height != edge.adjacentPoly2.height; // Can't have a river along a cliff
        }

        Dictionary<Point, GameObject> GenerateTerrainObjectsForMap(Map map, int seed)
        {
            return GetNewTerrainChunks(1, map);
        }

        public Dictionary<Point, GameObject> GetNewTerrainChunks(int terrainRadiusInChunks, Map map)
        {
            Dictionary<Point, GameObject> chunks = new Dictionary<Point, GameObject>();
            for (int y = 0; y < terrainRadiusInChunks; y++)
            {
                for (int x = 0; x < terrainRadiusInChunks; x++)
                {
                    Point chunkIndex = new Point(x, y);
                    Point minPos = new Point(chunkIndex.x * resolution, chunkIndex.y * resolution),
                            maxPos = new Point((chunkIndex.x + 1) * resolution, (chunkIndex.y + 1) * resolution);
                    GameObject terrainChunk = GenerateTerrainChunk(chunkIndex);
                    chunks[chunkIndex] = terrainChunk;
                    SetHeightsForTerrainChunk(terrainChunk, minPos, maxPos, map);
                }
            }

            return chunks;
        }

        GameObject GenerateTerrainChunk(Point worldSpaceChunkIndex)
        {
            GameObject terrainGO = new GameObject();
            Terrain terrain;
            terrainGO.name = "Chunk " + worldSpaceChunkIndex.x + ", " + worldSpaceChunkIndex.y;
            terrainGO.layer = LayerMask.NameToLayer("Terrain");
            terrain = terrainGO.AddComponent<Terrain>();
            terrain.terrainData = new TerrainData();
            terrainGO.AddComponent<TerrainCollider>().terrainData = terrain.terrainData;
            terrain.terrainData.heightmapResolution = resolution;
            terrain.terrainData.size = new Vector3(chunkSizeX, 30, chunkSizeZ);
            terrain.terrainData.splatPrototypes = terrainTextures;
            terrain.terrainData.treePrototypes = terrainTrees;
            terrain.transform.position = new Vector3(chunkSizeX * worldSpaceChunkIndex.x, 0, chunkSizeZ * worldSpaceChunkIndex.y);

            /* Water commented out because it lags older machines. Need to find a replacement. 
            Transform waterPlane = GameObject.Instantiate(waterPlanePrefab, terrainGO.transform.position, Quaternion.identity) as Transform;
            waterPlane.transform.position = new Vector3(waterPlane.position.x + chunkSizeX / 2, waterThreshold, waterPlane.position.z + chunkSizeZ / 2);
            waterPlane.localScale = new Vector3(5.1f, 1, 5.1f); // Yay magic
            waterPlane.SetParent(terrainGO.transform);*/

            // SetTerrainTextures(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex);
            // SetTerrainTrees(terrainGO.GetComponent<Terrain>());
            // SetTerrainResources(terrainGO.GetComponent<Terrain>(), worldSpaceChunkIndex);
            return terrainGO;
        }

        // may need to adjust for heights being 1 larger than terrain
        void SetHeightsForTerrainChunk(GameObject terrainChunk, Point minCoordinate, Point maxCoordinate, Map map)
        {
            TerrainData data = terrainChunk.GetComponent<Terrain>().terrainData;
            float[,] heights = new float[maxCoordinate.x - minCoordinate.x, maxCoordinate.y - minCoordinate.y];

            for (int y = minCoordinate.y; y < maxCoordinate.y; y++)
            {
                for (int x = minCoordinate.x; x < maxCoordinate.x; x++)
                {
                    // unity heights are mapped y,x, from 0-1 scaling by the configured height of terrain.terrainData.size
                    heights[y - minCoordinate.y, x - minCoordinate.x] = map.mapHeights[x, y] / data.size.y;
                }
            }

            data.SetHeights(0, 0, heights);
        }

        string GetMapAsSvg(Map map)
        {
            string header = $"<svg height=\"{map.mapSizeX + 2}\" width=\"1026\">";
            string footer = "</svg>";
            string body = "";
            float j = 0;
            float OldRange = map.polies.Length;
            float NewRange = 255;
            foreach (MapPoly p in map.polies)
            {
                int colorValue = (int)(j * NewRange / OldRange);
                foreach (TerrainPolyEdge e in p.edges)
                {
                    body += "<line x1='" + e.p1.x + "' y1='" + e.p1.y + "' x2='" + e.p2.x + "' y2='" + e.p2.y + "' style=\"stroke: rgb(" + colorValue + ", 0, 0); stroke - width:2\" />";
                }
                j++;
            }
            return header + body + footer;
        }

        public override float GetHeightFromGlobalCoords(float xPos, float zPos, World world)
        {
            throw new NotImplementedException();
        }

        public override bool DoesTerrainExistForPoint(Vector3 point, World world)
        {
            throw new NotImplementedException();
        }

        public override void ModifyTerrain(Vector3 position, float deltaH, int diameter, World world)
        {
            throw new NotImplementedException();
        }

        public override void GenerateChunkAtPositionIfMissing(Vector3 position, World world)
        {
            throw new NotImplementedException();
        }

        /*
        void TestVoronoi() {
            System.Random r = new System.Random(1);
            Voronoi vPoly = new Voronoi(10);
            int pointCount = 10;
            int min = 1;
            double[] randomPointsX = new double[pointCount], randomPointsY = new double[pointCount];
            MapPoly[] mapPolies = new MapPoly[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                randomPointsX[i] = r.Next(min, map.mapSizeX);
                randomPointsY[i] = r.Next(min, map.mapSizeY);

                mapPolies[i] = new MapPoly();
            }

            // GraphEdges returned have p1, p2 forming edge, as well as site1 and site2 int ids of bordering polygons.
            // The site id is the index of the xy point provided (so poly with site id 0 was generated from randomPointsX[0], randomPointsY[0])
            List<GraphEdge> terrainGraph = vPoly.generateVoronoi(randomPointsX, randomPointsY, min, map.mapSizeX, min, map.mapSizeY


            System.Random r = new System.Random(seed);
            Voronoi vPoly = new Voronoi(10);
            int pointCount = 1000;
            int min = 1;
            double[] randomPointsX = new double[pointCount], randomPointsY = new double[pointCount];
            MapPoly[] mapPolies = new MapPoly[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                randomPointsX[i] = r.Next(min, map.mapSizeX);
                randomPointsY[i] = r.Next(min, map.mapSizeY);

                mapPolies[i] = new MapPoly();
            }

            // GraphEdges returned have p1, p2 forming edge, as well as site1 and site2 int ids of bordering polygons.
            // The site id is the index of the xy point provided (so poly with site id 0 was generated from randomPointsX[0], randomPointsY[0])
            List<GraphEdge> terrainGraph = vPoly.generateVoronoi(randomPointsX, randomPointsY, min, map.mapSizeX, min, map.mapSizeY);
        }*/
    }
}
