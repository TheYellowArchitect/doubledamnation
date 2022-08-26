using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarBehaviour : MonoBehaviour
{
    [Tooltip("After hitting player, when does it fall?")]
    public float fallTime = 1;

    [Tooltip("After spawning, when does it die?")]
    public float deathTime = 2;

    [Tooltip("How fast should it fall?(speed)")]
    public float fallingSpeed;

    [Tooltip("When does it actually spawn the very frame summoned(it doesn't spawn right under the player, cuz collision ignore first)")]
    public int initSpawnDifference = 2048;

    [Tooltip("How far away is the player from the pillar, so as the pillar spawns right below his feet")]
    public float playerVerticalDifference;

    private Rigidbody2D commonRigidbody;

    private Vector3 playerPosition;
    private float positionY;

    // Use this for initialization
    void Start ()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();
        
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

        transform.position = new Vector3(playerPosition.x + initSpawnDifference, playerPosition.y + initSpawnDifference);
        transform.GetChild(0).transform.position = playerPosition;
        StartCoroutine(PlaceAfterCollision());

        Invoke("Fall", fallTime);
        Invoke("Death", deathTime);

        //VFX! (rising rocks/pebbles)

        //SFX!
    }


    public IEnumerator PlaceAfterCollision()
    {
        yield return new WaitForFixedUpdate();

        //tbh, instead of this, there should be "rising" for 0.15 seconds. Also helps in raising player if grounded, instead of disabling it like currently. After release I guess
        positionY = playerPosition.y - playerVerticalDifference;
        transform.position = new Vector3(playerPosition.x, positionY);
    }
	
    public void Fall()
    {
        commonRigidbody.velocity = new Vector2(0, -fallingSpeed);
    }

	public void Death()
    {
        Destroy(gameObject);
    }
}
