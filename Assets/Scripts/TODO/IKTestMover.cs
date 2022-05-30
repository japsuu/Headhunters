using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTestMover : MonoBehaviour
{
    [SerializeField]
    private Transform target1;
    
    [SerializeField]
    private Transform target2;

    [SerializeField]
    private float moveSpeed = 3f;

    private Vector3 targetPos;

    private void Start()
    {
        targetPos = target1.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target1.position) < 0.1f)
        {
            targetPos = target2.position;
        }
        else if(Vector3.Distance(transform.position, target2.position) < 0.1f)
        {
            targetPos = target1.position;
        }
    }
}
