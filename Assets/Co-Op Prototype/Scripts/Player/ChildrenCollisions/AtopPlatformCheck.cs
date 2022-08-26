using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//There is a bug I have to document for the future, when the layers become proper.
//Aka Walkable, Bouncable etc etc
//Level 3 platforms acting like Ground, trigger this without being at the very top of the platform.
//So you can walk from the middle of the platform instead of the very top. Negligible, bless the camera.

public class AtopPlatformCheck : MonoBehaviour
{
    private WarriorMovement warriorBehaviour;

    //A Dictionary of the (touching) GroundColliders
    private Dictionary<int, GameObject> currentPlatformCollisions = new Dictionary<int, GameObject>();

    [Header("Read Only")]
    public int currentCollisions = 0;//Read-Only for inspector, useless otherwise.

    private void Awake()
    {
        warriorBehaviour = GetComponentInParent<WarriorMovement>();
    }

    private void OnTriggerEnter2D(Collider2D platformToRegister)
    {
        //TODO: If (debugGround == true)
        //Debug.Log("Registering ground!");

        //warriorBehaviour.SetAtopGround(true);
        if (currentPlatformCollisions.ContainsKey(platformToRegister.gameObject.GetInstanceID()) == false)
            currentPlatformCollisions.Add(platformToRegister.gameObject.GetInstanceID(), platformToRegister.gameObject);



        warriorBehaviour.SetAtopPlatformCollisionDictionary(currentPlatformCollisions);

        currentCollisions = currentPlatformCollisions.Count;
    }

    private void OnTriggerExit2D(Collider2D platformToRegister)
    {
        //Remove the collided groundgameobject from the currentCollisions dictionary, so if its empty, this is definitely in the air.
        //If it is registered, remove it
        if (currentPlatformCollisions.ContainsKey(platformToRegister.gameObject.GetInstanceID()))
            currentPlatformCollisions.Remove(platformToRegister.gameObject.GetInstanceID());



        warriorBehaviour.SetAtopPlatformCollisionDictionary(currentPlatformCollisions);

        currentCollisions = currentPlatformCollisions.Count;
    }

    public void ClearCollisions()
    {
        currentPlatformCollisions.Clear();
    }

}
