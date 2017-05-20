using System;
using System.Collections.Generic;
using UnityEngine;

public class Setting
{
    //public Action action;
    public KeyCode key;
    public bool smartCast = false;
    public string activationType = "KeyUp";
    public List<KeyCode> keyModifiers = new List<KeyCode>();
}

public class SettingsManager : MonoBehaviour {

    public Dictionary<string, Setting> defaultKeyboardSettings;
    public Dictionary<string, Setting> keyboardSettings;
    

    void Awake()
    {
        defaultKeyboardSettings = new Dictionary<string, Setting>();
        /* leaving these in RTSCamera for now
        defaultKeyboardSettings.Add("camX+", new Setting() { actionName = "camX+", key = KeyCode.D });
        defaultKeyboardSettings.Add("camX-", new Setting() { actionName = "camX-", key = KeyCode.A });
        defaultKeyboardSettings.Add("camZ+", new Setting() { actionName = "camZ+", key = KeyCode.W });
        defaultKeyboardSettings.Add("camZ-", new Setting() { actionName = "camZ-", key = KeyCode.S });
        */


        defaultKeyboardSettings.Add("UseAbility", new Setting() { key = KeyCode.E, smartCast = true });
        defaultKeyboardSettings.Add("Stop", new Setting() { key = KeyCode.S });
        defaultKeyboardSettings.Add("Harvest", new Setting() { key = KeyCode.H });
        defaultKeyboardSettings.Add("Patrol", new Setting() {  key = KeyCode.P });
        defaultKeyboardSettings.Add("Guard", new Setting() { key = KeyCode.G });
        defaultKeyboardSettings.Add("Follow", new Setting() { key = KeyCode.F });

        defaultKeyboardSettings.Add("camY+", new Setting() { key = KeyCode.C, activationType = "KeyHold" });
        defaultKeyboardSettings.Add("camY-", new Setting() { key = KeyCode.C, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift }, activationType = "KeyHold" });
        defaultKeyboardSettings.Add("RaiseTerrain", new Setting() { key = KeyCode.T, activationType = "KeyHold" });
        defaultKeyboardSettings.Add("SpawnFactory", new Setting() { key = KeyCode.Q, activationType = "KeyHold" });

        for (int i = 0; i < 10; i++)
        {
            defaultKeyboardSettings.Add("numeric_" + i, new Setting() { key = KeyCode.Alpha0 + i });
            defaultKeyboardSettings.Add("ALTnumeric_" + i, new Setting() { key = KeyCode.Alpha0 + i, keyModifiers = new List<KeyCode>() { KeyCode.LeftAlt } });
            defaultKeyboardSettings.Add("SHIFTnumeric_" + i, new Setting() { key = KeyCode.Alpha0 + i, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
            defaultKeyboardSettings.Add("CTRLnumeric_" + i, new Setting() { key = KeyCode.Alpha0 + i, keyModifiers = new List<KeyCode>() { KeyCode.LeftControl } });
        }

        keyboardSettings = defaultKeyboardSettings;
    }
}
