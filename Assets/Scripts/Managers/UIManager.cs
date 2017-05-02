using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Type[] typesWithMenuIcons = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car), typeof(Commander), typeof(Worker), typeof(Tank), typeof(HarvestingStation), typeof(Factory), typeof(ResourceDeposit) };
    public static Dictionary<Type, Texture2D> menuIcon = new Dictionary<Type, Texture2D>();
    static Dictionary<string, Type> numericMenuTypes;
    GameObject floatingTextPrefab;
    public LoadingScreen loadingScreen;
    GameManager gameManager;

    public List<FloatingText> floatingText;
    //public List<StatusGraphic> unitStatus;
    public Vector3 mouseDown;
    public bool menuClicked = false;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        menuIcon = new Dictionary<Type, Texture2D>();
        numericMenuTypes = new Dictionary<string, Type>();
        floatingText = new List<FloatingText>();
        mouseDown = GameManager.vectorSentinel;
        
        // Get menu icons
        foreach (Type type in typesWithMenuIcons)
        {
            Debug.Log(type.ToString());
            menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/" + type.ToString() + "Icon");
            if (menuIcon[type] == null)
            {
                menuIcon[type] = Resources.Load<Texture2D>("MyAssets/Icons/None");
            }
        }

        numericMenuTypes["numeric_1"] = typeof(Worker);
        numericMenuTypes["numeric_2"] = typeof(HarvestingStation);
        numericMenuTypes["numeric_3"] = typeof(Factory);
        numericMenuTypes["numeric_4"] = typeof(Tank);
        numericMenuTypes["numeric_5"] = typeof(Tool);
        numericMenuTypes["numeric_6"] = typeof(Paper);
    }

    void Start()
    {
        RTSGameObjectManager rtsGameObjectManager = GameObject.FindGameObjectWithTag("RTSGameObjectManager").GetComponent<RTSGameObjectManager>();
        floatingTextPrefab = rtsGameObjectManager.prefabs["FloatingText"];
    }

    public static Type GetNumericMenuType(string key)
    {

        if (numericMenuTypes.ContainsKey(key))
        {
            return numericMenuTypes[key];
        }
        else
        {
            return typeof(RTSGameObject);
        }
    }

    public void CreateText(string text, Vector3 position, Color color, float scale = 1)
    {
        CreateText(text, position, scale);
        floatingText[floatingText.Count - 1].SetColor(color);
    }
    public void CreateText(string text, Vector3 position, float scale = 1)
    {
        Debug.Log("New floatingText: " + text);
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
}
