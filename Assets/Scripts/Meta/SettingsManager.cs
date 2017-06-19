using System;
using System.Collections.Generic;
using UnityEngine;

public class Setting
{
    //public Action action;
    public KeyCode key;
    public bool smartCast = false, isNumeric = false, useExactModifiers = false;
    public string activationType = "KeyUp";
    public List<KeyCode> keyModifiers = new List<KeyCode>();
    public Func<KeyCode, bool> checkActivationFunction = Input.GetKeyUp;
    public Func<Order> getOrder = delegate { return null; };
    public Action action = delegate { };
    public Action<RaycastHit> raycastHitAction = delegate { };
}

public class SettingsManager : MonoBehaviour {

    public List<Setting> defaultInputSettings;
    public List<Setting> inputSettings;

    void Awake()
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        RTSCamera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        defaultInputSettings = new List<Setting>();
        /* leaving these in RTSCamera for now
        defaultKeyboardSettings.Add("camX+", new Setting() { actionName = "camX+", key = KeyCode.D });
        defaultKeyboardSettings.Add("camX-", new Setting() { actionName = "camX-", key = KeyCode.A });
        defaultKeyboardSettings.Add("camZ+", new Setting() { actionName = "camZ+", key = KeyCode.W });
        defaultKeyboardSettings.Add("camZ-", new Setting() { actionName = "camZ-", key = KeyCode.S });
        */
        
        defaultInputSettings.Add(new Setting() { key = KeyCode.E, getOrder = OrderFactory.GetDefaultUseAbilityOrder, smartCast = true });
        defaultInputSettings.Add(new Setting() { key = KeyCode.C, getOrder = OrderFactory.GetDefaultCancelOrder });
        defaultInputSettings.Add(new Setting() { key = KeyCode.H, getOrder = OrderFactory.GetDefaultHarvestOrder });
        defaultInputSettings.Add(new Setting() { key = KeyCode.P, getOrder = OrderFactory.GetDefaultPatrolOrder });
        defaultInputSettings.Add(new Setting() { key = KeyCode.G, getOrder = OrderFactory.GetDefaultGuardOrder });
        defaultInputSettings.Add(new Setting() { key = KeyCode.F, getOrder = OrderFactory.GetDefaultFollowOrder });

        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyDown, action = gameManager.OnActionButtonPress });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyUp, raycastHitAction = gameManager.OnActionButtonRelease });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse1, checkActivationFunction = Input.GetKeyUp, raycastHitAction = gameManager.OnMoveButtonRelease });

        defaultInputSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, action = camera.RaiseCamera, useExactModifiers = true });
        defaultInputSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, action = camera.LowerCamera, useExactModifiers = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.T, checkActivationFunction = Input.GetKey, raycastHitAction = gameManager.RaiseTerrain });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Q, checkActivationFunction = Input.GetKey, raycastHitAction = gameManager.SpawnFactory });

        for (int i = 0; i < 10; i++)
        {
            defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, useExactModifiers = true });
            defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, useExactModifiers = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftAlt } });
            defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, useExactModifiers = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
            defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, isNumeric = true, useExactModifiers = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftControl } });
        }

        inputSettings = defaultInputSettings;
    }
}
