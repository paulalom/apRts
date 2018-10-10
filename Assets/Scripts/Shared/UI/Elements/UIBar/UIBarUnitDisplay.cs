using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarUnitDisplay : UIBarComponent
{
    RTSGameObjectManager rtsGameObjectManager;
    RTSGameObject prevSelectedUnit = null;
    GameObject displayObject = null;

    private void Awake()
    {
        rtsGameObjectManager = GameObject.Find("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
    }

    public override void UpdateDisplay(List<RTSGameObject> selectedUnits)
    {
        if (selectedUnits.Count != 1)
        {
            return;
        }
        RTSGameObject selectedUnit = selectedUnits[0];
        if (selectedUnit != prevSelectedUnit)
        {
            DisplayNewUnit(selectedUnit);
            prevSelectedUnit = selectedUnit;
        }
        UpdateDisplayStats(selectedUnit);
        AnimateUnitInDisplay(selectedUnit);
    }

    void DisplayNewUnit(RTSGameObject unit)
    {
        if (displayObject != null)
        {
            Destroy(displayObject);
        }
        displayObject = Instantiate(rtsGameObjectManager.modelPrefabs[unit.GetType().ToString() + "Model"], gameObject.transform);
        displayObject.transform.localScale = new Vector3(120, 120, 120);
        displayObject.transform.Rotate(new Vector3(0, 180, 0));
    }

    void UpdateDisplayStats(RTSGameObject unit)
    {

    }

    void AnimateUnitInDisplay(RTSGameObject unit)
    {

    }
}
