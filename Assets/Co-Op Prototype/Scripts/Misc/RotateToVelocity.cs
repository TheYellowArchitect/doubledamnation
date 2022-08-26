using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This code is a failure, because for some peculiar reason, you cannot really rotate a particle. Not by transform, not by particleSystem values. Weird.
public class RotateToVelocity : MonoBehaviour
{

    [Tooltip("Target rigidbody to convert velocity to rotation")]
    public Rigidbody2D targetRigidbody;

    [Tooltip("Checks whether to rotate or not")]
    public bool shouldRotate = false;

    private Vector3 tempVector;
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (shouldRotate == false)
            return;

        //transform.LookAt(targetRigidbody.velocity);

        //transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.zero, targetRigidbody.velocity, Vector3.forward), Vector3.forward);

        //transform.rotation = ConvertRotationToVelocity();
	}

    public Quaternion ConvertRotationToVelocity()
    {
        tempVector = targetRigidbody.velocity.normalized;

        return Quaternion.Euler(tempVector);
    }
}
