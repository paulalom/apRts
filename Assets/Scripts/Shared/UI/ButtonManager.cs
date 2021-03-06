﻿using System;
using UnityEngine;
using System.Collections.Generic;

// This class is a hack on the MenuManager (hack) class because overlapping buttons must be in seperate scripts so you can use GUI.Depth
public class ButtonManager : MyMonoBehaviour {

    public GameManager gameManager;
    public SelectionManager selectionManager;
    public UIManager uiManager;
    public Texture2D progressBarBackTex, progressBarFrontTex;
    public OrderManager orderManager;
    public ICommandManager commandManager;
    
    public override void MyAwake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        orderManager = GameObject.Find("OrderManager").GetComponent<OrderManager>();
    }

    void OnGUI()
    {
        GUI.depth = 10;

        int i = 0;
        int j;
        int itemCount = 0;
        int numInvsToDraw = Math.Min(selectionManager.selectedUnits.Count, 10);

        GUIStyle icon;
        Rect button;
        RTSGameObject newSelectedUnit = null;

        // This might be spawning a button for each selected unit... we should fix that (in addition to everything else in this function)
        foreach (RTSGameObject unit in selectionManager.selectedUnits)
        {
            j = 1;
            Rect menu = new Rect(10, 250 + i * 55 - numInvsToDraw * 10, 400, 50);
            itemCount = 0;
            Storage unitStorage = unit.GetComponent<Storage>();
            if (unitStorage == null)
            {
                continue;
            }
            foreach (KeyValuePair<Type, int> item in unitStorage.GetItems())
            {
                icon = new GUIStyle();
                icon.normal.background = UIManager.icons[item.Key.ToString()];
                icon.normal.textColor = Color.red;
                button = new Rect(menu.x + 10 + 45 * j, menu.y + 5, 40, 40);
                if (GUI.Button(button, item.Value.ToString(), icon))
                {
                    Debug.Log("Item button!");
                    gameManager.itemTransferSource = new MyPair<RTSGameObject, MyPair<Type, int>>(unit, new MyPair<Type, int>(item));
                    selectionManager.menuClicked = true;
                }
                j++;
                itemCount += item.Value;
            }
            icon = new GUIStyle();
            icon.normal.background = UIManager.icons[unit.GetType().ToString()];
            icon.normal.textColor = Color.red;
            button = new Rect(menu.x + 10, menu.y + 5, 40, 40);
            if (GUI.Button(button, itemCount + "/\n" + unitStorage.size, icon))
            {
                newSelectedUnit = unit;
                selectionManager.menuClicked = true;
            }

            Producer producer = unit.GetComponent<Producer>();
            
            if (producer != null && orderManager.orders.ContainsKey(unit))
            {
                List<Order> orders = orderManager.orders[unit];
                if (orders != null && orders.Count > 0 && orders[0] is ProductionOrder)
                {
                    Type currentProduction = orders[0].orderData.items[0].Key;

                    int qtyToProduce = GetSameProductionOrderCount(unit, currentProduction);
                    icon = new GUIStyle();
                    icon.normal.background = UIManager.icons[currentProduction.ToString()];
                    icon.normal.textColor = Color.red;
                    button = new Rect(menu.width - 50, menu.y + 5, 40, 40);

                    GUIStyle progressBarBackStyle = new GUIStyle();
                    progressBarBackStyle.normal.background = progressBarBackTex;
                    Rect progressBarBack = new Rect(menu.width - 48, menu.y + 37, 36, 5);
                    GUIStyle progressBarFrontStyle = new GUIStyle();
                    progressBarFrontStyle.normal.background = progressBarFrontTex;
                    Rect progressBarFront = new Rect(menu.width - 46, menu.y + 38, 34 * (producer.productionTime[currentProduction] - producer.timeLeftToProduce) / producer.productionTime[currentProduction], 3);

                    if (GUI.Button(button, qtyToProduce.ToString(), icon)
                        || GUI.Button(progressBarBack, "", progressBarBackStyle)
                        || GUI.Button(progressBarFront, "", progressBarFrontStyle))
                    {
                        Command command = new Command();
                        command.getOrder = OrderBuilderFunction.NewCancelOrder;
                        commandManager.AddCommand(command, new List<long>() { unit.unitId });
                    }
                }
            }

            i++;
            if (i > numInvsToDraw)
            {
                break;
            }
        }
        
        //Temporary selection code, a unit's inventory summary button was clicked so we select them
        if (newSelectedUnit != null)
        {
            selectionManager.SetSelectionToUnit(newSelectedUnit);
        }
    }

    int GetSameProductionOrderCount(RTSGameObject unit, Type productionType)
    {
        int numProductionOrders = 0;
        foreach (Order order in orderManager.orders[unit])
        {
            if (order.GetType() == typeof(ProductionOrder))
            {
                if (((ProductionOrder)order).orderData.items[0].Key == productionType)
                {
                    numProductionOrders++;
                }
                else
                {
                    break;
                }
            }
        }
        return numProductionOrders;
    }
}
