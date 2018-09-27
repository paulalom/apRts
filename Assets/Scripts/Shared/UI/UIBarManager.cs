using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarManager : MyMonoBehaviour {

    public UIBar uiBar;
    public SelectionManager selectionManager;
    
    public override void MyStart () {
        base.MyStart();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        selectionManager.onSelectionSubgroupChange.AddListener(UpdateSubselectionCategory);
	}
	
	public override void MyUpdate () {
        UpdateBarComponents(selectionManager.selectedUnits);
    }

    public void UpdateSubselectionCategory()
    {
        if (selectionManager.numSelectionSubgroups == 0)
        {
            ClearSelectionSubgroup();
        }
        else
        {
            SetSelectionSubgroup(selectionManager.selectionSubgroup);
        }
    }

    void UpdateBarComponents(List<RTSGameObject> selectedUnits)
    {
        foreach (UIBarComponent barComponent in uiBar.barComponents)
        {
            barComponent.UpdateDisplay(selectedUnits);
        }
    }

    void SetSelectionSubgroup(int categoryId)
    {
        foreach (UIBarComponent barComponent in uiBar.barComponents)
        {
            if (barComponent is ISelectionSubgroupToggleable)
            {
                ((ISelectionSubgroupToggleable)barComponent).SetSelectionSubgroup(categoryId);
            }
        }
    }

    void ClearSelectionSubgroup()
    {
        foreach (UIBarComponent barComponent in uiBar.barComponents)
        {
            if (barComponent is ISelectionSubgroupToggleable)
            {
                ((ISelectionSubgroupToggleable)barComponent).ClearSelectionSubgroup();
            }
        }
    }
}
