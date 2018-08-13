using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StructureConstructionStorage : Storage
{
    // neededItems may go negative, but we need to keep this to track incase someone adds extra then removes all
    public Dictionary<Type, int> totalRequiredItems, neededItems = new Dictionary<Type, int>(), usedItems = new Dictionary<Type, int>();
    
    public override int AddItem(Type type, int count, bool allOrNone = true)
    {
        int numItemsTaken = base.AddItem(type, count, allOrNone);
        if (neededItems.ContainsKey(type))
        {
            neededItems[type] -= count;
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
                Type type = item.Key;
                int count = item.Value;
                if (neededItems.ContainsKey(type))
                {
                    neededItems[type] -= count;
                }
            }
        }

        return success;
    }

    public override int TakeItem(Type type, int count, bool allOrNone = true)
    {
        int numItemsTaken = base.TakeItem(type, count, allOrNone);

        if (neededItems.ContainsKey(type))
        {
            neededItems[type] += count;
        }
        return numItemsTaken;
    }

    public override bool TakeItems(Dictionary<Type, int> items)
    {
        bool success = base.TakeItems(items);
        if (success)
        {
            foreach (KeyValuePair<Type, int> item in items)
            {
                if (neededItems.ContainsKey(item.Key))
                {
                    neededItems[item.Key] += item.Value;
                }
            }
        }
        return success;
    }

    public bool UseItems(Dictionary<Type, int> items)
    {
        return base.TakeItems(items);
    }
}
