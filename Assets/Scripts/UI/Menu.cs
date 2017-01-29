using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


// this class isnt used yet, everything is still in the manager

public class Menu : MonoBehaviour {

    public GUIStyle graphic;
    public Rect location;
    public int paddingSize;
    private List<Button> buttons; // must be private for menu row height management
    int tallestButtonHeight = 0;

	// Use this for initialization
	void Start () {
        buttons = new List<Button>();
        paddingSize = 5;
	}

    public void AddButton(Button button)
    {
        buttons.Add(button);
        if (button.height > tallestButtonHeight)
        {
            tallestButtonHeight = button.height;
        }
    }

	void OnGUI () {
        float xPos = location.x + paddingSize;
        float yPos = location.y + paddingSize;
	    for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            Rect r = new Rect(xPos, yPos, button.width, button.height);
            if (GUI.Button(r, "", button.graphic)){
                button.Click();
            }
            if (i < buttons.Count - 1) {
                // New row?
                if ((xPos - location.x + button.width + buttons[i + 1].width + 2 * paddingSize) / location.width > 1)
                {
                    xPos = location.x + paddingSize;
                    yPos += tallestButtonHeight + paddingSize;
                }
                else
                {
                    xPos += button.width + paddingSize;
                }
            }
        }
	}
}
