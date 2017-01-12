using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Storage))]
public class ResourceDeposit : MonoBehaviour
{
    private Storage storage;
    
    void Awake()
    {
        storage = GetComponent<Storage>();
    }
}
