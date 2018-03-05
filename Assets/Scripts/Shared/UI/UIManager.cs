using System;
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
    public ButtonManager buttonManager;
    public MenuManager menuManager;
    public SelectionManager selectionManager;
    public RTSCamera mainCamera;
    public GameManager gameManager;

    public List<FloatingText> floatingText;
    public bool menuClicked = false;

    public override void MyAwake()
    {
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


    public void HandleInput()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool rayCast = Physics.Raycast(ray, out hit);

        if (rayCast)
        {
            UICheckSelectionEvents(hit);
            CheckInputSettings(GetClickedUnit(hit.collider), hit.point);
            //terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z, playerManager.activeWorld) + 5, hit.point.z);
        }

        selectionManager.resizeSelectionBox();
        mainCamera.CheckCameraUpdate(); // Improve this eventually
    }

    RTSGameObject GetClickedUnit(Collider hitCollider)
    {
        if (hitCollider != null)
        {
            return hitCollider.GetComponentInParent<RTSGameObject>();
        }
        return null;
    }


    // This happens when the Game Loop processes commands for a step, but is queued on press
    public void OnActionButtonPress()
    {

    }

    // This happens when the Game Loop processes commands for a step, but is queued on release
    public void OnActionButtonRelease(List<long> unitIds, Command command)
    {
    }

    private void UICheckSelectionEvents(RaycastHit screenClickLocation)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            selectionManager.mouseDown = Input.mousePosition;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (selectionManager.mouseDown == Input.mousePosition)
            {
                selectionManager.CheckSingleSelectionEvent(screenClickLocation);
            }
            else
            {
                selectionManager.CheckBoxSelectionEvent(mainCamera.GetComponent<Camera>());
            }
            menuClicked = false;
        }
    }

    // This happens when the Game Loop processes commands for a step, but is queued on release
    public void OnMoveButtonRelease(List<long> unitIds, Command command)
    {
        if (command.clearExistingOrders) // if outside of loop is more efficient
        {
            foreach (RTSGameObject unit in gameManager.playerManager.GetUnits(unitIds))
            {
                Order moveOrder = OrderFactory.GetDefaultMoveOrder();
                moveOrder.orderData = command.orderData;
                gameManager.orderManager.SetOrder(unit, moveOrder);
            }
        }
        else
        {
            foreach (RTSGameObject unit in gameManager.playerManager.GetUnits(unitIds))
            {
                Order moveOrder = OrderFactory.GetDefaultMoveOrder();
                moveOrder.orderData = command.orderData;
                gameManager.orderManager.QueueOrder(unit, moveOrder);
            }
        }
    }

    void CheckInputSettings(RTSGameObject clickedUnit, Vector3 screenClickLocation)
    {
        foreach (Setting setting in gameManager.settingsManager.inputSettings)
        {
            if (setting.checkActivationFunction(setting.key) && AreExactModifiersActive(setting) && setting.command != null)
            {
                Command command = setting.command;
                if (Input.GetKey(setting.DontClearExistingOrdersToggle))
                {
                    command.clearExistingOrders = false;
                }
                if (setting.isNumeric)
                {
                    List<MyPair<Type, int>> items = new List<MyPair<Type, int>>();
                    items.Add(new MyPair<Type, int>(UIManager.GetNumericMenuType(setting.key), 1));
                    command.orderData.items = items;
                    command.getOrder = CommandGetOrderFunction.GetDefaultConstructionOrder;
                    command.overrideDefaultOrderData = true;
                }
                else
                {
                    command.orderData.targetPosition = screenClickLocation;
                    command.orderData.target = clickedUnit;
                }
                if (setting.isUIOnly)
                {
                    gameManager.commandManager.AddNonNetworkedCommand(command);
                }
                else
                {
                    gameManager.commandManager.AddCommand(command);
                }
            }
        }
    }

    // Returns true when all key modifiers down and nothing else
    bool AreExactModifiersActive(Setting setting)
    {
        foreach (KeyCode modifier in setting.keyModifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }

        if (setting.useExactModifiers)
        {
            // Ugh. Unity should maintain a list of all keys currently held down but I couldn't find any reference to it.
            // It also didn't end up being better to maintain my own list, because I would have to iterate
            // through the entire list every frame.
            // If you know a better way to do this please let me know.
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key) && !setting.keyModifiers.Contains(key) && setting.key != key)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
