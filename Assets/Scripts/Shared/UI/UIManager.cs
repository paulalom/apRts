using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Shared.UI;

public class UIManager : MyMonoBehaviour {

    public Type[] typesWithIcons = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car), typeof(Commander), typeof(ConstructionSphere), typeof(Tank), typeof(HarvestingStation), typeof(Factory), typeof(ResourceDeposit) };
    public static Dictionary<Type, Texture2D> icons = new Dictionary<Type, Texture2D>();
    
    GameObject floatingTextPrefab;
    public LoadingScreen loadingScreen;
    public ButtonManager buttonManager;
    public MenuManager menuManager;
    public SelectionManager selectionManager;
    public RTSCamera mainCamera;
    public GameManager gameManager;
    public UIBarManager uiBarManager;

    public List<FloatingText> floatingText;

    public override void MyAwake()
    {
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        buttonManager = GameObject.Find("ButtonManager").GetComponent<ButtonManager>();
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiBarManager = GameObject.Find("UIBarManager").GetComponent<UIBarManager>();
        InputActions.gameManager = gameManager;
        InputActions.menuManager = menuManager;
        InputActions.selectionManager = selectionManager;
        InputActions.uiManager = this;

        floatingText = new List<FloatingText>();
        icons = new Dictionary<Type, Texture2D>();

        // Get menu icons
        foreach (Type type in typesWithIcons)
        {
            icons[type] = Resources.Load<Texture2D>("MyAssets/Icons/" + type.ToString() + "Icon");
            if (icons[type] == null)
            {
                icons[type] = Resources.Load<Texture2D>("MyAssets/Icons/None");
            }
        }
    }

    public override void MyStart()
    {
        base.MyStart();
        RTSGameObjectManager rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        floatingTextPrefab = rtsGameObjectManager.prefabs["FloatingText"];
    }

    // ui cant init before objects are all set up because bad code and tight coupling... fix is todo
    public IEnumerator InitUI(GameManager gm, PlayerManager pm, SelectionManager sm)
    {
        return null;
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
        go.name = "floatingText" + floatingText.Count;
        go.transform.localScale = new Vector3(scale, scale, scale);

        FloatingText ft = go.GetComponent<FloatingText>();
        ft.textMesh.text = text;
        ft.transform.position = position;
        floatingText.Add(ft);
    }


    public void HandleInput()
    {
        foreach (Setting setting in gameManager.settingsManager.inputSettings)
        {
            if (setting.checkActivationFunction(setting.key) && AreKeyModifiersActive(setting))
            {
                setting.action.Invoke();
            }
        }

        //terrainManager.projector.position = new Vector3(hit.point.x, terrainManager.GetHeightFromGlobalCoords(hit.point.x, hit.point.z, playerManager.activeWorld) + 5, hit.point.z);

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
        
    // Returns true when all key modifiers down and nothing else
    bool AreKeyModifiersActive(Setting setting)
    {
        foreach (KeyCode modifier in setting.keyModifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }
        
        foreach (KeyCode key in setting.keyExclusions)
        {
            if (Input.GetKey(key))
            {
                return false;
            }
        }
        return true;
    }


    public void IncrementSelectionSubgroup()
    {
        selectionManager.IncrementSelectionSubgroup();
        uiBarManager.UpdateSubselectionCategory();
    }
    public void DecrementSelectionSubgroup()
    {
        selectionManager.DecrementSelectionSubgroup();
        uiBarManager.UpdateSubselectionCategory();
    }
    public void SetSelectionSubgroup(int groupId)
    {
        selectionManager.SetSelectionSubgroup(groupId);
        uiBarManager.UpdateSubselectionCategory();
    }
}
