using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Doesn't need rigidbody. A Lazy way to just "pull" something down.
/// </summary>
public class GravityNoAcceleration : MonoBehaviour
{
    public float gravitySpeed = 0.1f;
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - gravitySpeed, transform.position.z);
	}
}
