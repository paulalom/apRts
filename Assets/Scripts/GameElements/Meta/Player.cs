using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class Player {

    public string Name;
    public UnityEvent onSelectionChange;
    public List<RTSGameObject> selectedUnits;
    public List<RTSGameObject> units;
}
