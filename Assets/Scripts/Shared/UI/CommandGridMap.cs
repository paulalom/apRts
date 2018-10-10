using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Shared.UI
{
    // Stores data for a single Type's command grid
    public struct CommandGridItem
    {
        public string key;
        public Texture2D[] buttonIcons;
        public string[] buttonText;
        public Action[] buttonActions;
    }

    // class to store information about icons, text, and actions for buttons in the command grid.
    // Array sizes are fixed since data should be static, adding more elements than the size initilized with will throw exception.
    public class CommandGridMap
    {
        readonly Dictionary<string, int> keyToIndexMap;
        readonly string[] keys;
        readonly Texture2D[][] buttonIcons;
        readonly string[][] buttonText;
        readonly Action[][] buttonActions;

        int nextAvailableId = 0;

        public CommandGridMap(int numElements)
        {
            keyToIndexMap = new Dictionary<string, int>();
            keys = new string[numElements];
            buttonIcons = new Texture2D[numElements][];
            buttonText = new string[numElements][];
            buttonActions = new Action[numElements][];
        }

        /// <summary>
        /// All entries must have all 3 arrays, with length of SettingsManager.numButtonsInCommandGrid
        /// </summary>
        public void InsertElement(string key, Action[] buttonActions, Texture2D[] buttonIcons, string[] buttonText)
        {
            keys[nextAvailableId] = key;
            this.buttonIcons[nextAvailableId] = buttonIcons;
            this.buttonText[nextAvailableId] = buttonText;
            this.buttonActions[nextAvailableId] = buttonActions;
            keyToIndexMap.Add(key, nextAvailableId);
            nextAvailableId++;
        }

        public void InsertElement(string key, CommandGridItem item)
        {
            keys[nextAvailableId] = key;
            buttonIcons[nextAvailableId] = item.buttonIcons;
            buttonText[nextAvailableId] = item.buttonText;
            buttonActions[nextAvailableId] = item.buttonActions;
            keyToIndexMap.Add(key, nextAvailableId);
            nextAvailableId++;
        }

        public CommandGridItem GetItem(string key)
        {
            int index = keyToIndexMap[key];
            return new CommandGridItem()
            {
                key = key,
                buttonActions = this.buttonActions[index],
                buttonIcons = this.buttonIcons[index],
                buttonText = this.buttonText[index]
            };
        }

        public bool ContainsKey(string key)
        {
            return keyToIndexMap.ContainsKey(key);
        }
    }
}
