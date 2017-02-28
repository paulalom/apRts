using UnityEngine;
using System.Collections;

/// <summary>
/// Used with operations which span multiple terrains
/// </summary>
public class MultiTerrain {

    public Vector2[,] terrainCoords;
    public Vector2[,] localTerrainStartPos; // x,y coords on the terrain where the area begins
    public Vector2[,] localTerrainEndPos; // ends
}
