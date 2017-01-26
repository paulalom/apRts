using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Storage))]
[RequireComponent(typeof(Consumer))]
[RequireComponent(typeof(Producer))]
public class Factory : MonoBehaviour {

    private Storage storage;
    private Producer producer;
    private Consumer consumer;
    void Awake()
    {
        storage = GetComponent<Storage>();
        producer = GetComponent<Producer>();
        consumer = GetComponent<Consumer>();
    }
}
