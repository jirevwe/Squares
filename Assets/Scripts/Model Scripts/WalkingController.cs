using UnityEngine;
using System.Collections;
using System;

public class WalkingController : Controller
{
    public float walkSpeed = 3f;
    Vector3 walkVelocity;

    public override void ReadInput(InputData data)
    {
        walkVelocity = Vector3.zero;

        //set vertical movment
        if(data.axes[0] != 0)
        {
            walkVelocity += Vector3.forward * data.axes[0] * walkSpeed;
        }

        //set horizontal movment
        if (data.axes[1] != 0)
        {
            walkVelocity += Vector3.right * data.axes[1] * walkSpeed;
        }

        newInput = true;
    }

    void LateUpdate()
    {
        if(!newInput)
        {
            walkVelocity = Vector3.zero;
        }
        rb.velocity = new Vector3(walkVelocity.x, rb.velocity.y, walkVelocity.z);
        newInput = false;
    }
}
