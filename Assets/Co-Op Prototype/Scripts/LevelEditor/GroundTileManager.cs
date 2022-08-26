using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTileManager : MonoBehaviour 
{
	[Header("Prefabs")]
	public GameObject tileNormalLevel1;//4x4
	public GameObject tileHorizontalLevel1;//8x4
	public GameObject tileVerticalLevel1;//4x8
	public GameObject tileDoubleEndlessLevel1;//8x8 default
	public GameObject tileDoubleSingleLevel1;//8x8 but only when nothing above or below or left or right

	public List<GridItem> StaticGridGroundItems = new List<GridItem>();

	private MasterGridManager masterGrid;
	private GridItem tempGridItem;
	private GameObject tempTile;

	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();
	}

	public void CreateStaticGround(params float[] gridIndexValues)
	{
		int[] array = new int[gridIndexValues.Length];

		for (int i = 0; i < gridIndexValues.Length; i++)
			array[i] = (int) gridIndexValues[i];

		CreateStaticGround(array);
	}

	//params means it can accept any number of parameters on same variable
	public void CreateStaticGround(params int[] gridIndexValues)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(gridIndexValues[0], gridIndexValues[1]));
		tempGridItem.placementType = MasterGridManager.GROUND;


		//Single click creation aka default
		if (gridIndexValues.Length == 2)
		{
			tempTile = Object.Instantiate(tileNormalLevel1, masterGrid.GetGridTileLocation(gridIndexValues[0], gridIndexValues[1]), Quaternion.identity);

			tempGridItem.gameobject = tempTile;
		}
		else if (gridIndexValues.Length == 4)//Either horizontal or vertical
		{
			//If X is the same -> Vertical prefab
			if (gridIndexValues[0] == gridIndexValues[2])
			{
				Vector3 newY = masterGrid.GetGridTileLocation(gridIndexValues[0], gridIndexValues[1]);

				Debug.Log("newY is: " + newY);

				if (gridIndexValues[1] > gridIndexValues[3])
					newY = newY + new Vector3(0, -2.56f, 0);
				else if (gridIndexValues[1] < gridIndexValues[3])
					newY = newY + new Vector3(0, 2.56f, 0);

				tempTile = Object.Instantiate(tileVerticalLevel1, newY, Quaternion.identity);
				Debug.Log("newY is: " + newY);
			}
			else
			{
				Vector3 newX = masterGrid.GetGridTileLocation(gridIndexValues[0], gridIndexValues[1]);

				if (gridIndexValues[0] > gridIndexValues[2])
					newX = newX + new Vector3(-2.56f, 0, 0);
				else if (gridIndexValues[0] < gridIndexValues[2])
					newX = newX + new Vector3(2.56f, 0, 0);

				tempTile = Object.Instantiate(tileHorizontalLevel1, newX, Quaternion.identity);
			}

			tempGridItem.gameobject = tempTile;
			tempGridItem.placementLocation.Add(new Vector2(gridIndexValues[2], gridIndexValues[3]));
		}
		else if (gridIndexValues.Length == 8)//Either horizontal or vertical
		{
			//=================================================
			//===First 4 are latest tile's=====================
			//===Latest 4 are the tile I detected adjacently===
			//=================================================

			for (int i = 0; i < gridIndexValues.Length; i=i+2)
				Debug.LogWarning("GridLocation: [" + gridIndexValues[i] + "][" + gridIndexValues[i+1]+"]");
			
			Vector3 spawnLocation;
			//If Vertical -> Adjacent is vertical, will connect left or right
			if (gridIndexValues[0] == gridIndexValues[2])
			{
				//Y axis
				if (gridIndexValues[1] < gridIndexValues[3])
					spawnLocation = masterGrid.GetGridTileLocation(gridIndexValues[0], gridIndexValues[1]) + new Vector3(0, 2.56f, 0);
				else
					spawnLocation = masterGrid.GetGridTileLocation(gridIndexValues[2], gridIndexValues[3]) + new Vector3(0, 2.56f, 0);
				//^Above makes sure X is proper for creation, just in the center.

				//X axis
				if (gridIndexValues[0] < gridIndexValues[6])
					spawnLocation = spawnLocation + new Vector3(2.56f, 0, 0);
				else
					spawnLocation = spawnLocation + new Vector3(-2.56f, 0, 0);
			}

			//If Horizontal -> Adjacent is vertical, will connect up or down
			else//if (gridIndexValues[1] == gridIndexValues[3])
			{
				//X axis
				if (gridIndexValues[0] < gridIndexValues[2])
					spawnLocation = masterGrid.GetGridTileLocation(gridIndexValues[0], gridIndexValues[1]) + new Vector3(2.56f, 0, 0);
				else
					spawnLocation = masterGrid.GetGridTileLocation(gridIndexValues[2], gridIndexValues[3]) + new Vector3(2.56f, 0, 0);
				//^Above makes sure X is proper for creation, just in the center.

				//Y axis
				if (gridIndexValues[1] < gridIndexValues[7])
					spawnLocation = spawnLocation + new Vector3(0, 2.56f, 0);
				else
					spawnLocation = spawnLocation + new Vector3(0, -2.56f, 0);
			}

				//TODO: If nothing around, singletile. BUT I don't do it yet, because it would mean converting this to DoubleEndless when nearby tile is added, aka it can bloat the codebase
				tempTile = Object.Instantiate(tileDoubleEndlessLevel1, spawnLocation, Quaternion.identity);

				tempGridItem.gameobject = tempTile;
				tempGridItem.placementLocation.Add(new Vector2(gridIndexValues[2], gridIndexValues[3]));
				tempGridItem.placementLocation.Add(new Vector2(gridIndexValues[4], gridIndexValues[5]));
				tempGridItem.placementLocation.Add(new Vector2(gridIndexValues[6], gridIndexValues[7]));
		}

		tempTile.transform.SetParent(masterGrid.staticHolder);

		StaticGridGroundItems.Add(tempGridItem);

		for (int i = 0; i < gridIndexValues.Length; i = i + 2)
			masterGrid.DefaultGridOccupied[gridIndexValues[i], gridIndexValues[i + 1]] = true;

		masterGrid.hazardTileManager.UpdateHazardAdjacency(gridIndexValues);
	}
	
	public void DetermineGroundTileAdjacency(int latestCreatedTileX, int latestCreatedTileY)
	{
		//==================================
		//===Merge to Vertical/Horizontal===
		//==================================

		//Iterate all slots to find if anything nearby
		int x = -2;
		int y = -2;
		bool merged = false;
		for (int i = 0; i < StaticGridGroundItems.Count; i++)
		{
			if (merged)
				break;

			//If can merge to a horizontal/vertical
			if (StaticGridGroundItems[i].placementLocation.Count == 1)
			{
				x = (int) StaticGridGroundItems[i].placementLocation[0].x;
				y = (int) StaticGridGroundItems[i].placementLocation[0].y;

				//If Y is adjacent to our recently placed tile
				if (y == latestCreatedTileY - 1 && x == latestCreatedTileX)
				{
					//Remove our current tile
					masterGrid.RemoveGridItemFromList(masterGrid.GetGridItemIndexFromList(tempTile, ref StaticGridGroundItems), ref StaticGridGroundItems);

					//Remove the tile at latestCreatedTileY
					masterGrid.RemoveGridItemFromList(i, ref StaticGridGroundItems);

					masterGrid.DebugGridItemsFromList(ref StaticGridGroundItems, "Ground");

					CreateStaticGround(latestCreatedTileX, latestCreatedTileY, latestCreatedTileX, latestCreatedTileY - 1);

					merged = true;
				}
				else if (y == latestCreatedTileY + 1 && x == latestCreatedTileX)
				{
					//Remove our current tile
					masterGrid.RemoveGridItemFromList(masterGrid.GetGridItemIndexFromList(tempTile, ref StaticGridGroundItems), ref StaticGridGroundItems);

					//Remove the tile at latestCreatedTileY
					masterGrid.RemoveGridItemFromList(i, ref StaticGridGroundItems);

					masterGrid.DebugGridItemsFromList(ref StaticGridGroundItems, "Ground");

					CreateStaticGround(latestCreatedTileX, latestCreatedTileY, latestCreatedTileX, latestCreatedTileY + 1);

					merged = true;
				}

				//If X is adjacent to our recently placed tile
				else if (y == latestCreatedTileY && x == latestCreatedTileX - 1)
				{
					//Remove our current tile
					masterGrid.RemoveGridItemFromList(masterGrid.GetGridItemIndexFromList(tempTile, ref StaticGridGroundItems), ref StaticGridGroundItems);

					//Remove the tile at latestCreatedTileY
					masterGrid.RemoveGridItemFromList(i, ref StaticGridGroundItems);

					masterGrid.DebugGridItemsFromList(ref StaticGridGroundItems, "Ground");

					CreateStaticGround(latestCreatedTileX, latestCreatedTileY, latestCreatedTileX - 1, latestCreatedTileY);

					merged = true;
				}
				else if (y == latestCreatedTileY && x == latestCreatedTileX + 1)
				{
					//Remove our current tile
					masterGrid.RemoveGridItemFromList(masterGrid.GetGridItemIndexFromList(tempTile, ref StaticGridGroundItems), ref StaticGridGroundItems);

					//Remove the tile at latestCreatedTileY
					masterGrid.RemoveGridItemFromList(i, ref StaticGridGroundItems);

					masterGrid.DebugGridItemsFromList(ref StaticGridGroundItems, "Ground");

					CreateStaticGround(latestCreatedTileX, latestCreatedTileY, latestCreatedTileX + 1, latestCreatedTileY);

					merged = true;
				}
			}
			
			

			
		}

		//=====================
		//===Merge to Double===
		//=====================


		//Check if vertical and horizontal adjacent
		if (merged)//StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation.Count == 2)
		{
			bool mergeAdjacently = false;
			for (int i = 0; i < StaticGridGroundItems.Count - 1; i++)//-1 or you get itself!
			{

				//If can mergeAdjacently to a double (with a single or horizontal/vertical)
				if (StaticGridGroundItems[i].placementLocation.Count == 2)
				{
					//Starting from the end to back, so there are no "out of range" issues when you remove [0] then access [1+]
					//for (int j = StaticGridGroundItems[i].placementLocation.Count - 2; j >= 0; j--)
					//{
						//This is Vertical -> We try to find adjacent horizontally!
						//Last tile is always the one last created ;)
						if (StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x)
						{
							//Adjacent doubletile has the same Y
							if ((StaticGridGroundItems[i].placementLocation[0].y == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y && StaticGridGroundItems[i].placementLocation[1].y == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y)
							|| (StaticGridGroundItems[i].placementLocation[0].y == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y && StaticGridGroundItems[i].placementLocation[1].y == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y))
							{
								if ((StaticGridGroundItems[i].placementLocation[0].x - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x && StaticGridGroundItems[i].placementLocation[1].x - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x)
								|| (StaticGridGroundItems[i].placementLocation[0].x - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x && StaticGridGroundItems[i].placementLocation[1].x - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x))
									mergeAdjacently = true;
								else if ((StaticGridGroundItems[i].placementLocation[0].x + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x && StaticGridGroundItems[i].placementLocation[1].x + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x)
								|| (StaticGridGroundItems[i].placementLocation[0].x + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x && StaticGridGroundItems[i].placementLocation[1].x + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x))
									mergeAdjacently = true;
							}

						}

						//This is Horizontal -> We try to find adjacent vertically!
						else if (StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y)
						{
							//Adjacent doubletile has the same X
							if ((StaticGridGroundItems[i].placementLocation[0].x == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x && StaticGridGroundItems[i].placementLocation[1].x == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x)
							|| (StaticGridGroundItems[i].placementLocation[0].x == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x && StaticGridGroundItems[i].placementLocation[1].x == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x))
							{
								if ((StaticGridGroundItems[i].placementLocation[0].y - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y && StaticGridGroundItems[i].placementLocation[1].y - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y)
								|| (StaticGridGroundItems[i].placementLocation[0].y - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y && StaticGridGroundItems[i].placementLocation[1].y - 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y))
									mergeAdjacently = true;
								else if ((StaticGridGroundItems[i].placementLocation[0].y + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y && StaticGridGroundItems[i].placementLocation[1].y + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y)
								||(StaticGridGroundItems[i].placementLocation[0].y + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y && StaticGridGroundItems[i].placementLocation[1].y + 1 == StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y))
									mergeAdjacently = true;
							}
						}

						if (mergeAdjacently)
						{
							float[] tempTileGridLocations = new float[8] { StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].x, StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[0].y, StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].x, StaticGridGroundItems[StaticGridGroundItems.Count - 1].placementLocation[1].y, StaticGridGroundItems[i].placementLocation[0].x, StaticGridGroundItems[i].placementLocation[0].y, StaticGridGroundItems[i].placementLocation[1].x, StaticGridGroundItems[i].placementLocation[1].y};

							//Remove our current tile
							masterGrid.RemoveGridItemFromList(StaticGridGroundItems.Count - 1, ref StaticGridGroundItems);

							//Remove the tile at gridY
							masterGrid.RemoveGridItemFromList(i, ref StaticGridGroundItems);

							masterGrid.DebugGridItemsFromList(ref StaticGridGroundItems, "Ground");

							CreateStaticGround(tempTileGridLocations);

							return;
						}

					//}
				}
			}

			//if (mergeAdjacently){//the below}
			//So, we are a vertical/horizontal, with 2 vertical/horizontals nearby (always both are the same!)
			if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, 1, 0, 1, 1, 0, 1) != -1)
			{}
			else if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, 1, 0, 0, -1, 1, -1) != -1)
			{}
			else if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, -1, 0, -1, 1, 0, 1) != -1)
			{}
			else if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, -1, 0, -1, -1, 0, -1) != -1)
			{}
		}
		else//we are single, but there can be adjacent one horizontal and vertical!
		{
			if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, 1, 0, 1, 1, 0, 1) != -1)
			{}
			else if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, 1, 0, 1, -1, 0, -1) != -1)
			{}
			else if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, -1, 0, -1, 1, 0, 1) != -1)
			{}
			else if (MergeThreeTiledVerticalsHorizontals(latestCreatedTileX, latestCreatedTileY, -1, 0, -1, -1, 0, -1) != -1)
			{}
		}

	}

	public bool MergeThreeTiledVerticalsHorizontalsIsOutsideGrid(int latestCreatedTileX, int latestCreatedTileY, int offsetX1, int offsetY1, int offsetX2, int offsetY2, int offsetX3, int offsetY3)
	{
		if (latestCreatedTileX + offsetX1 < 0 || latestCreatedTileY + offsetX1 > masterGrid.maxTilesSpawned - 1)
		{
			//Debug.LogWarning("Outside X1");
			return true;
		}
		if (latestCreatedTileX + offsetX2 < 0 || latestCreatedTileY + offsetX2 > masterGrid.maxTilesSpawned - 1)
		{
			//Debug.LogWarning("Outside X2");
			return true;
		}
		if (latestCreatedTileX + offsetX3 < 0 || latestCreatedTileY + offsetX3 > masterGrid.maxTilesSpawned - 1)
		{
			//Debug.LogWarning("Outside X3");
			return true;
		}

		if (latestCreatedTileY + offsetY1 < 0 || latestCreatedTileY + offsetY1 > masterGrid.maxTilesSpawned - 1)
		{
			//Debug.LogWarning("Outside Y1");
			return true;
		}
		if (latestCreatedTileY + offsetY2 < 0 || latestCreatedTileY + offsetY2 > masterGrid.maxTilesSpawned - 1)
		{
			//Debug.LogWarning("Outside Y2");
			return true;
		}
		if (latestCreatedTileY + offsetY3 < 0 || latestCreatedTileY + offsetY3 > masterGrid.maxTilesSpawned - 1)
		{
			//Debug.LogWarning("Outside Y3");
			return true;
		}

		return false;
	}

	//Top-Right Dataset (one of four)
	//offsetX1 = 1
	//offsetY1 = 0
	//offsetX2 = 1
	//offsetY2 = 1
	//offsetX3 = 0
	//offsetY3 = 1
	//Bottom-Right Dataset (one of four)
	//offsetX1 = 1
	//offsetY1 = 0
	//offsetX2 = 0
	//offsetY2 = -1
	//offsetX3 = 1
	//offsetY3 = -1
	//THE INSPIRED MATRIX ALGORITHM
	public int MergeThreeTiledVerticalsHorizontals(int latestCreatedTileX, int latestCreatedTileY, int offsetX1, int offsetY1, int offsetX2, int offsetY2, int offsetX3, int offsetY3)
	{
		//Before all the below, I check if im at edges of [0] or [length], so it doesnt go [-1] or [length+1]
		if (MergeThreeTiledVerticalsHorizontalsIsOutsideGrid(latestCreatedTileX, latestCreatedTileY, offsetX1, offsetY1, offsetX2, offsetY2, offsetX3, offsetY3))
			return -1;

		//There are 4 combinations of adjacent vertical/horizontal, and 4 more for pure horizontal and 4 more for pure vertical.
		//It would be retarded to manually check all of them.
		//So, what I do is check the remaining 3 tiles needed to form a double tile (8x8)
		//
		//I then check if both of these are vertical/horizontal.
		//If yes, I create a double tile, which should be offset properly from our current tile. 
		//Then I place a tile where the vertical/horizontal tile is split.
		if (masterGrid.DefaultGridOccupied[latestCreatedTileX + offsetX1, latestCreatedTileY + offsetY1] == true && masterGrid.DefaultGridOccupied[latestCreatedTileX + offsetX2, latestCreatedTileY + offsetY2] == true && masterGrid.DefaultGridOccupied[latestCreatedTileX + offsetX3, latestCreatedTileY + offsetY3] == true)
		{
			GridItem gridItem1 = masterGrid.GetGridItemFromList(latestCreatedTileX + offsetX1, latestCreatedTileY + offsetY1, ref StaticGridGroundItems);
			GridItem gridItem2 = masterGrid.GetGridItemFromList(latestCreatedTileX + offsetX2, latestCreatedTileY + offsetY2, ref StaticGridGroundItems);
			GridItem gridItem3 = masterGrid.GetGridItemFromList(latestCreatedTileX + offsetX3, latestCreatedTileY + offsetY3, ref StaticGridGroundItems);

			//If tiles picked are all ground
			if ((gridItem1.placementType != MasterGridManager.ERROR && gridItem1.placementType == MasterGridManager.GROUND)
			&& (gridItem2.placementType != MasterGridManager.ERROR && gridItem2.placementType == MasterGridManager.GROUND)
			&& (gridItem3.placementType != MasterGridManager.ERROR && gridItem3.placementType == MasterGridManager.GROUND))
			{
				//If the adjacent corners are vertical/horizontal (note that x+1, y+1 can also be a vertical/horizontal which is unrelated to the other 2 tiles!)
				if ((gridItem1.gameobject != null && gridItem3.gameobject != null)
				&& (gridItem1.placementLocation.Count == 2 && gridItem3.placementLocation.Count == 2))
				{

					int[] tempTileGridLocations = new int[8] {latestCreatedTileX, latestCreatedTileY, latestCreatedTileX + offsetX1, latestCreatedTileY + offsetY1, latestCreatedTileX + offsetX2, latestCreatedTileY + offsetY2, latestCreatedTileX + offsetX3, latestCreatedTileY + offsetY3};

					//for (int i = 0; i < tempTileGridLocations.Length; i++)
						//Debug.Log("Temptilegridlocation["+ i + "] = " + tempTileGridLocations[i]);
						//good thus far

					//Locations outside the doubletile
					List<int> extrudingTileOutsideLocations = new List<int>();

					int pickedX;
					int pickedY;

					for (int i = 0; i < StaticGridGroundItems.Count; i++)
					{
						for (int j = 0; j < StaticGridGroundItems[i].placementLocation.Count; j++)
						{
							pickedX = (int) StaticGridGroundItems[i].placementLocation[j].x;
							pickedY = (int) StaticGridGroundItems[i].placementLocation[j].y;

							//If one of the 3 tiles (or the new/latest vertical/horizontal tile)
							if ((pickedX == latestCreatedTileX + offsetX1 && pickedY == latestCreatedTileY + offsetY1)
							||(pickedX == latestCreatedTileX + offsetX2 && pickedY == latestCreatedTileY + offsetY2)
							||(pickedX == latestCreatedTileX + offsetX3 && pickedY == latestCreatedTileY + offsetY3)
							||(pickedX == latestCreatedTileX && pickedY == latestCreatedTileY))
							{
								Debug.LogWarning("To check j = " + j + " [" + (int)StaticGridGroundItems[i].placementLocation[j].x + "][" + (int)StaticGridGroundItems[i].placementLocation[j].y + "]");

								//Get the other index this horizontal/vertical covers!
								if (StaticGridGroundItems[i].placementLocation.Count == 1)
									break;
								else if (j == 0)
									j = 1;
								else if (j == 1)
									j = 0;
								else
								{
									Debug.LogError("Beyond vertical?! Merging with double tile?!");
									return -1;
								}

								Debug.LogWarning("To check j = " + j + " [" + (int)StaticGridGroundItems[i].placementLocation[j].x + "][" + (int)StaticGridGroundItems[i].placementLocation[j].y + "]");

								//If NOT within the 4x4 doubletile
								if (masterGrid.IsTileIndexWithinTileIndexes((int)StaticGridGroundItems[i].placementLocation[j].x, (int)StaticGridGroundItems[i].placementLocation[j].y, latestCreatedTileX, latestCreatedTileY, latestCreatedTileX + offsetX1, latestCreatedTileY + offsetY1, latestCreatedTileX + offsetX2, latestCreatedTileY + offsetY2, latestCreatedTileX + offsetX3, latestCreatedTileY + offsetY3) == false)
								{
									//Captured the tiles to create outside the double (so we can now delete freely)
									Debug.LogWarning("Extruding is [" + StaticGridGroundItems[i].placementLocation[j].x + "][" + StaticGridGroundItems[i].placementLocation[j].y + "]");
									extrudingTileOutsideLocations.Add((int)StaticGridGroundItems[i].placementLocation[j].x);
									extrudingTileOutsideLocations.Add((int)StaticGridGroundItems[i].placementLocation[j].y);
								}

								break;
							}
						}
							
					}

					masterGrid.RemoveGridItemsFromList(ref StaticGridGroundItems, tempTileGridLocations);

					CreateStaticGround(tempTileGridLocations);

					//CreateSingleTile
					int currentExtrudedTileIndex;
					for (currentExtrudedTileIndex = 0; currentExtrudedTileIndex < extrudingTileOutsideLocations.Count; currentExtrudedTileIndex = currentExtrudedTileIndex + 2)
					{
						//Create Single Tile
						CreateStaticGround(extrudingTileOutsideLocations[currentExtrudedTileIndex], extrudingTileOutsideLocations[currentExtrudedTileIndex + 1]);//[50][48] works properly

						//Merge if any adjacent (yes this is low-key recursive :)
						DetermineGroundTileAdjacency(extrudingTileOutsideLocations[currentExtrudedTileIndex], extrudingTileOutsideLocations[currentExtrudedTileIndex + 1]);
					}

					if (extrudingTileOutsideLocations.Count > 0)
						return 1;

				}
			}

			
		}

		return -1;
	}

	//0 single ("4x4")
	//1 vertical ("4x8")
	//2 horizontal ("8x4")
	//3 double ("RepeatingFat")
	public void CreateFreeformGround(float x, float y, byte groundType)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(x, y));
		tempGridItem.placementType = MasterGridManager.GROUND;

		//Get tiletype!
		if (groundType == 0)
			tempTile = tileNormalLevel1;
		else if (groundType == 1)
			tempTile = tileVerticalLevel1;
		else if (groundType == 2)
			tempTile = tileHorizontalLevel1;
		else if (groundType == 3)
			tempTile = tileDoubleEndlessLevel1;

		tempTile = Object.Instantiate(tempTile, new Vector3(x, y, 0), Quaternion.identity);

		tempGridItem.gameobject = tempTile;

		tempTile.transform.SetParent(masterGrid.freeformHolder);

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);
	}

}