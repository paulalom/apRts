using System;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

    public Type[] typesWithMenuIcons = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car), typeof(Commander), typeof(Worker), typeof(Tank), typeof(HarvestingStation), typeof(Factory), typeof(ResourceDeposit) };
    public static Dictionary<Type, Texture2D> menuIcon = new Dictionary<Type, Texture2D>();

    static Dictionary<string, Type> numericMenuTypes;

    public List<FloatingText> floatingText;
    //public List<StatusGraphic> unitStatus;
    public Vector3 mouseDown;
    public bool menuClicked = false;

    void Awake()
    {
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
}
