using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct ItemRequests
{
    Type item;
    Storage storage;
}

[RequireComponent(typeof(Storage))]
public class Station : MonoBehaviour {
    /*
    private Storage storage;
    private List<Storage> watchList;

    private List<Route> connectedRoutes;

    void Awake()
    {
        storage = GetComponent<Storage>();
        watchList = new List<Storage>();
        connectedRoutes = new List<Route>();

        AddRoute(new Route());
    }
	
    public void RegisterStorageItem(Storage newStorage)
    {
        if (watchList.Contains(newStorage))
        {
            watchList.Add(newStorage);
            newStorage.onStorageChangedEvent.AddListener(UpdateStorageList);
        }
    }

    public void UnregisterStorageItem(Storage newStorage)
    {
        watchList.Remove(newStorage);
        newStorage.onStorageChangedEvent.RemoveListener(UpdateStorageList);
    }

    public void AddRoute(Route newRoute)
    {
        connectedRoutes.Add(newRoute);
        UpdateStorageList();
    }

    private void UpdateStorageList()
    {
        for (int i = 0; i < watchList.Count; i++)
        {
            List<Type> items = watchList[i].GetItems();

            for (int k = 0; k < items.Count; k++)
            {
                for (int j = 0; j < connectedRoutes.Count; j++)
                {
                    if (connectedRoutes[j].CanTransportItem(items[k]))
                    {
                        storage.AddItem(items[k]);
                        watchList[i].RemoveItem(items[k]);
                    }
                }
            }
        }
    }

    /*
    private ArrayList itemRequests;

    public void RequestItem(ItemRequests itemRequest)
    {
        itemRequests.Add(itemRequest);
    }
    */
}
