using System;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Events;

//All of the manager classes could probaby be static
public class RTSGameObjectManager : MonoBehaviour {
    
    //Instantiating prefabs with resources.load is slow so here we are
    //Maybe this should be in an assets class or something like that
    //We cant expose a dictionary to the inspector so we expose an array then populate the dictionary
    //if we need to do this too often ill just make a component for this
    public string[] InspectorPrefabNames;
    public GameObject[] InspectorPrefabTypes;
    public Dictionary<string, GameObject> prefabs;
//    public List<RTSGameObject> units;
    GameManager gameManager;
    TerrainManager terrainManager;
    PlayerManager playerManager;
    OrderManager orderManager;

    // lazy method to prevent spawning from breaking foreach loops
    private List<RTSGameObject> unitCreationQueue = new List<RTSGameObject>();
    private HashSet<RTSGameObject> unitDestructionQueue = new HashSet<RTSGameObject>();

    public LayerMask rtsGameObjectLayerMask;
    
    public class OnUnitCreatedEvent : UnityEvent<RTSGameObject> { }
    public OnUnitCreatedEvent onUnitCreated = new OnUnitCreatedEvent();

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        terrainManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<TerrainManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        orderManager = GameObject.FindGameObjectWithTag("OrderManager").GetComponent<OrderManager>();
        prefabs = new Dictionary<string, GameObject>();
        rtsGameObjectLayerMask = 1 << LayerMask.NameToLayer("RTSGameObject");


