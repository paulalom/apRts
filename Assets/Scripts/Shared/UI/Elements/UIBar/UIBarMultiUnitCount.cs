using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBarMultiUnitCount : UIBarComponent, ISelectionSubgroupToggleable
{
    public List<Text> unitCountTextFields;
    public Text elipsisTextField;
    public Text totalUnitCountTextField;
    public GameObject rowHighlight;
    List<Type> selectedTypes;
    
    void SetRowHighlightIndex(int index)
    {
        RectTransform highlightTransform = rowHighlight.GetComponent<RectTransform>();
        RectTransform targetTransform = unitCountTextFields[index].gameObject.GetComponent<RectTransform>();
        highlightTransform.anchorMax = targetTransform.anchorMax;
        highlightTransform.anchorMin = targetTransform.anchorMin;
    }

    public override void UpdateDisplay(List<RTSGameObject> selectedUnits)
    {
        selectedTypes = selectedUnits.Select(x => x.GetType()).Distinct().ToList();
        unitCountTextFields.ForEach(x => x.gameObject.SetActive(false));

        for (int i = 0; i < selectedTypes.Count && i < unitCountTextFields.Count; i++)
        {
            unitCountTextFields[i].text = selectedTypes[i].ToString() + ": " + selectedUnits.Count(x => x.GetType() == selectedTypes[i]).ToString();
            unitCountTextFields[i].gameObject.SetActive(true);
        }
        if (selectedTypes.Count > unitCountTextFields.Count)
        {
            elipsisTextField.gameObject.SetActive(true);
        }
        else
        {
            elipsisTextField.gameObject.SetActive(false);
        }
        totalUnitCountTextField.text = "Total: " + selectedUnits.Count;
    }

    public void SetSelectionSubgroup(int categoryId)
    {
        rowHighlight.SetActive(true);
        SetRowHighlightIndex(categoryId);
    }

    public void ClearSelectionSubgroup()
    {
        rowHighlight.SetActive(false);
    }
}
