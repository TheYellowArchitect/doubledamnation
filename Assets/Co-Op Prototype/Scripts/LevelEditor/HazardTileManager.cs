using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardTileManager : MonoBehaviour 
{
	[Header("Prefabs")]
	public GameObject spikeMidairLevel1;
	public GameObject spikeGroundLevel1;

	public List<GridItem> StaticGridHazardItems = new List<GridItem>();
	[Tooltip("The amount of distance needed from the center of a tile, to reach the ground aka be implemented/integrated onto the ground\nSo it doesn't float or look bad because spikes after all must be attached to the ground")]
	public float groundPlacementOffset = 1.6f;

	private MasterGridManager masterGrid;
	private GridItem tempGridItem;
	private GameObject tempTile;

	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();
	}

	public void CreateStaticHazard(int Xindex, int Yindex)
	{
		bool isAdjacentGroundUp = false;
		bool isAdjacentGroundDown = false;
		bool isAdjacentGroundRight = false;
		bool isAdjacentGroundLeft = false;

		//Vertical Check
		if (Yindex > 0 && masterGrid.DefaultGridOccupied[Xindex, Yindex - 1] == true)
			if (masterGrid.GetGridItemFromList(Xindex, Yindex - 1, ref masterGrid.groundTileManager.StaticGridGroundItems).placementType == MasterGridManager.GROUND)
				isAdjacentGroundDown = true;
		if (Yindex < masterGrid.maxTilesSpawned - 1 && masterGrid.DefaultGridOccupied[Xindex, Yindex + 1] == true)
			if (masterGrid.GetGridItemFromList(Xindex, Yindex + 1, ref masterGrid.groundTileManager.StaticGridGroundItems).placementType == MasterGridManager.GROUND)
				isAdjacentGroundUp = true;
		
		//Horizontal check
		if (Xindex > 0 && masterGrid.DefaultGridOccupied[Xindex - 1, Yindex] == true)
			if (masterGrid.GetGridItemFromList(Xindex - 1, Yindex, ref masterGrid.groundTileManager.StaticGridGroundItems).placementType == MasterGridManager.GROUND)
				isAdjacentGroundLeft = true;
		
		if (Xindex < masterGrid.maxTilesSpawned - 1 && masterGrid.DefaultGridOccupied[Xindex + 1, Yindex] == true)
			if (masterGrid.GetGridItemFromList(Xindex + 1, Yindex, ref masterGrid.groundTileManager.StaticGridGroundItems).placementType == MasterGridManager.GROUND)
				isAdjacentGroundRight = true;

		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(Xindex, Yindex));
		tempGridItem.placementType = MasterGridManager.HAZARD;

		

			

		//If midair spike (nothing adjacent, or only left and right)
		if ((isAdjacentGroundUp == false && isAdjacentGroundDown == false && isAdjacentGroundRight == false && isAdjacentGroundLeft == false)
		|| (isAdjacentGroundUp == false && isAdjacentGroundDown == false && isAdjacentGroundRight == true && isAdjacentGroundLeft == true))
			tempTile = Object.Instantiate(spikeMidairLevel1, masterGrid.GetGridTileLocation(Xindex, Yindex), Quaternion.identity);

		//If something vertical
		else if (isAdjacentGroundUp == true || isAdjacentGroundDown == true)
		{
			tempTile = Object.Instantiate(spikeGroundLevel1, masterGrid.GetGridTileLocation(Xindex, Yindex) + new Vector3(0,-groundPlacementOffset,0), Quaternion.identity);

			//If nothing below, but only above, stick it onto the above
			if (isAdjacentGroundUp == true && isAdjacentGroundDown == false)
			{
				tempTile.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
				tempTile.transform.position = tempTile.transform.position + new Vector3(0, groundPlacementOffset * 2, 0);
			}
				
		}

		//If something horizontal (without any vertical!)
		else//doesnt need check, always gets here on exclusively horizontal.
		{
			tempTile = Object.Instantiate(spikeGroundLevel1, masterGrid.GetGridTileLocation(Xindex, Yindex) + new Vector3(groundPlacementOffset,0,0), Quaternion.Euler(0, 0, 90f));

			if (isAdjacentGroundLeft)
			{
				tempTile.transform.rotation = Quaternion.Euler(0,0, 270f);
				tempTile.transform.position = tempTile.transform.position + new Vector3(-groundPlacementOffset * 2, 0, 0);
			}
		}

		tempGridItem.gameobject = tempTile;

		tempTile.transform.SetParent(masterGrid.staticHolder);

		StaticGridHazardItems.Add(tempGridItem);

		masterGrid.DefaultGridOccupied[Xindex, Yindex] = true;

		Debug.Log("Adjacency: Up = " + isAdjacentGroundUp + "! Down = " + isAdjacentGroundDown + "! Right = " + isAdjacentGroundRight + "! Left = " + isAdjacentGroundLeft);
	}

	//Called by GroundTileManager.cs and InputTileManager.cs (and one more?)
	public void UpdateHazardAdjacency(params int[] gridIndexValues)//gridIndexValues is the .placementLocation of groundTiles. E.g. for 4x4 it is just [0][1], for 8x8, it is [0][1]|[2][3]|[4][5]|[6][7]
	{
		//Iterate **each point** and find if there is a hazard nearby
		for (int i = 0; i < gridIndexValues.Length; i = i + 2)
		{
			//There is a hazard somewhere adjacently
			if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, gridIndexValues[i] + 1, gridIndexValues[i + 1], gridIndexValues[i] - 1, gridIndexValues[i + 1], gridIndexValues[i], gridIndexValues[i + 1] + 1, gridIndexValues[i], gridIndexValues[i + 1] - 1))
			{
				if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, gridIndexValues[i], gridIndexValues[i + 1] + 1))
					DetermineGroundTileAdjacency(masterGrid.GetGridItemFromList(gridIndexValues[i], gridIndexValues[i + 1] + 1, ref StaticGridHazardItems).gameobject);

				if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, gridIndexValues[i], gridIndexValues[i + 1] - 1))
					DetermineGroundTileAdjacency(masterGrid.GetGridItemFromList(gridIndexValues[i], gridIndexValues[i + 1] - 1, ref StaticGridHazardItems).gameobject);
					
				if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, gridIndexValues[i] + 1, gridIndexValues[i + 1]))
					DetermineGroundTileAdjacency(masterGrid.GetGridItemFromList(gridIndexValues[i] + 1, gridIndexValues[i + 1], ref StaticGridHazardItems).gameobject);

				if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, gridIndexValues[i] - 1, gridIndexValues[i + 1]))
					DetermineGroundTileAdjacency(masterGrid.GetGridItemFromList(gridIndexValues[i] - 1, gridIndexValues[i + 1], ref StaticGridHazardItems).gameobject);
			}

		}
			
	}

	public void DetermineGroundTileAdjacency(GameObject adjacentHazard)
	{
		int hazardIndex = masterGrid.GetGridItemIndexFromList(adjacentHazard, ref StaticGridHazardItems);

		int hazardIndexX = (int) StaticGridHazardItems[hazardIndex].placementLocation[0].x;
		int hazardIndexY = (int) StaticGridHazardItems[hazardIndex].placementLocation[0].y;

		//Debug.Log("We are here. hazardIndexX,y = " + hazardIndexX + " " + hazardIndexY);

		masterGrid.RemoveGridItemFromList(hazardIndex, ref StaticGridHazardItems);

		CreateStaticHazard(hazardIndexX, hazardIndexY);
	}

	public GameObject ConvertToSingleMidairStatic(GameObject pickedGameobject)
	{
		//Get hazard location of current tile
		Vector3 spawnLocation = pickedGameobject.transform.position;

		int hazardIndex = masterGrid.GetGridItemIndexFromList(pickedGameobject, ref StaticGridHazardItems);

		if (hazardIndex == -1)
			return null;

		//Sucks that I do string comparison, aka easy to bug, but whatever.
		//Anyway, if it gets here, its already converted so just return;
		if (pickedGameobject.name.Contains("Single"))
			return pickedGameobject;

		//Destroy the gameobject and replace it with the new gameobject, all on the same griditem!
		Destroy(pickedGameobject);
		
		//Create new gameobject
		tempTile = Object.Instantiate(spikeMidairLevel1, spawnLocation, Quaternion.identity);

		//Now we do the below codeline with bloat instead because the compiler doesn't allow re-assigning onto a List<struct> smh.
		//StaticGridHazardItems[hazardIndex].gameobject = tempTile;
			tempGridItem = new GridItem();
			tempGridItem.gameobject = tempTile;
			tempGridItem.placementType = StaticGridHazardItems[hazardIndex].placementType;
			tempGridItem.placementLocation = StaticGridHazardItems[hazardIndex].placementLocation;
			StaticGridHazardItems.RemoveAt(hazardIndex);
			StaticGridHazardItems.Add(tempGridItem);

		return tempGridItem.gameobject;
	}

	public GameObject ConvertToSingleMidairFreeform(GameObject pickedGameobject)
	{
		//Get hazard location of current tile
		Vector3 spawnLocation = pickedGameobject.transform.position;

		int hazardIndex = masterGrid.GetGridItemIndexFromList(pickedGameobject, ref masterGrid.freeformTileManager.FreeformGridItems);

		if (hazardIndex == -1)
			return null;

		//Sucks that I do string comparison, aka easy to bug, but whatever.
		//Anyway, if it gets here, its already converted so just return;
		if (pickedGameobject.name.Contains("Single"))
			return pickedGameobject;

		//Destroy the gameobject and replace it with the new gameobject, all on the same griditem!
		Destroy(pickedGameobject);
		
		//Create new gameobject
		tempTile = Object.Instantiate(spikeMidairLevel1, spawnLocation, Quaternion.identity);

		tempTile.transform.SetParent(masterGrid.freeformHolder);

		//Now we do the below codeline with bloat instead because the compiler doesn't allow re-assigning onto a List<struct> smh.
		//masterGrid.freeformTileManager.FreeformGridItems[hazardIndex].gameobject = tempTile;
			tempGridItem = new GridItem();
			tempGridItem.gameobject = tempTile;
			tempGridItem.placementType = masterGrid.freeformTileManager.FreeformGridItems[hazardIndex].placementType;
			tempGridItem.placementLocation = masterGrid.freeformTileManager.FreeformGridItems[hazardIndex].placementLocation;
			masterGrid.freeformTileManager.FreeformGridItems.RemoveAt(hazardIndex);
			masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);

		masterGrid.SwapIndexesFromList(masterGrid.freeformTileManager.FreeformGridItems.Count - 1, hazardIndex, ref masterGrid.freeformTileManager.FreeformGridItems);

		return tempGridItem.gameobject;
	}

	public List<int> AttachAdjacentStaticHazardsForFreeform(GameObject groundGameObject)
	{
		//Iterate through this tile's grid locations it occupies (e.g. 1 tile has only 2 locations, a horizontal has 4, a double has 8 locations)
		//And search for adjacent up down left right (cross pattern) for any hazard
		List<int> gridIndexValues = new List<int>();
		GridItem pickedHazardItem;
		GridItem groundItem = masterGrid.GetGridItemFromList(groundGameObject, ref masterGrid.groundTileManager.StaticGridGroundItems);
		for (int i = 0; i < groundItem.placementLocation.Count; i++)
		{
			if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, (int) groundItem.placementLocation[i].x + 1, (int) groundItem.placementLocation[i].y))
			{
				pickedHazardItem = masterGrid.GetGridItemFromList((int) groundItem.placementLocation[i].x + 1, (int) groundItem.placementLocation[i].y, ref StaticGridHazardItems);
				
				//If not edge case scenario where there is another tile to the other side and hence, its midair spike and shouldnt connect/fly
				if (pickedHazardItem.gameobject.name.Contains("Single") == false && pickedHazardItem.gameobject.transform.rotation.z != 0 && pickedHazardItem.gameobject.transform.rotation.z != 180)
				{
					tempTile = masterGrid.freeformTileManager.ConvertStaticToFreeform(pickedHazardItem.gameobject, ref StaticGridHazardItems);
					tempTile.transform.SetParent(groundGameObject.transform);
					i = 0;
				}
				
			}
			if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, (int) groundItem.placementLocation[i].x - 1, (int) groundItem.placementLocation[i].y))
			{
				pickedHazardItem = masterGrid.GetGridItemFromList((int) groundItem.placementLocation[i].x - 1, (int) groundItem.placementLocation[i].y, ref StaticGridHazardItems);
				
				//If not edge case scenario where there is another tile to the other side and hence, its midair spike and shouldnt connect/fly
				if (pickedHazardItem.gameobject.name.Contains("Single") == false && pickedHazardItem.gameobject.transform.rotation.z != 0 && pickedHazardItem.gameobject.transform.rotation.z != 180)
				{
					tempTile = masterGrid.freeformTileManager.ConvertStaticToFreeform(pickedHazardItem.gameobject, ref StaticGridHazardItems);
					tempTile.transform.SetParent(groundGameObject.transform);
					i = 0;
				}
			}
			if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, (int) groundItem.placementLocation[i].x, (int) groundItem.placementLocation[i].y + 1))
			{
				pickedHazardItem = masterGrid.GetGridItemFromList((int) groundItem.placementLocation[i].x, (int) groundItem.placementLocation[i].y + 1, ref StaticGridHazardItems);
				
				//If not edge case scenario where there is another tile to the other side and hence, its midair spike and shouldnt connect/fly
				if (pickedHazardItem.gameobject.name.Contains("Single") == false)
				{
					tempTile = masterGrid.freeformTileManager.ConvertStaticToFreeform(pickedHazardItem.gameobject, ref StaticGridHazardItems);
					tempTile.transform.SetParent(groundGameObject.transform);
					i = 0;
				}
			}
			if (masterGrid.DoesTileTypeOccupyIndexesFromList(MasterGridManager.HAZARD, ref StaticGridHazardItems, (int) groundItem.placementLocation[i].x, (int) groundItem.placementLocation[i].y - 1))
			{
				pickedHazardItem = masterGrid.GetGridItemFromList((int) groundItem.placementLocation[i].x, (int) groundItem.placementLocation[i].y - 1, ref StaticGridHazardItems);
				
				//If not edge case scenario where there is another tile to the other side and hence, its midair spike and shouldnt connect/fly
				if (pickedHazardItem.gameobject.name.Contains("Single") == false)
				{
					//If its attached to the ground below, do not pull it and attach it!
					if (pickedHazardItem.gameobject.transform.rotation.z != 0)
					{
						tempTile = masterGrid.freeformTileManager.ConvertStaticToFreeform(pickedHazardItem.gameobject, ref StaticGridHazardItems);
						tempTile.transform.SetParent(groundGameObject.transform);
						i = 0;
					}
				}
			}

			gridIndexValues.Add((int)groundItem.placementLocation[i].x);
			gridIndexValues.Add((int)groundItem.placementLocation[i].y);
		}

		return gridIndexValues;
	}

	public bool CreateFreeformHazard(float X, float Y, bool attachedToGround)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(X, Y));
		tempGridItem.placementType = MasterGridManager.HAZARD;

		if (attachedToGround == false)
		{
			tempTile = Object.Instantiate(spikeMidairLevel1, new Vector3(X, Y, 0), Quaternion.identity);

			tempTile.transform.SetParent(masterGrid.freeformHolder);	
		}

		/*Adjacency Numbers
			//Single Horizontal/Vertical Difference: 3.52 (so, <3.53 distance difference)
			//Vertical/Horizontal Difference: 6.081
			//Double Difference: 6.081 if top-right top-left bottom-right bottom-left (aka horizontally placed)
			^6.080 for Y if vertically placed lol. So for double, just check if 6.081 lol
			The other axis difference must be 2.56*/

		//Iterate all freeform ground tiles, and essentially do the same check as staticcreation to find what direction
		else//Attached to Ground
		{
			bool foundAttachment = false;

			tempTile = Object.Instantiate(spikeGroundLevel1, new Vector3(X, Y, 0), Quaternion.identity);

			//Iterate all freeform ground tiles
			for (int i = 0; i < masterGrid.freeformTileManager.FreeformGridItems.Count; i++)
			{
				if (masterGrid.freeformTileManager.FreeformGridItems[i].placementType == MasterGridManager.GROUND)
				{
					if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.name.Contains("4x4"))
					{
						if ((DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x, X, 3.51f, 3.53f)  )
						|| (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y, Y, 3.51f, 3.53f)  ))
						{
							foundAttachment = true;

							//Check adjacency
							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x == X)
							{
								if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y > Y)
									tempTile.transform.rotation = Quaternion.Euler(0, 0, 180f);
							}
							else
							{
								if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x > X)
									tempTile.transform.rotation = Quaternion.Euler(0, 0, 90f);
								else
									tempTile.transform.rotation = Quaternion.Euler(0, 0, 270f);
							}

						}
					}
					else if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.name.Contains("4x8"))
					{
						//Vertical up or down
						if (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y, Y, 6.079f, 6.082f))
						{
							foundAttachment = true;

							//Check adjacency
							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y > Y)
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 180f);
						}
						
						//Horizontal left or right
						else if (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x, X, 3.51f, 3.53f))
						{
							foundAttachment = true;

							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x > X)
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 90f);
							else
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 270f);
						}

					}
					else if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.name.Contains("8x4"))
					{
						//Horizontal left or right
						if (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x, X, 6.079f, 6.082f))
						{
							foundAttachment = true;

							//Check adjacency
							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x > X)
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 90f);
							else
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 270f);
						}
						
						//Vertical up or down
						else if (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y, Y, 3.51f, 3.53f))
						{
							foundAttachment = true;

							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y > Y)
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 180f);
						}

					}
					else if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.name.Contains("8x8"))
					{
						//Horizontally placed
						if (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x, X, 6.079f, 6.082f)  )
						{
							foundAttachment = true;

							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.x > X)
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 90f);
							else
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 270f);
						}
						else if (DifferenceBetweenFloatsIsBetween(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y, Y, 6.079f, 6.082f)  )
						{
							foundAttachment = true;

							if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.position.y > Y)
								tempTile.transform.rotation = Quaternion.Euler(0, 0, 180f);
						}
					}

				}

				if (foundAttachment)
				{
					tempTile.transform.SetParent(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform);

					break;
				}
			}
		}

		tempGridItem.gameobject = tempTile;

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);

		return false;//false = single midair
	}

	//This should become a utility function
	public bool DifferenceBetweenFloatsIsBetween(float x1, float x2, float minValue, float maxValue)
	{
		float Difference = x1 - x2;
		if (Difference < 0)
			Difference = Difference * -1;

		//Now we have the absolute difference^ aka |x| and |y|
		//Let's go for comparison of values

		if (Difference > minValue && Difference < maxValue)
			return true;

		return false;
	}

}
