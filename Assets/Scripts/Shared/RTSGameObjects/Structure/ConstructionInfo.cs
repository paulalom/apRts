using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ConstructionInfo : MyMonoBehaviour
{
    public int constructionTimeRemaining;
    public Dictionary<Type, int> totalRequiredItems;
    public Dictionary<Type, int> itemsUsedInConstruction;
    public Storage storage;

    // We never remove types from this list incase items are removed from the storage in the future after they are added.
    public void RecordItemsUsedInConstruction(Dictionary<Type, int> usedItems)
    {
        foreach (KeyValuePair<Type, int> item in usedItems)
        {
            itemsUsedInConstruction[item.Key] += item.Value;
        }
    }

    public Dictionary<Type,int> GetRemainingItemsNeeded()
    {
        Dictionary<Type, int> remainingItemsNeeded = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> neededItem in totalRequiredItems)
        {
            int amountNeeded = neededItem.Value - itemsUsedInConstruction[neededItem.Key] - storage.GetItemCount(neededItem.Key);
            if (amountNeeded > 0)
            {
                remainingItemsNeeded.Add(neededItem.Key, amountNeeded);
            }
        }
        return remainingItemsNeeded;
    }
}

