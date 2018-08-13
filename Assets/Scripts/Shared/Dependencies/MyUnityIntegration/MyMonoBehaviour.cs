using UnityEngine;
using System.Collections;

public abstract class MyMonoBehaviour : MonoBehaviour {
    
    void Start()
    {
        MyStart();
        GameManager.RegisterObject(this);
    }

    void Awake()
    {
        MyAwake();
    }

    public virtual void MyAwake() { }
    public virtual void MyStart() { }
    public virtual void MyUpdate() { }
}
