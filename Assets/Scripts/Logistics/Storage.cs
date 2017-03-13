using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class Storage : MonoBehaviour {

    public int size;
    public int usedSpace;
    public int freeSpace; //todo
    //public int maxItemQtyPerSlot; todo
    Dictionary<Type, int> items = new Dictionary<Type, int>();
    public HashSet<Type> canContain = new HashSet<Type>();
    public UnityEvent onStorageChangedEvent = new UnityEvent();
    public UnityEvent onStorageAddEvent = new UnityEvent();
    public List<Type> requiredAccessComponents = new List<Type>();

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
            int qtyAdded = AddItem(kvp.Key, kvp.Value, true);
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
                    TakeItem(kvp2.Key, kvp2.Value);
                }
                return false;
            }
        }
        onStorageAddEvent.Invoke();
        return true;
    }
    
    public bool TakeItems(Dictionary<Type, int> items)
    {
        Dictionary<Type, int> itemsTaken = new Dictionary<Type, int>();
        foreach (KeyValuePair<Type, int> kvp in items)
        {
            int qtyTaken = TakeItem(kvp.Key, kvp.Value);
            if (qtyTaken == kvp.Value)
            {
                itemsTaken.Add(kvp.Key, kvp.Value);
            }
            else
            {
                //Couldn't do it, put everything back
                foreach (KeyValuePair<Type, int> kvp2 in itemsTaken)
                {
                    AddItem(kvp2.Key, kvp2.Value);
                }
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    /// <param name="allOrNone"> TODO </param>
    /// <returns>the number of items added</returns>
    public int AddItem(Type type, int count, bool allOrNone = true)
    {
        if (freeSpace < count && allOrNone == true)
        {
            return 0;
        }
        else if (allOrNone == false)
        {
            //todo
            return 0;
        }
        else
        {
            AddItemInternal(type, count);
            return count;
        }

        //onStorageChangedEvent.Invoke();
    }

    private void AddItemInternal(Type type, int count)
    {
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
        onStorageAddEvent.Invoke();
    }

    private void TakeItemInternal(Type type, int count)
    {
        if (items[type] == count)
        {
            items.Remove(type);
        }
        else if (items[type] < count)
        {
            throw new Exception("Invalid use of TakeItemInternal");
        }
        else
        {
            items[type] -= count;
        }
        freeSpace += count;
        usedSpace -= count;
    }

    /// <summary>
    /// Gets an item from the storage
    /// </summary>
    /// <param name="type">Type of item to take</param>
    /// <param name="count">Number of item to take</param>
    /// <param name="allOrNone">Whether to return 0 if amount in storage is less than requested</param>
    /// <returns>number of items taken</returns>
    public int TakeItem(Type type, int count, bool allOrNone = true)
    {
        if (!items.ContainsKey(type))
        {
            return 0;
        }
        else if (items[type] > count)
        {
            TakeItemInternal(type, count);
            return count;
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
                TakeItemInternal(type, count);
                return count;
            }
        }
        else // amount in storage = requested amount
        {
            TakeItemInternal(type, count);
            return count;
        }

        //onStorageChangedEvent.Invoke();
    }

    public bool HasItems(Dictionary<Type, int> hasItems) 
    {
        foreach(KeyValuePair<Type, int> item in hasItems)
        {
            if (!items.ContainsKey(item.Key) || items[item.Key] < item.Value)
            {
                return false;
            }
        }
        return true;
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
}
