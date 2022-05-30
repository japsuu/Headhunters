using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestSpear : MonoBehaviour
{
    [SerializeField]
    private float throwForce = 20f;

    private Animator animator;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInParent<Animator>();
        rb.isKinematic = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            rb.isKinematic = false;
            rb.AddForce(rb.transform.forward * throwForce, ForceMode.Impulse);

            if (animator != null)
            {
                animator.SetTrigger("Throw");
            }
        }
    }
}
