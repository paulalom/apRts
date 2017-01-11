using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ProductionData
{
    public Item targetItem;
    public Item[] requiredItems;
    //time to produce items
    public float requiredTime;
    [HideInInspector]
    public float lastUpdateTime;
}

[CreateAssetMenu (menuName = "Factory")]
public class FactoryData : ScriptableObject {
    
    public ProductionData[] productions;
}
