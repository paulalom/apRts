using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

public abstract class GameManager : MonoBehaviour {

    public static List<MyMonoBehaviour> allObjects = new List<MyMonoBehaviour>(); // except loadingscreen this
    protected KeyCode prevKeyClicked;

    protected Stopwatch stepTimer = new Stopwatch();
    protected int realTimeSinceLastStep; // time in ms
    //public HashSet<Type> selectableTypes = new HashSet<Type>() { typeof(Commander), typeof(Worker), typeof(HarvestingStation), typeof(Tank), typeof(Factory), typeof(PowerPlant) };

    public MyPair<RTSGameObject, MyPair<Type, int>> itemTransferSource = null;

    public virtual void Awake()
    {
        // didnt start from start menu, switching...
        if (LoadingScreenManager.GetInstance() == null)
        {
            DestroyImmediate(this);
            throw new Exception("Warning: Game not started from start menu scene");
        }
        allObjects = new List<MyMonoBehaviour>(); // clean out any instances added before this game started
    }

    // Use this for initialization
    public void Start()
    {
        StartCoroutine(StartGame());
    }

    protected abstract IEnumerator StartGame();

    protected abstract IEnumerator MainGameLoop();

    public abstract void SetUpPlayer(int playerId, World world);

    public abstract void ProduceFromMenu(Type type);
    public abstract void ProduceFromMenu(Type type, int quantity);

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
