using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class Storage : MonoBehaviour {

    public int size;
    public int usedSpace;
    public int freeSpace;
    //public int maxItemQtyPerSlot; todo
    Dictionary<Type, int> items = new Dictionary<Type, int>();
    public HashSet<Type> canContain = new HashSet<Type>();
    public List<Type> requiredAccessComponents = new List<Type>();

    public UnityEvent onStorageChangedEvent = new UnityEvent();
    public class OnStorageAddEvent : UnityEvent<Dictionary<Type, int>> { }
    public OnStorageAddEvent onStorageAddEvent = new OnStorageAddEvent();
    public class OnStorageTakeEvent : UnityEvent<Dictionary<Type, int>> { }
    public OnStorageTakeEvent onStorageTakeEvent = new OnStorageTakeEvent();

    void Awake()
    {
        requiredAccessComponents.Add(typeof(Transporter));
    }

    // Use this for initialization
    void Start ()
    {
        //onStorageChangedEvent.AddListener(DebugStorage);
    }

    public bool AddItems(Dictionary<Type, int> items)
    {
        Dictionary<Type, int> itemsAdded = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> kvp in items)
        {
            int qtyAdded = AddItemInternal(kvp.Key, kvp.Value, true);
            if (qtyAdded == kvp.Value)
            {
                itemsAdded.Add(kvp.Key, kvp.Value);
            }
            else
            {
                //Couldn't do it, put everything back
                //beware concurrency
                foreach (KeyValuePair<Type, int> kvp2 in itemsAdded)
                {
                    TakeItemInternal(kvp2.Key, kvp2.Value, false);
                }
                return false;
            }
        }
        onStorageAddEvent.Invoke(itemsAdded);
        onStorageChangedEvent.Invoke();
        return true;
    }

    // Internal does not trigger onChanged events in case we fail to add a list of items, so we expose this instead
    public int AddItem(Type type, int count, bool allOrNone = true)
    {
        int numItemsAdded = AddItemInternal(type, count, allOrNone);
        if (numItemsAdded > 0)
        {
            Dictionary<Type, int> itemsAdded = new Dictionary<Type, int>();
            itemsAdded.Add(type, numItemsAdded);
            onStorageAddEvent.Invoke(itemsAdded);
            onStorageChangedEvent.Invoke();
        }
        return numItemsAdded;
    }

    private int AddItemInternal(Type type, int count, bool allOrNone)
    {
        if (freeSpace == 0 || count == 0)
        {
            return 0;
        }
        else if (freeSpace < count)
        {
            if (allOrNone)
            {
                return 0;
            }
            else
            { 
                count = freeSpace;
            }
        }

        if (!items.ContainsKey(type))
        {
            items.Add(type, count);
        }
        else
        {
            items[type] += count;
        }

        freeSpace -= count;
        usedSpace += count;
        return count;
    }

    public bool TakeItems(Dictionary<Type, int> items)
    {
        Dictionary<Type, int> itemsTaken = new Dictionary<Type, int>();
        int numItemsToTake;
        foreach (KeyValuePair<Type, int> kvp in items)
        {
            if (kvp.Value == -1) // -1 take what you can
            {
                numItemsToTake = this.items[kvp.Key];
            }
            else
            {
                numItemsToTake = items[kvp.Key];
            }
            int qtyTaken = TakeItemInternal(kvp.Key, numItemsToTake, true);
            if (qtyTaken == numItemsToTake)
            {
                itemsTaken.Add(kvp.Key, numItemsToTake);
            }
            else
            {
                //Couldn't do it, put everything back
                foreach (KeyValuePair<Type, int> kvp2 in itemsTaken)
                {
                    AddItemInternal(kvp2.Key, kvp2.Value, false);
                }
                return false;
            }
        }
        onStorageTakeEvent.Invoke(itemsTaken);
        onStorageChangedEvent.Invoke();
        return true;
    }

    // Internal does not trigger onChanged events in case we fail to add a list of items, so we expose this instead
    public int TakeItem(Type type, int count, bool allOrNone = true)
    {
        int numItemsTaken;
        if (!items.ContainsKey(type))
        {
            return 0;
        }
        else
        {
            if (count == -1) // -1 take what you can
            {
                count = items[type];
            }

            numItemsTaken = TakeItemInternal(type, count, allOrNone);
            if (numItemsTaken > 0)
            {
                Dictionary<Type, int> itemsTaken = new Dictionary<Type, int>();
                itemsTaken.Add(type, count);
                onStorageTakeEvent.Invoke(itemsTaken);
                onStorageChangedEvent.Invoke();
            }
            return numItemsTaken;
        }
    }

    private int TakeItemInternal(Type type, int count, bool allOrNone)
    {
        if (!items.ContainsKey(type))
        {
            return 0;
        }
        if (items[type] == count)
        {
            items.Remove(type);
        }
        else if (items[type] < count)
        {
            if (allOrNone)
            {
                return 0;
            }
            else
            {
                count = items[type];
                items[type] -= count;
            }
        }
        else
        {
            items[type] -= count;
        }
        freeSpace += count;
        usedSpace -= count;
        return count;
    }

    public bool HasItems(Dictionary<Type, int> hasItems) 
    {
        foreach(KeyValuePair<Type, int> item in hasItems)
        {
            if (!HasItem(item.Key, item.Value))
            {
                return false;
            }
        }
        return true;
    }

    public bool HasItems(List<MyPair<Type, int>> hasItems)
    {
        foreach (MyPair<Type, int> item in hasItems)
        {
            if (!HasItem(item.Key, item.Value))
            {
                return false;
            }
        }
        return true;
    }

    public bool HasItem(Type type, int quantity)
    {
        return items.ContainsKey(type) && items[type] >= quantity;
    }

    public List<MyPair<Type, int>> GetItemsInInventoryInOrder(List<MyPair<Type, int>> itemsToGet)
    {
        List<MyPair<Type, int>> foundItems = new List<MyPair<Type, int>>();
        Dictionary<Type, int> qtyFoundItems = new Dictionary<Type, int>();
        foreach (MyPair<Type, int> item in itemsToGet)
        {
            if (items.ContainsKey(item.Key) && items[item.Key] >= item.Value + (qtyFoundItems.ContainsKey(item.Key) ? qtyFoundItems[item.Key] : 0))
            {
                foundItems.Add(item);
                if (!qtyFoundItems.ContainsKey(item.Key))
                {
                    qtyFoundItems.Add(item.Key, item.Value);
                }
                else
                {
                    qtyFoundItems[item.Key] += item.Value;
                }
            }
            else
            {
                break;
            }
        }
        return foundItems;
    }

    void DebugStorage()
    {
        string debugMessage = this + "[";
        foreach (KeyValuePair<Type, int> item in items) { 
            debugMessage += item.Key.ToString() + ": " + item.Value + ", ";
        }
        debugMessage += "]";

        Debug.Log(debugMessage);
    }

    public Dictionary<Type, int> GetItems()
    {
        return items;
    }
    public List<MyPair<Type, int>> GetItemsMyKVP()
    {
        List<MyPair<Type, int>> outItems = new List<MyPair<Type, int>>();
        foreach (KeyValuePair<Type,int> item in items){
            outItems.Add(new MyPair<Type, int>(item));
        }
        return outItems;
    }
    public List<MyPair<Type, int>> GetItemsMyKVP(int quantity)
    {
        List<MyPair<Type, int>> outItems = new List<MyPair<Type, int>>();
        foreach (KeyValuePair<Type, int> item in items)
        {
            outItems.Add(new MyPair<Type, int>(item.Key, quantity));
        }
        return outItems;
    }
}
