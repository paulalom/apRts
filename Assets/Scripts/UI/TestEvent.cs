using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

// I feel like this is something I need to learn for efficiency sake
public class TestEvent : EventTrigger {
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("hello");
    }
}
