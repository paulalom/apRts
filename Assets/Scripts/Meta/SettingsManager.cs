using System;
using System.Collections.Generic;
using UnityEngine;

public class Setting
{
    //public Action action;
    public KeyCode key;
    public bool smartCast = false, isNumeric = false;
    public string activationType = "KeyUp";
    public List<KeyCode> keyModifiers = new List<KeyCode>();
    public Order order;
    public Func<KeyCode, bool> checkActivationFunction = Input.GetKeyDown;
    public Action action = delegate { };
    public Action<RaycastHit> raycastHitAction = delegate { };
}

public class SettingsManager : MonoBehaviour {

    public List<Setting> defaultKeyboardSettings;
    public List<Setting> keyboardSettings;

    void Awake()
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        RTSCamera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        defaultKeyboardSettings = new List<Setting>();
        /* leaving these in RTSCamera for now
        defaultKeyboardSettings.Add("camX+", new Setting() { actionName = "camX+", key = KeyCode.D });
        defaultKeyboardSettings.Add("camX-", new Setting() { actionName = "camX-", key = KeyCode.A });
        defaultKeyboardSettings.Add("camZ+", new Setting() { actionName = "camZ+", key = KeyCode.W });
        defaultKeyboardSettings.Add("camZ-", new Setting() { actionName = "camZ-", key = KeyCode.S });
        */
        
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.E, order = OrderFactory.GetDefaultUseAbilityOrder(), smartCast = true });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.C, order = OrderFactory.GetDefaultCancelOrder() });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.H, order = OrderFactory.GetDefaultHarvestOrder() });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.P, order = OrderFactory.GetDefaultPatrolOrder() });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.G, order = OrderFactory.GetDefaultGuardOrder() });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.F, order = OrderFactory.GetDefaultFollowOrder() });

        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, action = camera.RaiseCamera });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, action = camera.LowerCamera, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.T, checkActivationFunction = Input.GetKey, raycastHitAction = gameManager.RaiseTerrain });
        defaultKeyboardSettings.Add(new Setting() { key = KeyCode.Q, checkActivationFunction = Input.GetKey, raycastHitAction = gameManager.SpawnFactory });

        for (int i = 0; i < 10; i++)
        {
            defaultKeyboardSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true });
            defaultKeyboardSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftAlt } });
            defaultKeyboardSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
            defaultKeyboardSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftControl } });
        }

        keyboardSettings = defaultKeyboardSettings;
    }
}
