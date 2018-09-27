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
    public static KeyCode queueOrderInsteadOfClearing = KeyCode.LeftShift;
    public static KeyCode addOrderToFrontOfQueue = KeyCode.LeftAlt;

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
        defaultInputSettings = new List<Setting>
        {
            /* leaving these in RTSCamera for now
            defaultKeyboardSettings.Add("camX+", new Setting() { actionName = "camX+", key = KeyCode.D });
            defaultKeyboardSettings.Add("camX-", new Setting() { actionName = "camX-", key = KeyCode.A });
            defaultKeyboardSettings.Add("camZ+", new Setting() { actionName = "camZ+", key = KeyCode.W });
            defaultKeyboardSettings.Add("camZ-", new Setting() { actionName = "camZ-", key = KeyCode.S });
            */
            new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift }, action = InputActions.RaiseCamera },
            new Setting() { key = KeyCode.C, checkActivationFunction = Input.GetKey, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift }, action = InputActions.LowerCamera },

            new Setting() { key = KeyCode.E, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewUseAbilityOrder); } },
            new Setting() { key = KeyCode.X, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCancelOrder); } },
            new Setting() { key = KeyCode.H, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewHarvestOrder); } },
            new Setting() { key = KeyCode.P, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewPatrolOrder); } },
            new Setting() { key = KeyCode.G, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewGuardOrder); } },
            new Setting() { key = KeyCode.F, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewFollowOrder); } },
            new Setting() { key = KeyCode.T, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCheatRaiseTerrainOrder); } },
            new Setting() { key = KeyCode.Q, action = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCheatSpawnFactoryOrder); } },

            new Setting() { key = KeyCode.Tab, keyExclusions = new List<KeyCode>(){ KeyCode.LeftShift }, action = delegate { InputActions.IncrementSelectionSubgroup(); } },
            new Setting() { key = KeyCode.Tab, keyModifiers = new List<KeyCode>(){ KeyCode.LeftShift }, action = delegate { InputActions.DecrementSelectionSubgroup(); } },
            new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyDown, action = InputActions.OnActionButtonPress },
            new Setting() { key = KeyCode.Mouse1, checkActivationFunction = Input.GetKeyDown, action = InputActions.OnMoveButtonPress },
            new Setting() { key = KeyCode.Mouse0, action = InputActions.OnActionButtonRelease },
            new Setting() { key = KeyCode.Mouse1, action = InputActions.OnMoveButtonRelease }
        };

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
