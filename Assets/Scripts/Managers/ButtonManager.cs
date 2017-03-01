using System;
using UnityEngine;
using System.Collections.Generic;


// This class is a hack on the MenuManager (hack) class because overlapping buttons must be in seperate scripts so you can use GUI.Depth
public class ButtonManager : MonoBehaviour {

    GameManager gameManager;
    
	void Start ()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
	
    void OnGUI()
    {
        GUI.depth = 10;

        int i = 0;
        int j;
        int itemCount = 0;

        GUIStyle icon;
        Rect button;
        RTSGameObject newSelectedUnit = null;

        foreach (RTSGameObject unit in gameManager.selectedUnits)
        {
            j = 1;
            Rect menu = new Rect(50, 250 + i * 55, 400, 50);
            itemCount = 0;
            Storage unitStorage = unit.GetComponent<Storage>();
            foreach (KeyValuePair<Type, int> item in unitStorage.GetItems())
            {
                icon = new GUIStyle();
                icon.normal.background = UIManager.menuIcon[item.Key];
                icon.normal.textColor = Color.red;
                button = new Rect(menu.x + 10 + 45 * j, menu.y + 5, 40, 40);
                if (GUI.Button(button, item.Value.ToString(), icon))
                {
                    Debug.Log("Item button!");
                    gameManager.itemTransferSource = new MyKVP<RTSGameObject, MyKVP<Type, int>>(unit, new MyKVP<Type, int>(item));
                    gameManager.menuClicked = true;
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
                gameManager.menuClicked = true;
            }
            i++;
        }
        
        //Temporary selection code, a unit's inventory summary button was clicked so we select them
        if (newSelectedUnit != null)
        {
            foreach (RTSGameObject unit in gameManager.selectedUnits)
            {
                if (unit != newSelectedUnit)
                {
                    unit.selected = false;
                    unit.flagRenderer.material.color = Color.white;
                }
            }
            gameManager.selectedUnits.Clear();
            gameManager.selectedUnits.Add(newSelectedUnit);
        }
    }
}
