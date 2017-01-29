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
            if (GUI.Button(button, "", icon))
            {
                Debug.Log(icon.name);
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
        int itemCount = 0;

        GUIStyle icon;
        Rect button;
        // very ugly state hack for selection from menu (this can be fixed once selection box is fixed)
        gameManager.newSelectedUnit = null;

        foreach (RTSGameObject unit in gameManager.selectedUnits)
        {
            Rect menu = new Rect(50, 250 + i * 55, 400, 50);
            drawMenu(menu);
            j = 1;
            itemCount = 0;
            Storage unitStorage = unit.GetComponent<Storage>();
            foreach (KeyValuePair<RTSGameObjectType, int> item in unitStorage.GetItems())
            {
                icon = new GUIStyle();
                icon.normal.background = RTSGameObject.menuIcon[item.Key];
                icon.normal.textColor = Color.red;
                button = new Rect(menu.x + 10 + 45 * j, menu.y + 5, 40, 40);
                GUI.Button(button, item.Value.ToString(), icon);
                j++;
                itemCount += item.Value;
            }
            icon = new GUIStyle();
            icon.normal.background = RTSGameObject.menuIcon[RTSGameObjectType.None];
            icon.normal.textColor = Color.red;
            button = new Rect(menu.x + 10, menu.y + 5, 40, 40);
            if (GUI.Button(button, itemCount + "/\n" + unitStorage.size, icon))
            {
                gameManager.newSelectedUnit = unit;
            }
            i++;
        }

        //Temporary selection code, a unit's inventory summary button was clicked so we select them
        if (gameManager.newSelectedUnit != null)
        {
            foreach (RTSGameObject unit in gameManager.selectedUnits)
            {
                if (unit != gameManager.newSelectedUnit)
                {
                    unit.selected = false;
                    unit.flagRenderer.material.color = Color.white;
                }
            }
            gameManager.selectedUnits.Clear();
            gameManager.selectedUnits.Add(gameManager.newSelectedUnit);
        }
    }

    void DrawConstructionMenu()
    {
        drawMenu(constructionMenuRect);
        drawMenuButtons(constructionMenuRect, constructionIcons);
    }
}
