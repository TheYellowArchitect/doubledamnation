using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//DEPRECATED!
//See: WallSlideCheck and WallSlideNotify

/// <summary>
/// This is exclusively made for getting walls on the ground. Hopefully in the future it helps for all walls 
/// because holy fuck getting colliders every frame to get if a wall collision is a thing, is fucking spagghetti.
/// </summary>
public class WallDetectionPlayer : MonoBehaviour
{
    private WarriorMovement warriorBehaviour;

    //A Dictionary of the (touching) GroundColliders
    private Dictionary<int, GameObject> currentWallCollisions = new Dictionary<int, GameObject>();

    [Header("Read Only")]
    public bool currentlySideWalled = false;
    public int currentCollisions = 0;

    private void Awake()
    {
        warriorBehaviour = GetComponentInParent<WarriorMovement>();
    }

    private void OnTriggerEnter2D(Collider2D wallToRegister)
    {
        //TODO: If (debugGround == true)
        //Debug.Log("Registering wall!");

        //warriorBehaviour.SetAtopGround(true);
        if (currentWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
            currentWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

        if (currentlySideWalled == false && currentWallCollisions.Count > 0)
        {
            currentlySideWalled = true;

            //warriorBehaviour.SetSideWalled(true);
        }

        currentCollisions = currentWallCollisions.Count;
    }

    private void OnTriggerExit2D(Collider2D groundToRegister)
    {
        //TODO: If (debugGround == true)
        //Debug.Log("Un-registering wall!");

        //Remove the collided groundgameobject from the currentCollisions dictionary, so if its empty, this is definitely in the air.
        //If it is registered, remove it
        if (currentWallCollisions.ContainsKey(groundToRegister.GetInstanceID()))
            currentWallCollisions.Remove(groundToRegister.GetInstanceID());

        //Empty list
        if (currentWallCollisions.Count == 0)
        {
            currentlySideWalled = false;

            //warriorBehaviour.SetSideWalled(false);
        }

        currentCollisions = currentWallCollisions.Count;
    }

    public void ClearCollisions()
    {
        currentWallCollisions.Clear();
    }
}
