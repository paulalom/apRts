using UnityEngine;
using System.Collections.Generic;

public class Harvester : MonoBehaviour {

    int harvesterLevel = 1;
    float levelQuantityMultiplier = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public bool Harvest(ResourceDeposit target)
    {
        Dictionary<RTSGameObjectType, int> resourcesToCollect = new Dictionary<RTSGameObjectType, int>();
        if (target.type == DepositType.MineralVein)
        {
            resourcesToCollect.Add(RTSGameObjectType.Iron, (int)(RTSGameObject.productionQuantity[RTSGameObjectType.Iron] * harvesterLevel * levelQuantityMultiplier));
            resourcesToCollect.Add(RTSGameObjectType.Coal, (int)(RTSGameObject.productionQuantity[RTSGameObjectType.Coal] * harvesterLevel * levelQuantityMultiplier));
        }
        else if (target.type == DepositType.Forest)
        {
            resourcesToCollect.Add(RTSGameObjectType.Wood, (int)(RTSGameObject.productionQuantity[RTSGameObjectType.Wood] * harvesterLevel * levelQuantityMultiplier));
        }
        else if (target.type == DepositType.Stone)
        {
            resourcesToCollect.Add(RTSGameObjectType.Stone, (int)(RTSGameObject.productionQuantity[RTSGameObjectType.Stone] * harvesterLevel * levelQuantityMultiplier));
        }

        if (target.GetStorage(gameObject).TakeItems(resourcesToCollect)) //Do they have the items?
        {
            if (!target.GetStorage(gameObject).AddItems(resourcesToCollect)) // Do we have room?
            {
                target.GetStorage(gameObject).AddItems(resourcesToCollect);
                return false;
            }
        }
        return true;
    }
}
