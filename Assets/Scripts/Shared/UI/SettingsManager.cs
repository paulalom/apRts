using Assets.Scripts.Shared.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Setting
{
    public KeyCode key;

    // Input qualifiers
    public List<KeyCode> keyModifiers = new List<KeyCode>(); // setting will only fire if all of these are held
    public List<KeyCode> keyExclusions = new List<KeyCode>(); // setting will not fire if any of these are held
    public Func<KeyCode, bool> checkActivationFunction = Input.GetKeyUp;

    // when this key is down, we will queue orders instead of set
    public static KeyCode dontClearExistingOrdersToggle = KeyCode.LeftShift; 

    // The only thing that happens when key is pressed
    public Action action = delegate { };
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
        defaultInputSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift }, action = InputActions.RaiseCamera });
        defaultInputSettings.Add(new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift }, action = InputActions.LowerCamera });

        defaultInputSettings.Add(new Setting() { key = KeyCode.E, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewUseAbilityOrder); }});
        defaultInputSettings.Add(new Setting() { key = KeyCode.X, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCancelOrder); }});
        defaultInputSettings.Add(new Setting() { key = KeyCode.H, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewHarvestOrder); }});
        defaultInputSettings.Add(new Setting() { key = KeyCode.P, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewPatrolOrder); }});
        defaultInputSettings.Add(new Setting() { key = KeyCode.G, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewGuardOrder); }});
        defaultInputSettings.Add(new Setting() { key = KeyCode.F, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewFollowOrder); }});
        defaultInputSettings.Add(new Setting() { key = KeyCode.T, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCheatRaiseTerrainOrder); } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Q, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCheatSpawnFactoryOrder); } });
                
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse0, action = InputActions.OnActionButtonRelease });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse1, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewMoveOrder); } });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyDown, action = InputActions.OnActionButtonPress });
        defaultInputSettings.Add(new Setting() { key = KeyCode.Mouse1, checkActivationFunction = Input.GetKeyDown, action = InputActions.OnMoveButtonPress });

        for (int i = 0; i < 10; i++)
        {
            // We need to create a new local variable j each iteration instead of using i
            // so that our delegates will use that instead of all being set to 10 (the value of i after the loop exits)
            int j = i; 
            
            defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift, KeyCode.LeftAlt, KeyCode.LeftControl }, action = delegate { InputActions.NumericMenuButton(j); } });
            //defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift, KeyCode.LeftControl }, keyModifiers = new List<KeyCode>() { KeyCode.LeftAlt } });
            //defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftAlt, KeyCode.LeftControl }, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
            //defaultInputSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift, KeyCode.LeftAlt }, keyModifiers = new List<KeyCode>() { KeyCode.LeftControl } });
        }

        inputSettings = defaultInputSettings;
    }
}
