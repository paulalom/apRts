using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarMultiUnitInfoContainer : UIBarComponent {

    public override void UpdateDisplay(List<RTSGameObject> selectedUnits)
    {
        gameObject.SetActive(selectedUnits.Count > 1);
    }
}