        if (InspectorPrefabNames.Length != InspectorPrefabTypes.Length)
        {
            throw new System.Exception("fix the prefabs arrays in the rts game object manager");
        }
        if (InspectorPrefabNames.Length <= 0)
        {
            throw new System.Exception("Populate the prefabs arrays in the rts game object manager");
        }
        for (int i = 0; i < InspectorPrefabTypes.Length; i++)
        {
            Debug.Log(InspectorPrefabNames[i] + ", " + InspectorPrefabTypes[i].ToString());
            prefabs.Add(InspectorPrefabNames[i], InspectorPrefabTypes[i]);
        }
        if (InspectorPrefabNames.Length != prefabs.Count)
        {
            throw new System.Exception("No duplicate prefab names in the rts game object manager");
        }
    }

    void Update()
    {
        if (unitCreationQueue.Count > 0)
        {
            foreach (RTSGameObject unit in unitCreationQueue)
            {
                playerManager.AddUnit(unit, unit.ownerId);
            }
            unitCreationQueue.Clear();
        }
        if (unitDestructionQueue.Count > 0)
        {
            foreach (RTSGameObject unit in unitDestructionQueue)
            {
                playerManager.players[unit.ownerId].units.Remove(unit);
                playerManager.players[unit.ownerId].units.Remove(unit);

                try {
                    // I don't think this is the right way to handle death animations, but it should be good enough for now.
                    Explosion explosion = unit.GetComponent<Explosion>();
                    if (explosion != null)
                    {
                        GameObject go = Instantiate(prefabs["Explosion"],
                                                            unit.transform.position,
                                                            Quaternion.identity) as GameObject;
                        go.name = "Explosion xyz: " + unit.transform.position.x + ", " + unit.transform.position.y + ", " + unit.transform.position.z;
                        Destroy(go, go.GetComponent<ParticleSystem>().duration);
                    }

                    Destroy(unit.gameObject, .2f); // .2s delay is hack to ensure this loop finishes before objects are destroyed
                }
                catch (Exception e)
                {
                    Debug.Log("Exception: Trying to destroy an object which no longer exists");
                }
            }
            unitDestructionQueue.Clear();
            
        }
    }

    public void SnapToTerrainHeight(RTSGameObject obj)
    {
        TerrainManager tMan = gameManager.terrainManager;
        Vector3 position = obj.transform.position;
        position.y = tMan.GetHeightFromGlobalCoords(position.x, position.z) + obj.transform.localScale.y/2 + obj.flyHeight;
        obj.transform.position = position;
    }
    public void SnapToTerrainHeight(List<RTSGameObject> objs)
    {
        foreach (RTSGameObject obj in objs)
        {
            SnapToTerrainHeight(obj);
        }
    }

    public void SetTargetLocation(RaycastHit hit)
    {

    }

    public GameObject NewDeposit(DepositType type, Dictionary<Type, int> items, Vector3 position)
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
        return NewDeposit(name, color, type, items, position);
    }


    public GameObject NewDeposit(string name, Color color, DepositType type, Dictionary<Type, int> items, Vector3 position)
    {
        GameObject go = SpawnUnit(typeof(ResourceDeposit), position, 0);
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


    //The "around" bit is todo
    public bool SpawnUnitsAround(Type type, int quantity, GameObject producer)
    {
        if (!prefabs.ContainsKey(type.ToString()))
        {
            throw new ArgumentException("Attempting to spawn type: " + type + " which does not exist in prefab list");
        }
        for (int i = 0; i < quantity; i++)
        {
            SpawnUnit(type, new Vector3(producer.transform.position.x + producer.transform.localScale.x/2 + prefabs[type.ToString()].transform.localScale.x/2 + 1, 
                producer.transform.position.y, producer.transform.position.z), producer.GetComponent<RTSGameObject>().ownerId);
        }
        return true;
    }

    public GameObject SpawnUnit(Type type, Vector3 position, int ownerId)
    {
        Debug.Log(type.ToString());
        GameObject go = Instantiate(prefabs[type.ToString()],
            position,
            Quaternion.identity) as GameObject;
        go.name = type.ToString() + playerManager.GetNumUnits(type, ownerId);
        RTSGameObject rtsGo = go.GetComponent<RTSGameObject>();
        rtsGo.ownerId = ownerId;
        rtsGo.flagRenderer = go.GetComponent<Renderer>();
        if (rtsGo.flagRenderer == null)
        {
            rtsGo.flagRenderer = go.GetComponentInChildren<Renderer>();
        }
        if (ownerId == 2)
        {
            rtsGo.flagRenderer.material.color = Color.black;
        }
        unitCreationQueue.Add(rtsGo);
        
        if (gameManager.debug && type == typeof(Factory))
        {
            Dictionary<Type, int> items = new Dictionary<Type, int>();
            items.Add(typeof(Coal), 2000);
            items.Add(typeof(Iron), 2000);
            items.Add(typeof(Wood), 2000);
            items.Add(typeof(Stone), 2000);
            items.Add(typeof(Paper), 200);
            items.Add(typeof(Tool), 100);
            rtsGo.storage.AddItems(items);
        }
        if (rtsGo.unitType == UnitType.Structure)
        {
            terrainManager.FlattenTerrainUnderObject(rtsGo);
        }
        onUnitCreated.Invoke(rtsGo);
        rtsGo.Idle = true;
        return go;
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
    
    public void TakeItem(RTSGameObject taker, RTSGameObject target, MyKVP<Type, int> item)
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

    public void GiveItem(RTSGameObject giver, RTSGameObject target, MyKVP<Type, int> item)
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
            Vector2 newPos = Vector2.MoveTowards(new Vector2(unit.transform.position.x, unit.transform.position.z), targetPos, moveSpeed * dt);
            unit.transform.position = new Vector3(newPos.x, unit.transform.position.y, newPos.y);
        }
    }

    public void MoveUnit(RTSGameObject unit, Vector2 targetPos, float dt)
    {
        MoveUnit(unit, targetPos, unit.GetComponent<Mover>().moveSpeed, dt);
    }

    public void UseAbility(RTSGameObject unit, RTSGameObject target, Vector3 targetPosition, Ability ability)
    {
        if (ability.GetType() == typeof(Shoot))
        {
            // still need to refine the damage system of course
            BasicCannonProjectile projectile = SpawnUnit(unit.GetComponent<Shoot>().projectileType, unit.transform.position, unit.ownerId).GetComponent<BasicCannonProjectile>();
            projectile.parent = unit;
            projectile.GetComponent<Explosion>().damage = unit.GetComponent<Cannon>().basedamage + projectile.baseDamage;
            orderManager.SetOrder(projectile.GetComponent<RTSGameObject>(), new Order() { target = target, targetPosition = targetPosition, orderRange = 0.3f, type = OrderType.UseAbillity, ability = projectile.GetComponent<Explosion>()});
        }
        else if (ability.GetType() == typeof(Explosion))
        {
            Explosion explosion = ((Explosion)(ability));
            DamageAllInRadius(unit, explosion.radius, explosion.damage);
            DestroyUnit(unit);
        }
    }

    public void DamageAllInRadius(RTSGameObject source, float range, float damage)
    {
        List<Defense> defenses = GetAllComponentsInRangeOfType<Defense>(source.transform.position, range, rtsGameObjectLayerMask);
        foreach(Defense defense in defenses)
        {
            defense.hull.hullPoints -= damage; // simplistic for now
            if (defense.hull.hullPoints <= 0)
            {
                DestroyUnit(defense.GetComponent<RTSGameObject>());
            }
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

    public List<componentType> GetAllComponentsInRangeOfType<componentType>(Vector3 source, float range, LayerMask mask)
    {
        Collider[] objectsInRange = GetAllCollidersInRangeOfType(source, range, mask);
        List<componentType> componentsInRange = new List<componentType>();
        foreach (Collider collider in objectsInRange)
        {
            componentType component = collider.GetComponent<componentType>();
            if (component != null)
            {
                componentsInRange.Add(component);
            }
        }
        return componentsInRange;
    }

    private Collider[] GetAllCollidersInRangeOfType(Vector3 source, float range, LayerMask mask)
    {
        return Physics.OverlapSphere(source, range, mask);
    }
    
    // O(n) search + whatever sphereCast is (couldnt find it, but im assuming with octTree implementation it should be O(log(n))
    public RTSGameObject GetNearestComponentInRange(Collider sourceCollider, Vector3 searchPosition, float range, LayerMask mask, Type ComponentType = null)
    {
        Collider closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Collider[] objectsInRange = GetAllCollidersInRangeOfType(searchPosition, range, mask);

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
}
