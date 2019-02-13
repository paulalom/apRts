using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarRadar : UIBarComponent
{
    public Camera mainCamera;
    public RectTransform mainCameraViewAreaDisplay;
    public World currentWorld;

    public override void MyAwake()
    {
        PlayerManager playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        playerManager.onWorldChangeEvent.AddListener(SetWorld);
    }

    //HashSet<RTSGameObject> radarObjects = new HashSet<RTSGameObject>();
    //List<GameObject> radarIcons = new List<GameObject>();

    void SetWorld(World world)
    {
        currentWorld = world;
    }

    public override void UpdateDisplay(List<RTSGameObject> selectedUnits)
    {
        base.UpdateDisplay(selectedUnits);
        Vector2 radarDimensions = new Vector2(mainCameraViewAreaDisplay.rect.width, mainCameraViewAreaDisplay.rect.height);
        Vector2 camRadarPos = new Vector2(mainCamera.transform.localPosition.z / (currentWorld.worldSizeY) * 100 - currentWorld.worldSizeY/2 + 25, 
                                          (-1 * mainCamera.transform.localPosition.x/(currentWorld.worldSizeX) * 100) + currentWorld.worldSizeY / 2 - 12.5f);
        Vector2 camRadarDimensions = new Vector2(currentWorld.worldSizeX / radarDimensions.x, currentWorld.worldSizeY / radarDimensions.y);

        mainCameraViewAreaDisplay.localPosition = camRadarPos;
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        RTSGameObject rtsGo = other.GetComponent<RTSGameObject>();

        rtsGo.onDestroyed.AddListener(RemoveObject);
        radarObjects.Add(rtsGo);
    }

    private void OnTriggerExit(Collider other)
    {
        RTSGameObject rtsGo = other.GetComponent<RTSGameObject>();

        rtsGo.onDestroyed.RemoveListener(RemoveObject);
        radarObjects.Remove(rtsGo);
    }

    void RemoveObject(RTSGameObject obj)
    {
        radarObjects.Remove(obj);
    }*/
}