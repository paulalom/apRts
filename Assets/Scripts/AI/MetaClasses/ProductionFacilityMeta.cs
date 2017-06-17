using UnityEngine;
using System;
using System.Collections.Generic;

public class ProductionFacilityMeta {
    public HashSet<RTSGameObject> productionFacilities = new HashSet<RTSGameObject>();
    public Dictionary<RTSGameObject, Type> productionTargets = new Dictionary<RTSGameObject, Type>();

    public ProductionFacilityMeta()
    {

    }
    
    public void AddProductionFacility(RTSGameObject facility)
    {
        if (facility.GetComponent<Producer>() != null && facility.GetComponent<Worker>() == null)
        {
            productionFacilities.Add(facility);
        }
    }

    public void RemoveProductionFacility(RTSGameObject facility)
    {
        if (productionFacilities.Contains(facility))
        {
            productionTargets.Remove(facility);
            productionFacilities.Remove(facility);
        }
    }
}
