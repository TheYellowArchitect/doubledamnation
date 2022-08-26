using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeformTileManager : MonoBehaviour 
{
	[Header("Prefabs")]
	public List<GridItem> FreeformGridItems = new List<GridItem>();

	private MasterGridManager masterGrid;
	private GridItem tempGridItem;
	private GameObject tempTile;

	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();
	}

	public void SetFreeformItemPosition(GameObject pickedGameobject, float x, float y, float z)
	{
		//Set it on proper category (for debug clarity in the editor)
		if (pickedGameobject.transform.parent != masterGrid.freeformHolder)
			pickedGameobject.transform.SetParent(masterGrid.freeformHolder);

		//Set the position of the tilegriditem to be the same as mouse position
		pickedGameobject.transform.position = new Vector3(x, y, z);//0 so it displays and -1 so it displays over static grid tiles//changed a bit because warrior camera didnt like that

		//Cache new position so we know where the moved tile is
		int tempIndex = masterGrid.GetGridItemIndexFromList(pickedGameobject, ref FreeformGridItems);
		if (tempIndex != -1)
			FreeformGridItems[tempIndex].placementLocation[0] = new Vector2(x, y);

		//Update spikes attached to ground tile, if any
		if (FreeformGridItems[tempIndex].placementType == MasterGridManager.GROUND)
		{
			//Iterate the ground's children
			for (int i = 0; i < FreeformGridItems[tempIndex].gameobject.transform.childCount; i++)
			{
				//Iterate all freeform tiles, so as to find only spikes, and cross-match with i^
				for (int j = 0; j < FreeformGridItems.Count; j++)
				{
					//If spike, and the same gameobject as child of groundtile
					if (FreeformGridItems[j].placementType == MasterGridManager.HAZARD && FreeformGridItems[j].gameobject.transform == FreeformGridItems[tempIndex].gameobject.transform.GetChild(i))
						FreeformGridItems[j].placementLocation[0] = new Vector2(FreeformGridItems[j].gameobject.transform.position.x, FreeformGridItems[j].gameobject.transform.position.y);

				}
			}
		}
	}

	public GameObject DetermineFreeform(Collider2D pickedCollider)
	{
		//If decouple spikes and convert them
		if (pickedCollider.gameObject.layer == LayerMask.NameToLayer("Hazard") && masterGrid.GetGridItemIndexFromList(pickedCollider.gameObject, ref FreeformGridItems) != -1)
			return masterGrid.hazardTileManager.ConvertToSingleMidairFreeform(pickedCollider.gameObject);
			

		//If already freeform
		if (masterGrid.GetGridItemIndexFromList(pickedCollider.gameObject, ref FreeformGridItems) != -1)
			return pickedCollider.gameObject;

		if (pickedCollider.gameObject.CompareTag("Ground"))
		{
			List<int> gridIndexValues = masterGrid.hazardTileManager.AttachAdjacentStaticHazardsForFreeform(pickedCollider.gameObject);

			tempTile = ConvertStaticToFreeform(pickedCollider.gameObject, ref masterGrid.groundTileManager.StaticGridGroundItems);

			masterGrid.hazardTileManager.UpdateHazardAdjacency(gridIndexValues.ToArray());
		}
		else if (pickedCollider.gameObject.CompareTag("Platform"))
			tempTile = ConvertStaticToFreeform(pickedCollider.gameObject, ref masterGrid.platformTileManager.StaticGridPlatformItems);
		else if (pickedCollider.gameObject.CompareTag("Enemy"))
			tempTile = ConvertStaticToFreeform(pickedCollider.gameObject, ref masterGrid.enemyTileManager.StaticGridEnemyItems);
		else if (pickedCollider.gameObject.layer == LayerMask.NameToLayer("Hazard"))
		{
			tempTile = masterGrid.hazardTileManager.ConvertToSingleMidairStatic(pickedCollider.gameObject);			

			tempTile = ConvertStaticToFreeform(tempTile, ref masterGrid.hazardTileManager.StaticGridHazardItems);
		}
			


		return tempTile;
	}

	public GameObject ConvertStaticToFreeform(GameObject convertedGameObject, ref List<GridItem> GridListItems)
	{
		int tempIndex = masterGrid.GetGridItemIndexFromList(convertedGameObject, ref GridListItems);

		//If in static grid, convert it!
		if (tempIndex != -1)
		{
			tempGridItem = GridListItems[tempIndex];
			FreeformGridItems.Add(tempGridItem);
			masterGrid.RemoveGridItemFromList(tempIndex, ref GridListItems, false);

			//Reset location
			for (int i = FreeformGridItems[FreeformGridItems.Count - 1].placementLocation.Count - 1; i > -1; i--)
				FreeformGridItems[FreeformGridItems.Count - 1].placementLocation.RemoveAt(i);

			//Add latest location
			FreeformGridItems[FreeformGridItems.Count - 1].placementLocation.Add(new Vector2(tempGridItem.gameobject.transform.position.x, tempGridItem.gameobject.transform.position.y));

			//Cache the latest clicked left object for further usage on movement and the like
			return tempGridItem.gameobject;
		}
		
		//Cache the latest clicked left object for further usage on movement and the like
		return convertedGameObject;//Is this not the same as above return?
	}
}
