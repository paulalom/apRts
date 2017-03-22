using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class Player {

    public UnityEvent onSelectionChange;
    public List<RTSGameObject> selectedUnits;
    public List<RTSGameObject> units;
}
