using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Controller : MonoBehaviour {

    //TODO: Add method ReadInput
    public abstract void ReadInput(InputData data);

    protected Rigidbody rb;
    protected bool newInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}
