using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class GravBody : MonoBehaviour
{
    GravAttractor planet;
    Rigidbody rb;

    void Start()
    {
        planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<GravAttractor>();
        rb = GetComponent<Rigidbody>();


        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        planet.Attract(rb);
    }
}
