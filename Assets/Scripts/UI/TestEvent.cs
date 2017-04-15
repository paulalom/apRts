using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TestEvent : EventTrigger {
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("hello");
    }
}
