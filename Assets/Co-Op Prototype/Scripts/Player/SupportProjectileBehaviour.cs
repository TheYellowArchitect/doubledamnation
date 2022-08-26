using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: In the future, add a child with a box2d TRIGGER collider, that detects only ground, and is for the VFX.
public class SupportProjectileBehaviour : MonoBehaviour
{

    [Tooltip("How fast it rises upwards, aka velocity, on spawn, when player is grounded")]
    public float pillarStartingGroundedVelocity = 70;

    [Tooltip("How fast it rises upwards, aka velocity, on spawn, when player is midair\nRight now, useless.")]
    public float pillarStartingMidairVelocity = 150;

    [Tooltip("Once spawned, how fast it reaches player's foot. On this, the speed is actually determined ;)")]
    public float midairSecondsToArrive = 0.2f;

    [Tooltip("How fast it rises on contact with player, before stopping completely.")]
    public float pillarDelayedVelocity = 25;

    [Tooltip("How many times the distance from spawn, should pillar spawn.")]
    public float DistanceMultiplier = 3;

    [Tooltip("After hitting player, when does it die/crumble?")]
    public float DeathTime = 8;

    [Tooltip("After hitting falling platform, when does it die/crumble?")]
    public float DeathPlatformTime = 3;

    [Tooltip("If player spawns it right when standing on ground, it should start lower, hence by default, it always goes this much downwards on start")]
    public float heightDifferenceConstant = 4;

    [Tooltip("So it does the VFX upon impact")]
    public LayerMask WhatIsGround;

    [Tooltip("So it can detect player and stop speeding up")]
    public LayerMask WhatIsPlayer;

    private Rigidbody2D commonRigidbody;

    private bool activated = false;

    //By WarriorMovement, upon Instantiate ;)
    public void InitializeValues (Vector2 playerPosition, float distanceFromGround, bool isPlayerGrounded)
    {

        //Set location to spawn
        transform.position = CalculateSpawnLocation(playerPosition, distanceFromGround);

        //Register the rigidbody
        commonRigidbody = GetComponent<Rigidbody2D>();

        //Set speed
        if (isPlayerGrounded)
            commonRigidbody.velocity = new Vector2(0f, pillarStartingGroundedVelocity);
        else
            //Speed is equal to meters/second. In other words, it should reach player in midairSecondsToArrive and hence by reverse-thinking, using distance and the optimal/fun time to reach, ta-da!
            commonRigidbody.velocity = new Vector2(0f, (playerPosition.y - transform.position.y) / midairSecondsToArrive);

        Debug.Log("Grounded: " + isPlayerGrounded);

        Invoke("ExpirationDeath", 15);
    }

    public Vector3 CalculateSpawnLocation(Vector3 playerPosition, float distanceFromGround)
    {
        //Debug.Log("Player position is: " + playerPosition + "target position is: " + new Vector3(playerPosition.x, playerPosition.y - distanceFromGround * DistanceMultiplier, 0f));

        //Debug.Log("Distance and multiplier: " + distanceFromGround + " " + DistanceMultiplier);

        float cameraLowestPoint = GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().GetLowestBoundPoint();

        Vector3 spawnLocation = new Vector3(playerPosition.x, playerPosition.y - heightDifferenceConstant - distanceFromGround * DistanceMultiplier, 0f);

        //Camera is higher, spawn right on camera point
        if (cameraLowestPoint < spawnLocation.y)
            spawnLocation = new Vector3(playerPosition.x, cameraLowestPoint - GetComponent<Collider2D>().bounds.extents.y, 0f);

        if (GameManager.testing)
            Debug.Log("Extents: " + GetComponent<Collider2D>().bounds.extents.y);

        return spawnLocation;
    }

    IEnumerator StartCrumblingTimer()
    {
        yield return new WaitForSeconds(DeathTime);

        //Play animation

        //yield return new WaitForSeconds(crumblingTime);

        Destroy(this.gameObject);
    }

    //WasTriggerEnter2D
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Without this check, it stops at platforms.
        if (collision.gameObject.CompareTag("Platform") && collision.gameObject.GetComponent<FallingPlatform>() == null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);

            //Puts it at a small velocity, so it won't insta-stop and barely make a difference in-game (if grounded, it doesnt even sprawl out lmao!)
            commonRigidbody.velocity = new Vector2(0f, pillarDelayedVelocity);
        }
        //If activated boolean didnt exist, player could use it as an perma-jumper lmao
        else if (activated == false && collision.contacts[0].normal.y < -0.95f && (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<FallingPlatform>() != null) )
        {
            activated = true;

            if (GameManager.testing)
                Debug.Log("PILLARMEN!");

            //If it's a falling platform, dont last full seconds.
            if (collision.gameObject.CompareTag("Player") == false)
                DeathTime = DeathPlatformTime;
            else
            //Necessary, otherwise, it kicks player to the moon...
            {
                //collision.gameObject.GetComponent<WarriorMovement>().SetAtopGround(true);//cuz my ground collision sucks.
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
                

            //Start deathtimer
            StartCoroutine(StartCrumblingTimer());

            StartCoroutine(FallingVelocity(1f));
        }
    }

    IEnumerator FallingVelocity (float delay)
    {
        //Puts it at a small velocity, so it won't insta-stop and barely make a difference in-game (if grounded, it doesnt even sprawl out lmao!)
        commonRigidbody.velocity = new Vector2(0f, pillarDelayedVelocity);

        yield return new WaitForSeconds(delay);

        //Stop advancing
        commonRigidbody.velocity = new Vector2(0f, -50);
    }

    public void ExpirationDeath()
    {
        Destroy(this.gameObject);
    }
}
