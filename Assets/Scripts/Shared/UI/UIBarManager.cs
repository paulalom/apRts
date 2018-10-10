using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarManager : MyMonoBehaviour {

    public UIBar uiBar;
    public SelectionManager selectionManager;
    public UIBarCommandGrid commandGrid;
    
    public override void MyStart () {
        base.MyStart();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        selectionManager.onSelectionSubgroupChange.AddListener(UpdateSelectionSubgroup);
        selectionManager.onSelectionSubgroupChange.AddListener(commandGrid.OnSelectionSubgroupChange);
    }
	
	public override void MyUpdate () {
        UpdateBarComponents(selectionManager.selectedUnits);
    }

    public void UpdateSelectionSubgroup(List<Type> subgroups, int index)
    {
        SetSelectionSubgroup(subgroups, index);
    }

    void UpdateBarComponents(List<RTSGameObject> selectedUnits)
    {
        foreach (UIBarComponent barComponent in uiBar.barComponents)
        {
            barComponent.UpdateDisplay(selectedUnits);
        }
    }

    void SetSelectionSubgroup(List<Type> subgroups, int index)
    {
        foreach (UIBarComponent barComponent in uiBar.barComponents)
        {
            if (barComponent is ISelectionSubgroupToggleable)
            {
                ((ISelectionSubgroupToggleable)barComponent).SetSelectionSubgroup(subgroups, index);
            }
        }
    }
}
