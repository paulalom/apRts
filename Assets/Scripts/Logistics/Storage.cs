using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public enum ItemType
{
    None,
    Stone,
    Wood,
    Iron,
    Coal,

    Paper,
    Cars
}

[System.Serializable]
public class Item
{
    public ItemType type;
    public int count;
}

public class Storage : MonoBehaviour {
    //max number of items
    public int maxStorageSize;
    //raw item data
    private List<Item> items; 
    public UnityEvent onStorageChangedEvent;
	// Use this for initialization
	void Start ()
    {
        items = new List<Item>();

        onStorageChangedEvent.AddListener(DebugStorage);
    }

    public void AddItem(Item item)
    {
        Debug.Log("Got Item " + item.type);
        int curItemIndex = GetItem(item.type);

        if (curItemIndex == -1)
        {
            items.Add(item);
        }
        else
        {
            items[curItemIndex].count += item.count;
        }

        onStorageChangedEvent.Invoke();
    }

    public int GetItem(ItemType Type)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].type == Type)
            {
                return i;
            }
        }
        return -1;
    }

    public void RemoveItem(Item item)
    {
        int index = GetItem(item.type);

        if (index == -1)
        {
            return;
        }
        else if (items[index].count > item.count)
        {
            items[index].count -= item.count;
        }
        else
        {
            items.RemoveAt(index);
        }
        onStorageChangedEvent.Invoke();
    }

    void DebugStorage()
    {
        string debugMessage = this + "[";
        for (int i = 0; i < items.Count; i++)
        {
            debugMessage += items[i].type + ": " + items[i].count + ", ";
        }
        debugMessage += "]";

        Debug.Log(debugMessage);
    }
    
    public int GetItemCount(ItemType type)
    {
        int itemIndex = GetItem(type);
        if (itemIndex == -1)
        {
            return 0;
        }
        else
        {
            return items[itemIndex].count;
        }
    }

    public List<Item> GetItems()
    {
        return items;
    }
}
