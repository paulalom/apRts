﻿using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Events;

//All of the manager classes could probaby be static
public class RTSGameObjectManager : MyMonoBehaviour {

    public GameObject[] editorUnitPrefabs;
    public GameObject[] editorNonUnitPrefabs;
    public GameObject[] editorModelPrefabs;
    public Dictionary<string, GameObject> unitPrefabs;
    public Dictionary<string, GameObject> nonUnitPrefabs;
    public Dictionary<string, GameObject> modelPrefabs;
    public CollisionAvoidanceManager collisionAvoidanceManager = new CollisionAvoidanceManager();
    GameManager gameManager;
    TerrainManager terrainManager;
    PlayerManager playerManager;
    SelectionManager selectionManager;
    OrderManager orderManager;
    UIManager uiManager;
    IStateManager stateManager;

    public static Dictionary<long, RTSGameObject> allUnits = new Dictionary<long, RTSGameObject>();
    // lazy method to prevent spawning from breaking foreach loops
    private List<RTSGameObject> unitCreationQueue = new List<RTSGameObject>();
    private HashSet<RTSGameObject> unitDestructionQueue = new HashSet<RTSGameObject>();

    public LayerMask rtsGameObjectLayerMask;
    
    public class OnUnitChangedEvent : UnityEvent<RTSGameObject> { }
    public OnUnitChangedEvent onUnitCreated = new OnUnitChangedEvent();

    public override void MyAwake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        stateManager = GameObject.Find("StateManager").GetComponent<IStateManager>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        unitPrefabs = new Dictionary<string, GameObject>();
        nonUnitPrefabs = new Dictionary<string, GameObject>();
        modelPrefabs = new Dictionary<string, GameObject>();

