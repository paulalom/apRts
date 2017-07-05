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
    public AIManager aiManager;
    public SelectionManager selectionManager;
    public WorldManager worldManager;
    public NetworkStateManager netStateManager;
    public ManagerManager managerManager;
    Order nextOrder;

    public List<Command> commandInputQueue = new List<Command>();
    public List<Command> uiOnlyCommandInputQueue = new List<Command>(); // non state affecting
    public Dictionary<long, List<MyPair<List<long>, Command>>> localCommands = new Dictionary<long, List<MyPair<List<long>, Command>>>(); // commands happen on a step for a list of units, orders are persistent
    public Dictionary<long, List<MyPair<List<long>, Command>>> serverCommands = new Dictionary<long, List<MyPair<List<long>, Command>>>();

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

            while (stepDt < realTimeSinceLastStep || StepManager.CurrentStep < netStateManager.serverStep - 4)
            {
                if (StepManager.CurrentStep > netStateManager.serverStep + 1)
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
        ProcessCommandInputQueue();
        ProcessCommandsForStep();
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
    public void OnActionButtonRelease(RTSGameObject clickedUnit, Vector3 screenClickLocation)
    {
        if (selectionManager.mouseDown == Input.mousePosition && nextOrder != null)
        {
            commandInputQueue.Add(new Command() { orderData = new OrderData { target = clickedUnit, targetPosition = screenClickLocation } });
        }
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
    public void OnMoveButtonRelease(RTSGameObject clickedUnit, Vector3 screenClickLocation)
    {
        commandInputQueue.Add(new Command() {
            clearExistingOrders = !Input.GetKey(KeyCode.LeftShift), // fix me left shift shouldnt be hardcoded
            getOrder = CommandGetOrderFunction.GetDefaultMoveOrder,
            orderData = new OrderData() { targetPosition = screenClickLocation } });
    }

    void CheckInputSettings(RTSGameObject clickedUnit, Vector3 screenClickLocation)
    {
        foreach (Setting setting in settingsManager.inputSettings)
        {
            if (setting.checkActivationFunction(setting.key) && AreExactModifiersActive(setting) && setting.command != null)
            {
                Command command = setting.command;
                if (setting.isNumeric)
                {
                    List<MyPair<Type, int>> items = new List<MyPair<Type, int>>();
                    items.Add(new MyPair<Type, int>(UIManager.GetNumericMenuType(setting.key), 1));
                    command.orderData.items = items;
                    command.getOrder = CommandGetOrderFunction.GetDefaultConstructionOrder;
                    command.overrideDefaultOrderData = true;
                }
                command.orderData.targetPosition = screenClickLocation;
                command.orderData.target = clickedUnit;
                
                if (setting.isUIOnly)
                {
                    uiOnlyCommandInputQueue.Add(command);
                }
                else
                {
                    commandInputQueue.Add(command);
                }
            }
        }
    }

    public void AddCommand(Command command, List<long> unitIds)
    {
        List<long> stepUnitIds = playerManager.GetOrderableSelectedUnitIds();
        long stepToRunCommands = StepManager.CurrentStep + StepManager.numStepsToDelayInputProcessing;

        if (command != null)
        {
            List<MyPair<List<long>, Command>> commands = new List<MyPair<List<long>, Command>>();
            commands.Add(new MyPair<List<long>, Command>(unitIds, command));
            if (localCommands.ContainsKey(stepToRunCommands))
            {
                localCommands[stepToRunCommands].AddRange(commands);
            }
            else
            {
                localCommands.Add(stepToRunCommands, commands);
            }
            netStateManager.SendCommandRequest(unitIds, command, stepToRunCommands);
        }
    }

    void ProcessCommandInputQueue()
    {
        if (commandInputQueue.Count == 0)
        {
            return;
        }

        List<Command> stepCommands = commandInputQueue;
        commandInputQueue = new List<Command>();
        List<long> commandUnits = playerManager.GetOrderableSelectedUnitIds();

        foreach (Command command in stepCommands)
        {
            AddCommand(command, commandUnits);
        }
    }

    void ProcessCommandsForStep()
    {
        if (serverCommands.ContainsKey(StepManager.CurrentStep)) {
            List<MyPair<List<long>, Command>> commands = serverCommands[StepManager.CurrentStep];
            foreach (MyPair<List<long>, Command> command in commands)
            {
                Command comm = command.Value;
                nextOrder = Command.GetNextDefaultOrderFunction(comm.getOrder, this).Invoke();
                Command.GetAction(comm.action, this).Invoke();
                Command.GetRayCastHitAction(comm.raycastHitAction, this).Invoke(comm.orderData.target, comm.orderData.targetPosition);
                if (nextOrder != null)
                {
                    if (comm.overrideDefaultOrderData)
                    {
                        nextOrder.orderData = comm.orderData;
                    }
                    ProcessNextOrder(command.Key, command.Value);
                }
            }
        }
    }

    public void AddCommandsFromServer(MyPair<List<long>, Command> command, long step)
    {
        if (!serverCommands.ContainsKey(step))
        {
            serverCommands.Add(step, new List<MyPair<List<long>, Command>>() { command });
        }
        else 
        { // unlikely
            serverCommands[step].Add(command); 
            Debug.Log("appending commands for step: " + step);
        }
    }
    
    // Assumes command.order is not null
    void ProcessNextOrder(List<long> unitIds, Command command)
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach(long unitId in unitIds)
        {
            RTSGameObject unit = playerManager.GetUnit(unitId);
            if (unit == null) { continue; }
            units.Add(unit);
        }
        Dictionary<RTSGameObject, Order> orders = BuildOrdersForUnits(units, command.orderData.target, command.orderData.targetPosition, nextOrder);
        AddOrdersToUnits(orders, command.clearExistingOrders);
        nextOrder = null;
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

        return nextOrder;
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
        foreach (RTSGameObject unit in playerManager.GetPlayerSelectedUnits())
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
