using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//If you have the rare bug of player tipping right on the edge of the platform
//Consider changing the values of this gameobject to the following:
//GameObject.position.y -> -0.6
//ColliderY -> 3.76

/// <summary>
/// Copy-pasted from other similar ChildrenColliderCheck behaviours
/// Tbh, the dictionary and keys thing is not needed imo
/// since player will never be in 2 platforms, but whatever
///
/// It works properly, but there is a minor bug.
/// Each platform collision happens 2 times
/// This is weird. The collision is triggered 2 times in the same frame, instead of once.
/// This gameobject has only 1 trigger collider, and every platform has only 1 collider.
/// Anyhow, it's a truly negligible bug, but if it ends up not so
/// Having read the above, and with the below 2 debug commands, you should be able to solve it :D
/// </summary>
public class InsidePlatformCheck : MonoBehaviour
{

	private WarriorMovement warriorBehaviour;

	//A Dictionary of the (touching) GroundColliders
	private Dictionary<int, GameObject> currentPlatformCollisions = new Dictionary<int, GameObject>();

	[Header("Read Only")]
	public bool currentlyInsidePlatform = false;
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
		if (currentPlatformCollisions.ContainsKey(platformToRegister.GetInstanceID()) == false)
		{
			currentPlatformCollisions.Add(platformToRegister.GetInstanceID(), platformToRegister.gameObject);
			///Debug.Log("Name of collision: " + platformToRegister.gameObject.name);
		}


		if (currentlyInsidePlatform == false && currentPlatformCollisions.Count > 0)
		{
			currentlyInsidePlatform = true;

			warriorBehaviour.SetInsidePlatform(true);

			///DEBUG//While 2 collisions, this shows only 1 name (of the platform), only 1 time instead of 2...
			///foreach (var pickedDictionaryKey in currentPlatformCollisions.Keys)
				///Debug.Log("Key of collision: " + pickedDictionaryKey);
		}

		currentCollisions = currentPlatformCollisions.Count;
	}

	private void OnTriggerExit2D(Collider2D platformToRegister)
	{

		//Remove the collided groundgameobject from the currentCollisions dictionary, so if its empty, this is definitely in the air.
		//If it is registered, remove it
		if (currentPlatformCollisions.ContainsKey(platformToRegister.GetInstanceID()))
			currentPlatformCollisions.Remove(platformToRegister.GetInstanceID());

		//Empty list
		if (currentPlatformCollisions.Count == 0)
		{
			currentlyInsidePlatform = false;

			warriorBehaviour.SetInsidePlatform(false);
		}

		currentCollisions = currentPlatformCollisions.Count;
	}

	public void ClearCollisions()
	{
		currentPlatformCollisions.Clear();
	}
}
