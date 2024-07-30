using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class InteractionLogic : MonoBehaviour
{

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void onRelease()
    {
        _rb.isKinematic = false;
       Invoke("Release", 5f);
    }

    void Release()
    {
       
        _rb.isKinematic = true;
        
    }
}
