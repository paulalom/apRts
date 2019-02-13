using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBarUnitDisplay : UIBarComponent
{
    RTSGameObjectManager rtsGameObjectManager;
    public GameObject displayObjectContainer;
    public Text unitDisplayName;
    public StatusBarContainer healthStatusBarContainer, energyStatusBarContainer;

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
        CleanupOldDisplayUnit();
        DisplayUnitName(unit);
        DisplayNewUnitModel(unit);
        DisplayNewUnitStatusBars(unit);
    }

    void CleanupOldDisplayUnit()
    {
        if (displayObject != null)
        {
            Destroy(displayObject);
        }
    }

    void DisplayUnitName(RTSGameObject unit)
    {
        unitDisplayName.text = unit.GetType().ToString();
    }

    void DisplayNewUnitModel(RTSGameObject unit)
    {
        GameObject selectedUnitModelPrefab = rtsGameObjectManager.modelPrefabs[unit.GetType().ToString() + "Model"];

        displayObject = Instantiate(selectedUnitModelPrefab, displayObjectContainer.transform);
        displayObject.transform.localScale = new Vector3(120, 120, 120);
        displayObject.transform.Rotate(new Vector3(0, 180, 0));
        Transform flagRenderer = displayObject.transform.Find(unit.flagRenderer.name);
        if (flagRenderer != null)
        {
            flagRenderer.GetComponent<Renderer>().material.color = unit.flagRenderer.material.color;
        }
    }
    
    void DisplayNewUnitStatusBars(RTSGameObject unit)
    {
        GameObject statusBarPrefab = rtsGameObjectManager.nonUnitPrefabs["StatusBar"];
        float[] maxDefenseBarValues = unit.InOrderDefenses.Select(x => x.maxHitPoints).ToArray();
        string[] barDefenseTypes = unit.InOrderDefenses.Select(x => x.GetType().ToString()).ToArray();

        healthStatusBarContainer.InstantiateStatusBars(barDefenseTypes, maxDefenseBarValues, statusBarPrefab);

        // unit may have no applicable flowsystems (eg. battery, capacitor... not a wire)
        List<FlowSystem> flowSystems = unit.InOrderFlowSystemsWithStorage;
        if (flowSystems.Count != 0)
        {
            string[] barEnergyTypes = flowSystems.Select(x => x.GetType().ToString()).ToArray();
            float[] maxEnergyBarValues = flowSystems.Select(x => x.capacity).ToArray();
            energyStatusBarContainer.InstantiateStatusBars(barEnergyTypes, maxEnergyBarValues, statusBarPrefab);
        }
        else
        {
            energyStatusBarContainer.CleanupStatusBars();
        }
    }

    void UpdateDisplayStats(RTSGameObject unit)
    {
        float[][] defensesHitPointValues = unit.InOrderDefenses.Select(x => new float[] { x.currentHitPoints, x.maxHitPoints }).ToArray();

        for (int i = 0; i < defensesHitPointValues.Length; i++)
        {
            StatusBar bar = healthStatusBarContainer.statusBars[i];
            float defenseCurrentHitPoints = defensesHitPointValues[i][0];//, defenseMaxHitPoints = defensesHitPointValues[i][1];
            bar.SetBarValue(defenseCurrentHitPoints);
        }

        // unit may have no applicable flowsystems (eg. battery, capacitor... not a wire)
        List<FlowSystem> flowSystems = unit.InOrderFlowSystemsWithStorage;
        if (flowSystems != null)
        {
            for (int i = 0; i < flowSystems.Count; i++)
            {
                StatusBar bar = energyStatusBarContainer.statusBars[i];
                float currentValue = flowSystems[i].currentCharge;
                bar.SetBarValue(currentValue);
            }
        }
    }

    void AnimateUnitInDisplay(RTSGameObject unit)
    {

    }
}
