using System;
using UnityEngine;
using System.Collections.Generic;

// This class is a hack on the MenuManager (hack) class because overlapping buttons must be in seperate scripts so you can use GUI.Depth
public class ButtonManager : MyMonoBehaviour {

    GameManager gameManager;
    PlayerManager playerManager;
    SelectionManager selectionManager;
    UIManager uiManager;
    public Texture2D progressBarBackTex, progressBarFrontTex;

    void Start ()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        selectionManager = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }
	
    void OnGUI()
    {
        GUI.depth = 10;

        int i = 0;
        int j;
        int itemCount = 0;
        int numInvsToDraw = Math.Min(playerManager.PlayerSelectedUnits.Count, 10);

        GUIStyle icon;
        Rect button;
        RTSGameObject newSelectedUnit = null;

        foreach (RTSGameObject unit in playerManager.PlayerSelectedUnits)
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
                icon.normal.background = UIManager.menuIcon[item.Key];
                icon.normal.textColor = Color.red;
                button = new Rect(menu.x + 10 + 45 * j, menu.y + 5, 40, 40);
                if (GUI.Button(button, item.Value.ToString(), icon))
                {
                    Debug.Log("Item button!");
                    gameManager.itemTransferSource = new MyPair<RTSGameObject, MyPair<Type, int>>(unit, new MyPair<Type, int>(item));
                    uiManager.menuClicked = true;
                }
                j++;
                itemCount += item.Value;
            }
            icon = new GUIStyle();
            icon.normal.background = UIManager.menuIcon[unit.GetType()];
            icon.normal.textColor = Color.red;
            button = new Rect(menu.x + 10, menu.y + 5, 40, 40);
            if (GUI.Button(button, itemCount + "/\n" + unitStorage.size, icon))
            {
                newSelectedUnit = unit;
                uiManager.menuClicked = true;
            }

            Producer producer = unit.GetComponent<Producer>();
            if (producer != null && producer.productionQueue.Count >= 1)
            {
                MyPair<Type, int> nextInQueue = producer.productionQueue[0];
                icon = new GUIStyle();
                icon.normal.background = UIManager.menuIcon[nextInQueue.Key];
                icon.normal.textColor = Color.red;
                button = new Rect(menu.width - 50, menu.y + 5, 40, 40);
                
                GUIStyle progressBarBackStyle = new GUIStyle();
                progressBarBackStyle.normal.background = progressBarBackTex;
                Rect progressBarBack = new Rect(menu.width - 48, menu.y + 37, 36, 5);
                GUIStyle progressBarFrontStyle = new GUIStyle();
                progressBarFrontStyle.normal.background = progressBarFrontTex;
                Rect progressBarFront = new Rect(menu.width - 46, menu.y + 38, 34 *(producer.productionTime[nextInQueue.Key] - producer.timeLeftToProduce)/producer.productionTime[nextInQueue.Key], 3);

                if (GUI.Button(button, nextInQueue.Value.ToString(), icon) 
                    || GUI.Button(progressBarBack, "", progressBarBackStyle) 
                    || GUI.Button(progressBarFront, "", progressBarFrontStyle))
                {
                    producer.CancelProduction();
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
}
