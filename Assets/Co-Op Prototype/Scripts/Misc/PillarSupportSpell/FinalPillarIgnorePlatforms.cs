using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPillarIgnorePlatforms : MonoBehaviour
{

	[Tooltip("Pillar's Colliders (ground layer), not this trigger's")]
	public List<BoxCollider2D> pillarColliders;

	//Triggered by FinalPillarBehaviour
	public void SetColliders(List<BoxCollider2D> referencedPillarColliders)
	{
		pillarColliders = referencedPillarColliders;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		for (int i = 0; i < pillarColliders.Count; i++)
			//So it won't stop on platforms.
			if (collision.gameObject.CompareTag("Platform"))
				Physics2D.IgnoreCollision(pillarColliders[i], collision);
	}
}
