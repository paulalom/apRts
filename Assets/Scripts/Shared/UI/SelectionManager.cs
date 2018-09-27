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
    public int selectionSubgroup = -1;
    public int numSelectionSubgroups = 0;

    public class OnSelectionChangeEvent : UnityEvent { };
    public OnSelectionChangeEvent onSelectionChange = new OnSelectionChangeEvent();
    public OnSelectionChangeEvent onSelectionSubgroupChange = new OnSelectionChangeEvent();


    public override void MyAwake()
    {
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mouseDown = mouseDownVectorSentinel;
        onSelectionChange.AddListener(UpdateSelectionSubgroupCount);
    }

    void OnGUI()
    {
        if (mouseDown != mouseDownVectorSentinel)
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.DrawTexture(selectionBox, selectionHighlight);
        }
    }

    public void CheckSingleSelectionEvent(RTSGameObject objectClicked)
    {
        // Select one
        if (objectClicked != null && !(objectClicked is Projectile))// && selectableTypes.Contains(objectClicked.GetType()))
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                foreach (RTSGameObject unit in selectedUnits)
                {
                    unit.selected = false;
                    unit.selectionCircle.enabled = false;
                }
                if (selectedUnits.Count() > 0)
                {
                    selectedUnits.Clear();
                    onSelectionChange.Invoke();
                }
            }
            if (!objectClicked.selected)
            {
                Select(objectClicked, true);
            }
        }
    }

    // Selection needs to be reworked/refactored, i copied a tutorial and have been hacking at it
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

        bool selectingFriendlyUnitsOnly = DoesUnitListContainFriendlies(unitsInSelectionBox);

        foreach (RTSGameObject unit in units)
        {
            if (unit is Projectile)
            {
                continue;
            }
            bool selected = unitsInSelectionBox.Contains(unit) && (selectingFriendlyUnitsOnly ? unit.ownerId == playerManager.ActivePlayerId : true);
            bool previouslySelected = selectedUnits.Contains(unit);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                selected = selected || previouslySelected;
            }

            if (selected && !previouslySelected)
            {
                Select(unit, true);
            }
            else if (!selected)
            {
                Select(unit, false);
            }
        }
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
        onSelectionChange.Invoke();
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
        selectionSubgroup = selectionSubgroup >= numSelectionSubgroups - 1 ? 0 : selectionSubgroup + 1;       
    }
    public void DecrementSelectionSubgroup()
    {
        selectionSubgroup = selectionSubgroup <= 0 ? numSelectionSubgroups - 1 : selectionSubgroup - 1;
    }
    public void SetSelectionSubgroup(int groupId)
    {
        selectionSubgroup = Math.Max(Math.Min(groupId, 0), numSelectionSubgroups - 1);
    }

    void UpdateSelectionSubgroupCount()
    {
        int newNumSelectionSubgroups = selectedUnits.Select(x => x.GetType()).Distinct().Count();
        if (newNumSelectionSubgroups != numSelectionSubgroups)
        {
            numSelectionSubgroups = newNumSelectionSubgroups;

            if (selectionSubgroup >= numSelectionSubgroups) // numSelectedSubgroups has decreased, adjust selectionSubgroup accordingly
            {
                selectionSubgroup = numSelectionSubgroups - 1; // -1 when 0 subgroups
            }
            else if (selectionSubgroup <= -1 && numSelectionSubgroups > 0)
            {
                selectionSubgroup = 0;
            }
            onSelectionSubgroupChange.Invoke();
        }
    }
}
