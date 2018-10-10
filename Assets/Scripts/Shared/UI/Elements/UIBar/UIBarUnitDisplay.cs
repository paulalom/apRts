using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class UIBarUnitDisplay : UIBarComponent
{
    RTSGameObjectManager rtsGameObjectManager;
    RTSGameObject prevSelectedUnit = null;
    GameObject displayObject = null;
    public StatusBarContainer healthStatusBarContainer, energyStatusBarContainer;

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

        string[] barDefenseTypes = unit.InOrderDefenses.Select(x => x.GetType().ToString()).ToArray();
        float[] maxDefenseBarValues = unit.InOrderDefenses.Select(x => x.maxHitPoints).ToArray();

        StatusBar[] healthArmorShieldBars = healthStatusBarContainer.InstantiateStatusBars(barDefenseTypes, maxDefenseBarValues, rtsGameObjectManager.nonUnitPrefabs["StatusBar"]);

        // unit may have no applicable flowsystems (eg. battery, capacitor... not a wire)
        List<FlowSystem> flowSystems = unit.InOrderFlowSystemsWithStorage;
        if (flowSystems.Count != 0) {
            string[] barEnergyTypes = flowSystems.Select(x => x.GetType().ToString()).ToArray();
            float[] maxEnergyBarValues = flowSystems.Select(x => x.capacity).ToArray();
            StatusBar[] energyManaBars = energyStatusBarContainer.InstantiateStatusBars(barEnergyTypes, maxEnergyBarValues, rtsGameObjectManager.nonUnitPrefabs["StatusBar"]);
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
