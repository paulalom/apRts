using Assets.Scripts.Shared.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBarCommandGrid : UIBarComponent
{
    public List<Button> buttons;
    SettingsManager settingsManager;
    UIManager uiManager;

    private void Awake()
    {
        settingsManager = GameObject.Find("SettingsManager").GetComponent<SettingsManager>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }

    // Update command display for selected units
    public override void UpdateDisplay(List<RTSGameObject> selectedUnits)
    {
        base.UpdateDisplay(selectedUnits);
    }

    internal void ClickButton(int row, int column)
    {
        Button button = buttons[(row - 1) * 4 + (column - 1)];

        // Trigger the button by actually sending a click (instead of onClick.Invoke) so it animates the button when you press the hotkey
        EventSystem eventSystem = button.GetComponent<EventSystem>();
        ExecuteEvents.Execute(button.gameObject, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
    }

    internal void OnSelectionSubgroupChange(List<Type> subgroups, int index)
    {
        string key = subgroups.Count > 0 ? subgroups[index].ToString() : "Default";
        CommandGridItem commandGridItem = settingsManager.GetCommandGridItem(key);
        SetCommandGrid(commandGridItem);
    }

    internal void SetCommandGrid(string gridVariantKey)
    {
        SetCommandGrid(settingsManager.GetCommandGridItem(gridVariantKey));
    }

    internal void SetCommandGrid(CommandGridItem item)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int j = i; // closure requires this
            Button button = buttons[j];
            button.onClick.RemoveAllListeners();
            if (item.buttonActions[j] != null)
            {
                button.onClick.AddListener(delegate { item.buttonActions[j].Invoke(); });
                button.GetComponent<RawImage>().texture = item.buttonIcons[j];
                button.GetComponentInChildren<Text>().text = item.buttonText[j];
            }
            else
            {
                button.GetComponent<RawImage>().texture = UIManager.icons["None"];
                button.GetComponentInChildren<Text>().text = "";
            }
        }
    }
}
