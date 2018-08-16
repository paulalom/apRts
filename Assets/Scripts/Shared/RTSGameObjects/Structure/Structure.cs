using UnityEngine;
using System.Collections;

public class Structure : RTSGameObject {

    public ConstructionInfo constructionInfo;

    public void DemolishStructure(string reason, GameManager gameManager, RTSGameObjectManager rtsGameObjectManager)
    {
        gameManager.CreateText(reason, transform.position);
        rtsGameObjectManager.DestroyUnit(this);
    }

    public void CompleteConstruction(RTSGameObjectManager rtsGameObjectManager)
    {
        RTSGameObject finishedStructure = rtsGameObjectManager.SpawnUnit(GetType(), transform.position, ownerId, gameObject, world);
        finishedStructure.storage.AddItems(storage.GetItems()); // transfer any excess or accidentally transferred items to new storage
        // finishedStructure.InOrderDefenses ... copy over health from taking damage during construction... also add construction increasing health over time
        rtsGameObjectManager.DestroyUnit(this);
    }

    // this should be in some sort of validationManager instead of on the unit itself, 
    // as we will end up having lots of dependencies the units dont need, like terrainManager
    public virtual bool ValidatePlacement(RTSGameObjectManager rtsGameObjectManager, Vector3 targetPosition)
    {
        return true;
    }
}
