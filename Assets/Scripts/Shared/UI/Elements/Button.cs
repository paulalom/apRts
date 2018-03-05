using UnityEngine;
using UnityEngine.Events;

public class Button {

    public GUIStyle graphic;
    public int width, height;
    public UnityEvent onClick = new UnityEvent();

    public Button(GUIStyle graphic, int width, int height)
    {
        this.graphic = graphic;
        this.width = width;
        this.height = height;
    }
	
    public void Click()
    {
        onClick.Invoke();
    }
}
