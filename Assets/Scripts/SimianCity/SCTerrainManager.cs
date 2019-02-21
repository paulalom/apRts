using Assets.Scripts.Shared.World;
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

    public class River
    {
        public List<TerrainPolyEdge> terrainEdgesInOrder = new List<TerrainPolyEdge>();
    }

    public class SCTerrainManager : TerrainManager
    {
        public string loadingState = "";

        public override void MyStart()
        {
            base.MyStart();

        }

        IEnumerator GenerateTerrain(int seed)
        {
            loadingState = "Generating Polies...";
            MapPoly[] polies = GenerateTerrainPolies(seed);
            SetHeightsForPoints(polies, seed);
            List<River> rivers = GenerateRivers(polies, 0.1, seed);
            GenerateTerrainFromPolies(polies, seed);
        }

        MapPoly[] GenerateTerrainPolies(int seed)
        {
            System.Random r = new System.Random(seed);
            Voronoi vPoly = new Voronoi(10);
            int pointCount = 1000;
            int min = 0, max = 512;
            double[] randomPointsX = new double[pointCount], randomPointsY = new double[pointCount];
            MapPoly[] mapPolies = new MapPoly[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                randomPointsX[i] = r.Next(min, max);
                randomPointsY[i] = r.Next(min, max);

                mapPolies[i] = new MapPoly();
            }

            // GraphEdges returned have p1, p2 forming edge, as well as site1 and site2 int ids of bordering polygons.
            // The site id is the index of the xy point provided (so poly with site id 0 was generated from randomPointsX[0], randomPointsY[0])
            List<GraphEdge> terrainGraph = vPoly.generateVoronoi(randomPointsX, randomPointsY, min, max, min, max);
            Dictionary<Point, TerrainPolyPoint> polyPoints = new Dictionary<Point, TerrainPolyPoint>();
            foreach (GraphEdge edge in terrainGraph)
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

        void SetHeightsForPoints(MapPoly[] mapPolies, int seed)
        {
            System.Random r = new System.Random(seed+2);
            double lakeRate = .1, heightDifferentialBias = 0.1; // HDB -> 0 means none, positive means more highlands, negative means more water
            int maxHeight = 5, nextHeight = r.Next(0, 5); // start height at a random value

            loadingState = "Setting Heights...";
            foreach (MapPoly poly in mapPolies)
            {
                poly.height = nextHeight;

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
            foreach (MapPoly poly in mapPolies)
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

        List<River> GenerateRivers(MapPoly[] mapPolies, double riverRate, int seed)
        {
            System.Random r = new System.Random(seed+3);
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

            if (!CanEdgeBeRiver(nextEdge)) // Can't have a river along a cliff
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
            return edge.adjacentPoly1.height != edge.adjacentPoly2.height;
        }

        IEnumerator GenerateTerrainFromPolies(MapPoly[] polies, int seed)
        {
            yield return null;
        }
    }
}
