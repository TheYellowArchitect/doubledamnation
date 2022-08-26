using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformReactivation : MonoBehaviour
{
    /*
    Rigidbody2D warriorRigidbody;

    private void Start()
    {
        warriorRigidbody = GetComponentInParent<Rigidbody2D>();
    }
    */
    private PlatformBehaviour tempPlatformBehaviour;

    private void OnTriggerExit2D(Collider2D platformCollision)
    {
        //Debug.Log("Ceiling thingy Detected");

        tempPlatformBehaviour = platformCollision.GetComponent<PlatformBehaviour>();

        if (tempPlatformBehaviour == null)
            return;

        //Going downwards
        if (tempPlatformBehaviour.playerIgnoring)
        //if (warriorRigidbody.velocity.y < 0)
            tempPlatformBehaviour.ReactivatePlatformForPlayer();
            
    }
}
