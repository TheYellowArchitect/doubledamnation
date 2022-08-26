using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkFireballDesyncSphere: MonoBehaviour 
{
	public float warriorX;
	public float distanceX;
	
	[Header("New Fireball Values")]
	public Vector2 targetFacing;
	public Vector3 targetRotation;
	public Vector3 targetOffsetSpawnPoint;
	public GameObject tempGameObject;

	public void OnTriggerEnter2D(Collider2D triggeredCollider)
	{
		//Edge-cases, both of these.
		if (NetworkCommunicationController.globalInstance == null || NetworkDamageShare.globalInstance.IsSynchronized())
			return;

		//If DesyncSphere
		if (triggeredCollider.gameObject.CompareTag("DesyncSphere"))
		{
			warriorX = GameObject.FindGameObjectWithTag("Player").transform.position.x;
			distanceX = warriorX - triggeredCollider.transform.position.x;

			//Cannot have negative distance, just flip it lol
			if (distanceX < 0)
				distanceX = distanceX * -1;

			Debug.Log("Distance is: " + distanceX);

			//If hugging desync sphere aka in the same spot, ofc it will insta-sync each time you press "fire" -_-
			if (distanceX > 4.8f)
			{
				GameObject.FindGameObjectWithTag("Player").transform.position = triggeredCollider.gameObject.transform.position;

            	NetworkSynchronizer.globalInstance.MageSynchronizeLink();
			}
			else//cast another fireball (since it is consumed -_-)
			{

				//if (warriorX > triggeredCollider.transform.position.x)
				if (GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().GetFacingLeft())
				{
					targetFacing = Vector2.left;
					targetRotation = new Vector3(0, 0, 180);
					targetOffsetSpawnPoint = Vector3.zero;
				}
				else
				{
					targetFacing = Vector2.right;
					targetRotation = Vector3.zero;
					targetOffsetSpawnPoint = Vector3.zero;
				}
					
				GameObject.FindGameObjectWithTag("Mage").GetComponent<SpellFire>().SetFireballRotationValues(targetFacing, targetRotation, targetOffsetSpawnPoint);
				tempGameObject = GameObject.FindGameObjectWithTag("Mage").GetComponent<SpellFire>().CreateFireball(triggeredCollider.transform.position, false);

				//With this, you dont even need to offset lol
				Physics2D.IgnoreCollision(tempGameObject.GetComponent<Collider2D>(), triggeredCollider);
			}
			
		}
	}
}
