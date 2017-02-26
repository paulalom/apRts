using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

    RTSCamera mainCamera;
    [HideInInspector]
    public TerrainManager terrainManager;
    RTSGameObjectManager rtsGameObjectManager;
    AbilityManager abilityManager;
    public OrderManager orderManager;
    List<RTSGameObject> units;
    public List<RTSGameObject> selectedUnits;
    static Vector3 vectorSentinel = new Vector3(-99999, -99999, -99999);
    float prevTime;
    Order nextOrder;
    public List<FloatingText> floatingText;

    public UnityEvent onSelectionChange;

    // these are hackish and needs to change
    public MyKVP<RTSGameObject, MyKVP<Type, int>> itemTransferSource = null;
    public Texture2D selectionHighlight;
    public static Rect selectionBox = new Rect(0, 0, 0, 0);
    public Vector3 mouseDown = vectorSentinel;
    public bool menuClicked = false;

    void Awake()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        abilityManager = GameObject.FindGameObjectWithTag("AbilityManager").GetComponent<AbilityManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        units = new List<RTSGameObject>();
        selectedUnits = new List<RTSGameObject>();
        floatingText = new List<FloatingText>();
    }

    // Use this for initialization
    void Start()
    {
        SetUpPlayer();
    }

    // Update is called once per frame
    void Update() {
        float now = Time.time;

        HandleInput();
        orderManager.CarryOutOrders(units);
        // Ideally this would only happen for units that have moved
        // maybe orderManager returns list of affected units and passes to rtsGameObjectManager
        rtsGameObjectManager.SnapToTerrainHeight(units);
        /*
        
        if (now - prevTime > 0.05)
        {
            
        }*/
        prevTime = now;
    }


    void HandleInput()
    {
        mainCamera.CheckCameraUpdate(); // Improve this eventually
        CheckKeyPress();
    }

    void CheckKeyPress()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool rayCast = Physics.Raycast(ray, out hit);

        //Left click. Assume mouseDown must preceed mouseUp
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseDown = Input.mousePosition;
        }
        if (Input.GetKey(KeyCode.G))
        {
            nextOrder = new Order() { type = OrderType.Guard, orderRange = 1f };
        }
        if (Input.GetKey(KeyCode.P))
        {
            nextOrder = new Order() { type = OrderType.Patrol, orderRange = 1f };
        }
        if (Input.GetKey(KeyCode.S))
        {
            nextOrder = new Order() { type = OrderType.Stop, orderRange = 1f };
        }
        if (Input.GetKey(KeyCode.H))
        {
            nextOrder = new Order() { type = OrderType.Harvest, orderRange = 15f };
        }
        if (Input.GetKey(KeyCode.F))
        {
            nextOrder = new Order() { type = OrderType.Follow, orderRange = 6f };
            
        }

        if (rayCast)
        {
            if (Input.GetKey(KeyCode.T))
            {
                try { //Try catch to swallow exception. FixMe
                    //Should be called raiseTerrain
                    terrainManager.ModifyTerrain(hit.point, .001f, 20);
                }
                catch (Exception e)
                {

                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (mouseDown == Input.mousePosition)
                {
                    // objectClicked May be null
                    RTSGameObject objectClicked = hit.collider.GetComponentInParent<RTSGameObject>();
                    if (nextOrder != null)
                    {
                        nextOrder.target = objectClicked;
                        nextOrder.targetPosition = hit.point;
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            foreach (RTSGameObject unit in selectedUnits)
                            {
                                orderManager.QueueOrder(unit, nextOrder);
                            }
                        }
                        else
                        {
                            foreach (RTSGameObject unit in selectedUnits)
                            {
                                orderManager.SetOrder(unit, nextOrder);
                            }
                        }
                        nextOrder = null;
                    }
                    else
                    {
                        // Select one
                        if (objectClicked != null)
                        {
                            if (!Input.GetKey(KeyCode.LeftShift))
                            {
                                foreach (RTSGameObject unit in selectedUnits)
                                {
                                    unit.selected = false;
                                    unit.flagRenderer.material.color = Color.white;
                                }
                                selectedUnits.Clear();
                            }

                            selectedUnits.Add(objectClicked);
                            objectClicked.selected = true;
                            objectClicked.flagRenderer.material.color = Color.red;
                        }
                    }
                }
            }

            // Right click to move
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                nextOrder = new Order() { type = OrderType.Move, targetPosition = hit.point };
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (RTSGameObject unit in selectedUnits)
                    {
                        orderManager.QueueOrder(unit, nextOrder);
                    }
                }
                else
                {
                    foreach (RTSGameObject unit in selectedUnits)
                    {
                        orderManager.SetOrder(unit, nextOrder);
                    }
                }
                nextOrder = null;
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                
            }
            if (Input.GetKey(KeyCode.C))
            {
                float cameraElevationRate = 1f;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - cameraElevationRate, mainCamera.transform.position.z);
                }
                else {
                    mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + cameraElevationRate, mainCamera.transform.position.z);
                }
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                rtsGameObjectManager.SpawnUnit(typeof(Factory), hit.point);
            }

            terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z) + 5, hit.point.z);

        }
        resizeSelectionBox();

        if (Input.GetMouseButtonUp(0))
        {
            // Only do box selection / selection clearing if we drag a box or we click empty space
            if (!menuClicked && (Input.mousePosition != mouseDown || !rayCast || hit.collider.GetComponentInParent<RTSGameObject>() == null))
            {
                CheckSelected(units);
            }
            mouseDown = vectorSentinel;
        }
        menuClicked = false;
    }

    // Selection needs to be reworked/refactored, i copied a tutorial and have been hacking at it

    void resizeSelectionBox()
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

        }

        if (Input.GetMouseButton(0))
        {
            selectionBox = new Rect(mouseDown.x,
                                    RTSCamera.InvertMouseY(mouseDown.y),
                                    Input.mousePosition.x - mouseDown.x,
                                    RTSCamera.InvertMouseY(Input.mousePosition.y) - RTSCamera.InvertMouseY(mouseDown.y));
        }
    }

    void OnGUI()
    {
        if (mouseDown != vectorSentinel)
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.DrawTexture(selectionBox, selectionHighlight);
        }
    }

    void CheckSelected(List<RTSGameObject> units)
    {
        foreach (RTSGameObject unit in units)
        { 
            if (unit.flagRenderer.isVisible)
            {
                Vector3 camPos = mainCamera.GetComponent<Camera>().WorldToScreenPoint(unit.transform.position);
                camPos.y = RTSCamera.InvertMouseY(camPos.y);
                unit.selected = selectionBox.Contains(camPos);
            }
            if (unit.selected)
            {
                unit.flagRenderer.material.color = Color.red;
                if (!selectedUnits.Contains(unit))
                {
                    Select(unit, true);
                }
            }
            else
            {
                unit.flagRenderer.material.color = Color.white;
                Select(unit, false);
            }
        }
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
        onSelectionChange.Invoke();
    }

    /* selection to be refactored ends here */

    /*
    void SelectOne(RaycastHit clickLocation)
    {
        selectedUnits.Clear();
        RTSGameObject selectedUnit = clickLocation.collider.gameObject.GetComponent<RTSGameObject>();
        if (selectedUnit != null) {
            selectedUnits.Add(selectedUnit.type, selectedUnit);
        }
    }*/

    void SetUpPlayer()
    {
        //Temporary testing setup
        //This shouldnt be outside of -1,-1 to 1,1 (probably get a null reference)
        Vector2 startTerrainIndex = new Vector2(0, 0);
        Vector2 startTerrainPositionOffset = new Vector2(0, 0); //x,z not larger than chunkSize (probably arrayoutofbounds)
        //Terrain startTerrain = terrainManager.terrainChunks[startTerrainIndex].GetComponent<Terrain>();
        // Height is managed by Awake and Move functions
        // todo multiply by terrain index
        Vector2 startLocation = new Vector2(startTerrainPositionOffset.x,
                                            startTerrainPositionOffset.y);

        // Our start location is a factory! hooray
        GameObject startingFactory = rtsGameObjectManager.SpawnUnit(typeof(Factory), startLocation);

        Dictionary<Type, int> startingItems = new Dictionary<Type, int>();

        startingItems.Add(typeof(Tool), 20);
        startingItems.Add(typeof(Iron), 200);
        startingItems.Add(typeof(Wood), 500);
        startingItems.Add(typeof(Coal), 2000);

        units[units.Count - 1].GetComponent<Storage>().AddItems(startingItems);

        mainCamera.transform.position = new Vector3(startLocation.x + 50,
            terrainManager.GetHeightFromGlobalCoords(startLocation.x, startLocation.y) + 150,
            startLocation.y - 50);
        mainCamera.transform.LookAt(startingFactory.transform);
        
    }

    public void QueueUnit(Type type)
    {
        QueueUnit(type, 1);
    }


    public void QueueUnit(Type type, int quantity)
    {
        foreach (RTSGameObject unit in selectedUnits)
        {
            Producer producer = unit.GetComponent<Producer>();
            if (producer != null)
            {
                producer.TryQueueItem(type, quantity);
            }
        }
    }
    
    // O(n) search + whatever sphereCast is (couldnt find it, but im assuming with octTree implementation it should be O(log(n))
    public RTSGameObject GetNearestUnitInRangeOfType(RTSGameObject source, float range, Type type)
    {
        int layerMask;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = source.transform.position;
        Collider closest = null, sourceCollider = source.GetComponent<Collider>();
        if (type == typeof(ResourceDeposit))
        {
            layerMask = LayerMask.NameToLayer("Resource");
        }
        else
        {
            layerMask = LayerMask.NameToLayer("RTSGameObject");
        }
        Collider[] objectsInRange = Physics.OverlapSphere(currentPosition, range, 1 << layerMask);
        foreach (Collider c in objectsInRange)
        {
            if (c == sourceCollider)
            {
                continue;
            }
            Vector3 directionToTarget = c.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = c;
            }
        }
        if (closest != null) {
            return closest.GetComponent<RTSGameObject>();
        }
        else
        {
            return null;
        }
    }
/*
    public RTSGameObject GetNearestUnitInRange(RTSGameObject source, float range)
    {

    }
    */
    public void CreateText(string text, Vector3 position)
    {
        Debug.Log("New floatingText: " + text);
        position.y += 5; // floating text starts above the object
        GameObject go = Instantiate(rtsGameObjectManager.prefabs["FloatingText"],
            position,
            Quaternion.identity) as GameObject;
        go.name = "FloatingText" + floatingText.Count();

        FloatingText ft = go.GetComponent<FloatingText>();
        ft.textMesh.text = text;
        ft.transform.position = position;
        floatingText.Add(ft);
    }
    public int GetNumUnits(Type type)
    {
        return units.Count(i => i.GetType() == type);
    }

    public int GetNumUnits()
    {
        return units.Count;
    }
    
    public void AddUnit(RTSGameObject unit)
    {
        units.Add(unit);
    }
}
