using Assets.Scripts.Shared.UI;
using System;
using System.Linq;
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
    CommandGridMap commandGridMap;
    const int numCommandGridVariants = 6; // includes Default
    public const int numButtonsInCommandGrid = 12;

    public override void MyStart()
    {
        base.MyStart();
        defaultInputSettings = GetDefaultInputSettings();
        inputSettings = defaultInputSettings;
        commandGridMap = GetCommandGridMap(numCommandGridVariants);
    }

    // The config should be moved out to a file eventually

    List<Setting> GetDefaultInputSettings()
    {
        List<Setting> defaultSettings = new List<Setting>
        {
            new Setting() { key = KeyCode.J, checkActivationFunction = Input.GetKey, action = InputActions.RaiseCamera },
            new Setting() { key = KeyCode.K, checkActivationFunction = Input.GetKey, action = InputActions.LowerCamera },

            new Setting() { key = KeyCode.Q, action = delegate { InputActions.ClickCommandGrid(1, 1); } },
            new Setting() { key = KeyCode.W, action = delegate { InputActions.ClickCommandGrid(1, 2); } },
            new Setting() { key = KeyCode.E, action = delegate { InputActions.ClickCommandGrid(1, 3); } },
            new Setting() { key = KeyCode.R, action = delegate { InputActions.ClickCommandGrid(1, 4); } },

            new Setting() { key = KeyCode.A, action = delegate { InputActions.ClickCommandGrid(2, 1); } },
            new Setting() { key = KeyCode.S, action = delegate { InputActions.ClickCommandGrid(2, 2); } },
            new Setting() { key = KeyCode.D, action = delegate { InputActions.ClickCommandGrid(2, 3); } },
            new Setting() { key = KeyCode.F, action = delegate { InputActions.ClickCommandGrid(2, 4); } },

            new Setting() { key = KeyCode.Z, action = delegate { InputActions.ClickCommandGrid(3, 1); } },
            new Setting() { key = KeyCode.X, action = delegate { InputActions.ClickCommandGrid(3, 2); } },
            new Setting() { key = KeyCode.C, action = delegate { InputActions.ClickCommandGrid(3, 3); } },
            new Setting() { key = KeyCode.V, action = delegate { InputActions.ClickCommandGrid(3, 4); } },
            
            new Setting() { key = KeyCode.Tab, keyExclusions = new List<KeyCode>(){ KeyCode.LeftShift }, action = delegate { InputActions.IncrementSelectionSubgroup(); } },
            new Setting() { key = KeyCode.Tab, keyModifiers = new List<KeyCode>(){ KeyCode.LeftShift }, action = delegate { InputActions.DecrementSelectionSubgroup(); } },
            new Setting() { key = KeyCode.Mouse0, checkActivationFunction = Input.GetKeyDown, action = InputActions.OnActionButtonPress },
            new Setting() { key = KeyCode.Mouse1, checkActivationFunction = Input.GetKeyDown, action = InputActions.OnMoveButtonPress },
            new Setting() { key = KeyCode.Mouse0, action = InputActions.OnActionButtonRelease },
            new Setting() { key = KeyCode.Mouse1, action = InputActions.OnMoveButtonRelease }
        };

        for (int i = 0; i < 10; i++)
        {
            int j = i; // needed for closure (delegate will use max value of i rather than current if we don't create a new variable inside of the loop)

            defaultSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift, KeyCode.LeftAlt, KeyCode.LeftControl }, action = delegate { InputActions.NumericMenuButton(j); } });
            //defaultSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift, KeyCode.LeftControl }, keyModifiers = new List<KeyCode>() { KeyCode.LeftAlt } });
            //defaultSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftAlt, KeyCode.LeftControl }, keyModifiers = new List<KeyCode>() { KeyCode.LeftShift } });
            //defaultSettings.Add(new Setting() { key = KeyCode.Alpha0 + i, keyExclusions = new List<KeyCode>() { KeyCode.LeftShift, KeyCode.LeftAlt }, keyModifiers = new List<KeyCode>() { KeyCode.LeftControl } });
        }
        return defaultSettings;
    }

    CommandGridMap GetCommandGridMap(int mapSize)
    {
        CommandGridMap commandGridMap = new CommandGridMap(mapSize);
        commandGridMap.InsertElement("Default", GetDefaultCommandGridActions(), GetDefaultCommandGridIcons(), new string[numButtonsInCommandGrid]);

        var keys = GetCommandGridKeys();
        var actions = GetCommandGridActions();
        var icons = GetCommandGridIcons();

        // Iterate through all 3 loops simultaneously and add to map
        using (var key = keys.GetEnumerator())
        using (var action = actions.GetEnumerator())
        using (var icon = icons.GetEnumerator())
        {
            while (key.MoveNext() && action.MoveNext() && icon.MoveNext())
            {
                commandGridMap.InsertElement(key.Current, action.Current, icon.Current, new string[numButtonsInCommandGrid]);
            }
        }

        return commandGridMap;
    }

    // Note WASD is still used for camera movement temporarily
    // Also the stuff below is all going to be reworked as I think about/use it more, for now I am just prioritizing dev time.
    // Will definitely need a better/generic method of loading/storing data for unit configurations
    // Need to fix the key instantiation duplication... 
    // it's repeated and should be factored out, but gives structure to make the code more readable so im leaving it for now

    List<string> GetCommandGridKeys()
    {
        List<string> keys = new List<string>(numCommandGridVariants);
        keys.Add(typeof(Commander).ToString());
        keys.Add(typeof(Commander).ToString() + CommandMenuType.UnitConstruction.ToString());
        keys.Add(typeof(Factory).ToString());
        keys.Add(typeof(Factory).ToString() + CommandMenuType.UnitConstruction.ToString());
        keys.Add(typeof(Factory).ToString() + CommandMenuType.ResourceConstruction.ToString());

        return keys;
    }

    List<Texture2D[]> GetCommandGridIcons()
    {
        List<Texture2D[]> icons = new List<Texture2D[]>(numCommandGridVariants);

        Texture2D[] commanderIcons = new Texture2D[numButtonsInCommandGrid];
        commanderIcons[0] = UIManager.icons["Factory"];
        commanderIcons[2] = UIManager.icons["DefaultAbility"];
        commanderIcons[3] = UIManager.icons["RaiseTerrainAbility"];
        commanderIcons[9] = UIManager.icons["StopAbility"];
        commanderIcons[10] = UIManager.icons["Factory"]; // UnitConstruction
        icons.Add(commanderIcons);

        Texture2D[] commanderUnitConstructionIcons = new Texture2D[numButtonsInCommandGrid];
        commanderUnitConstructionIcons[0] = UIManager.icons["Factory"];
        commanderUnitConstructionIcons[1] = UIManager.icons["PowerPlant"];
        commanderUnitConstructionIcons[2] = UIManager.icons["HarvestingStation"];
        icons.Add(commanderUnitConstructionIcons);

        Texture2D[] FactoryIcons = new Texture2D[numButtonsInCommandGrid];
        FactoryIcons[10] = UIManager.icons["Factory"];
        FactoryIcons[11] = UIManager.icons["Tool"];
        icons.Add(FactoryIcons);

        Texture2D[] factoryUnitConstructionIcons = new Texture2D[numButtonsInCommandGrid];
        factoryUnitConstructionIcons[0] = UIManager.icons["ConstructionSphere"];
        factoryUnitConstructionIcons[1] = UIManager.icons["Tank"];
        icons.Add(factoryUnitConstructionIcons);

        Texture2D[] factoryResourceConstructionIcons = new Texture2D[numButtonsInCommandGrid];
        factoryResourceConstructionIcons[0] = UIManager.icons["Tool"];
        factoryResourceConstructionIcons[1] = UIManager.icons["Paper"];
        icons.Add(factoryResourceConstructionIcons);

        return icons;
    }

    List<Action[]> GetCommandGridActions()
    {
        List<Action[]> actions = new List<Action[]>(numCommandGridVariants);

        Action[] commanderActions = new Action[numButtonsInCommandGrid];
        commanderActions[0] = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCheatSpawnFactoryOrder); };
        commanderActions[2] = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewUseAbilityOrder); };
        commanderActions[3] = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCheatRaiseTerrainOrder); };
        commanderActions[9] = delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCancelOrder); };
        commanderActions[10] = delegate { InputActions.OpenCommandMenuForType(CommandMenuType.UnitConstruction, typeof(Commander)); };
        actions.Add(commanderActions);

        Action[] commanderUnitConstructionActions = new Action[numButtonsInCommandGrid];
        commanderUnitConstructionActions[0] = delegate { InputActions.StartConstruction(typeof(Factory)); };
        commanderUnitConstructionActions[1] = delegate { InputActions.StartConstruction(typeof(PowerPlant)); };
        commanderUnitConstructionActions[2] = delegate { InputActions.StartConstruction(typeof(HarvestingStation)); };
        actions.Add(commanderUnitConstructionActions);

        Action[] factoryActions = new Action[numButtonsInCommandGrid];
        factoryActions[10] = delegate { InputActions.OpenCommandMenuForType(CommandMenuType.UnitConstruction, typeof(Factory)); };
        factoryActions[11] = delegate { InputActions.OpenCommandMenuForType(CommandMenuType.ResourceConstruction, typeof(Factory)); };
        actions.Add(factoryActions);

        Action[] factoryUnitConstructionActions = new Action[numButtonsInCommandGrid];
        factoryUnitConstructionActions[0] = delegate { InputActions.StartConstruction(typeof(ConstructionSphere)); };
        factoryUnitConstructionActions[1] = delegate { InputActions.StartConstruction(typeof(Tank)); };
        actions.Add(factoryUnitConstructionActions);

        Action[] factoryResourceConstructionActions = new Action[numButtonsInCommandGrid];
        factoryResourceConstructionActions[0] = delegate { InputActions.StartConstruction(typeof(Tool)); };
        factoryResourceConstructionActions[1] = delegate { InputActions.StartConstruction(typeof(Paper)); };
        actions.Add(factoryResourceConstructionActions);

        return actions;
    }

    Texture2D[] GetDefaultCommandGridIcons()
    { 
        List<string> iconStrings = new List<string>
        {
            "None", "None", "DefaultAbility", "None",
            "None", "None", "None", "None",
            "None", "StopAbility", "None", "None"
        };
        
        return iconStrings.Select(x => UIManager.icons[x]).ToArray();
    }

    Action[] GetDefaultCommandGridActions()
    {
        // Note WASD is still used for camera movement temporarily
        return new Action[]
        {
            null, null, delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewUseAbilityOrder); }, null,
            null, null, null, null,
            null, delegate { InputActions.IssueCommand((int)OrderBuilderFunction.NewCancelOrder); }, null, null
        };
    }

    public CommandGridItem GetCommandGridItem(string key)
    {
        if (commandGridMap.ContainsKey(key))
        {
            return commandGridMap.GetItem(key);
        }
        else
        {
            return commandGridMap.GetItem("Default");
        }
    }
}
