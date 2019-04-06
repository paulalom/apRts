using System.Collections;
using UnityEngine;

public abstract class TerrainManager : MyMonoBehaviour
{
    public abstract float GetHeightFromGlobalCoords(float xPos, float zPos, World world);
    public abstract bool DoesTerrainExistForPoint(Vector3 point, World world);
    public abstract void ModifyTerrain(Vector3 position, float deltaH, int diameter, World world);
    public abstract void GenerateChunkAtPositionIfMissing(Vector3 position, World world);
    //public abstract IEnumerator SetUpWorld();
}