using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour {

    RTSCamera mainCamera;
    [HideInInspector]
    public TerrainManager terrainManager;
    [HideInInspector]
    public OrderManager orderManager;
    RTSGameObjectManager rtsGameObjectManager;
    UIManager uiManager;
    PlayerManager playerManager;
    SettingsManager settingsManager;
    AIManager aiManager;
    public static Vector3 vectorSentinel = new Vector3(-99999, -99999, -99999);
    float prevTime, lastEnemySpawn;
    Order nextOrder;
    public bool debug = true;
    string gameMode = "NOT Survival";
    public static string mainSceneName = "Main Scene";
    public float dt = .001f;
    float mouseSlipTolerance = 4; // The square of the distance you are allowed to move your mouse before a drag select is detected
    public int myPlayerId = 1, enemyPlayerId = 2;
    int numWorlds = 0;
    public float enemySpawnRateBase;
    RTSGameObject commander;
    public HashSet<Type> selectableTypes = new HashSet<Type>() { typeof(Commander), typeof(Worker), typeof(HarvestingStation), typeof(Tank), typeof(Factory), typeof(PowerPlant) };

    public MyKVP<RTSGameObject, MyKVP<Type, int>> itemTransferSource = null;
    public Texture2D selectionHighlight;
    public static Rect selectionBox = new Rect(0, 0, 0, 0);

    void Awake()
    {
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        settingsManager = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
        aiManager = GameObject.FindGameObjectWithTag("AIManager").GetComponent<AIManager>();
        LoadingScreenManager.SetLoadingProgress(0.05f);
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 120;
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SetupWorld());
    }

    IEnumerator SetupWorld()
    {
        LoadingScreenManager.SetLoadingProgress(0.10f);
        yield return null;
        WorldSettings worldSettings = GetWorldSettings(numWorlds);
        LoadingScreenManager.GetInstance().ReplaceTextTokens(LoadingScreenManager.GetWorldGenerationTextTokens(worldSettings));
        for (int i = 0; i < 30; i++)
        {
            LoadingScreenManager.SetLoadingProgress(.15f + 0.02f * i);
            yield return null;
        }
        playerManager.activeWorld = GenerateWorld(worldSettings);
        LoadingScreenManager.SetLoadingProgress(0.85f);
        yield return null;
        numWorlds++;
        mainCamera.world = playerManager.activeWorld;
        playerManager.InitPlayers(worldSettings.numStartLocations);
        for (int i = 1; i <= playerManager.activeWorld.worldSettings.numStartLocations; i++)
        {
            SetUpPlayer(i, playerManager.activeWorld);
        }
        LoadingScreenManager.SetLoadingProgress(0.99f);
        LoadingScreenManager.CompleteLoadingScreen();
    }

    // Update is called once per frame
    void Update() {
        float now = Time.time;
        debug = true;
        dt = now - prevTime;

        HandleInput();
        orderManager.CarryOutOrders(playerManager.GetNonNeutralUnits(), dt);
        rtsGameObjectManager.SnapToTerrain(playerManager.GetNonNeutralUnits(), playerManager.activeWorld);
        if (gameMode == "Survival")
        {
            SpawnEnemies();
        }

        prevTime = now;
    }

    void SpawnEnemies()
    {
        float nextEnemySpawn = enemySpawnRateBase / (1 + (.05f * (Time.time / 30)));
        
        if (Time.time - lastEnemySpawn > nextEnemySpawn)
        {
            rtsGameObjectManager.SpawnUnit(typeof(Tank), GetEnemySpawnPosition(), enemyPlayerId, playerManager.activeWorld);
            lastEnemySpawn = Time.time;
        }
    }
    
    WorldSettings GetWorldSettings(int randomSeed)
    {
        return new WorldSettings()
        {
            randomSeed = 2,
            resourceAbundanceRating = WorldSettings.starterWorldResourceAbundance,
            resourceQualityRating = WorldSettings.starterWorldResourceRarity,
            sizeRating = WorldSettings.starterWorldSizeRating,
            numStartLocations =  WorldSettings.starterWorldNumStartLocations,
            startLocationSizeRating = WorldSettings.starterWorldStartLocationSizeRating,
            aiStrengthRating = WorldSettings.starterWorldAIStrengthRating,
            aiPresenceRating = WorldSettings.starterWorldAIPresenceRating
        };
    }

    World GenerateWorld(WorldSettings worldSettings)
    {
        World world = new World() { worldSettings = worldSettings };

        world.BuildWorld(terrainManager);

        return world;
    }

    Vector3 GetEnemySpawnPosition()
    {
        return RandomCircle(commander.transform.position, 200);
    }

    Vector3 RandomCircle(Vector3 center, float radius)
    {
        float ang = UnityEngine.Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        return pos;
    }

    void OnGUI()
    {
        if (uiManager.mouseDown != vectorSentinel)
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.DrawTexture(selectionBox, selectionHighlight);
        }
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
            uiManager.mouseDown = Input.mousePosition;
        }

        foreach (KeyValuePair<string, Setting> setting in settingsManager.defaultKeyboardSettings)
        {
            if (setting.Value.activationType == "KeyUp")
            {
                if (Input.GetKeyUp(setting.Value.key))
                {
                    bool modifiersActivated = true;
                    foreach (KeyCode modifier in setting.Value.keyModifiers)
                    {
                        if (!Input.GetKey(modifier))
                        {
                            modifiersActivated = false;
                            break;
                        }
                    }
                    if (modifiersActivated)
                    {
                        switch (setting.Key) {
                            case "Guard":
                                nextOrder = new GuardOrder() { orderRange = 6f };
                                break;
                            case "Patrol":
                                nextOrder = new PatrolOrder() { orderRange = 1f };
                                break;
                            case "Stop":
                                nextOrder = new StopOrder() { orderRange = 1f };
                                break;
                            case "Harvest":
                                nextOrder = new HarvestOrder() { orderRange = 15f };
                                break;
                            case "Follow":
                                nextOrder = new FollowOrder() { orderRange = 6f };
                                break;
                            case "UseAbility":
                                nextOrder = new UseAbilityOrder();
                                break;
                            default:
                                break;
                        }

                        if (setting.Key.Contains("numeric_"))
                        {
                            QueueUnit(UIManager.GetNumericMenuType(setting.Key));
                        }
                    }
                }
            }
            else if (setting.Value.activationType == "KeyHold")
            {
                if (Input.GetKey(setting.Value.key))
                {
                    bool modifiersActivated = true;
                    foreach (KeyCode modifier in setting.Value.keyModifiers)
                    {
                        if (!Input.GetKey(modifier))
                        {
                            modifiersActivated = false;
                            break;
                        }
                    }
                    if (modifiersActivated)
                    {
                        float cameraElevationRate = 1f;
                        switch (setting.Key)
                        {
                            case "CamY+":
                                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + cameraElevationRate, mainCamera.transform.position.z);
                                break;
                            case "CamY-":
                                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - cameraElevationRate, mainCamera.transform.position.z);
                                break;
                            case "SpawnFactory":
                                if (debug)
                                {
                                    rtsGameObjectManager.SpawnUnit(typeof(Factory), hit.point, 1, playerManager.activeWorld);
                                }
                                break;
                            case "RaiseTerrain":
                                if (rayCast)
                                {
                                    try
                                    { //Try catch to swallow exception. FixMe
                                      // only does raiseTerrain
                                        terrainManager.ModifyTerrain(hit.point, .003f, 20, playerManager.activeWorld);
                                    }
                                    catch (Exception e) { }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (setting.Value.smartCast)
            {
                ProcessNextOrderInput(hit);
                if (nextOrder != null)
                {
                    nextOrder = null;
                }
            }
        }
        
        if (rayCast)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0) && uiManager.mouseDown == Input.mousePosition)
            {
                ProcessNextOrderInput(hit);
                CheckSingleSelectionEvent(rayCast, hit);

                if (nextOrder != null)
                {
                    nextOrder = null;
                }
            }

            // Right click to move/attack
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                nextOrder = new MoveOrder() { targetPosition = hit.point, orderRange = .3f };
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
                    {
                        orderManager.QueueOrder(unit, nextOrder);
                    }
                }
                else
                {
                    foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
                    {
                        orderManager.SetOrder(unit, nextOrder);
                    }
                }
                nextOrder = null;
            }
            terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z, playerManager.activeWorld) + 5, hit.point.z);
        }
        CheckBoxSelectionEvent();
    }

    void ProcessNextOrderInput(RaycastHit hit)
    {
        if (nextOrder == null)
        {
            return;
        }

        // objectClicked May be null
        RTSGameObject objectClicked = hit.collider != null ? hit.collider.GetComponentInParent<RTSGameObject>() : null;

        nextOrder.target = objectClicked;
        nextOrder.targetPosition = hit.point;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
            {
                if (nextOrder.GetType() == typeof(UseAbilityOrder) && unit.defaultAbility != null)
                {
                    nextOrder.ability = unit.defaultAbility;
                    nextOrder.ability.target = objectClicked;
                    nextOrder.ability.targetPosition = hit.point;
                    nextOrder.orderRange = unit.defaultAbility.range;
                    nextOrder.remainingChannelTime = unit.defaultAbility.cooldown;
                }
                orderManager.QueueOrder(unit, nextOrder);
            }
        }
        else
        {
            foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
            {
                if (nextOrder.GetType() == typeof(UseAbilityOrder) && unit.defaultAbility != null)
                {
                    nextOrder.ability = unit.defaultAbility;
                    nextOrder.ability.target = objectClicked;
                    nextOrder.ability.targetPosition = hit.point;
                    nextOrder.orderRange = unit.defaultAbility.range;
                }
                orderManager.SetOrder(unit, nextOrder);
            }
        }
    }

    void CheckSingleSelectionEvent(bool rayCast, RaycastHit hit)
    {
        if (nextOrder == null)
        {
            // objectClicked May be null
            RTSGameObject objectClicked = hit.collider.GetComponentInParent<RTSGameObject>();
            // Select one
            if (objectClicked != null && selectableTypes.Contains(objectClicked.GetType()) && objectClicked.ownerId == myPlayerId)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
                    {
                        unit.selected = false;
                        unit.flagRenderer.material.color = Color.white;
                    }
                    playerManager.PlayerSelectedUnits.Clear();
                }

                playerManager.PlayerSelectedUnits.Add(objectClicked);
                objectClicked.selected = true;
                objectClicked.flagRenderer.material.color = Color.red;
            }
        }
        uiManager.menuClicked = false;
    }

    // Selection needs to be reworked/refactored, i copied a tutorial and have been hacking at it
    void CheckBoxSelectionEvent()
    {
        resizeSelectionBox();

        if (Input.GetMouseButtonUp(0))
        {
            // Only do box selection / selection clearing if we drag a box or we click empty space
            if (!uiManager.menuClicked && (Input.mousePosition - uiManager.mouseDown).sqrMagnitude > mouseSlipTolerance) // && hit.collider.GetComponentInParent<RTSGameObject>() == null))
            {
                CheckSelected(playerManager.PlayerUnits);
            }
            uiManager.mouseDown = vectorSentinel;
        }
    }

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
            selectionBox = new Rect(uiManager.mouseDown.x,
                                    RTSCamera.InvertMouseY(uiManager.mouseDown.y),
                                    Input.mousePosition.x - uiManager.mouseDown.x,
                                    RTSCamera.InvertMouseY(Input.mousePosition.y) - RTSCamera.InvertMouseY(uiManager.mouseDown.y));
        }
    }

    void CheckSelected(List<RTSGameObject> units)
    {
        foreach (RTSGameObject unit in units)
        { 
            if (!selectableTypes.Contains(unit.GetType()) || unit.ownerId != myPlayerId)
            {
                continue;
            }
            if (unit.flagRenderer.isVisible)
            {
                Vector3 camPos = mainCamera.GetComponent<Camera>().WorldToScreenPoint(unit.transform.position);
                camPos.y = RTSCamera.InvertMouseY(camPos.y);
                unit.selected = selectionBox.Contains(camPos);
            }
            if (unit.selected)
            {
                unit.flagRenderer.material.color = Color.red;
                if (!playerManager.PlayerSelectedUnits.Contains(unit))
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
            playerManager.PlayerSelectedUnits.Add(obj);
        }
        else
        {
            playerManager.PlayerSelectedUnits.Remove(obj);
        }
        playerManager.OnPlayerSelectionChange.Invoke();
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

    void SetUpPlayer(int playerId, World world)
    {
        Vector2 startLocation = world.startLocations[playerId-1];

        // Our start location is a factory! hooray
        commander = rtsGameObjectManager.SpawnUnit(typeof(Commander), new Vector3(startLocation.x, 0, startLocation.y), playerId, world).GetComponent<RTSGameObject>();
        
        Dictionary<Type, int> startingItems = new Dictionary<Type, int>();

        startingItems.Add(typeof(Iron), 1000);
        startingItems.Add(typeof(Stone), 4500);
        startingItems.Add(typeof(Wood), 1000);
        startingItems.Add(typeof(Tool), 400);
        startingItems.Add(typeof(Coal), 2000);

        commander.GetComponent<Storage>().AddItems(startingItems);

        if (playerId == 1)
        {
            mainCamera.transform.position = new Vector3(startLocation.x + 50,
                terrainManager.GetHeightFromGlobalCoords(startLocation.x, startLocation.y, world) + 200,
                startLocation.y - 50);
            mainCamera.transform.LookAt(commander.transform);
        }
    }

    public void QueueUnit(Type type)
    {
        QueueUnit(type, 1);
    }


    public void QueueUnit(Type type, int quantity)
    {
        foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
        {
            Producer producer = unit.GetComponent<Producer>();
            Mover mover = unit.GetComponent<Mover>();
            if (producer != null && mover != null)
            {
                aiManager.SetNewPlanForUnit(unit, new ConstructionPlan() { thingsToBuild = new List<MyKVP<Type, int>>() { new MyKVP<Type, int>(type, quantity) } });
            }
            else
            {
                producer.TryQueueItem(type, quantity);
            }
        }
    }
}
