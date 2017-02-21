﻿using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour {

    public List<Texture2D> constructionIcons;
    [HideInInspector]
    public List<Texture2D> inventoryIcons;
    public GameManager gameManager;
    public Texture2D menuGraphic;
    float menuWidth = 400, menuHeight = 50;
    Rect constructionMenuRect;
    Dictionary<RTSGameObject, Rect> inventoryMenuRects;


    // Use this for initialization
    void Start () {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        //something is wonky with the height, magic *3 ftw
        constructionMenuRect = new Rect(Screen.width / 2 - menuWidth / 2, Screen.height - menuHeight, menuWidth, menuHeight);
        inventoryMenuRects = new Dictionary<RTSGameObject, Rect>();
        gameManager.onSelectionChange.AddListener(UpdateInventoryMenuDisplay);
    }
	
	// Update is called once per frame
	void Update () {

    }


    void OnGUI()
    {
        GUI.depth = 100; // Smaller is closer. Buttons need < menus
        DrawConstructionMenu();
        drawInventoryMenus();
    }

    void drawMenu(Rect menu)
    {
        GUIStyle container = new GUIStyle();
        container.normal.background = menuGraphic;
        GUI.Box(menu, "", container);
    }


    void drawMenuButtons(Rect menu, List<Texture2D> iconList)
    {
        //Make this better later
        for (int x = 0; x < iconList.Count; x++)
        {
            GUIStyle icon = new GUIStyle();
            icon.normal.background = iconList[x];
            Rect button = new Rect(menu.x + 10 + 5 * x + 40 * x, menu.y + 5, 40, 40);
            if (GUI.Button(button, x.ToString(), icon))
            {
                if (x == 0)
                {
                    gameManager.QueueUnit(typeof(Worker));
                }
                else if (x == 1)
                {
                    gameManager.QueueUnit(typeof(HarvestingStation));
                }
                else if (x == 2)
                {
                    gameManager.QueueUnit(typeof(Factory));
                }
            }
        }
    }

    void UpdateInventoryMenuDisplay()
    {

    }

    void drawInventoryMenus()
    {
        int i = 0;
        int j;
        
        foreach (RTSGameObject unit in gameManager.selectedUnits)
        {
            GUIStyle container = new GUIStyle();
            container.normal.background = menuGraphic;
            Rect menu = new Rect(50, 250 + i * 55, 400, 50);
            if (GUI.Button(menu, "", container))
            {
                Debug.Log("Menu button!");
                if (gameManager.itemTransferSource != null)
                {
                    RTSGameObject sourceUnit = gameManager.itemTransferSource.Key;
                    // Transportation orders always go to the transporter
                    // Source is a transporter
                    if (sourceUnit.GetComponent<Transporter>() != null)
                    {
                        gameManager.orderManager.SetOrder(sourceUnit, new Order()
                        {
                            type = OrderType.Give,
                            target = unit,
                            item = gameManager.itemTransferSource.Value
                        });
                    }
                    // Destination is a transporter
                    else if (unit.GetComponent<Transporter>() != null)
                    {
                        gameManager.orderManager.SetOrder(unit, new Order()
                        {
                            type = OrderType.Take,
                            target = sourceUnit,
                            item = gameManager.itemTransferSource.Value
                        });
                    }
                    else
                    {
                        // Nothing, we have a source but the destination is not valid, or the source was unintended
                    }
                }
                gameManager.menuClicked = true;
            }
            i++;
        }
    }

    void DrawConstructionMenu()
    {
        drawMenu(constructionMenuRect);
        drawMenuButtons(constructionMenuRect, constructionIcons);
    }
}