using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CTFGameManager : GameManager {

    public ApRTSTerrainManager terrainManager;
    public RTSCamera mainCamera;
    public OrderManager orderManager;
    public RTSGameObjectManager rtsGameObjectManager;
    public UIManager uiManager;
    public PlayerManager playerManager;
    public SettingsManager settingsManager;
    public SelectionManager selectionManager;
    public WorldManager worldManager;
    public NetworkedCommandManager commandManager;

    public override void Awake()
    {
        base.Awake();

        mainCamera = GameObject.Find("MainCamera").GetComponent<RTSCamera>();
        terrainManager = GameObject.Find("TerrainManager").GetComponent<ApRTSTerrainManager>();
        orderManager = GameObject.Find("OrderManager").GetComponent<OrderManager>();
        rtsGameObjectManager = GameObject.Find("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        settingsManager = GameObject.Find("SettingsManager").GetComponent<SettingsManager>();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        commandManager = GameObject.Find("NetworkedCommandManager").GetComponent<NetworkedCommandManager>();
    }

    // Update is called once per frame
    protected void Update()
    {
        uiManager.HandleInput();
    }

    protected override IEnumerator StartGame()
    {
        yield return SetUpWorld(null, null);
        //yield return uiManager.InitUI(null, null, null);
        yield return MainGameLoop();
    }

    public IEnumerator SetUpWorld(ApRTSTerrainManager terrainManager, RTSCamera mainCamera)
    {
        //SetUpPlayer(i, playerManager.activeWorld);
        yield return null;
        LoadingScreenManager.SetLoadingProgress(.99f);
        LoadingScreenManager.CompleteLoadingScreen();
    }

    protected override IEnumerator MainGameLoop()
    {
        while (true)
        {
            realTimeSinceLastStep += (int)(Time.deltaTime * 1000);
            int stepDt = StepManager.GetDeltaStep();

            while (stepDt < realTimeSinceLastStep)
            {
                StepGame(stepDt);
                realTimeSinceLastStep -= stepDt;
                StepManager.Step();
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }
    }
    
    protected void StepGame(int dt)
    {
        ProcessCommandsForStep(StepManager.CurrentStep);
        foreach (MyMonoBehaviour obj in allObjects)
        {
            obj.MyUpdate();
        }

        HashSet<RTSGameObject> units = playerManager.GetAllUnits();
        List<RTSGameObject> nonNeutralUnits = playerManager.GetNonNeutralUnits();
        foreach (RTSGameObject unit in nonNeutralUnits)
        {
            //rtsGameObjectManager.collisionAvoidanceManager.SyncObjectState(unit, dt);
        }
        orderManager.CarryOutOrders(nonNeutralUnits, dt);
        rtsGameObjectManager.UpdateAll(units, nonNeutralUnits, dt);
        rtsGameObjectManager.HandleUnitCreation();
        rtsGameObjectManager.HandleUnitDestruction();
    }
    
    // Note that new order validation happens only after network instead of 
    // before network on clients and after network on server
    // this will result in invalid orders such as constructing harvesting stations outside of resource range to be sent to server when they could be discarded
    public void ProcessCommandsForStep(long step)
    {
        foreach (MyPair<List<long>, Command> command in commandManager.GetCommandsForStep(step))
        {
            Command comm = command.Value;

            Order order = Command.GetDefaultOrderFunction(comm.getOrder, this).Invoke();
            if (order != null)
            {
                if (comm.overrideDefaultOrderData)
                {
                    order.orderData = comm.orderData;
                }

                ProcessOrder(command.Key, command.Value, order);
            }
        }
    }

    // Assumes order is not null
    public void ProcessOrder(List<long> unitIds, Command command, Order order)
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (long unitId in unitIds)
        {
            RTSGameObject unit = rtsGameObjectManager.GetUnit(unitId);
            if (unit == null) { continue; }
            units.Add(unit);
        }
        Dictionary<RTSGameObject, Order> orders = BuildOrdersForUnits(units, command.orderData.target, command.orderData.targetPosition, order);
        AddOrdersToUnits(orders, command.queueOrderInsteadOfClearing, command.queueOrderAtFront);
        prevKeyClicked = KeyCode.None;
    }


    protected Dictionary<RTSGameObject, Order> BuildOrdersForUnits(List<RTSGameObject> units, RTSGameObject clickedUnit, Vector3 screenClickLocation, Order order)
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

    protected Order AddOrderTargets(Order order, RTSGameObject objectClicked, Vector3 screenClickLocation)
    {
        order.orderData.target = objectClicked;
        order.orderData.targetPosition = screenClickLocation;

        return order;
    }

    protected Order AddOrderDefaultAbility(Order order, RTSGameObject unit)
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

    protected void AddOrdersToUnits(Dictionary<RTSGameObject, Order> orders, bool queueOrdersInsteadOfClearing, bool queueOrdersAtFront)
    {
        if (queueOrdersInsteadOfClearing || queueOrdersAtFront)
        {
            orderManager.QueueOrders(orders, queueOrdersAtFront);
        }
        else
        {
            orderManager.SetOrders(orders);
        }
    }

    protected void AddStartingItems(Player player, RTSGameObject commander)
    {
        Dictionary<Type, int> startingItems = new Dictionary<Type, int>();

        startingItems.Add(typeof(Iron), 1000);
        startingItems.Add(typeof(Stone), 4500);
        startingItems.Add(typeof(Wood), 1000);
        startingItems.Add(typeof(Tool), 400);
        startingItems.Add(typeof(Coal), 2000);

        commander.GetComponent<Storage>().AddItems(startingItems);
    }

    public override void ProduceFromMenu(Type type)
    {
        ProduceFromMenu(type, 1);
    }

    public override void ProduceFromMenu(Type type, int quantity)
    {
        List<RTSGameObject> selectedUnits = selectionManager.GetOrderableSelectedUnitsFromCurrentSubgroup();
        List<long> unitIds = selectedUnits.Select(x => x.unitId).ToList();

        List<MyPair<Type, int>> items = new List<MyPair<Type, int>>() {
                    new MyPair<Type,int>(type, quantity) };

        foreach (RTSGameObject unit in selectedUnits)
        {
            Producer producer = unit.GetComponent<Producer>();
            Mover mover = unit.GetComponent<Mover>();
            if (producer != null && mover != null)
            {
                playerManager.ActivePlayer.aiManager.SetNewPlanForUnit(unit, new ConstructionPlan(playerManager.ActivePlayer.aiManager, rtsGameObjectManager) { thingsToBuild = items });
            }
            else if (producer != null)
            {
                if (type == typeof(Structure))
                {
                    Order order = OrderFactory.BuildConstructionOrder(items);
                    Command command = new Command() { orderData = order.orderData };
                    command.getOrder = OrderBuilderFunction.NewConstructionOrder;
                    command.overrideDefaultOrderData = true;
                    command.queueOrderInsteadOfClearing = true;
                    command.queueOrderAtFront = Input.GetKey(Setting.addOrderToFrontOfQueue);
                    commandManager.AddCommand(command, unitIds);
                }
                else
                {
                    Order order = OrderFactory.BuildProductionOrder(items);
                    Command command = new Command() { orderData = order.orderData };
                    command.getOrder = OrderBuilderFunction.NewProductionOrder;
                    command.overrideDefaultOrderData = true;
                    command.queueOrderInsteadOfClearing = true;
                    command.queueOrderAtFront = Input.GetKey(Setting.addOrderToFrontOfQueue);
                    commandManager.AddCommand(command, unitIds);
                }
            }
        }
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

    public override void SetUpPlayer(int playerId, World world)
    {
        Vector2 startLocation = world.startLocations[playerId - 1];
        Player player = playerManager.players[playerId];
        RTSGameObject commander = rtsGameObjectManager.SpawnUnit(typeof(Commander), new Vector3(startLocation.x, 0, startLocation.y), playerId, null, world).GetComponent<RTSGameObject>();
        player.commander = commander;

        AddStartingItems(player, commander);
        SetStartingCamera(player, startLocation, world);
        playerManager.ActivePlayer.aiManager.SetUpPlayerAIManagers(player);
    }
}
