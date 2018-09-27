using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBarComponent : MyMonoBehaviour {
    
    public virtual void UpdateDisplay(List<RTSGameObject> selectedUnits) { }

}
