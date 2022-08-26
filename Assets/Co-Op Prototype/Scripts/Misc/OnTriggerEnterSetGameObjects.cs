using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OnTriggerEnterSetGameObjects : MonoBehaviour
{
	public List<GameObject> gameObjectsToActivate = new List<GameObject>();

	public List<GameObject> gameObjectsToDeactivate = new List<GameObject>();

	private int i;

	public void OnTriggerEnter2D(Collider2D collisionDetected)
	{
		for (i = 0; i < gameObjectsToActivate.Count; i++)
			gameObjectsToActivate[i].SetActive(true);

		for (i = 0; i < gameObjectsToDeactivate.Count; i++)
			gameObjectsToDeactivate[i].SetActive(false);
	}
}
