using UnityEngine;
using NaughtyAttributes;

public class PlatformBehaviour : MonoBehaviour
{
    public BoxCollider2D triggerCollider;   //The one that detects state, triggers stuff
    public BoxCollider2D platformCollider;  //The rigidbody/physics hitbox that player collides with.

    private BoxCollider2D boxPlayerCollider;       //Body(Top&Mid)
    private CircleCollider2D circlePlayerCollider; //Feet(Bottom)

    [ReadOnly]
    public bool playerIgnoring = false;

    public static bool playerSideIgnoring = false;
    //#JustTimerThings
    //private float timeLeft = 1;
    //private bool playerFalling = false;

    private void Start()
    {
        //Store player's colliders here
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        boxPlayerCollider = player.GetComponent<BoxCollider2D>();
        circlePlayerCollider = player.GetComponent<CircleCollider2D>();

        //player.GetComponent<WarriorMovement>().touchedFloor += ReactivatePlatformForPlayer;
    }

    //Before blaming the useless wrappers, it was made more "efficiently" after, and no time to find all the references to this
    public void ReactivatePlatformForPlayer()
    {
        IgnorePlayerCollision(false);
    }

    public void DisablePlatformForPlayer()
    {
        IgnorePlayerCollision(true);

        //Physics2D.IgnoreLayerCollision(8, 12, true);//... mandatory commenting to laugh on myself
    }

    public void IgnorePlayerCollision(bool toIgnore)
    {
        playerIgnoring = toIgnore;
        
        Physics2D.IgnoreCollision(platformCollider, boxPlayerCollider, toIgnore);
        Physics2D.IgnoreCollision(platformCollider, circlePlayerCollider, toIgnore);
    }

    /*
    //====================================== Side platform for warrior below ==============================================================//
    //When player lands on the sides/edge of platform. In a normal game, you get a stumble/getup animation. Not this one, not today. Gotta keep the flow!
    //This should be on warrior's responsibility, but ah well. -> DONE! :)
    public void OnCollisionEnter2D(Collision2D collision)
    {
        //Side-platform'd
        if (collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.x != 0)
        {
            playerSideIgnoring = true;
            SetPlayerCollision(true);
        }
            
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerSideIgnoring)
        {
            playerSideIgnoring = false;
            SetPlayerCollision(false);
        }
    }
    //====================================== ==================================== ==============================================================//
    */

    #region //Old and useless stuff.
    /*
    private void Update()
    {
        //Timer to restore the platform's collider
        if (playerFalling)
        {
            timeLeft = timeLeft - Time.deltaTime;
            if (timeLeft < 0)
            {
                timeLeft = 1;
                platformCollider.enabled = true;
                playerFalling = false;
            }
        }
    }
    */

    /*
    private void OnTriggerExit2D(Collider2D collision)
    {
        Vector2 tempDirection = (collision.transform.position - transform.position);
        Vector2 tempContactPoint = new Vector2(transform.position.x, transform.position.y) + tempDirection;

        if (collision.CompareTag("Player"))
        {
            Debug.Log("PlayerTriggered! also, playerfalling == " + playerFalling + " and collisionTypeIs: " + collision.GetType());
        }

        if (collision.GetType() == typeof(BoxCollider2D))
            Debug.Log("boxes are: " + collision.gameObject.name);

        //If player fully exitted the platform
        if (playerFalling && collision.CompareTag("Player") && collision.GetType() == typeof(BoxCollider2D))
        {
            Debug.Break();
            Debug.Log("Exitted, contact point is: " + tempContactPoint);
            playerFalling = false;
            Physics2D.IgnoreLayerCollision(8, 12, false);
        }
    }
    */

    /*
    //Old DisablePlatform
    public void DisablePlatform()
    {
        Debug.Log("||PlatformDebugging|| Disabled platform");
        platformCollider.enabled = false;
        playerFalling = true;
    }
    */
    #endregion
}
