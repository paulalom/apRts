using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StructureConstructionStorage : Storage
{
    public Dictionary<Type, int> totalRequiredItems, suppliedItems = new Dictionary<Type, int>(), usedItems = new Dictionary<Type, int>();
    public bool constructionInitiated = false;

    public override int AddItem(Type type, int count, bool allOrNone = true)
    {
        int numItemsTaken = base.AddItem(type, count, allOrNone);
        if (suppliedItems.ContainsKey(type))
        {
            suppliedItems[type] += count;
        }
        else
        {
            suppliedItems.Add(type, count);
        }
        return numItemsTaken;
    }

    public override bool AddItems(Dictionary<Type, int> items)
    {
        bool success = base.AddItems(items);
        if (success)
        {
            foreach (KeyValuePair<Type, int> item in items)
            {
                if (suppliedItems.ContainsKey(item.Key)){
                    suppliedItems[item.Key] += item.Value;
                }
                else
                {
                    suppliedItems.Add(item.Key, item.Value);
                }
            }
        }

        return success;
    }

    public override int TakeItem(Type type, int count, bool allOrNone = true)
    {
        int numItemsTaken = base.TakeItem(type, count, allOrNone);
        suppliedItems[type] -= numItemsTaken;
        return numItemsTaken;
    }

    public override bool TakeItems(Dictionary<Type, int> items)
    {
        bool success = base.TakeItems(items);
        if (success)
        {
            foreach (KeyValuePair<Type, int> item in items)
            {
                suppliedItems[item.Key] -= item.Value;
            }
        }
        return success;
    }

    public bool UseItems(Dictionary<Type, int> items)
    {
        return base.TakeItems(items);
    }
}
