using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBarContainer : MyMonoBehaviour
{
    public StatusBar[] statusBars;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns an array of StautsBar ordered visually from left to right, contained by this</returns>
    public StatusBar[] InstantiateStatusBars(string[] barTypes, float[] maxBarValues, GameObject statusBarPrefab)
    {
        statusBars = new StatusBar[barTypes.Length];

        // Invert array so InOrder components are left to right
        for (int i = 0; i < barTypes.Length; i++)
        {
            StatusBar newBar = Instantiate(statusBarPrefab, transform).GetComponent<StatusBar>();
            newBar.name = barTypes[i];

            RectTransform barTransform = newBar.GetComponent<RectTransform>();
            barTransform.anchorMin = new Vector2((1f / barTypes.Length) * (barTypes.Length - i - 1), barTransform.anchorMin.y);
            barTransform.anchorMax = new Vector2((1f / barTypes.Length) * (barTypes.Length - i), barTransform.anchorMax.y);

            Color[] barColors = StatusBar.statusBarTypes[barTypes[i]];
            newBar.fullColor = barColors[0];
            newBar.emptyColor = barColors[1];
            newBar.maxValue = maxBarValues[i];
            newBar.currentValue = 0;
            newBar.minValue = 0;

            statusBars[i] = newBar;
        }

        return statusBars;
    }
}
