using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Remake the whole ground detection, ffs. what a spagghetti, combining 2 ways into 1... so fucked up
public class GroundDetectionPlayer : MonoBehaviour
{
    private WarriorMovement warriorBehaviour;

    //A Dictionary of the (touching) GroundColliders
    private Dictionary<int, GameObject> currentGroundCollisions = new Dictionary<int, GameObject>();

    [Header("Read Only")]
    public bool currentlyGrounded = false;
    public int currentCollisions = 0;

    private void Awake()
    {
        warriorBehaviour = GetComponentInParent<WarriorMovement>();
    }

    private void OnTriggerEnter2D(Collider2D groundToRegister)
    {
        ///if (debugGround == true)
        ///Debug.Log("Registering ground!");

        //warriorBehaviour.SetAtopGround(true);
        if (currentGroundCollisions.ContainsKey(groundToRegister.GetInstanceID()) == false)
            currentGroundCollisions.Add(groundToRegister.GetInstanceID(), groundToRegister.gameObject);

        if (currentlyGrounded == false && currentGroundCollisions.Count > 0)
        {
            currentlyGrounded = true;

            warriorBehaviour.SetAtopGround(true);

            ///DEBUG
            ///foreach (var pickedDictionaryKey in currentGroundCollisions.Keys)
                ///Debug.Log("Name of collision: " + currentGroundCollisions[pickedDictionaryKey].name);
        }

        currentCollisions = currentGroundCollisions.Count;
    }

    private void OnTriggerExit2D(Collider2D groundToRegister)
    {
        ///if (debugGround == true)
        ///Debug.Log("Un-registering ground!");

        //Remove the collided groundgameobject from the currentCollisions dictionary, so if its empty, this is definitely in the air.
        //If it is registered, remove it
        if (currentGroundCollisions.ContainsKey(groundToRegister.GetInstanceID()))
            currentGroundCollisions.Remove(groundToRegister.GetInstanceID());

        //Empty list
        if (currentGroundCollisions.Count == 0)
        {
            currentlyGrounded = false;

            warriorBehaviour.SetAtopGround(false);
        }

        currentCollisions = currentGroundCollisions.Count;
    }

    public void ClearCollisions()
    {
        currentGroundCollisions.Clear();
    }
}
