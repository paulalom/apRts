using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour {

    public List<Texture2D> constructionIcons;
    [HideInInspector]
    public List<Texture2D> inventoryIcons;
    GameManager gameManager;
    PlayerManager playerManager;
    UIManager uiManager;
    public Texture2D menuGraphic;
    float menuWidth = 400, menuHeight = 50;
    Rect constructionMenuRect;
    Dictionary<RTSGameObject, Rect> inventoryMenuRects;


    // Use this for initialization
    void Start () {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        //something is wonky with the height, magic *3 ftw
        constructionMenuRect = new Rect(Screen.width / 2 - menuWidth / 2, Screen.height - menuHeight, menuWidth, menuHeight);
        inventoryMenuRects = new Dictionary<RTSGameObject, Rect>();
        playerManager.OnPlayerSelectionChange.AddListener(UpdateInventoryMenuDisplay);
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

    void DrawConstructionMenu()
    {
        drawMenu(constructionMenuRect);
        drawMenuButtons(constructionMenuRect, constructionIcons);
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
            if (GUI.Button(button, (x+1).ToString(), icon))
            {
                gameManager.QueueUnit(UIManager.GetNumericMenuType("numeric_" + (x + 1)));
                uiManager.menuClicked = true;
            }
        }
    }

    void UpdateInventoryMenuDisplay()
    {

    }

    void drawInventoryMenus()
    {
        int i = 0;
        int numInvsToDraw = Math.Min(playerManager.PlayerSelectedUnits.Count, 10);
        
        foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
        {
            GUIStyle container = new GUIStyle();
            container.normal.background = menuGraphic;
            Rect menu = new Rect(10, 250 + i * 55 - numInvsToDraw * 10, 400, 50);
            if (GUI.Button(menu, "", container))
            {
                Debug.Log("Menu button!");
                if (gameManager.itemTransferSource != null)
                {
                    RTSGameObject sourceUnit = gameManager.itemTransferSource.Key;
                    // May need to rework the transporter/harvester relationship
                    // Transportation orders always go to the transporter
                    // Source is a transporter
                    if (sourceUnit.GetComponent<Transporter>() != null && sourceUnit.GetComponent<Mover>() != null)
                    {
                        gameManager.orderManager.SetOrder(sourceUnit, new Order()
                        {
                            type = OrderType.Give,
                            target = unit,
                            orderRange = 3f,
                            items = new List<MyKVP<Type, int>>() { gameManager.itemTransferSource.Value }
                        });
                    }
                    // Destination is a transporter
                    else if (unit.GetComponent<Transporter>() != null && unit.GetComponent<Mover>() != null)
                    {
                        gameManager.orderManager.SetOrder(unit, new Order()
                        {
                            type = OrderType.Take,
                            target = sourceUnit,
                            orderRange = 3f,
                            items = new List<MyKVP<Type, int>>() { gameManager.itemTransferSource.Value }
                        });
                    }
                    else
                    {
                        // Nothing, we have a source but the destination is not valid, or the source was unintended
                    }
                }
                uiManager.menuClicked = true;
            }
            i++;

            if (i > numInvsToDraw)
            {
                break;
            }
        }
    }
}
