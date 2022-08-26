using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetectionEnemy : MonoBehaviour
{
    private EnemyBehaviour enemyBehaviour;

    //A Dictionary of the (touching) GroundColliders
    private Dictionary<int, GameObject> currentGroundCollisions = new Dictionary<int, GameObject>();

    // Use this for initialization
    void Awake ()
    {
        enemyBehaviour = GetComponentInParent<EnemyBehaviour>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Raycast downwards? Not needed since by making the ground collision a little smaller on the sides means it will never touch the sides (and obv never the top.)

        enemyBehaviour.SetIsGrounded(true);

        //Add the colided ground gameobject to the currentCollisions dictionary, so when it exits, the identification will be ez
        //If the collision isn't registered before, add it in the dictionary
        if (currentGroundCollisions.ContainsKey(collision.gameObject.GetInstanceID()) == false)
            currentGroundCollisions.Add(collision.gameObject.GetInstanceID(), collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Remove the collided groundgameobject from the currentCollisions dictionary, so if its empty, this is definitely in the air.
        //If it is registered, remove it
        if (currentGroundCollisions.ContainsKey(collision.gameObject.GetInstanceID()))
            currentGroundCollisions.Remove(collision.gameObject.GetInstanceID());

        //Empty list
        if (currentGroundCollisions.Count == 0)
            enemyBehaviour.SetIsGrounded(false);
    }
    
    private void OnDisable()
    {
        //Reset Ground Collisions
        currentGroundCollisions.Clear();

        //Not grounded!
        enemyBehaviour.SetIsGrounded(false);
    }
    
}
