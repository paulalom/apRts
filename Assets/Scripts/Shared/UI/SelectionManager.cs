using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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

    public override void MyAwake()
    {
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mouseDown = mouseDownVectorSentinel;
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
        List<RTSGameObject> selectedUnits = playerManager.GetPlayerSelectedUnits();
                
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
                playerManager.PlayerSelectedUnits.Clear();
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
            bool previouslySelected = playerManager.GetPlayerSelectedUnits().Contains(unit);

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

    public void Select(RTSGameObject obj, bool select)
    {
        if (select)
        {
            playerManager.PlayerSelectedUnits.Add(obj.unitId);
        }
        else
        {
            playerManager.PlayerSelectedUnits.Remove(obj.unitId);
        }
        obj.selected = select;
        obj.selectionCircle.enabled = select;
        playerManager.OnPlayerSelectionChange.Invoke();
    }

    public void SetSelectionToUnit(RTSGameObject newlySelectedUnit)
    {
        foreach(RTSGameObject previouslySelectedUnit in playerManager.GetPlayerSelectedUnits())
        {
            previouslySelectedUnit.selected = false;
            previouslySelectedUnit.selectionCircle.enabled = false;
        }
        playerManager.PlayerSelectedUnits.Clear();

        playerManager.PlayerSelectedUnits.Add(newlySelectedUnit.unitId);
        newlySelectedUnit.selected = true;
        newlySelectedUnit.selectionCircle.enabled = true;
        playerManager.OnPlayerSelectionChange.Invoke();
    }
}
