using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceExitTileManager : MonoBehaviour 
{
	[Header("Prefabs")]
	public GameObject entranceLevel1;
	public GameObject exitLevel1;//This is prefab of the above but without TriggerLevelCutscene() so as to loop ;)
	public GameObject brazierLevel1;

	[Header("Misc")]
	public bool brazierExists = false;
	public Vector2 brazierOffsetFromEntrance = Vector2.zero;
	[Tooltip("When we get brazier to the right, gotta check if there is a ground to place it atop. This checks from vertical level of entrance, downwards how much downwards must there be ground?")]
	public float brazierVerticalRaycastDistance = 20f;
	[Tooltip("What is considered a Ground Layer?\nSo the raycasting from enemy to player will see if obstructed or not")]
    public LayerMask WhatIsGround;
	[Tooltip("Once placed on the ground, how much distance is the center of the brazier from its legs?")]
	public float offsetGroundFromCenter = 3.75f;
	[Header("Legs Offset Edges")]
	public float leftEdgesOffsetFromCenterX = 1f;
	public float leftEdgesOffsetFromCenterY = -2f;
	public float rightEdgesOffsetFromCenterX = 1f;
	public float rightEdgesOffsetFromCenterY = -2f;
	

	private MasterGridManager masterGrid;
	private GameObject checkpoint;
	private GridItem tempGridItem;
	private GameObject tempTile;

	private Vector3 entrancePosition = Vector3.zero;
	private RaycastHit2D hit;
	private bool leftLegGrounded = false;
	private bool rightLegGrounded = false;

	private GameObject darkwindBrazier;

	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();

		checkpoint = GameObject.FindGameObjectWithTag("Checkpoint");
	}

	//In mastergrid, after grid is initialized, mastergrid calls this
	public void CreateInitialEntrance()
	{
		//Three 8x4 tiles, to house both entrance and brazier
			masterGrid.groundTileManager.CreateStaticGround(20, 20, 21, 20);
			masterGrid.groundTileManager.CreateStaticGround(22, 20, 23, 20);
			masterGrid.groundTileManager.CreateStaticGround(24, 20, 25, 20);

		//Create the entrance
			CreateEntrance(110.95f, 112.21f, false);

		//Create the brazier
		DetermineBrazier();
		//Optimally ^ would be automatic instead of user dragging it, and would have to link it to a ground tile, and whenever I delete a ground tile, check if darkwind brazier is linked to it. A real problem occurs when merging ground tiles!
	}

	//Invoked from CreateInitialEntrance()
	public void DetermineBrazier()
	{
		GameObject[] entranceExits = GameObject.FindGameObjectsWithTag("Cutscene");
		for (int i = 0; i < entranceExits.Length; i++)
		{
			if (entranceExits[i].name.Contains("Entrance"))
			{
				entrancePosition = entranceExits[i].transform.position;
				i = entranceExits.Length;
			}
		}

		Debug.Log("Entrance Position is: " + entrancePosition);

		//Now that we got the position of the entrance, we got to do the offset.
		entrancePosition = entrancePosition + new Vector3(brazierOffsetFromEntrance.x, brazierOffsetFromEntrance.y, 0);

		Vector2 entrancePosition2D = new Vector2(entrancePosition.x, entrancePosition.y);

		leftLegGrounded = false;
		rightLegGrounded = false;

		//Goes at position of entrance, then does 2 vertical raycasts. If both hit ground and on same Y pretty much, then its placed there.
        hit = Physics2D.Raycast(entrancePosition2D + new Vector2(leftEdgesOffsetFromCenterX, leftEdgesOffsetFromCenterY), Vector2.down, brazierVerticalRaycastDistance, WhatIsGround);
        if (hit.collider != null)
		{
            Debug.Log("Left one hit!");
			leftLegGrounded = true;
		}

		Debug.DrawRay(entrancePosition2D + new Vector2(leftEdgesOffsetFromCenterX, leftEdgesOffsetFromCenterY), new Vector2(0, -brazierVerticalRaycastDistance), Color.red, 10f);

		hit = Physics2D.Raycast(entrancePosition2D + new Vector2(rightEdgesOffsetFromCenterX, rightEdgesOffsetFromCenterY), Vector2.down, brazierVerticalRaycastDistance, WhatIsGround);
        if (hit.collider != null)
       	{
            Debug.Log("Right one hit!");
			rightLegGrounded = true;
		}

		Debug.DrawRay(entrancePosition2D + new Vector2(rightEdgesOffsetFromCenterX, rightEdgesOffsetFromCenterY), new Vector2(0, -brazierVerticalRaycastDistance), Color.red, 10f);

		if (leftLegGrounded && rightLegGrounded)
		{
			brazierExists = true;
			CreateBrazier(entrancePosition + new Vector3(0, offsetGroundFromCenter + rightEdgesOffsetFromCenterY - hit.distance, 0));
			Debug.Log("hit.distance = " + hit.distance);
		}
	}

	public void CreateBrazier(Vector3 spawnPosition)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(spawnPosition.x, spawnPosition.y));
		tempGridItem.placementType = MasterGridManager.BRAZIER;

		tempTile = Object.Instantiate(brazierLevel1, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);

		tempGridItem.gameobject = tempTile;

		tempTile.transform.SetParent(masterGrid.freeformHolder);

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);
	}

	public void CreateEntrance(float x, float y, bool disableCutscene = true)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(x, y));
		tempGridItem.placementType = MasterGridManager.ENTRANCE;

		tempTile = Object.Instantiate(entranceLevel1, new Vector3(x, y, 0), Quaternion.identity);

		tempGridItem.gameobject = tempTile;

		tempTile.transform.SetParent(masterGrid.freeformHolder);

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);

		//TODO: If this doesn't work, then make the prefab without this class (see CreateExit), and it is added rightafter.
		if (disableCutscene)
			//Destroy(tempGridItem.gameobject.GetComponent<TriggerLevelCutscene>());
			tempGridItem.gameobject.GetComponent<TriggerLevelCutscene>().triggeredOnce = true;
	}

	public void CreateExit(float x, float y)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(x, y));
		tempGridItem.placementType = MasterGridManager.EXIT;

		tempTile = Object.Instantiate(exitLevel1, new Vector3(x, y, 0), Quaternion.identity);

		tempGridItem.gameobject = tempTile;

		tempTile.transform.SetParent(masterGrid.freeformHolder);

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);

		//TODO: It doesnt have triggerlevelcutscene class. But have a class which does similar things, e.g. fadein/fadeout and loop at beginning and puts you on edit mode.
	}

	public void ResetExits()
	{
		GameObject[] entrances = GameObject.FindGameObjectsWithTag("Cutscene");
		for (int i = 0; i < entrances.Length; i++)
		{
			if (entrances[i].GetComponent<TriggerLevelClear>() != null)
				entrances[i].GetComponent<TriggerLevelClear>().triggeredOnce = false;
		}
	}

	public void DetermineCheckpointMovementToEntrance(GameObject possibleEntrance)
	{
		Debug.Log("Determination time!");

		//If Entrance, move checkpoint there
		if (possibleEntrance != null && possibleEntrance.layer == LayerMask.NameToLayer("PlayerExclusively") && possibleEntrance.GetComponent<TriggerLevelCutscene>() != null)
		{
			Debug.Log("is checkpoint null? " + checkpoint.transform.gameObject.name);
			//Could cache them, but since they are used just here, might as well search for these gameobjects.
			checkpoint.transform.position = new Vector3(possibleEntrance.transform.position.x, possibleEntrance.transform.position.y - 2.74f, 0);//2.74f is the offset of entrance to checkpoint, for both touching the ground
			GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().UpdateCheckpointPosition(checkpoint.transform.position);
		}
	}
}
