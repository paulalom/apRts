using UnityEngine;
using System.Collections;

public abstract class MyMonoBehaviour : MonoBehaviour {
    
    void Awake()
    {
        // If we didnt load the start scene, do nothing
        if (LoadingScreenManager.GetInstance() != null)
        {
            MyAwake();
        }
    }

    void Start()
    {
        // If we didnt load the start scene, do nothing
        if (LoadingScreenManager.GetInstance() != null)
        {
            MyStart();
            GameManager.RegisterObject(this);
        }
    }

    public virtual void MyAwake() { }
    public virtual void MyStart() { }
    public virtual void MyUpdate() { }
}
