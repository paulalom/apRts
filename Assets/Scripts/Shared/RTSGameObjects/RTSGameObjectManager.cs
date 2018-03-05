using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Events;

//All of the manager classes could probaby be static
public class RTSGameObjectManager : MyMonoBehaviour {

    public GameObject[] InspectorPrefabTypes;
    public Dictionary<string, GameObject> prefabs;
    public CollisionAvoidanceManager collisionAvoidanceManager = new CollisionAvoidanceManager();
    GameManager gameManager;
    TerrainManager terrainManager;
    PlayerManager playerManager;
    OrderManager orderManager;

    public static Dictionary<long, RTSGameObject> allUnits = new Dictionary<long, RTSGameObject>();
    // lazy method to prevent spawning from breaking foreach loops
    private List<RTSGameObject> unitCreationQueue = new List<RTSGameObject>();
    private HashSet<RTSGameObject> unitDestructionQueue = new HashSet<RTSGameObject>();

    public LayerMask rtsGameObjectLayerMask;
    
    public class OnUnitCreatedEvent : UnityEvent<RTSGameObject> { }
    public OnUnitCreatedEvent onUnitCreated = new OnUnitCreatedEvent();

    public override void MyAwake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        prefabs = new Dictionary<string, GameObject>();
        
        for (int i = 0; i < InspectorPrefabTypes.Length; i++)
        {
            prefabs.Add(InspectorPrefabTypes[i].name, InspectorPrefabTypes[i]);
        }
        collisionAvoidanceManager.MyStart();
    }
    
    public void UpdateAll(HashSet<RTSGameObject> units, List<RTSGameObject> nonNeutralUnits, float dt)
    {
        collisionAvoidanceManager.Update();
        foreach (RTSGameObject unit in nonNeutralUnits)
        {
            MoveUnit(unit);
        }
        SnapToTerrain(nonNeutralUnits, playerManager.activeWorld);
    }

    public void HandleUnitCreation()
    {
        if (unitCreationQueue.Count > 0)
        {
            foreach (RTSGameObject unit in unitCreationQueue)
            {
                playerManager.AddUnit(unit, unit.ownerId);
                allUnits.Add(unit.uid, unit);
                SnapToTerrainHeight(unit, unit.world);
            }
            unitCreationQueue.Clear();
        }
    }

    public void HandleUnitDestruction()
    {
        foreach (RTSGameObject unit in unitDestructionQueue)
        {
            playerManager.ActivePlayer.selectedUnits.Remove(unit.uid);
            playerManager.players[unit.ownerId].units.Remove(unit.uid);
            allUnits.Remove(unit.uid);
            if (!(unit is Projectile))
            {
                playerManager.players[unit.ownerId].onUnitCountDecrease.Invoke(unit);
            }
            GameManager.DeregisterObject(unit);
            collisionAvoidanceManager.FreeObject(unit);
            try
            {
                // I don't think this is the right way to handle death animations, but it should be good enough for now.
                Explode explosion = unit.GetComponent<Explode>();
                if (explosion != null)
                {
                    GameObject go = Instantiate(prefabs["Explosion"],
                                                        unit.transform.position,
                                                        Quaternion.identity) as GameObject;
                    go.name = "Explosion xyz: " + unit.transform.position.x + ", " + unit.transform.position.y + ", " + unit.transform.position.z;
                    Destroy(go, go.GetComponent<ParticleSystem>().duration);
                }

                Destroy(unit.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log("Exception: Trying to destroy an object which no longer exists: " + e.Message);
            }
        }
        unitDestructionQueue.Clear();
    }

    public void SnapToTerrainHeight(RTSGameObject obj, World world)
    {
        Vector3 position = obj.transform.position;
        position.y = terrainManager.GetHeightFromGlobalCoords(position.x, position.z, world) + obj.transform.localScale.y/2 + obj.flyHeight;
        obj.transform.position = position;
    }
    public void SnapToTerrain(RTSGameObject obj, World world)
    {
        if (!terrainManager.DoesTerrainExistForPoint(obj.transform.position, world))
        {
            SnapToTerrainHeight(obj, obj.world);
        }
    }

    public void SnapToTerrain(List<RTSGameObject> objs, World world)
    {
        foreach (RTSGameObject obj in objs)
        {
            if (obj.prevPositionForHeightMapCheck != obj.transform.position)
            {
                if (terrainManager.DoesTerrainExistForPoint(obj.transform.position, world))
                {
                    SnapToTerrainHeight(obj, obj.world);
                }
                else
                {
                    //SnapToTerrain(obj, obj.world);
                }
            }
            obj.prevPositionForHeightMapCheck = obj.transform.position;
        }
    }
    
    public GameObject NewDeposit(DepositType type, Dictionary<Type, int> items, Vector3 position, World world)
    {
        Color color = Color.gray;
        string name = "Deposit";
        if (type == DepositType.Iron)
        {
            color = Color.red;
            name = "IronDeposit";
        }
        else if (type == DepositType.Coal)
        {
            color = Color.black;
            name = "CoalDeposit";
        }
        else if (type == DepositType.Forest)
        {
            color = Color.green;
            name = "ForstDeposit";
        }
        name += playerManager.GetNumUnits(0);
        return NewDeposit(name, color, type, items, position, world);
    }
    
    public GameObject NewDeposit(string name, Color color, DepositType type, Dictionary<Type, int> items, Vector3 position, World world)
    {
        GameObject go = SpawnUnit(typeof(ResourceDeposit), position, 0, null, world);
        ResourceDeposit deposit = go.GetComponent<ResourceDeposit>();
        deposit.type = type;
        go.name = name;
        try {
            go.GetComponentInChildren<Renderer>().material.color = color;
        }
        catch (Exception e)
        {
            throw new Exception("Dont be lazy next time");
        }
        go.GetComponent<Storage>().AddItems(items);

        if (type == DepositType.Coal)
        {
            deposit.harvestItems.Add(typeof(Coal), 50);
            deposit.harvestItems.Add(typeof(Stone), 50);
        }
        else if (type == DepositType.Forest)
        {
            deposit.harvestItems.Add(typeof(Wood), 50);
        }
        else if (type == DepositType.Iron)
        {
            deposit.harvestItems.Add(typeof(Iron), 25);
            deposit.harvestItems.Add(typeof(Stone), 50);
        }

        return go;
    }
    
    public bool StartNewStructure(Type type, int quantity, GameObject producer)
    {
        RTSGameObject rtsGo = producer.GetComponent<RTSGameObject>();
        if (!prefabs.ContainsKey(type.ToString()))
        {
            throw new Exception("Attempting to spawn type: " + type + " which does not exist in prefab list");
        }
        for (int i = 0; i < quantity; i++)
        {
            Vector3 positionToSpawn = GetPositionToSpawn(producer, type);
            StartNewStructure(type, positionToSpawn, rtsGo.ownerId, producer, rtsGo.world);
        }
        return true;
    }

    //The "around" bit is todo
    public bool SpawnUnitsAround(Type type, int quantity, GameObject producer)
    {
        RTSGameObject rtsGo = producer.GetComponent<RTSGameObject>();
        if (!prefabs.ContainsKey(type.ToString()))
        {
            throw new ArgumentException("Attempting to spawn type: " + type + " which does not exist in prefab list");
        }
        for (int i = 0; i < quantity; i++)
        {
            Vector3 positionToSpawn = GetPositionToSpawn(producer, type);
            SpawnUnit(type, positionToSpawn, rtsGo.ownerId, producer, rtsGo.world);
        }
        return true;
    }

    public GameObject StartNewStructure(Type type, Vector3 position, int ownerId, GameObject producer, World world)
    {
        string unitPrefabType = type.ToString() + "UnderConstruction";
        return SpawnUnit(unitPrefabType, position, ownerId, producer, world);
    }

    public GameObject SpawnUnit(Type type, Vector3 position, int ownerId, GameObject producer, World world)
    {
        return SpawnUnit(type.ToString(), position, ownerId, producer, world);
    }
    
    public GameObject SpawnUnit(string type, Vector3 position, int ownerId, GameObject producer, World world)
    {
        GameObject newUnit = Instantiate(prefabs[type],
            position,
            Quaternion.identity) as GameObject;
        newUnit.name = type.ToString() + playerManager.GetNumUnits(type, ownerId);

        BuildNewRTSGameObject(newUnit, type, ownerId, producer, world);
        
        return newUnit;
    }

    public RTSGameObject BuildNewRTSGameObject(GameObject newUnit, string requestedType, int ownerId, GameObject producer, World world)
    {
        RTSGameObject rtsGo = newUnit.GetComponent<RTSGameObject>();
        Storage storage = newUnit.GetComponent<Storage>();

        if (requestedType.Contains("UnderConstruction"))
        {
            producer.GetComponent<Worker>().unitUnderConstruction = rtsGo;
        }
        else if(rtsGo is Structure){
            ((Structure)rtsGo).underConstruction = false;
        }

        rtsGo.ownerId = ownerId;
        rtsGo.world = world;
        rtsGo.flagRenderer = newUnit.GetComponent<Renderer>();
        rtsGo.uid = gameManager.netStateManager.GetNextUID();

        if (storage != null)
        {
            storage.onStorageAddEvent.AddListener(playerManager.players[ownerId].AddResources);
            storage.onStorageTakeEvent.AddListener(playerManager.players[ownerId].TakeResources);
        }
        if (rtsGo.flagRenderer == null)
        {
            rtsGo.flagRenderer = newUnit.GetComponentInChildren<Renderer>();
        }
        switch (ownerId)
        {
            case 1:
                rtsGo.flagRenderer.material.color = Color.red;
                break;
            case 2:
                rtsGo.flagRenderer.material.color = Color.blue;
                break;
            case 3:
                rtsGo.flagRenderer.material.color = Color.cyan;
                break;
            case 4:
                rtsGo.flagRenderer.material.color = Color.magenta;
                break;
            case 5:
                rtsGo.flagRenderer.material.color = Color.yellow;
                break;
            case 6:
                rtsGo.flagRenderer.material.color = Color.green;
                break;
            case 7:
                rtsGo.flagRenderer.material.color = Color.gray;
                break;
            case 8:
                rtsGo.flagRenderer.material.color = Color.black;
                break;
            default:
                break;
        }


        if (gameManager.debug && rtsGo.GetType() == typeof(Factory))
        {
            Dictionary<Type, int> items = new Dictionary<Type, int>();
            items.Add(typeof(Coal), 2000);
            items.Add(typeof(Iron), 2000);
            items.Add(typeof(Wood), 2000);
            items.Add(typeof(Stone), 2000);
            items.Add(typeof(Paper), 200);
            items.Add(typeof(Tool), 10);
            rtsGo.storage.AddItems(items);
        }
        InsertRTSGameObjectIntoGame(rtsGo);
        if (!(rtsGo is Projectile))
        {
            playerManager.players[ownerId].onUnitCountIncrease.Invoke(rtsGo);
        }
        
        return rtsGo;
    }

    public void InsertRTSGameObjectIntoGame(RTSGameObject unit)
    {
        if (unit.GetType().IsSubclassOf(typeof(Structure)))
        {
            terrainManager.FlattenTerrainUnderObject(unit, unit.world);
        }
        onUnitCreated.Invoke(unit);
        unit.Idle = true;
        unitCreationQueue.Add(unit);
    }

    public bool Harvest(RTSGameObject taker, ResourceDeposit target)
    {
        Harvester harvester = taker.GetComponent<Harvester>();
        Producer producer = taker.GetComponent<Producer>();
        if (target == null || harvester == null)
        {
            Debug.Log("Whats Going on!?");
            return false; // some weird joojoo here
        }
        harvester.harvestTarget = target;
        harvester.IsActive = true;
        return true;
    }

    public void TakeItems(RTSGameObject taker, RTSGameObject target, List<MyPair<Type, int>> items)
    {
        foreach (MyPair<Type, int> item in items)
        {
            TakeItem(taker, target, item);
        }
    }

    public void GiveItems(RTSGameObject giver, RTSGameObject target, List<MyPair<Type, int>> items)
    {
        foreach (MyPair<Type, int> item in items)
        {
            GiveItem(giver, target, item);
        }
    }

    public void TakeItem(RTSGameObject taker, RTSGameObject target, MyPair<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage takerStorage = taker.GetComponent<Storage>();
        int taken = targetStorage.TakeItem(item.Key, item.Value, false);
        int acquired = takerStorage.AddItem(item.Key, taken, false);
        if (acquired != taken)
        {
            targetStorage.AddItem(item.Key, taken - acquired, false);
        }
    }
    public void GiveItem(RTSGameObject giver, RTSGameObject target, MyPair<Type, int> item)
    {
        Storage targetStorage = target.GetComponent<Storage>();
        Storage giverStorage = giver.GetComponent<Storage>();
        int given = giverStorage.TakeItem(item.Key, item.Value, false);
        int acquired = targetStorage.AddItem(item.Key, given, false);
        if (acquired != given)
        {
            giverStorage.AddItem(item.Key, given - acquired, false);
        }
    }

    public void MoveUnit(RTSGameObject unit, Vector2 targetPos, float moveSpeed, float dt)
    {
        Mover mover = unit.GetComponent<Mover>();
        if (mover.isActive)
        {
            Vector2 newPos = Vector2.MoveTowards(unit.Position2D, targetPos, moveSpeed * dt);
            unit.transform.position = new Vector3(newPos.x, unit.transform.position.y, newPos.y);
        }
    }
    public void MoveUnit(RTSGameObject unit)
    {
        Mover mover = unit.GetComponent<Mover>();
        if (mover != null && mover.isActive)
        {
            Vector3 vel = mover.velocity;
            unit.transform.position += vel;
        }
    }

    public void SetUnitMoveTarget(RTSGameObject unit, Vector2 targetPos, float dt)
    {
        Mover mover = unit.GetComponent<Mover>();
        if (mover != null && mover.isActive)
        {
            Vector2 velocity = Vector2.MoveTowards(unit.Position2D, targetPos, mover.moveSpeed * dt) - unit.Position2D;
            mover.SetVelocity2D(velocity);
        }
    }

    public void UseAbility(RTSGameObject unit, RTSGameObject target, Vector3 targetPosition, Ability ability)
    {
        if (ability.GetType() == typeof(Shoot))
        {
            // still need to refine the damage system of course
            BasicCannonProjectile projectile = SpawnUnit(unit.GetComponent<Shoot>().projectileType, unit.transform.position, unit.ownerId, unit.gameObject, unit.world).GetComponent<BasicCannonProjectile>();
            projectile.parent = unit;
            projectile.ownerId = unit.ownerId;
            projectile.GetComponent<Explode>().damage = unit.GetComponent<Cannon>().basedamage + projectile.baseDamage;
            orderManager.SetOrder(projectile.GetComponent<RTSGameObject>(), OrderFactory.BuildAbilityOrder(target, targetPosition, 0.3f, projectile.GetComponent<Explode>()));
        }
        else if (ability.GetType() == typeof(Explode))
        {
            Explode explosion = ((Explode)(ability));
            DamageAllInRadius(unit, explosion.radius, explosion.damage);
            DestroyUnit(unit);
        }
    }

    public void DamageAllInRadius(RTSGameObject source, float range, float damage)
    {
        List<Defense> defenses = GetAllComponentsInRangeOfType<Defense>(source.transform.position, range, rtsGameObjectLayerMask);
        HashSet<RTSGameObject> unitsToDamage = new HashSet<RTSGameObject>(); // this set must be unique

        foreach (Defense defense in defenses)
        {
            unitsToDamage.Add(defense.owner);
        }
        foreach(RTSGameObject unit in unitsToDamage)
        {
            unit.TakeDamage(damage);
        }
    }

    public void DestroyUnit(RTSGameObject unit)
    {
        unitDestructionQueue.Add(unit);
    }

    public bool lazyWithinDist(Vector3 o1, Vector3 o2, float dist)
    {
        return Math.Abs(o1.x - o2.x) < dist && Math.Abs(o1.z - o2.z) < dist;
    }
    
    public RTSGameObject GetNearestEnemy(RTSGameObject unit)
    {
        float minDist = -1;
        RTSGameObject closest = null;
        float dist;
        foreach (RTSGameObject rtsGo in playerManager.GetEnemyUnits(unit))
        {
            dist = (unit.transform.position - rtsGo.transform.position).sqrMagnitude;
            if (dist < minDist || minDist == -1)
            {
                closest = rtsGo;
                minDist = dist;
            }
        }
        return closest;
    }
    
    public void CheckIfUnitsInTerrain(List<RTSGameObject> units)
    {
        foreach (RTSGameObject unit in units)
        {
            terrainManager.GenerateChunkAtPositionIfMissing(unit.transform.position, unit.world);
        }
    }
    public List<componentType> GetAllComponentsInRangeOfTypeOwnedByPlayerInOrder<componentType>(Vector3 source, float range, int ownerId, LayerMask mask)
    {
        List<componentType> components = GetAllComponentsInRangeOfTypeOwnedByPlayer<componentType>(source, range, ownerId, mask);
        components.Reverse(); // nearest to farthest
        return components;
    }

    public List<componentType> GetAllComponentsInRangeOfTypeOwnedByPlayer<componentType>(Vector3 source, float range, int ownerId, LayerMask mask)
    {
        List<Collider> colliders = GetAllCollidersInRangeOfType<componentType>(source, range, mask);
        List<componentType> componentsOwnedByPlayer = new List<componentType>();
        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<RTSGameObject>().ownerId == ownerId)
            {
                componentsOwnedByPlayer.Add(collider.GetComponent<componentType>());
            }
        }
        return componentsOwnedByPlayer;
    }

    public List<componentType> GetAllComponentsInRangeOfTypeInOrder<componentType>(Vector3 source, float range, LayerMask mask)
    {
        List<componentType> components = GetAllComponentsInRangeOfType<componentType>(source, range, mask);
        components.Reverse(); // nearest to farthest
        return components;
    }

    public List<componentType> GetAllComponentsInRangeOfType<componentType>(Vector3 source, float range, LayerMask mask)
    {
        List<Collider> collidersInRangeOfType = GetAllCollidersInRangeOfType<componentType>(source, range, mask);
        List<componentType> componentsInRangeOfType = new List<componentType>();
        foreach (Collider collider in collidersInRangeOfType)
        {
            componentsInRangeOfType.Add(collider.GetComponent<componentType>());
        }
        return componentsInRangeOfType;
    }

    private List<Collider> GetAllCollidersInRangeOfType<componentType>(Vector3 source, float range, LayerMask mask)
    {
        Collider[] collidersInRange = Physics.OverlapSphere(source, range, mask);
        List<Collider> collidersInRangeOfType = new List<Collider>();
        foreach (Collider collider in collidersInRange)
        {
            componentType component = collider.GetComponent<componentType>();
            if (component != null)
            {
                collidersInRangeOfType.Add(collider);
            }
        }
        return collidersInRangeOfType;
    }
    
    // O(n) search + whatever sphereCast is (couldnt find it, but im assuming with octTree implementation it should be O(log(n))
    public RTSGameObject GetNearestComponentInRange(Collider sourceCollider, Vector3 searchPosition, float range, LayerMask mask, Type ComponentType = null)
    {
        Collider closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Collider[] objectsInRange = Physics.OverlapSphere(searchPosition, range, mask);

        foreach (Collider c in objectsInRange)
        {
            if (c == sourceCollider || (ComponentType != null && c.GetComponent(ComponentType) == null))
            {
                continue;
            }
            Vector3 directionToTarget = c.transform.position - searchPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = c;
            }
        }
        if (closest != null)
        {
            return closest.GetComponent<RTSGameObject>();
        }
        else
        {
            return null;
        }
    }

    Vector3 GetPositionToSpawn(GameObject producer, Type typeToSpawn)
    {
        return new Vector3(producer.transform.position.x + producer.transform.localScale.x / 2 + prefabs[typeToSpawn.ToString()].transform.localScale.x / 2 + 1,
                producer.transform.position.y, producer.transform.position.z);
    }
}
