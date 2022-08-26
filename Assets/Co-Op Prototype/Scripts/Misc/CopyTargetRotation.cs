using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTargetRotation : MonoBehaviour
{
    [Tooltip("Target gameobject/transform to have same rotation with")]
    public Transform targetTransform;
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.localRotation = targetTransform.rotation;
	}
}
