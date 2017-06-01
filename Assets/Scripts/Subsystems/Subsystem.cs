using UnityEngine;
using System.Collections;

public class Subsystem : MonoBehaviour {

    public Subsystem optimalSystem; // when we repair, we cap out at this object
    public float resillience; // damage can break or weaken subsystems. This governs how quickly that happens.
}
