using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory : MonoBehaviour {

    public static string ItemsToString(List<MyPair<Type, int>> items)
    {
        if (items == null) { return ""; }
        string itemString = "";
        foreach (MyPair<Type, int> pair in items)
        {
            itemString += pair.Key.ToString() + "." + pair.Value + ":";
        }
        itemString = itemString.TrimEnd(':');
        return itemString;
    }

    public static List<MyPair<Type, int>> GetItemsFromString(string itemString)
    {
        if (itemString == "") { return null; }
        List<MyPair<Type, int>> items = new List<MyPair<Type, int>>();
        string[] parsedItems = itemString.Split(':');
        foreach (string s in parsedItems)
        {
            string[] keyValue = s.Split('.');
            items.Add(new MyPair<Type, int>(Type.GetType(keyValue[0]), int.Parse(keyValue[1])));
        }
        return items;
    }
}
