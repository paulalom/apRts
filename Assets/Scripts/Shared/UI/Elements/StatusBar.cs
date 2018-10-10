using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public Image foreground, background;
    public Color fullColor, emptyColor;
    public float minValue, maxValue, currentValue;
    public Text text;

    public static Dictionary<string, Color[]> statusBarTypes = new Dictionary<string, Color[]>()
    {
        { "Health", new Color[]{new Color(0,255,0), new Color(255,0,0) } },
        { "Armor", new Color[]{new Color(255,75,0), new Color(0,0,0) } },
        { "Shield", new Color[]{new Color(0,0,255), new Color(255,255,255) } },
        { "Battery", new Color[]{new Color(255,255,0), new Color(0,0,0) } },
        { "Capacitor", new Color[]{new Color(255,255,255), new Color(0,0,0) } },
        { "Mana", new Color[]{new Color(0,0,255), new Color(255,255,255) } }
    };

    /// <param name="minMaxColors">array of length 2 containing color when bar is empty and color when bar is full (to be linearly interpolated between)</param>
    /// <param name="maxMinCurrentValues">array of length 2 or 3 containing max, min, and optionally current values</param>
    public void InitBar(Color[] minMaxColors, float[] maxMinCurrentValues)
    {
        emptyColor = minMaxColors[0];
        fullColor = minMaxColors[1];
        minValue = maxMinCurrentValues[0];
        maxValue = maxMinCurrentValues[1];
        currentValue = maxMinCurrentValues.Length > 2 ? maxMinCurrentValues[2] : 0;
        text.text = currentValue + "/" + maxValue;
    }

    public void SetBarValue(float currentValue)
    {
        this.currentValue = currentValue;
        float fillRatio = (currentValue - minValue) / (maxValue - minValue);

        Color newColor = new Color(fullColor.r * fillRatio + (emptyColor.r * (1 - fillRatio)),
                                    fullColor.g * fillRatio + (emptyColor.g * (1 - fillRatio)),
                                    fullColor.b * fillRatio + (emptyColor.b * (1 - fillRatio)));

        foreground.color = newColor;
        RectTransform filledBar = foreground.GetComponent<RectTransform>();
        filledBar.anchorMax = new Vector2(fillRatio, filledBar.anchorMax.y);
        text.text = (int)currentValue + "/" + (int)maxValue;
    }
}
