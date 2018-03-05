using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MenuManager : MyMonoBehaviour {

    public List<Texture2D> constructionIcons;
    [HideInInspector]
    public List<Texture2D> inventoryIcons;
    public GameManager gameManager;
    public PlayerManager playerManager;
    public UIManager uiManager;
    public Texture2D menuGraphic;
    float menuWidth = 400, menuHeight = 50;
    Rect constructionMenuRect;
    Dictionary<RTSGameObject, Rect> inventoryMenuRects;


    // Use this for initialization
    public override void MyAwake() {
        
        constructionMenuRect = new Rect(Screen.width / 2 - menuWidth / 2, Screen.height - menuHeight, menuWidth, menuHeight);
        inventoryMenuRects = new Dictionary<RTSGameObject, Rect>();
    }
    
    void OnGUI()
    {
        if (playerManager == null || gameManager == null || uiManager == null)
        {
            return;
        }
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
                gameManager.ProduceFromMenu(UIManager.GetNumericMenuType("Alpha" + (x + 1)), 1);
                uiManager.menuClicked = true;
            }
        }
    }

    void drawInventoryMenus()
    {
        int i = 0;
        int numInvsToDraw = Math.Min(playerManager.PlayerSelectedUnits.Count, 10);
        
        foreach (RTSGameObject unit in playerManager.GetPlayerSelectedUnits())
        {
            GUIStyle container = new GUIStyle();
            container.normal.background = menuGraphic;
            Rect menu = new Rect(10, 250 + i * 55 - numInvsToDraw * 10, 400, 50);
            if (GUI.Button(menu, "", container))
            {
                Debug.Log("Menu button!");
                if (gameManager.itemTransferSource != null && 
                    gameManager.itemTransferSource.Key.ownerId == playerManager.ActivePlayerId &&
                    unit.ownerId == playerManager.ActivePlayerId)
                {
                    RTSGameObject sourceUnit = gameManager.itemTransferSource.Key;
                    // May need to rework the transporter/harvester relationship
                    // Transportation orders always go to the transporter
                    // Source is a transporter
                    if (sourceUnit.GetComponent<Transporter>() != null && sourceUnit.GetComponent<Mover>() != null)
                    {
                        List<MyPair<Type, int>> items = new List<MyPair<Type, int>>() { gameManager.itemTransferSource.Value };
                        Order order = OrderFactory.BuildGiveOrder(unit, 3f, items);
                        Command command = new Command() { orderData = order.orderData };
                        command.getOrder = CommandGetOrderFunction.GetDefaultGiveOrder;
                        command.overrideDefaultOrderData = true;
                        gameManager.commandManager.AddCommand(command, new List<long>() { sourceUnit.uid });
                    }
                    // Destination is a transporter
                    else if (unit.GetComponent<Transporter>() != null && unit.GetComponent<Mover>() != null)
                    {
                        List<MyPair<Type, int>> items = new List<MyPair<Type, int>>() { gameManager.itemTransferSource.Value };
                        Order order = OrderFactory.BuildTakeOrder(sourceUnit, 3f, items);
                        Command command = new Command() { orderData = order.orderData };
                        command.getOrder = CommandGetOrderFunction.GetDefaultTakeOrder;
                        command.overrideDefaultOrderData = true;
                        gameManager.commandManager.AddCommand(command, new List<long>() { unit.uid });
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
