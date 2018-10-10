using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarSingleUnitInfoContainer : UIBarComponent {

    public override void UpdateDisplay(List<RTSGameObject> selectedUnits)
    {
        gameObject.SetActive(selectedUnits.Count == 1);
    }
}
