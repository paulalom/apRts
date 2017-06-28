using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MyMonoBehaviour {

    UIManager uiManager;
    PlayerManager playerManager;
    public Texture2D selectionHighlight;
    public static Rect selectionBox = new Rect(0, 0, 0, 0);
    public static Vector3 mouseDownVectorSentinel = new Vector3(-99999, -99999, -99999);
    float mouseSlipTolerance = 4; // The square of the distance you are allowed to move your mouse before a drag select is detected
    public Vector3 mouseDown;

    public override void MyAwake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
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

    public void CheckSingleSelectionEvent(RaycastHit hit)
    {
        HashSet<RTSGameObject> selectedUnits = playerManager.ActivePlayer.selectedUnits;
        // objectClicked May be null
        RTSGameObject objectClicked = hit.collider.GetComponentInParent<RTSGameObject>();
        
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
                selectedUnits.Clear();
            }
            Select(objectClicked, true);
        }
    }

    // Selection needs to be reworked/refactored, i copied a tutorial and have been hacking at it
    public void CheckBoxSelectionEvent(Camera mainCamera)
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
            if (!uiManager.menuClicked && (Input.mousePosition - mouseDown).sqrMagnitude > mouseSlipTolerance) // && hit.collider.GetComponentInParent<RTSGameObject>() == null))
            {
                CheckSelected(playerManager.GetAllUnits(), mainCamera);
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

    public void CheckSelected(HashSet<RTSGameObject> units, Camera mainCamera)
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
        }

        bool selectingFriendlyUnitsOnly = DoesUnitListContainFriendlies(unitsInSelectionBox);

        foreach (RTSGameObject unit in units)
        {
            if (unit is Projectile)
            {
                continue;
            }
            bool selected = unitsInSelectionBox.Contains(unit) && (selectingFriendlyUnitsOnly ? unit.ownerId == playerManager.ActivePlayerId : true);
            bool previouslySelected = playerManager.PlayerSelectedUnits.Contains(unit);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                selected = selected || previouslySelected;
            }

            if (selected)
            {
                Select(unit, true);
            }
            else
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
            playerManager.PlayerSelectedUnits.Add(obj);
        }
        else
        {
            playerManager.PlayerSelectedUnits.Remove(obj);
        }
        obj.selected = select;
        obj.selectionCircle.enabled = select;
        playerManager.OnPlayerSelectionChange.Invoke();
    }

    public void SetSelectionToUnit(RTSGameObject newlySelectedUnit)
    {
        foreach(RTSGameObject previouslySelectedUnit in playerManager.PlayerSelectedUnits)
        {
            previouslySelectedUnit.selected = false;
            previouslySelectedUnit.selectionCircle.enabled = false;
        }
        playerManager.PlayerSelectedUnits.Clear();

        playerManager.PlayerSelectedUnits.Add(newlySelectedUnit);
        newlySelectedUnit.selected = true;
        newlySelectedUnit.selectionCircle.enabled = true;
        playerManager.OnPlayerSelectionChange.Invoke();
    }
}
