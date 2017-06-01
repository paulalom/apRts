using UnityEngine;
using System.Collections;

public class Structure : RTSGameObject {

    public bool underConstruction = true;

    public void DemolishStructure(string reason, GameManager gameManager, RTSGameObjectManager rtsGameObjectManager)
    {
        gameManager.CreateText(reason, transform.position);
        rtsGameObjectManager.DestroyUnit(this);
    }

    public void CompleteConstruction(RTSGameObjectManager rtsGameObjectManager)
    {
        rtsGameObjectManager.SpawnUnit(GetType(), transform.position, ownerId, gameObject, world);
        rtsGameObjectManager.DestroyUnit(this);
    }
}
