using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour {

    public static List<MyMonoBehaviour> allObjects = new List<MyMonoBehaviour>(); // except loadingscreen this
    public RTSCamera mainCamera;
    public TerrainManager terrainManager;
    public OrderManager orderManager;
    public RTSGameObjectManager rtsGameObjectManager;
    public UIManager uiManager;
    public ButtonManager buttonManager;
    public MenuManager menuManager;
    public PlayerManager playerManager;
    public SettingsManager settingsManager;
    public SelectionManager selectionManager;
    public WorldManager worldManager;
    public NetworkStateManager netStateManager;
    public ManagerManager managerManager;
    public NetworkedCommandManager commandManager;
    KeyCode prevKeyClicked;
    
    public bool debug;
    public static string mainSceneName = "Main Scene";
    float realTimeSinceLastStep;
    //public HashSet<Type> selectableTypes = new HashSet<Type>() { typeof(Commander), typeof(Worker), typeof(HarvestingStation), typeof(Tank), typeof(Factory), typeof(PowerPlant) };

    public MyPair<RTSGameObject, MyPair<Type, int>> itemTransferSource = null;

    public void Awake()
    {
        debug = true;
        if (LoadingScreenManager.GetInstance() == null)
        {
            throw new InvalidOperationException("The game must be started from the start menu scene");
        }
        Debug.Log("Start time: " + DateTime.Now);
                   
        WorldSettings worldSettings = worldManager.GetWorldSettings(worldManager.numWorlds);
        playerManager.numAIPlayers = worldSettings.aiPresenceRating;
        playerManager.gameManager = this;
        netStateManager = GameObject.Find("NetworkStateManager").GetComponent<NetworkStateManager>();
        managerManager = GameObject.Find("ManagerManager").GetComponent<ManagerManager>();
        commandManager.netStateManager = netStateManager;
        commandManager.playerManager = playerManager;

        LoadingScreenManager.SetLoadingProgress(0.05f);

        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 120;
    }

    // Use this for initialization
    public void Start()
    {
        StartCoroutine(StartGame());
    }

    // Input happens on a unity timeframe
    void Update()
    {
        // too far behind, disable input.
        if (StepManager.CurrentStep < netStateManager.serverStep - 7)
        {
            return;
        }
        HandleInput();
    }

    public IEnumerator StartGame()
    {
        yield return netStateManager.InitilizeLocalGame(this, playerManager);
        yield return worldManager.SetupWorld(terrainManager, mainCamera);
        yield return uiManager.InitUI(this, playerManager, selectionManager);
        yield return MainGameLoop();
    }

    IEnumerator MainGameLoop()
    {
        while (true)
        {
            realTimeSinceLastStep += Time.deltaTime;
            float stepDt = StepManager.GetDeltaStep();

            while (stepDt < realTimeSinceLastStep || StepManager.CurrentStep < netStateManager.serverStep - 1)
            {
                if (StepManager.CurrentStep >= netStateManager.serverStep)
                {
                    yield return null;
                }
                StepGame(stepDt);
                realTimeSinceLastStep -= stepDt;
                netStateManager.Step();
                StepManager.Step();
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }
    }

    void StepGame(float dt)
    {
        commandManager.ProcessCommandsForStep(StepManager.CurrentStep, this);
        foreach (MyMonoBehaviour obj in allObjects)
        {
            obj.MyUpdate();
        }
        
        HashSet<RTSGameObject> units = playerManager.GetAllUnits();
        List<RTSGameObject> nonNeutralUnits = playerManager.GetNonNeutralUnits();
        foreach (RTSGameObject unit in nonNeutralUnits)
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
            UICheckSelectionEvents(hit);
            CheckInputSettings(GetClickedUnit(hit.collider), hit.point);
            terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z, playerManager.activeWorld) + 5, hit.point.z);
        }

        selectionManager.resizeSelectionBox();
        mainCamera.CheckCameraUpdate(); // Improve this eventually
    }

    RTSGameObject GetClickedUnit(Collider hitCollider)
    {
        if (hitCollider != null)
        {
            return hitCollider.GetComponentInParent<RTSGameObject>();
        }
        return null;
    }


    // This happens when the Game Loop processes commands for a step, but is queued on press
    public void OnActionButtonPress()
    {
        
    }

    // This happens when the Game Loop processes commands for a step, but is queued on release
    public void OnActionButtonRelease(List<long> unitIds, Command command)
    {
    }

    private void UICheckSelectionEvents(RaycastHit screenClickLocation)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            selectionManager.mouseDown = Input.mousePosition;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (selectionManager.mouseDown == Input.mousePosition)
            {
                selectionManager.CheckSingleSelectionEvent(screenClickLocation);
            }
            else
            {
                selectionManager.CheckBoxSelectionEvent(mainCamera.GetComponent<Camera>());
            }
            uiManager.menuClicked = false;
        }
    }

    // This happens when the Game Loop processes commands for a step, but is queued on release
    public void OnMoveButtonRelease(List<long> unitIds, Command command)
    {
        if (command.clearExistingOrders) // if outside of loop is more efficient
        {
            foreach (RTSGameObject unit in playerManager.GetUnits(unitIds))
            {
                Order moveOrder = OrderFactory.GetDefaultMoveOrder();
                moveOrder.orderData = command.orderData;
                orderManager.SetOrder(unit, moveOrder);
            }
        }
        else
        {
            foreach (RTSGameObject unit in playerManager.GetUnits(unitIds))
            {
                Order moveOrder = OrderFactory.GetDefaultMoveOrder();
                moveOrder.orderData = command.orderData;
                orderManager.QueueOrder(unit, moveOrder);
            }
        }
    }

    void CheckInputSettings(RTSGameObject clickedUnit, Vector3 screenClickLocation)
    {
        foreach (Setting setting in settingsManager.inputSettings)
        {
            if (setting.checkActivationFunction(setting.key) && AreExactModifiersActive(setting) && setting.command != null)
            {
                Command command = setting.command;
                if (Input.GetKey(setting.DontClearExistingOrdersToggle))
                {
                    command.clearExistingOrders = false;
                }
                if (setting.isNumeric)
                {
                    List<MyPair<Type, int>> items = new List<MyPair<Type, int>>();
                    items.Add(new MyPair<Type, int>(UIManager.GetNumericMenuType(setting.key), 1));
                    command.orderData.items = items;
                    command.getOrder = CommandGetOrderFunction.GetDefaultConstructionOrder;
                    command.overrideDefaultOrderData = true;
                }
                else
                {
                    command.orderData.targetPosition = screenClickLocation;
                    command.orderData.target = clickedUnit;
                }
                if (setting.isUIOnly)
                {
                    commandManager.AddNonNetworkedCommand(command);
                }
                else
                {
                    commandManager.AddCommand(command);
                }
            }
        }
    }
    
    // Assumes order is not null
    public void ProcessOrder(List<long> unitIds, Command command, Order order)
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach(long unitId in unitIds)
        {
            RTSGameObject unit = playerManager.GetUnit(unitId);
            if (unit == null) { continue; }
            units.Add(unit);
        }
        Dictionary<RTSGameObject, Order> orders = BuildOrdersForUnits(units, command.orderData.target, command.orderData.targetPosition, order);
        AddOrdersToUnits(orders, command.clearExistingOrders);
        prevKeyClicked = KeyCode.None;
    }

    Dictionary<RTSGameObject, Order> BuildOrdersForUnits(List<RTSGameObject> units, RTSGameObject clickedUnit, Vector3 screenClickLocation, Order order)
    {
        Dictionary<RTSGameObject, Order> orders = new Dictionary<RTSGameObject, Order>();
        foreach (RTSGameObject unit in units)
        {
            order = AddOrderTargets(order, clickedUnit, screenClickLocation);
            order = AddOrderDefaultAbility(order, unit);
            orders.Add(unit, order);
        }
        return orders;
    }

    Order AddOrderTargets(Order order, RTSGameObject objectClicked, Vector3 screenClickLocation)
    {
        order.orderData.target = objectClicked;
        order.orderData.targetPosition = screenClickLocation;

        return order;
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
    
    void AddOrdersToUnits(Dictionary<RTSGameObject, Order> orders, bool clearExistingOrders)
    {
        if (clearExistingOrders)
        {
            orderManager.SetOrders(orders);
        }
        else
        {
            orderManager.QueueOrders(orders);
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

    
    public void SetUpPlayer(int playerId, World world)
    {
        Vector2 startLocation = world.startLocations[playerId-1];
        Player player = playerManager.players[playerId];
        RTSGameObject commander = rtsGameObjectManager.SpawnUnit(typeof(Commander), new Vector3(startLocation.x, 0, startLocation.y), playerId, null, world).GetComponent<RTSGameObject>();
        player.commander = commander;

        AddStartingItems(player, commander);
        SetStartingCamera(player, startLocation, world);
        playerManager.AiManager.SetUpPlayerAIManagers(player);
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

    public void ProduceFromMenu(Type type)
    {
        ProduceFromMenu(type, 1);
    }
    
    public void ProduceFromMenu(Type type, int quantity)
    {
        List<long> unitIds = playerManager.GetOrderableSelectedUnitIds();

        List<MyPair<Type, int>> items = new List<MyPair<Type, int>>() {
                    new MyPair<Type,int>(type, quantity) };

        foreach (RTSGameObject unit in playerManager.GetUnits(unitIds))
        {
            Producer producer = unit.GetComponent<Producer>();
            Mover mover = unit.GetComponent<Mover>();
            if (producer != null && mover != null)
            {
                playerManager.AiManager.SetNewPlanForUnit(unit, new ConstructionPlan(playerManager.AiManager, rtsGameObjectManager) { thingsToBuild = items });
            }
            else if (producer != null)
            {
                Order order = OrderFactory.BuildConstructionOrder(items);
                Command command = new Command() { orderData = order.orderData };
                command.getOrder = CommandGetOrderFunction.GetDefaultConstructionOrder;
                command.overrideDefaultOrderData = true;
                command.clearExistingOrders = false;
                commandManager.AddCommand(command, unitIds);
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

    public void RaiseTerrain(object nothing, Vector3 position)
    {
        try
        { //Try catch to swallow exception. FixMe
          // only does raiseTerrain
            terrainManager.ModifyTerrain(position, .003f, 20, playerManager.activeWorld);
        }
        catch (Exception e) { }
    }

    public void SpawnFactory(object nothing, Vector3 position)
    {
        rtsGameObjectManager.SpawnUnit(typeof(Factory), position, 1, playerManager.ActivePlayer.units.FirstOrDefault().Value.gameObject, playerManager.activeWorld);
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
