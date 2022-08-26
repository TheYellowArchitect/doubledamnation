using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Triggered? NO COLLISION FOR YOU!
public class FlyingPlatformTogglerScript : MonoBehaviour
{
    private Collider2D enemyCollider;
    private PlatformBehaviour tempPlatformBehaviour;

    private void Start()
    {
        //Gets collider of parent
        enemyCollider = transform.parent.GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        tempPlatformBehaviour = collision.gameObject.GetComponent<PlatformBehaviour>();

        if (tempPlatformBehaviour != null)//Waves of tutorial level for some reason trigger this, even though their Layer is Uninteractive... Anyway, it also helps for edge-cases I do not yet know about.
            Physics2D.IgnoreCollision(enemyCollider, tempPlatformBehaviour.platformCollider, true);
    }
}
