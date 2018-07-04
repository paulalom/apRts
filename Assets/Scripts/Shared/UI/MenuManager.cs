using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MenuManager : MyMonoBehaviour {

    public GameManager gameManager;
    public PlayerManager playerManager;
    public SelectionManager selectionManager;
    public Texture2D menuGraphic;
    float menuWidth = 400, menuHeight = 50;
    Rect constructionMenuRect;
    Dictionary<RTSGameObject, Rect> inventoryMenuRects;
    static Type[] menuTypes;

    // Use this for initialization
    public override void MyAwake()
    {
        selectionManager = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        constructionMenuRect = new Rect(Screen.width / 2 - menuWidth / 2, Screen.height - menuHeight, menuWidth, menuHeight);
        inventoryMenuRects = new Dictionary<RTSGameObject, Rect>();
        
        menuTypes = new Type[6];
        menuTypes[0] = typeof(ConstructionSphere);
        menuTypes[1] = typeof(HarvestingStation);
        menuTypes[2] = typeof(Factory);
        menuTypes[3] = typeof(Tank);
        menuTypes[4] = typeof(Tool);
        menuTypes[5] = typeof(Paper);
    }

    public override void MyStart()
    {

    }
    
    void OnGUI()
    {
        /*
        if (playerManager == null || gameManager == null || selectionManager == null)
        {
            return;
        }*/
        GUI.depth = 100; // Smaller is closer. Buttons need < menus
        DrawConstructionMenu();
        drawInventoryMenus();
    }

    void DrawConstructionMenu()
    {
        drawMenu(constructionMenuRect);
        drawMenuButtons(constructionMenuRect);
    }

    void drawMenu(Rect menu)
    {
        GUIStyle container = new GUIStyle();
        container.normal.background = menuGraphic;
        GUI.Box(menu, "", container);
    }

    public Type[] GetNumericMenuTypes()
    {
        return menuTypes;
    }

    void drawMenuButtons(Rect menu)
    {
        //Make this better later
        for (int x = 0; x < menuTypes.Length; x++)
        {
            GUIStyle icon = new GUIStyle();
            icon.normal.background = UIManager.icons[menuTypes[x]];
            Rect button = new Rect(menu.x + 10 + 5 * x + 40 * x, menu.y + 5, 40, 40);
            if (GUI.Button(button, (x+1).ToString(), icon))
            {
                gameManager.ProduceFromMenu(menuTypes[x], 1);
                selectionManager.menuClicked = true;
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
                        command.getOrder = OrderBuilderFunction.NewGiveOrder;
                        command.overrideDefaultOrderData = true;
                        gameManager.commandManager.AddCommand(command, new List<long>() { sourceUnit.uid });
                    }
                    // Destination is a transporter
                    else if (unit.GetComponent<Transporter>() != null && unit.GetComponent<Mover>() != null)
                    {
                        List<MyPair<Type, int>> items = new List<MyPair<Type, int>>() { gameManager.itemTransferSource.Value };
                        Order order = OrderFactory.BuildTakeOrder(sourceUnit, 3f, items);
                        Command command = new Command() { orderData = order.orderData };
                        command.getOrder = OrderBuilderFunction.NewTakeOrder;
                        command.overrideDefaultOrderData = true;
                        gameManager.commandManager.AddCommand(command, new List<long>() { unit.uid });
                    }
                    else
                    {
                        // Nothing, we have a source but the destination is not valid, or the source was unintended
                    }
                }
                selectionManager.menuClicked = true;
            }
            i++;

            if (i > numInvsToDraw)
            {
                break;
            }
        }
    }
}
