using System;
using System.Collections.Generic;
using UnityEngine;

public class Setting
{
    //public Action action;
    public KeyCode key;
    public bool isNumeric = false, useExactModifiers = false, isUIOnly = false;
    public string activationType = "KeyUp";
    public KeyCode DontClearExistingOrdersToggle = KeyCode.LeftShift; // when this key is down, we will queue orders instead of set
    public List<KeyCode> keyModifiers = new List<KeyCode>();
    public Func<KeyCode, bool> checkActivationFunction = Input.GetKeyUp;
    public Command command;
}


public class SettingsManager : MyMonoBehaviour
{

    public List<Setting> defaultInputSettings;
    public List<Setting> inputSettings;

    public override void MyAwake()
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
        
        defaultInputSettings.Add(new Setting() { key = KeyCode.E, command = new Command() { getOrder = CommandGetOrderFunction.GetDefaultUseAbilityOrder, smartCast = true } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.X, command = new Command() { getOrder = CommandGetOrderFunction.GetDefaultCancelOrder } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.H, command = new Command() { getOrder = CommandGetOrderFunction.GetDefaultHarvestOrder } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.P, command = new Command() { getOrder = CommandGetOrderFunction.GetDefaultPatrolOrder } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.G, command = new Command() { getOrder = CommandGetOrderFunction.GetDefaultGuardOrder } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.F, command = new Command() { getOrder = CommandGetOrderFunction.GetDefaultFollowOrder } });

        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyDown, command = new Command() { action = CommandAction.OnActionButtonPress } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyUp, command = new Command() { raycastHitAction = CommandRaycastAction.OnActionButtonRelease } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse1, checkActivationFunction = Input.GetKeyUp, command = new Command() { raycastHitAction = CommandRaycastAction.OnMoveButtonRelease } });

        defaultInputSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, useExactModifiers = true, isUIOnly = true, command = new Command() { action = CommandAction.RaiseCamera } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, useExactModifiers = true, isUIOnly = true, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift }, command = new Command() { action = CommandAction.LowerCamera } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.T, /*checkActivationFunction = Input.GetKey,*/ command = new Command() { raycastHitAction = CommandRaycastAction.RaiseTerrain } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Q, /*checkActivationFunction = Input.GetKey,*/ command = new Command() { raycastHitAction = CommandRaycastAction.SpawnFactory } });

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
