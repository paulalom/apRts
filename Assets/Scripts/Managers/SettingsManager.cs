using System;
using System.Collections.Generic;
using UnityEngine;

public class Setting
{
    //public Action action;
    public KeyCode key;
    public string activationType = "KeyUp";
    public List<KeyCode> keyModifiers = new List<KeyCode>();
}

public class SettingsManager : MonoBehaviour {

    public Dictionary<string, Setting> defaultKeyboardSettings;
    public Dictionary<string, Setting> keyboardSettings;
    

    void Start()
    {
        defaultKeyboardSettings = new Dictionary<string, Setting>();
        /* leaving these in RTSCamera for now
        defaultKeyboardSettings.Add("camX+", new Setting() { actionName = "camX+", key = KeyCode.D });
        defaultKeyboardSettings.Add("camX-", new Setting() { actionName = "camX-", key = KeyCode.A });
        defaultKeyboardSettings.Add("camZ+", new Setting() { actionName = "camZ+", key = KeyCode.W });
        defaultKeyboardSettings.Add("camZ-", new Setting() { actionName = "camZ-", key = KeyCode.S });
        */

        defaultKeyboardSettings.Add("camY+", new Setting() { key = KeyCode.C });
        defaultKeyboardSettings.Add("camY-", new Setting() { key = KeyCode.C, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });

        defaultKeyboardSettings.Add("RaiseTerrain", new Setting() { key = KeyCode.T });
        defaultKeyboardSettings.Add("SpawnFactory", new Setting() { key = KeyCode.Q });
        defaultKeyboardSettings.Add("UseAbility", new Setting() { key = KeyCode.A });
        defaultKeyboardSettings.Add("Stop", new Setting() { key = KeyCode.S });
        defaultKeyboardSettings.Add("Harvest", new Setting() { key = KeyCode.H });
        defaultKeyboardSettings.Add("Patrol", new Setting() {  key = KeyCode.P });
        defaultKeyboardSettings.Add("Guard", new Setting() { key = KeyCode.G });
        defaultKeyboardSettings.Add("Follow", new Setting() { key = KeyCode.F });

        for (int i = 0; i < 10; i++)
        {
            defaultKeyboardSettings.Add("numeric_" + i, new Setting() { key = KeyCode.Alpha0 + i });
        }

        keyboardSettings = defaultKeyboardSettings;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