        if (modelPrefabs.Count != unitPrefabs.Count)
        {
            throw new Exception("Modelprefab/prefab count mismatch, please check RTSGameObjectManager");
        }
        for (int i = 0; i < editorUnitPrefabs.Length; i++)
        {
            unitPrefabs.Add(editorUnitPrefabs[i].name, editorUnitPrefabs[i]);
            modelPrefabs.Add(editorModelPrefabs[i].name, editorModelPrefabs[i]);
        }
        for (int i = 0; i < editorNonUnitPrefabs.Length; i++)
        {
            nonUnitPrefabs.Add(editorNonUnitPrefabs[i].name, editorNonUnitPrefabs[i]);
        }
            //collisionAvoidanceManager.MyStart();
        }
    
    public void UpdateAll(HashSet<RTSGameObject> units, List<RTSGameObject> nonNeutralUnits, int dt)
    {
        //collisionAvoidanceManager.Update();

        MoveUnits(nonNeutralUnits);        
        SnapToTerrain(nonNeutralUnits, playerManager.ActiveWorld);
    }

    public void HandleUnitCreation()
    {
        if (unitCreationQueue.Count > 0)
        {
            foreach (RTSGameObject unit in unitCreationQueue)
            {
                playerManager.AddUnitForPlayer(unit, unit.ownerId);
                allUnits.Add(unit.unitId, unit);
                SnapToTerrainHeight(unit, unit.world);
            }
            unitCreationQueue.Clear();
        }
    }

    public void HandleUnitDestruction()
    {
        foreach (RTSGameObject unit in unitDestructionQueue)
        {
            selectionManager.selectedUnits.Remove(unit);
            playerManager.players[unit.ownerId].units.Remove(unit);
            allUnits.Remove(unit.unitId);
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
                    GameObject go = Instantiate(nonUnitPrefabs["Explosion"],
                                                        unit.transform.position,
                                                        Quaternion.identity) as GameObject;
                    go.name = "Explosion xyz: " + unit.transform.position.x + ", " + unit.transform.position.y + ", " + unit.transform.position.z;
                    Destroy(go, go.GetComponent<ParticleSystem>().main.duration);
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
        name += playerManager.GetNumUnitsForPlayer(0);
        return NewDeposit(name, color, type, items, position, world);
    }
    
    public GameObject NewDeposit(string name, Color color, DepositType type, Dictionary<Type, int> items, Vector3 position, World world)
    {
        GameObject go = SpawnUnit(typeof(ResourceDeposit), position, 0, null, world).gameObject;
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
    
    public RTSGameObject StartNewStructure(Type type, GameObject producer)
    {
        RTSGameObject rtsGo = producer.GetComponent<RTSGameObject>();
        
        if (!unitPrefabs.ContainsKey(type.ToString()))
        {
            throw new Exception("Attempting to spawn type: " + type + " which does not exist in prefab list");
        }
        Vector3 positionToSpawn = GetPositionToSpawn(producer, type);
        return StartNewStructure(type, positionToSpawn, rtsGo.ownerId, producer, rtsGo.world);
    }

    //The "around" bit is todo
    public List<RTSGameObject> SpawnUnitsAround(Type type, int quantity, GameObject producer)
    {
        RTSGameObject rtsGo = producer.GetComponent<RTSGameObject>();
        List<RTSGameObject> newUnits = new List<RTSGameObject>();

        if (!unitPrefabs.ContainsKey(type.ToString()))
        {
            throw new ArgumentException("Attempting to spawn type: " + type + " which does not exist in prefab list");
        }
        for (int i = 0; i < quantity; i++)
        {
            Vector3 positionToSpawn = GetPositionToSpawn(producer, type);
            newUnits.Add(SpawnUnit(type, positionToSpawn, rtsGo.ownerId, producer, rtsGo.world));
        }
        return newUnits;
    }

    public RTSGameObject StartNewStructure(Type type, Vector3 position, int ownerId, GameObject producer, World world)
    {
        string unitPrefabType = type.ToString() + "UnderConstruction";
        return SpawnUnit(unitPrefabType, position, ownerId, producer, world);
    }

    public RTSGameObject SpawnUnit(Type type, Vector3 position, int ownerId, GameObject producer, World world)
    {
        return SpawnUnit(type.ToString(), position, ownerId, producer, world);
    }
    
    public RTSGameObject SpawnUnit(string type, Vector3 position, int ownerId, GameObject producer, World world)
    {
        GameObject prefab = GetPrefab(type);
        GameObject newUnit = Instantiate(prefab,
            position,
            Quaternion.identity) as GameObject;
        newUnit.name = type.ToString() + playerManager.GetNumUnitsForPlayer(type, ownerId);

        return BuildNewRTSGameObject(newUnit, ownerId, producer, world);
    }

    GameObject GetPrefab(string type)
    {
        if (unitPrefabs.ContainsKey(type))
        {
            return unitPrefabs[type];
        }
        if (nonUnitPrefabs.ContainsKey(type))
        {
            return nonUnitPrefabs[type];
        }
        if (modelPrefabs.ContainsKey(type))
        {
            return modelPrefabs[type];
        }
        throw new Exception("Attempting to instantiate type not in prefabs: " + type);
    }

    public RTSGameObject BuildNewRTSGameObject(GameObject newUnit, int ownerId, GameObject producer, World world)
    {
        RTSGameObject rtsGo = newUnit.GetComponent<RTSGameObject>();
        Storage storage = newUnit.GetComponent<Storage>();
        
        rtsGo.ownerId = ownerId;
        rtsGo.world = world;
        rtsGo.unitId = stateManager.GetNextUID();

        if (storage != null)
        {
            storage.onStorageAddEvent.AddListener(playerManager.players[ownerId].AddResources);
            storage.onStorageTakeEvent.AddListener(playerManager.players[ownerId].TakeResources);
        }
        if (rtsGo.flagRenderer == null)
        {
            rtsGo.flagRenderer = newUnit.GetComponent<Renderer>();
            if (rtsGo.flagRenderer == null)
            {
                rtsGo.flagRenderer = newUnit.GetComponentInChildren<Renderer>();
            }
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
        if (rtsGo is Factory)
        {
            var storage2 = rtsGo.GetComponent<Storage>();
            storage.AddItems(new Dictionary<Type, int>()
            {
                { typeof(Wood), 1000 },
                { typeof(Stone), 1000 },
                { typeof(Coal), 1000 },
                { typeof(Iron), 1000 },
                { typeof(Paper), 1000 },
                { typeof(Tool), 1000 }
            });
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
            ((ApRTSTerrainManager)terrainManager).FlattenTerrainUnderObject(unit, unit.world);
        }
        onUnitCreated.Invoke(unit);
        unit.IsIdle = true;
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

    private void MoveUnits(List<RTSGameObject> units)
    {
        List<Mover> movers = new List<Mover>();

        foreach (RTSGameObject unit in units)
        {
            movers.Add(unit.GetComponent<Mover>());
        }
        movers.RemoveAll(x => x == null || x.isActive == false);
        foreach (Mover mover in movers)
        {
            Vector3 vel = mover.velocity;
            mover.transform.position += vel;
            mover.SetVelocity2D(Vector2.zero);
        }
    }
    
    public void SetUnitMoveTarget(RTSGameObject unit, Vector2 targetPos, int dt)
    {
        Mover mover = unit.GetComponent<Mover>();
        if (mover != null && mover.isActive)
        {
            Vector2 velocity = Vector2.MoveTowards(unit.Position2D, targetPos, mover.moveSpeed * dt/1000f) - unit.Position2D;
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
    
    public void CheatRaiseTerrain(Vector3 position)
    {
        try
        { //Try catch to swallow exception. FixMe
          // only does raiseTerrain
            terrainManager.ModifyTerrain(position, .003f, 20, playerManager.ActiveWorld);
        }
        catch (Exception e) { }
    }

    public void CheatSpawnFactory(Vector3 position, int ownerId)
    {
        SpawnUnit(typeof(Factory), position, ownerId, playerManager.ActivePlayer.units.FirstOrDefault().gameObject, playerManager.ActiveWorld);
    }

    public void DestroyUnit(RTSGameObject unit)
    {
        unitDestructionQueue.Add(unit);
        unit.onDestroyed.Invoke(unit);
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

    /// <summary>
    /// Same as GetNearestComponentInRange except this cant filter for the source unit, so make sure you're not searching for a type the unit contains.
    /// </summary>
    public RTSGameObject GetNearestComponentInRangeOfType(Vector3 searchPosition, Type ComponentType, float range, LayerMask mask)
    {
        Collider closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Collider[] objectsInRange = Physics.OverlapSphere(searchPosition, range, mask);

        foreach (Collider c in objectsInRange)
        {
            if ((ComponentType != null && c.GetComponent(ComponentType) == null))
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
        return new Vector3(producer.transform.position.x,
                producer.transform.position.y, 
                producer.transform.position.z + producer.transform.localScale.z / 2 
                + unitPrefabs[typeToSpawn.ToString()].transform.localScale.z / 2 + 1);
    }

    public RTSGameObject GetUnit(long unitId)
    {
        if (allUnits.ContainsKey(unitId))
        {
            return allUnits[unitId];
        }
        else
        {
            return null;
        }
    }

    public List<RTSGameObject> GetUnits(List<long> unitIds)
    {
        List<RTSGameObject> units = new List<RTSGameObject>();
        foreach (long id in unitIds)
        {
            RTSGameObject unit = GetUnit(id);
            if (unit != null)
            {
                units.Add(unit);
            }
        }
        return units;
    }

    // no null return, send a new RTSGameObject instead
    public RTSGameObject GetUnitOrDefault(long unitId)
    {

        if (allUnits.ContainsKey(unitId))
        {
            return allUnits[unitId];
        }
        else
        {
            return new RTSGameObject() { ownerId = -1 };
        }
    }

    public void CreateText(string text, Vector3 position)
    {
        uiManager.CreateText(text, position);
    }
}
