using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class SelectionManager : MyMonoBehaviour {

    PlayerManager playerManager;
    Camera mainCamera;
    public Texture2D selectionHighlight;
    public static Rect selectionBox = new Rect(0, 0, 0, 0);
    public static Vector3 mouseDownVectorSentinel = new Vector3(-99999, -99999, -99999);
    float mouseSlipTolerance = 4; // The square of the distance you are allowed to move your mouse before a drag select is detected
    public Vector3 mouseDown;
    public const int maxSelectedUnits = 100;
    public bool menuClicked = false;
    public List<RTSGameObject> selectedUnits = new List<RTSGameObject>();
    public List<Type> selectionSubgroups = new List<Type>();
    public int selectionSubgroup = -1;

    public class OnSelectionChangeEvent : UnityEvent { };
    public OnSelectionChangeEvent onSelectionChange = new OnSelectionChangeEvent();
    public class OnSelectionSubgroupChangeEvent : UnityEvent<List<Type>, int> { };
    public OnSelectionSubgroupChangeEvent onSelectionSubgroupChange = new OnSelectionSubgroupChangeEvent();


    public override void MyAwake()
    {
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mouseDown = mouseDownVectorSentinel;
        onSelectionChange.AddListener(UpdateSelectionSubgroups);
    }

    void OnGUI()
    {
        if (mouseDown != mouseDownVectorSentinel)
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.DrawTexture(selectionBox, selectionHighlight);
        }
    }

    // Select one
    public void CheckSingleSelectionEvent(RTSGameObject objectClicked)
    {
        if (objectClicked != null && !(objectClicked is Projectile))// && selectableTypes.Contains(objectClicked.GetType()))
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                if (selectedUnits.Count() > 0)
                {
                    foreach (RTSGameObject unit in selectedUnits)
                    {
                        unit.selected = false;
                        unit.selectionCircle.enabled = false;
                    }
                    selectedUnits.Clear();
                }
            }
            if (!objectClicked.selected)
            {
                Select(objectClicked, true);
            }
            onSelectionChange.Invoke();
        }
    }

    public void CheckBoxSelectionEvent()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (selectionBox.width < 0)
            {
                selectionBox.x += selectionBox.width;
                selectionBox.width = -selectionBox.width;
            }
            if (selectionBox.height < 0)
            {
                selectionBox.y += selectionBox.height;
                selectionBox.height = -selectionBox.height;
            }

            // Only do box selection / selection clearing if we drag a box or we click empty space
            if (!menuClicked && (Input.mousePosition - mouseDown).sqrMagnitude > mouseSlipTolerance) // && hit.collider.GetComponentInParent<RTSGameObject>() == null))
            {
                CheckSelected(playerManager.GetAllUnits());
            }
            mouseDown = mouseDownVectorSentinel;
        }
    }

    public void resizeSelectionBox()
    {
        if (Input.GetMouseButton(0))
        {
            selectionBox = new Rect(mouseDown.x,
                                    RTSCamera.InvertMouseY(mouseDown.y),
                                    Input.mousePosition.x - mouseDown.x,
                                    RTSCamera.InvertMouseY(Input.mousePosition.y) - RTSCamera.InvertMouseY(mouseDown.y));
        }
    }

    public void CheckSelected(HashSet<RTSGameObject> units)
    {
        HashSet<RTSGameObject> previousSelectedUnits = new HashSet<RTSGameObject>(selectedUnits);
        HashSet<RTSGameObject> unitsInSelectionBox = GetUnitsInSelectionBox(units);

        bool selectingFriendlyUnitsOnly = DoesUnitListContainFriendlies(unitsInSelectionBox);
        bool additiveSelection = Input.GetKey(KeyCode.LeftShift); // get from setting manager
        bool selectionChanged = false;

        if (!additiveSelection) {
            selectionChanged = UnselectUnitsNotInSelectionBox(previousSelectedUnits, unitsInSelectionBox);
        }

        selectionChanged = SelectUnitsInSelectionBox(unitsInSelectionBox, previousSelectedUnits, selectingFriendlyUnitsOnly) || selectionChanged;
        
        if (selectionChanged) {
            onSelectionChange.Invoke();
        }
    }

    HashSet<RTSGameObject> GetUnitsInSelectionBox(HashSet<RTSGameObject> units)
    {
        HashSet<RTSGameObject> unitsInSelectionBox = new HashSet<RTSGameObject>();
        foreach (RTSGameObject unit in units)
        {
            if (unit.flagRenderer.isVisible && !(unit is Projectile))// && selectableTypes.Contains(unit.GetType()))
            {
                Vector3 camPos = mainCamera.WorldToScreenPoint(unit.transform.position);
                camPos.y = RTSCamera.InvertMouseY(camPos.y);
                if (selectionBox.Contains(camPos))
                {
                    unitsInSelectionBox.Add(unit);
                }
            }
            if (unitsInSelectionBox.Count >= maxSelectedUnits)
            {
                break;
            }
        }
        return unitsInSelectionBox;
    }

    bool UnselectUnitsNotInSelectionBox(HashSet<RTSGameObject> previouslySelectedUnits, HashSet<RTSGameObject> unitsInSelectionBox) {
        bool selectionChanged = false;
        foreach (RTSGameObject unit in previouslySelectedUnits)
        {
            if (!unitsInSelectionBox.Contains(unit))
            {
                Select(unit, false);
                selectionChanged = true;
            }
        }
        return selectionChanged;
    }

    bool SelectUnitsInSelectionBox(HashSet<RTSGameObject> unitsInSelectionBox, HashSet<RTSGameObject> previouslySelectedUnits, bool selectingFriendlyUnitsOnly)
    {
        bool selectionChanged = false;
        foreach (RTSGameObject unit in unitsInSelectionBox.Except(previouslySelectedUnits))
        {
            if (unit is Projectile)
            {
                continue;
            }
            bool shouldSelectUnit = (selectingFriendlyUnitsOnly ? unit.ownerId == playerManager.ActivePlayerId : true);

            if (shouldSelectUnit)
            {
                Select(unit, true);
                selectionChanged = true;
            }
        }
        return selectionChanged;
    }

    bool DoesUnitListContainFriendlies(HashSet<RTSGameObject> units)
    {
        foreach (RTSGameObject unit in units)
        {
            if (unit.ownerId == playerManager.ActivePlayerId)
            {
                return true;
            }
        }
        return false;
    }

    public List<RTSGameObject> GetOrderableSelectedUnitsFromCurrentSubgroup()
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (RTSGameObject unit in selectedUnits.Where(x => x.GetType() == selectionSubgroups[selectionSubgroup] 
                                                               && x.ownerId == playerManager.ActivePlayerId))
        {
            units.Add(unit);
        }
        return units;
    }

    public List<RTSGameObject> GetOrderableSelectedUnits()
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (RTSGameObject unit in selectedUnits.Where(x => x.ownerId == playerManager.ActivePlayerId))
        {
            units.Add(unit);
        }
        return units;
    }

    public void Select(RTSGameObject obj, bool select)
    {
        if (select)
        {
            selectedUnits.Add(obj);
        }
        else
        {
            selectedUnits.Remove(obj);
        }
        obj.selected = select;
        obj.selectionCircle.enabled = select;
    }

    public void SetSelectionToUnit(RTSGameObject newlySelectedUnit)
    {
        foreach(RTSGameObject previouslySelectedUnit in selectedUnits)
        {
            previouslySelectedUnit.selected = false;
            previouslySelectedUnit.selectionCircle.enabled = false;
        }
        selectedUnits.Clear();

        selectedUnits.Add(newlySelectedUnit);
        newlySelectedUnit.selected = true;
        newlySelectedUnit.selectionCircle.enabled = true;
        onSelectionChange.Invoke();
    }

    public void IncrementSelectionSubgroup()
    {
        selectionSubgroup = selectionSubgroup >= selectionSubgroups.Count - 1 ? 0 : selectionSubgroup + 1;
        onSelectionSubgroupChange.Invoke(selectionSubgroups, selectionSubgroup);
    }
    public void DecrementSelectionSubgroup()
    {
        selectionSubgroup = selectionSubgroup <= 0 ? selectionSubgroups.Count - 1 : selectionSubgroup - 1;
        onSelectionSubgroupChange.Invoke(selectionSubgroups, selectionSubgroup);
    }
    public void SetSelectionSubgroup(int groupId)
    {
        selectionSubgroup = Math.Max(Math.Min(groupId, 0), selectionSubgroups.Count - 1);
        onSelectionSubgroupChange.Invoke(selectionSubgroups, selectionSubgroup);
    }

    void UpdateSelectionSubgroups()
    {
        int prevSelectionSubgroupsCount = selectionSubgroups.Count;
        List<Type> newSelectionSubgroups = selectedUnits.Select(x => x.GetType()).Distinct().ToList();
        selectionSubgroups = newSelectionSubgroups;

        // Clamp SelectionSubgroup to subgroups.count
        selectionSubgroup = selectionSubgroup == -1 ? 0 : selectionSubgroup;
        selectionSubgroup = Math.Min(selectionSubgroup, selectionSubgroups.Count - 1); // set to -1 when subgroup count is 0

        onSelectionSubgroupChange.Invoke(selectionSubgroups, selectionSubgroup);
    }

}
