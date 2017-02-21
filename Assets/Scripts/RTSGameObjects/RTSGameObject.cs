using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class RTSGameObject : MonoBehaviour
{
    public static Dictionary<Type, Texture2D> menuIcon = new Dictionary<Type, Texture2D>();

    public bool selected = false;
    public Renderer flagRenderer; // the part of the object which contains the flag
    public GameManager gameManager;
    public GameObject graphicObject; // should this be a thing?
    public Storage storage; // SHOULD ONLY BE ACCESSED THROUGH OBJECTMANAGER.GetStorage?

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        storage = GetComponent<Storage>();
        flagRenderer = GetComponent<Renderer>(); // just get any part of the object
    }

    void Update()
    {
    }
}