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
    SelectionManager selectionManager;
    WorldManager worldManager;
    float prevTime, lastEnemySpawn;
    Order nextOrder;
    public bool debug = true;
    public static string mainSceneName = "Main Scene";
    public float enemySpawnRateBase;
    public float dt = .001f;
    //public HashSet<Type> selectableTypes = new HashSet<Type>() { typeof(Commander), typeof(Worker), typeof(HarvestingStation), typeof(Tank), typeof(Factory), typeof(PowerPlant) };

    public MyPair<RTSGameObject, MyPair<Type, int>> itemTransferSource = null;

    void Awake()
    {
        if (LoadingScreenManager.GetInstance() == null)
        {
            throw new InvalidOperationException("The game must be started from the start menu scene");
        }
        rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        aiManager = GameObject.FindGameObjectWithTag("AIManager").GetComponent<AIManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        selectionManager = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
        settingsManager = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
        worldManager = GameObject.FindGameObjectWithTag("WorldManager").GetComponent<WorldManager>();
        LoadingScreenManager.SetLoadingProgress(0.05f);
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 120;

        WorldSettings worldSettings = worldManager.GetWorldSettings(worldManager.numWorlds);
        playerManager.InitPlayers(worldSettings.numStartLocations);
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(worldManager.SetupWorld(terrainManager, mainCamera));
    }

    // Update is called once per frame
    void Update() {
        float now = Time.time;
        debug = true;
        dt = now - prevTime;

        HandleInput();
        orderManager.CarryOutOrders(playerManager.GetNonNeutralUnits(), dt);
        rtsGameObjectManager.SnapToTerrain(playerManager.GetNonNeutralUnits(), playerManager.activeWorld);
        playerManager.UpdatePlayers();

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
            selectionManager.mouseDown = Input.mousePosition;
        }

        if (rayCast)
        {
            CheckInputSettings(hit);

            if (Input.GetKeyUp(KeyCode.Mouse0) && selectionManager.mouseDown == Input.mousePosition)
            {
                if (nextOrder != null)
                {
                    ProcessNextOrderInput(hit);
                    nextOrder = null;
                }
                else
                {
                    selectionManager.CheckSingleSelectionEvent(hit);
                }
                
                uiManager.menuClicked = false;
            }
            // Right click to move/attack
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                nextOrder = new MoveOrder() { targetPosition = hit.point, orderRange = .3f };
                ProcessNextOrderInput(hit);
                nextOrder = null;
            }
            terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z, playerManager.activeWorld) + 5, hit.point.z);
        }
        // Needs to be outside of raycast so we still check selection if the mouseUp event is off of the terrain
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            selectionManager.CheckBoxSelectionEvent(mainCamera.GetComponent<Camera>());
        }
        selectionManager.resizeSelectionBox();
    }

    void CheckInputSettings(RaycastHit hit)
    {
        foreach (Setting setting in settingsManager.defaultKeyboardSettings)
        {
            if (setting.checkActivationFunction(setting.key) && AreModifiersActive(setting))
            {
                nextOrder = setting.order;
                setting.action.Invoke();
                setting.raycastHitAction.Invoke(hit);
                if (setting.isNumeric)
                {
                    QueueUnit(UIManager.GetNumericMenuType(setting.key));
                }
            }

            if (setting.smartCast && nextOrder != null)
            {
                ProcessNextOrderInput(hit);
                nextOrder = null;
            }
        }
    }

    bool AreModifiersActive(Setting setting)
    {
        foreach (KeyCode modifier in setting.keyModifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }
        return true;
    }

    void ProcessNextOrderInput(RaycastHit screenClickLocation)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
            {
                AddNextOrder(nextOrder, unit, screenClickLocation, false);
            }
        }
        else
        {
            foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
            {
                AddNextOrder(nextOrder, unit, screenClickLocation, true);
            }
        }
    }

    void AddNextOrder(Order nextOrder, RTSGameObject unit, RaycastHit screenClickLocation, bool clearOrderQueueBeforeSetting)
    {
        // objectClicked May be null
        RTSGameObject objectClicked = screenClickLocation.collider != null ? screenClickLocation.collider.GetComponentInParent<RTSGameObject>() : null;
        nextOrder.target = objectClicked;
        nextOrder.targetPosition = screenClickLocation.point;
        if (unit.ownerId == playerManager.ActivePlayerId)
        {
            if (nextOrder.GetType() == typeof(UseAbilityOrder) && unit.defaultAbility != null)
            {
                nextOrder.ability = unit.defaultAbility;
                nextOrder.ability.target = objectClicked;
                nextOrder.ability.targetPosition = screenClickLocation.point;
                nextOrder.orderRange = unit.defaultAbility.range;
                nextOrder.remainingChannelTime = unit.defaultAbility.cooldown;
            }
            if (clearOrderQueueBeforeSetting)
            {
                orderManager.SetOrder(unit, nextOrder);
            }
            else
            {
                orderManager.QueueOrder(unit, nextOrder);
            }
        }
    }
    
    public void SetUpPlayer(int playerId, World world)
    {
        Vector2 startLocation = world.startLocations[playerId-1];
        Player player = playerManager.players[playerId];
        RTSGameObject commander = rtsGameObjectManager.SpawnUnit(typeof(Commander), new Vector3(startLocation.x, 0, startLocation.y), playerId, null, world).GetComponent<RTSGameObject>();
        player.commander = commander;

        AddStartingItems(player, commander);
        SetStartingCamera(player, startLocation, world);
        aiManager.SetUpPlayerAIManagers(player);
    }

    void AddStartingItems(Player player, RTSGameObject commander)
    {
        Dictionary<Type, int> startingItems = new Dictionary<Type, int>();

        startingItems.Add(typeof(Iron), 1000);
        startingItems.Add(typeof(Stone), 4500);
        startingItems.Add(typeof(Wood), 1000);
        startingItems.Add(typeof(Tool), 400);
        startingItems.Add(typeof(Coal), 2000);

        commander.GetComponent<Storage>().AddItems(startingItems);
    }

    void SetStartingCamera(Player player, Vector2 startLocation, World world)
    {
        if (player.isHuman)
        {
            float startLocationHeight = terrainManager.GetHeightFromGlobalCoords(startLocation.x, startLocation.y, world);
            mainCamera.transform.position = new Vector3(startLocation.x + 30,
                startLocationHeight + 150,
                startLocation.y);
            mainCamera.transform.LookAt(new Vector3(startLocation.x, startLocationHeight, startLocation.y));
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
            if (unit.ownerId == playerManager.ActivePlayerId)
            {
                Producer producer = unit.GetComponent<Producer>();
                Mover mover = unit.GetComponent<Mover>();
                if (producer != null && mover != null)
                {
                    aiManager.SetNewPlanForUnit(unit, new ConstructionPlan() { thingsToBuild = new List<MyPair<Type, int>>() { new MyPair<Type, int>(type, quantity) } });
                }
                else if (producer != null)
                {
                    producer.TryQueueItem(type, quantity);
                }
            }
        }
    }

    public void CreateText(string text, Vector3 position, Color color, float scale = 1)
    {
        uiManager.CreateText(text, position, color, scale);
    }

    public void CreateText(string text, Vector3 position, float scale = 1)
    {
        uiManager.CreateText(text, position, scale);
    }

    public void RaiseTerrain(RaycastHit hit)
    {
        try
        { //Try catch to swallow exception. FixMe
          // only does raiseTerrain
            terrainManager.ModifyTerrain(hit.point, .003f, 20, playerManager.activeWorld);
        }
        catch (Exception e) { }
    }

    public void SpawnFactory(RaycastHit hit)
    {
        rtsGameObjectManager.SpawnUnit(typeof(Factory), hit.point, 1, playerManager.ActivePlayer.units.FirstOrDefault().gameObject, playerManager.activeWorld);
    }
}
