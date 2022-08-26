using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tweaked. It used to literally copy the transform.position of the object, but now it is only for vector2.
/// </summary>
public class FollowObjectSimple : MonoBehaviour
{
    public GameObject objectToFollow;

	void Update ()
    {
        //transform.position =  objectToFollow.transform.position;
        transform.position = new Vector3 (objectToFollow.transform.position.x, objectToFollow.transform.position.y, transform.position.z);
	}
}
