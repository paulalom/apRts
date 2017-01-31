using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

    //Instantiating prefabs with resources.load is slow so here we are
    //Maybe this should be in an assets class or something like that
    //We cant expose a dictionary to the inspector so we expose an array then populate the dictionary
    //if we need to do this too often ill just make a component for this
    public string[] InspectorPrefabNames;
    public GameObject[] InspectorPrefabTypes;
    Dictionary<string, GameObject> prefabs;
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

    public UnityEvent onSelectionChange;

    // these are hackish and needs to change
    public MyKVP<RTSGameObject, MyKVP<RTSGameObjectType, int>> itemTransferSource = null;
    public Texture2D selectionHighlight;
    public static Rect selectionBox = new Rect(0, 0, 0, 0);
    public Vector3 mouseDown = vectorSentinel;
    public bool menuClicked = false;

    // Use this for initialization
    void Start() {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        //static class
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        abilityManager = GameObject.FindGameObjectWithTag("AbilityManager").GetComponent<AbilityManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        units = new List<RTSGameObject>();
        prefabs = new Dictionary<string, GameObject>();
        selectedUnits = new List<RTSGameObject>();
        if (InspectorPrefabNames.Length != InspectorPrefabTypes.Length)
        {
            throw new System.Exception("fix the prefabs arrays in the game manager");
        }
        if (InspectorPrefabNames.Length <= 0)
        {
            throw new System.Exception("Populate the prefabs arrays in the game manager");
        }
        for (int i = 0; i < InspectorPrefabTypes.Length; i++)
        {
            Debug.Log(InspectorPrefabNames[i] + ", " + InspectorPrefabTypes[i].ToString());
            prefabs.Add(InspectorPrefabNames[i], InspectorPrefabTypes[i]);
        }
        if (InspectorPrefabNames.Length != prefabs.Count)
        {
            throw new System.Exception("No duplicate prefab names in the game manager");
        }

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
            orderManager.SetNextOrderType(OrderType.Guard);
        }
        if (Input.GetKey(KeyCode.P))
        {
            orderManager.SetNextOrderType(OrderType.Patrol);
        }
        if (Input.GetKey(KeyCode.S))
        {
            orderManager.SetNextOrderType(OrderType.Stop);
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
                    if (orderManager.GetNextOrderType() != OrderType.Move)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            foreach (RTSGameObject unit in selectedUnits)
                            {
                                orderManager.QueueOrder(unit, hit.point, objectClicked, null);
                            }
                        }
                        else
                        {
                            foreach (RTSGameObject unit in selectedUnits)
                            {
                                orderManager.SetOrder(unit, hit.point, objectClicked, null);
                            }
                        }
                        orderManager.ClearNextOrderType();
                    }
                    else
                    {
                        // Select one
                        if (objectClicked != null)
                        {
                            foreach (RTSGameObject unit in selectedUnits)
                            {
                                unit.selected = false;
                                unit.flagRenderer.material.color = Color.white;
                            }
                            selectedUnits.Clear();

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
                orderManager.ClearNextOrderType();
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (RTSGameObject unit in selectedUnits)
                    {
                        orderManager.QueueOrder(unit, hit.point, null, null);
                    }
                }
                else
                {
                    foreach (RTSGameObject unit in selectedUnits)
                    {
                        orderManager.SetOrder(unit, hit.point, null, null);
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                RTSGameObjectType typeToMake = RTSGameObjectType.HarvestingStation;
                int quantityToMake = 1;
                
            }
            if (Input.GetKeyUp(KeyCode.C))
            {
                RTSGameObjectType typeToMake = RTSGameObjectType.Worker;
                int quantityToMake = 1;
                foreach (RTSGameObject unit in selectedUnits)
                {
                    Producer producer = unit.GetComponent<Producer>();
                    if (RTSGameObject.canProduce[unit.type] != null && RTSGameObject.canProduce[unit.type].Contains(typeToMake))
                    {
                        producer.QueueItem(typeToMake, quantityToMake);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                SpawnUnit(RTSGameObjectType.Factory, hit.point);
            }

            terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z), hit.point.z);

        }
        resizeSelectionBox();

        if (Input.GetMouseButtonUp(0))
        {
            // Only do box selection / selection clearing if we drag a box or we click empty space
            if (!menuClicked && (!rayCast || hit.collider.GetComponentInParent<RTSGameObject>() == null))
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

    //The "around" bit is todo
    public bool SpawnUnitsAround(RTSGameObjectType type, int quantity, GameObject producer)
    {
        for (int i = 0; i < quantity; i++)
        {
            SpawnUnit(type, new Vector3(producer.transform.position.x, producer.transform.position.y, producer.transform.position.z));
        }
        return true;
    }

    bool SpawnUnit(RTSGameObjectType type, Vector3 position)
    {
        Debug.Log(type.ToString());
        GameObject go = Instantiate(prefabs[type.ToString()],
            position,
            Quaternion.identity) as GameObject;
        go.name = type.ToString() + units.Count;
        units.Add(go.GetComponent<RTSGameObject>());
        
        return true;
    }

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
        SpawnUnit(RTSGameObjectType.Factory, startLocation);

        Dictionary<RTSGameObjectType, int> startingItems = new Dictionary<RTSGameObjectType, int>();

       
        foreach (KeyValuePair<RTSGameObjectType, int> kvp in RTSGameObject.productionCosts[RTSGameObjectType.Worker])
        {
            startingItems.Add(kvp.Key, 3 * kvp.Value);
        }

        foreach (KeyValuePair<RTSGameObjectType, int> kvp in RTSGameObject.productionCosts[RTSGameObjectType.HarvestingStation])
        {
            if (startingItems.ContainsKey(kvp.Key))
            {
                startingItems[kvp.Key] += 3 * kvp.Value;
            }
            else {
                startingItems.Add(kvp.Key, 3 * kvp.Value);
            }
        }

        units[0].GetComponent<Storage>().AddItems(startingItems);

        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x,
            terrainManager.GetHeightFromGlobalCoords(mainCamera.transform.position.x, mainCamera.transform.position.z) + 50,
            mainCamera.transform.position.z);
        mainCamera.transform.LookAt(units[0].transform);
    }

    public void QueueUnit(RTSGameObjectType type)
    {
        QueueUnit(type, 1);
    }


    public void QueueUnit(RTSGameObjectType type, int quantity)
    {
        foreach (RTSGameObject unit in selectedUnits)
        {
            Producer producer = unit.GetComponent<Producer>();
            if (RTSGameObject.canProduce[unit.type] != null
                && RTSGameObject.canProduce[unit.type].Contains(type)
                && producer != null)
            {
                producer.QueueItem(type, quantity);
            }
        }
    }

}
