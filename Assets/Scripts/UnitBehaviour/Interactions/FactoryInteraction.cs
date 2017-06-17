using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class FactoryInteraction
{

    public static List<Order> DumpCargoAtNearestDepot(RTSGameObject unit, Vector3 searchPosition, RTSGameObjectManager rtsGameObjectManager)
    {
        List<Order> dropOffOrders = new List<Order>(); // This should be a single order, but order does not yet support take of multiple items.

        Factory depot = GetTargetDepot(unit, searchPosition, rtsGameObjectManager);

        // No station meets criteria, unit should remain idle
        if (depot == null)
        {
            return dropOffOrders;
        }
        else
        {
            List<MyPair<Type, int>> items = unit.storage.GetItemsMyKVP(-1);
            if (items.Count != 0)
            {
                dropOffOrders.Add(new GiveOrder() { target = depot, items = items });
            }
            return dropOffOrders;
        }
    }

    // should merge the get target functions but ill do that later
    public static Factory GetTargetDepot(RTSGameObject unit, Vector3 searchPosition, RTSGameObjectManager rtsGameObjectManager)
    {
        List<Factory> factories = rtsGameObjectManager.GetAllComponentsInRangeOfTypeOwnedByPlayerInOrder<Factory>(searchPosition,
                                                                            AITacticsManager.rangeToSearchForResources,
                                                                            unit.ownerId,
                                                                            rtsGameObjectManager.rtsGameObjectLayerMask);
        if (factories == null || factories.Count == 0)
        {
            return null;
        }

        foreach (Factory factory in factories)
        {
            Storage storage = factory.GetComponent<Storage>();
            if (storage != null && storage.freeSpace >= AITacticsManager.shouldDepositAtFactoryThreshold)
            {
                return factory.GetComponent<Factory>();
            }
        }
        return null;
    }
}