using System;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

    public Type[] typesWithMenuIcons = new Type[] { typeof(Iron), typeof(Wood), typeof(Coal), typeof(Stone), typeof(Paper), typeof(Tool), typeof(Car), typeof(Worker), typeof(Tank), typeof(Factory), typeof(HarvestingStation), typeof(ResourceDeposit) };
    public static Dictionary<Type, Texture2D> menuIcon = new Dictionary<Type, Texture2D>();

    public List<FloatingText> floatingText;
    //public List<StatusGraphic> unitStatus;
    public Vector3 mouseDown;
    public bool menuClicked = false;

    void Awake()
    {
        menuIcon = new Dictionary<Type, Texture2D>();
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
    }
}
