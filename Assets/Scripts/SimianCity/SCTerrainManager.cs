using Assets.Scripts.Shared.World;
using System.Collections.Generic;
using UnityEngine;
using Voronoi2;
using System;

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
        public const int magicDefaultHeight = -99999;
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

        int x, y, height = magicDefaultHeight;
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

        public TerrainPolyPoint p1, p2;
        public MapPoly adjacentPoly1, adjacentPoly2;
    }

    public class MapPoly
    {
        public int id;
        public TerrainPolyPoint center;
        public List<TerrainPolyEdge> edges = new List<TerrainPolyEdge>();
    }

    public class SCTerrainManager : TerrainManager
    {
        public override void MyStart()
        {
            base.MyStart();

        }

        void GenerateTerrain(int seed)
        {
            MapPoly[] polies = GenerateTerrainPolies(seed);
            SetHeightsForPoints(polies);
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
            // casting to int here will probably cause unforseen problems, but for now im going to pretend everything is okay
            // todo think more about it later
            // could use Point(x,y) and sort by distance to nearest point, adjusting the points which are too close (or re-rolling)
            foreach (GraphEdge edge in terrainGraph)
            {
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
                mapPolies[edge.site1].edges.Add(myEdge);
                mapPolies[edge.site2].edges.Add(myEdge);
                mapPolies[edge.site1].center = new TerrainPolyPoint((int)randomPointsX[edge.site1], (int)randomPointsY[edge.site1]);
                mapPolies[edge.site2].center = new TerrainPolyPoint((int)randomPointsX[edge.site2], (int)randomPointsY[edge.site2]);
            }

            return mapPolies;
        }

        void SetHeightsForPoints(MapPoly[] mapPolies)
        {
            /*foreach(MapPoly poly in mapPolies)
            {
                if ()
            }*/
        }
    }
}
