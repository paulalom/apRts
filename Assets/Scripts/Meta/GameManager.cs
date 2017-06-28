using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour {

    public static List<MyMonoBehaviour> allObjects = new List<MyMonoBehaviour>(); // except loadingscreen this
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
    Order nextOrder;
    public bool debug;
    public static string mainSceneName = "Main Scene";
    public float enemySpawnRateBase;
    //public HashSet<Type> selectableTypes = new HashSet<Type>() { typeof(Commander), typeof(Worker), typeof(HarvestingStation), typeof(Tank), typeof(Factory), typeof(PowerPlant) };

    public MyPair<RTSGameObject, MyPair<Type, int>> itemTransferSource = null;

    public void Awake()
    {
        debug = true;
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
    public void Start()
    {
        StartCoroutine(worldManager.SetupWorld(terrainManager, mainCamera));
    }

    // Everything in the game loop happens here
    void Update()
    {
        foreach (MyMonoBehaviour obj in allObjects)
        {
            obj.MyUpdate();
        }
        float dt = StepManager.GetDeltaStep();
        HashSet<RTSGameObject> units = playerManager.GetAllUnits();
        List<RTSGameObject> nonNeutralUnits = playerManager.GetNonNeutralUnits();
        HandleInput();
        foreach (RTSGameObject unit in units)
        {
            Mover mover = unit.GetComponent<Mover>();
            if (mover != null)
            {
                mover.velocity = new Vector3();
            }
            rtsGameObjectManager.collisionAvoidanceManager.SyncObjectState(unit, dt);
        }
        orderManager.CarryOutOrders(nonNeutralUnits, dt);
        rtsGameObjectManager.UpdateAll(units, nonNeutralUnits, dt);

        rtsGameObjectManager.HandleUnitCreation();
        rtsGameObjectManager.HandleUnitDestruction();
    }
    
    void HandleInput()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool rayCast = Physics.Raycast(ray, out hit);
        
        if (rayCast)
        {
            CheckInputSettings(hit);
            terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z, playerManager.activeWorld) + 5, hit.point.z);
        }

        selectionManager.resizeSelectionBox();
        mainCamera.CheckCameraUpdate(); // Improve this eventually
    }
    
    public void OnActionButtonPress()
    {
        selectionManager.mouseDown = Input.mousePosition;
    }

    public void OnActionButtonRelease(RaycastHit screenClickLocation)
    {
        if (selectionManager.mouseDown == Input.mousePosition)
        {
            if (nextOrder != null)
            {
                ProcessNextOrder(screenClickLocation);
            }
            else
            {
                selectionManager.CheckSingleSelectionEvent(screenClickLocation);
            }

            uiManager.menuClicked = false;
        }

        selectionManager.CheckBoxSelectionEvent(mainCamera.GetComponent<Camera>());
    }

    public void OnMoveButtonRelease(RaycastHit screenClickLocation)
    {
        nextOrder = new MoveOrder();
        nextOrder.orderData.targetPosition = screenClickLocation.point;
        nextOrder.orderData.orderRange = .3f;
        ProcessNextOrder(screenClickLocation);
    }

    void CheckInputSettings(RaycastHit screenClickLocation)
    {
        foreach (Setting setting in settingsManager.inputSettings)
        {
            if (setting.checkActivationFunction(setting.key) && AreExactModifiersActive(setting))
            {
                nextOrder = setting.getOrder.Invoke();
                setting.action.Invoke();
                setting.raycastHitAction.Invoke(screenClickLocation);
                if (setting.isNumeric)
                {
                    QueueUnit(UIManager.GetNumericMenuType(setting.key));
                }
            }

            if (setting.smartCast && nextOrder != null)
            {
                ProcessNextOrder(screenClickLocation);
            }
        }
    }

    void ProcessNextOrder(RaycastHit screenClickLocation)
    {
        Dictionary<RTSGameObject, Order> orders = BuildOrdersForUnits(playerManager.GetOrderableSelectedUnits(), screenClickLocation, nextOrder);
        AddOrdersToUnits(orders, Input.GetKey(KeyCode.LeftShift));
        nextOrder = null;
    }

    // Returns true when all key modifiers down and nothing else
    bool AreExactModifiersActive(Setting setting)
    {
        foreach (KeyCode modifier in setting.keyModifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }

        if (setting.useExactModifiers)
        {
            // Ugh. Unity should maintain a list of all keys currently held down but I couldn't find any reference to it.
            // It also didn't end up being better to maintain my own list, because I would have to iterate
            // through the entire list every frame.
            // If you know a better way to do this please let me know.
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key) && !setting.keyModifiers.Contains(key) && setting.key != key)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void AddOrdersToUnits(Dictionary<RTSGameObject, Order> orders, bool queueToggleEnabled)
    {
        if (queueToggleEnabled)
        {
            orderManager.QueueOrders(orders);
        }
        else
        {
            orderManager.SetOrders(orders);
        }
    }

    void AddOrderToUnit(Order order, RTSGameObject unit, bool queueToggleEnabled)
    {
        if (queueToggleEnabled)
        {
            orderManager.QueueOrder(unit, order);
        }
        else
        {
            orderManager.SetOrder(unit, order);
        }
    }

    Dictionary<RTSGameObject, Order> BuildOrdersForUnits(HashSet<RTSGameObject> units, RaycastHit screenClickLocation, Order order)
    {
        Dictionary<RTSGameObject, Order> orders = new Dictionary<RTSGameObject, Order>();
        foreach (RTSGameObject unit in units)
        {
            RTSGameObject objectClicked = GetObjectClicked(screenClickLocation);
            
            order = AddOrderTargets(order, objectClicked, screenClickLocation);
            order = AddOrderDefaultAbility(order, unit);
            orders.Add(unit, order);
        }
        return orders;
    }

    RTSGameObject GetObjectClicked(RaycastHit screenClickLocation)
    {
        if(screenClickLocation.collider != null)
        {
            return screenClickLocation.collider.GetComponentInParent<RTSGameObject>();
        }
        return null;
    }

    Order AddOrderDefaultAbility(Order order, RTSGameObject unit)
    {
        if (order.GetType() == typeof(UseAbilityOrder) && unit.defaultAbility != null)
        {
            UseAbilityOrder abilityOrder = new UseAbilityOrder(order);
            abilityOrder.orderData.ability = unit.defaultAbility;
            abilityOrder.orderData.ability.target = order.orderData.target;
            abilityOrder.orderData.ability.targetPosition = order.orderData.targetPosition;
            abilityOrder.orderData.orderRange = unit.defaultAbility.range;
            abilityOrder.orderData.remainingChannelTime = unit.defaultAbility.cooldown;
            return abilityOrder;
        }
        else
        {
            return order;
        }        
    }

    Order AddOrderTargets(Order order, RTSGameObject objectClicked, RaycastHit screenClickLocation)
    {
        order.orderData.target = objectClicked;
        order.orderData.targetPosition = screenClickLocation.point;
        
        return nextOrder;
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

    public static void RegisterObject(MyMonoBehaviour obj)
    {
        allObjects.Add(obj);
    }

    public static void DeregisterObject(MyMonoBehaviour obj)
    {
        MyMonoBehaviour[] components = obj.GetComponents<MyMonoBehaviour>();
        foreach (MyMonoBehaviour component in components)
        {
            allObjects.Remove(component);
        }
        allObjects.Remove(obj);

    }
}
