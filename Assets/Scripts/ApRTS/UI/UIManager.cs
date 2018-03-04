﻿using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MyMonoBehaviour {

    public Type[] typesWithMenuIcons = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car), typeof(Commander), typeof(ConstructionSphere), typeof(Tank), typeof(HarvestingStation), typeof(Factory), typeof(ResourceDeposit) };
    public static Dictionary<Type, Texture2D> menuIcon = new Dictionary<Type, Texture2D>();
    static Dictionary<string, Type> numericMenuTypes;
    GameObject floatingTextPrefab;
    public LoadingScreen loadingScreen;
    GameManager gameManager;
    ButtonManager buttonManager;
    MenuManager menuManager;
    

    public List<FloatingText> floatingText;
    public bool menuClicked = false;

    public override void MyAwake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        menuIcon = new Dictionary<Type, Texture2D>();
        numericMenuTypes = new Dictionary<string, Type>();
        floatingText = new List<FloatingText>();

        // Get menu icons
        foreach (Type type in typesWithMenuIcons)
        {
            menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/" + type.ToString() + "Icon");
            if (menuIcon[type] == null)
            {
                menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/None");
            }
        }

        numericMenuTypes[KeyCode.Alpha1.ToString()] = typeof(ConstructionSphere);
        numericMenuTypes[KeyCode.Alpha2.ToString()] = typeof(HarvestingStation);
        numericMenuTypes[KeyCode.Alpha3.ToString()] = typeof(Factory);
        numericMenuTypes[KeyCode.Alpha4.ToString()] = typeof(Tank);
        numericMenuTypes[KeyCode.Alpha5.ToString()] = typeof(Tool);
        numericMenuTypes[KeyCode.Alpha6.ToString()] = typeof(Paper);
    }

    public override void MyStart()
    {
        RTSGameObjectManager rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        floatingTextPrefab = rtsGameObjectManager.prefabs["FloatingText"];
    }

    // ui cant init before objects are all set up because bad code and tight coupling... fix is todo
    public IEnumerator InitUI(GameManager gm, PlayerManager pm, SelectionManager sm)
    {
        buttonManager = gameManager.managerManager.SpawnManager(typeof(ButtonManager)).GetComponent<ButtonManager>();
        menuManager = gameManager.managerManager.SpawnManager(typeof(MenuManager)).GetComponent<MenuManager>();
        buttonManager.InjectDependencies(gm, pm, sm, this);
        menuManager.InjectDependencies(gm, pm, this);
        gameManager.menuManager = menuManager;
        gameManager.buttonManager = buttonManager;
        return null;
    }

    public static Type GetNumericMenuType(KeyCode key)
    {
        return numericMenuTypes[key.ToString()];
    }

    public static Type GetNumericMenuType(string key)
    {
        return numericMenuTypes[key];
    }

    public void RemoveText(FloatingText text)
    {
        floatingText.Remove(text);
    }

    public void CreateText(string text, Vector3 position, Color color, float scale = 1)
    {
        CreateText(text, position, scale);
        floatingText[floatingText.Count - 1].SetColor(color);
    }
    public void CreateText(string text, Vector3 position, float scale = 1)
    {
        position.y += 5; // floating text starts above the object
        GameObject go = Instantiate(floatingTextPrefab,
            position,
            Quaternion.identity) as GameObject;
        go.name = "FloatingText" + floatingText.Count;
        go.transform.localScale = new Vector3(scale, scale, scale);

        FloatingText ft = go.GetComponent<FloatingText>();
        ft.textMesh.text = text;
        ft.transform.position = position;
        floatingText.Add(ft);
    }
}