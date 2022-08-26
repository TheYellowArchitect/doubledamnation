using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


//What a clean nice code here. A sigh of relief from the rest...
[RequireComponent(typeof(CircleCollider2D))]
public class PlatformTogglerScript : MonoBehaviour
{
    //A dictionary of all the platforms currently ignored. (and stored ofc ;)
    private Dictionary<int, GameObject> currentPlatformCollisions = new Dictionary<int, GameObject>();

    private Rigidbody2D enemyRigidbody;
    private Collider2D enemyCollider;

    private EnemyBehaviour enemyBehaviour;

    public bool collideWithPlatforms = true;

    //TODO: Remove this at final vers.
    [ReadOnly]
    public int collisionsCount;

    private PlatformBehaviour tempPlatformBehaviour;

    private void Start()
    {
        //Gets collider of parent
        enemyCollider = transform.parent.GetComponent<Collider2D>();//so as to ignore collisions
        enemyRigidbody = transform.parent.GetComponent<Rigidbody2D>();//so as to detect velocity

        enemyBehaviour = transform.parent.GetComponent<EnemyBehaviour>();

        //Subscribes the TouchedGround function to EnemyJump.TouchedGround
        if (enemyBehaviour.jumpAction != null)
                enemyBehaviour.jumpAction.touchedGround.AddListener(ResetToggle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Add the collided platform gameobject to the currentPlatformCollisions dictionary, so when it exits, the identification will be ez

        //If the collision isn't registered before, add it in the dictionary
        if (currentPlatformCollisions.ContainsKey(collision.gameObject.GetInstanceID()) == false)
            //Register it to the dictionary
            currentPlatformCollisions.Add(collision.gameObject.GetInstanceID(), collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //If it is stored, time to release it and do the Exitting stuff here
        if (currentPlatformCollisions.ContainsKey(collision.gameObject.GetInstanceID()))
            //Remove from registry/dictionary.
            currentPlatformCollisions.Remove(collision.gameObject.GetInstanceID());
    }

    //TODO: Coroutine so it's more performant. Also, starting only when jumping.
    private void Update()
    {
        //if (gameManager.testing)
        //Debug.Log("Velocity is: " + enemyRigidbody.velocity.y);

        //No need to check further.
        if (enemyBehaviour.isGrounded)
            return;

        #region old remains
        //Activate/Deactivate collisions, depending on velocityY.
        /*
        if (enemyRigidbody.velocity.y > 0)
        {
            //Reset the toggle, so it happens only once.
            if (collideWithPlatforms)
                toggled = false;
            collideWithPlatforms = false;
        }

        else
        {
            //Reset the toggle, so it happens only once.
            if (collideWithPlatforms == false)
                toggled = false;
            collideWithPlatforms = true;
        }*/
        #endregion

        //Purely for debugging
        collisionsCount = currentPlatformCollisions.Count;

        //Instead of enabling/disabling collisions every frame, or by trigger-detection/collision, the below uses event-driven architecture (still mixed with per-frame check) to toggle only when needed.

        //===========================================================
            //Deactivate collisions!
            if (enemyRigidbody.velocity.y > 0 && collideWithPlatforms)
            {
                //toggled = true;
                collideWithPlatforms = false;

                //Iterate through every value of the dictionary
                foreach (GameObject pickedPlatform in currentPlatformCollisions.Values)
                {
                    //Debug.Log("Ignoring");

                    tempPlatformBehaviour = pickedPlatform.GetComponent<PlatformBehaviour>();
                    if (tempPlatformBehaviour == null)//Rare bug with Antonis testing on Steam
                        continue;

                    //If not ignoring
                    if (Physics2D.GetIgnoreCollision(enemyCollider, tempPlatformBehaviour.platformCollider) == false)
                        //Ignore collision with the platform.
                        Physics2D.IgnoreCollision(enemyCollider, tempPlatformBehaviour.platformCollider, true);

                }

            }
            //Reactivate collisions!
            else if (enemyRigidbody.velocity.y <= 0 && collideWithPlatforms == false)
            {
                collideWithPlatforms = true;

                //Iterate through every value of the dictionary
                foreach (GameObject pickedPlatform in currentPlatformCollisions.Values)
                {
                    tempPlatformBehaviour = pickedPlatform.GetComponent<PlatformBehaviour>();
                    if (tempPlatformBehaviour == null)//Rare bug with Antonis testing on Steam
                        continue;

                    //If ignoring (so it won't reset every frame.)
                    if (Physics2D.GetIgnoreCollision(enemyCollider, tempPlatformBehaviour.platformCollider))
                        //Re-activate the collision with the platform.
                        Physics2D.IgnoreCollision(enemyCollider, tempPlatformBehaviour.platformCollider, false);
                }

            }
            //else, keep all the flags as is, it works perfectly.

            //Reminder that the above if/else conditions, work ONCE when switched.
            //Look for an example.
            //  Satyr jumps up and right. Collision count is 2, but going upwards it becomes 0. When he is about to drop down, there is a platform.
            //  However, the collidewithPlatforms=true and Physics2D.Don'tIgnoreCollision(), happens WHEN THERE IS NO PLATFORM CACHED!
            //  Which means, if the collision range is small, he will just pass/fall through.
            //  Above example happened with Satyr(20).
        //===========================================================

    }

    public void ResetToggle()
    {
        collideWithPlatforms = true;
    }

}
