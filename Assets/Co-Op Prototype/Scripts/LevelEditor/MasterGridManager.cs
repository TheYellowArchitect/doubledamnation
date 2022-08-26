using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/*
	First of all, you create a 2D array of gameobjects.
	Starting with [100][100] up to [500][500] I guess.
	If you add collision to each sprite (to detect mouse raycast), game crashes, or has <1 FPS.
	For this reason, I dont have collision on any default sprite gameobject. I detect which tile I am on, by using mouse position's and knowing the tile offsets by generation (see gridX, gridY)

	Now that this is done, you can detect tiles and add stuff on top of default sprite gameobject.
	I use Vector2 for each object.
	Static:
		Vector2.x = X of Grid (ushort)
		Vector2.y = Y of Grid (ushort)
		byte = PlacementType
	Freeform:
		Vector2.x = X of world space (float)
		Vector2.y = Y of world space (float)
		byte = PlacementType

	Saving Data:
		Tilemap is ushort aka 0 to 65536 (2^16)
		Placement Type is below enumerated as byte, but ofc in the end is just a hexadecimal I guess
*/

//Shouldnt this be in its own file?
public struct GridItem
{
	public GameObject gameobject;
	public List<Vector2> placementLocation;
	public byte placementType;
}

public class MasterGridManager : MonoBehaviour 
{
	public const byte GROUND = 0, DEFAULTPLATFORM = 1, FALLINGPLATFORM = 2, BOOSTPLATFORM = 3, HAZARD = 4, HOLLOW = 5, SATYR = 6, CENTAUR = 7, CYCLOPS = 8, MINOTAUR = 9, HARPY = 10, SPIKEBOI = 11, ENTRANCE = 12, EXIT = 13, BRAZIER = 14, ERROR = 15;
	public const int MAX_DEFAULTGRID = 1000;

	public static MasterGridManager globalInstance;

	[Header("Tile Generation Values")]
		[Tooltip("The gap distance seperating tiles. 10.24 is of normal ones (8x8), 5.12 is for one tile here.")]
		public float tileOffset = 5.12f;
		[Tooltip("How many tiles to spawn at the very start.\n300 is recommended.")]
		public int initialTileCount = 100;//300 rn
		[Tooltip("This is initialized with the value of initialTiles^, but can expand obviously")]
		public int maxTilesSpawned = 0;
		[Tooltip("How much before maxTilesSpawned, should you spawn more tiles?\nFor example, if initialTile is 100 and this is 50, it spawns them when you reach [50] of X or Y")]
		public int tileSpawnThresholdOffset = 10;
		[Tooltip("How many tiles to spawn when you reach the edges?^")]
		public int tileSpawnCount = 50;
		//[Tooltip("How many times max should more tiles be spawned?\n-1 means no limit, but until max length of the defaultgrid")]
		//public int tileSpawnMaxWaves = 5;

	[Header("Gameobject Holders")]
		public Transform gridHolder;
		public Transform staticHolder;
		public Transform freeformHolder;

		public bool[,] DefaultGridOccupied = new bool[MAX_DEFAULTGRID, MAX_DEFAULTGRID];
		private GameObject[,] DefaultGrid = new GameObject[MAX_DEFAULTGRID, MAX_DEFAULTGRID];
		private GameObject tempTile;
		
	public GameObject tileDefaultEmpty;

	public bool playMode = false;


	private GridItem tempGridItem;

	[Header("TileManager Children GetComponent<>")]
	public GroundTileManager groundTileManager;
	public PlatformTileManager platformTileManager;
	public EnemyTileManager enemyTileManager;
	public HazardTileManager hazardTileManager;
	public FreeformTileManager freeformTileManager;
	public InputTileManager inputTileManager;
	public SerializationTileManager serializationTileManager;
	public CameraTileManager cameraTileManager;
	public EntranceExitTileManager entranceExitTileManager;

	//Instead of adding a new bool parameter to each function, this flag turns true when RPC is received, so it won't send it to the other player!
	public bool networkSendBackFlag = true;

	// Use this for initialization
	void Start () 
	{
		//Singleton ftw
		globalInstance = this;

		LevelEditorMenu.globalInstance.ShowLevelEditorUI(true);

		Initialize();

		//This is 2nd because it isn't instant and kills the framerate
		GenerateGridTiles(initialTileCount);

		//This creates the startPos, the groundtiles below it, and the brazier (brazier auto-checks on save level, by checking if ground X tiles from entrance)
		entranceExitTileManager.CreateInitialEntrance();

	}

	public void Initialize()
	{
		groundTileManager = GetComponent<GroundTileManager>();
		groundTileManager.Initialize();

		platformTileManager = GetComponent<PlatformTileManager>();
		platformTileManager.Initialize();

		enemyTileManager = GetComponent<EnemyTileManager>();
		enemyTileManager.Initialize();

		hazardTileManager = GetComponent<HazardTileManager>();
		hazardTileManager.Initialize();

		freeformTileManager = GetComponent<FreeformTileManager>();
		freeformTileManager.Initialize();

		inputTileManager = GetComponent<InputTileManager>();
		inputTileManager.Initialize();

		serializationTileManager = GetComponent<SerializationTileManager>();
		serializationTileManager.Initialize();

		cameraTileManager = GetComponent<CameraTileManager>();
		cameraTileManager.Initialize();

		entranceExitTileManager = GetComponent<EntranceExitTileManager>();
		entranceExitTileManager.Initialize();
	}

	public void GenerateGridTiles(int endIndex)
	{
		if (endIndex > MAX_DEFAULTGRID)
		{
			Debug.LogError("You tried to create more tiles than the defaultGrid allows to.\nAssuming you haven't died of lag, i now return; to not crash.");
			return;
		}

		for (int i = maxTilesSpawned; i < endIndex; i++)
			for (int j = 0; j < endIndex; j++)
				CreateDefaultGridTile(i,j);

		for (int i = 0; i < endIndex; i++)
			for (int j = maxTilesSpawned; j < endIndex; j++)
				CreateDefaultGridTile(i,j);

		maxTilesSpawned = endIndex;
	}

	public void CreateDefaultGridTile(int i, int j)
	{
		tempTile = Object.Instantiate(tileDefaultEmpty, new Vector3(i * tileOffset + 2.56f, j * tileOffset + 2.56f, 0), Quaternion.identity);
		tempTile.transform.SetParent(gridHolder);
		DefaultGrid[i,j] = tempTile;
		DefaultGridOccupied[i,j] = false;
	}

	//Purely comparing integers, no list/gridstaticitems manipulation.
	public bool IsTileIndexWithinTileIndexes(int pickedX, int pickedY, params int[] tileIndexValues)
	{
		for (int i = 0; i < tileIndexValues.Length; i = i + 2)
		{
			if (pickedX == tileIndexValues[i] && pickedY == tileIndexValues[i + 1])
				return true;
		}

		return false;
	}

	public Vector3 GetGridTileLocation(int x, int y)
	{
		return new Vector3(DefaultGrid[x, y].transform.position.x, DefaultGrid[x, y].transform.position.y, 0);
	}

	public GridItem GetGridItemFromList(int locationX, int locationY, ref List<GridItem> GridListItems)
	{
		for (int i = 0; i < GridListItems.Count; i++)
		{
			for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
				if (GridListItems[i].placementLocation[j].x == locationX && GridListItems[i].placementLocation[j].y == locationY)
					return GridListItems[i];
		}

		GridItem tempGridItem = new GridItem();
		tempGridItem.placementType = ERROR;
		return tempGridItem;
	}

	public GridItem GetGridItemFromList(GameObject pickedGameobject, ref List<GridItem> GridListItems)
	{
		GridItem tempGridItem = new GridItem();
		tempGridItem.placementType = ERROR;

		if (pickedGameobject == null)
			return tempGridItem;

		for (int i = 0; i < GridListItems.Count; i++)
			if (GridListItems[i].gameobject == pickedGameobject)
				return GridListItems[i];

		return tempGridItem;
	}

	public int GetGridItemIndexFromList(GameObject pickedGameobject, ref List<GridItem> GridListItems)
	{
		if (pickedGameobject == null)
			return -1;

		for (int i = 0; i < GridListItems.Count; i++)
		{
			if (GridListItems[i].gameobject == pickedGameobject)
				return i;
		}

		return -1;
	}

	//You have the GridItem's index, but don't know in which GridList it belongs!
	public List<GridItem> GetGridListFromIndex(int index)
	{
		if (index < groundTileManager.StaticGridGroundItems.Count && groundTileManager.StaticGridGroundItems[index].gameobject != null)
			return groundTileManager.StaticGridGroundItems;
		else if (index < platformTileManager.StaticGridPlatformItems.Count && platformTileManager.StaticGridPlatformItems[index].gameobject != null)
			return platformTileManager.StaticGridPlatformItems;
		else if (index < hazardTileManager.StaticGridHazardItems.Count && hazardTileManager.StaticGridHazardItems[index].gameobject != null)
			return hazardTileManager.StaticGridHazardItems;
		else if (index < enemyTileManager.StaticGridEnemyItems.Count && enemyTileManager.StaticGridEnemyItems[index].gameobject != null)
			return enemyTileManager.StaticGridEnemyItems;
		else if (index < freeformTileManager.FreeformGridItems.Count && freeformTileManager.FreeformGridItems[index].gameobject != null)
			return freeformTileManager.FreeformGridItems;
		else
			return null;
	}

	public int RemoveGridItemFromList(int index, ref List<GridItem> GridListItems, bool destroyGameobject = true, bool removeStaticGridOccupation = true)
	{
		if (index != -1)
		{
			//Get the tiles it occupies, and set them to be deleted
			if (removeStaticGridOccupation)//false when freeform (so deleting freeform wont free occupation of a random static tile lol)
			{
				for (int i = 0; i < GridListItems[index].placementLocation.Count; i++)
					DefaultGridOccupied[(int) GridListItems[index].placementLocation[i].x, (int) GridListItems[index].placementLocation[i].y] = false;
			}

			//Destroy it
			if (destroyGameobject)
			{
				GameObject tempTile = GridListItems[index].gameobject;
				Destroy(tempTile);
			}
			
			//Then clear it from the list
			GridListItems.RemoveAt(index);
			Debug.Log ("Removing " + index);

			return index;
		}

		return -1;
	}

	public int RemoveGridItemFromList(GameObject pickedGameobject, ref List<GridItem> GridListItems, bool destroyGameobject = true, bool removeStaticGridOccupation = true)
	{
		if (pickedGameobject == null)
			return -1;

		for (int i = 0; i < GridListItems.Count; i++)
			if (GridListItems[i].gameobject == pickedGameobject)
				return RemoveGridItemFromList(i, ref GridListItems, destroyGameobject, removeStaticGridOccupation);

		return -1;
	}

	public void RemoveGridItemsFromList(ref List<GridItem> GridListItems, params int[] gridIndexValues)
	{
		if (gridIndexValues.Length % 2 == 1)
		{
			Debug.LogError("You missed an x or y index.");
			//return false;
		}

		for (int pickedIndex = 0; pickedIndex < gridIndexValues.Length; pickedIndex = pickedIndex + 2)
		{
			for (int i = GridListItems.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
				{
					//If found our tile, time to remove it!
					if (GridListItems[i].placementLocation[j].x == gridIndexValues[pickedIndex] && GridListItems[i].placementLocation[j].y == gridIndexValues[pickedIndex + 1])
					{
						RemoveGridItemFromList(i, ref GridListItems);
						break;
					}
				}
				
			}
		}
	}

	public bool DoesTileTypeOccupyIndexesFromList(byte tileType, ref List<GridItem> GridListItems, params int[] gridIndexValues)
	{
		if (gridIndexValues.Length % 2 == 1)
		{
			Debug.LogError("You missed an x or y index.");
			return false;
		}

		for (int i = 0; i < gridIndexValues.Length; i = i + 2)
		{
			if (GetGridItemFromList(gridIndexValues[i], gridIndexValues[i+1], ref GridListItems).placementType == tileType)
				return true;
		}

		return false;
	}

	public List<GridItem> GetStaticGridFromPlacementType(byte tileType)
	{
		if (tileType == GROUND)
			return groundTileManager.StaticGridGroundItems;
		else if (tileType == DEFAULTPLATFORM || tileType == FALLINGPLATFORM || tileType == BOOSTPLATFORM)
			return platformTileManager.StaticGridPlatformItems;
		else if (tileType == HAZARD)
			return hazardTileManager.StaticGridHazardItems;
		else if (tileType >= HOLLOW && tileType <= SPIKEBOI)
			return enemyTileManager.StaticGridEnemyItems;

		return freeformTileManager.FreeformGridItems;
		/*else if (tiletype == EXIT)
			return entranceExitTileManager;*/
	}

	public byte GetEnumOfStaticGrid(ref List<GridItem> GridListItems)
	{
		if (GridListItems == groundTileManager.StaticGridGroundItems)
			return 0;
		else if (GridListItems == platformTileManager.StaticGridPlatformItems)
			return 1;
		else if (GridListItems == hazardTileManager.StaticGridHazardItems)
			return 2;
		else if (GridListItems == enemyTileManager.StaticGridEnemyItems)
			return 3;
		
		return 4;//error!
	}

	public List<GridItem> GetStaticGridFromEnum(byte gridType)
	{
		if (gridType == 0)
			return groundTileManager.StaticGridGroundItems;
		else if (gridType == 1)
			return platformTileManager.StaticGridPlatformItems;
		else if (gridType == 2)
			return hazardTileManager.StaticGridHazardItems;
		else if (gridType == 3)
			return enemyTileManager.StaticGridEnemyItems;
		
		return freeformTileManager.FreeformGridItems;
	}

	public List<int> GetGridItemLocationsFromList(int gridIndex, ref List<GridItem> GridListItems)
	{
		List<int> gridIndexValues = new List<int>();
		for (int i = 0; i < GridListItems[gridIndex].placementLocation.Count; i++)
		{
			gridIndexValues.Add(((int) GridListItems[gridIndex].placementLocation[i].x));
			gridIndexValues.Add(((int) GridListItems[gridIndex].placementLocation[i].y));
		}

		return gridIndexValues;
	}

	public void SwapIndexesFromList(int gridIndex1, int gridIndex2, ref List<GridItem> GridListItems)
	{
		//gameobject
		//placementlocation
		//placementtype

		GridItem tempGridItem1;
		GridItem tempGridItem2;

		tempGridItem1 = GridListItems[gridIndex1];
		tempGridItem2 = GridListItems[gridIndex2];

		GridListItems[gridIndex2] = tempGridItem1;
		GridListItems[gridIndex1] = tempGridItem2;

		//RemoveGridItemFromList(i, ref GridListItems, false, false);
	}

	//=========================
	//===Debug/Print Section===
	//=========================

	public void DebugGridItemsFromList(ref List<GridItem> GridListItems, string itemType)
	{
		Debug.LogWarning("Total " + itemType + " Tiles: " + GridListItems.Count);

		for (int i = 0; i < GridListItems.Count; i++)
		{
			for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
			{
				Debug.LogWarning("i = " + i + " ["+GridListItems[i].placementLocation[j].x+"]["+GridListItems[i].placementLocation[j].y+"]");
			}
		}
	}

	public void DebugOccupiedGridItems()
	{
		for (int i = 0; i < MAX_DEFAULTGRID; i++)
			for (int j = 0; j < MAX_DEFAULTGRID; j++)
				if (DefaultGridOccupied[i,j] == true)
					Debug.LogWarning("[" + i + "][" + j + "] == true");
	}

	public int ClearGridStatic(ref List<GridItem> GridListItems)
	{
		int i = GridListItems.Count - 1;
		for (; i > -1; i--)
		{
			for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
				DefaultGridOccupied[(int) GridListItems[i].placementLocation[j].x, (int) GridListItems[i].placementLocation[j].y] = false;

			tempTile = GridListItems[i].gameobject;
			GridListItems.RemoveAt(i);
			Destroy(tempTile);
			
		}
		return i;
	}

	public int ClearGridFreeform(ref List<GridItem> GridListItems, bool removeEntranceBrazier)
	{
		int i = GridListItems.Count - 1;
		for (; i > -1; i--)
		{
			tempTile = GridListItems[i].gameobject;

			if (removeEntranceBrazier == false && (tempTile.GetComponent<TriggerLevelCutscene>() != null || tempTile.CompareTag("Brazier")))
			{
				//Do not destroy it ;)
			}
			else
			{
				GridListItems.RemoveAt(i);
				Destroy(tempTile);
			}

			
			
		}
		return i;
	}

	public void ClearAllGrid(bool removeEntranceBrazier)
	{
		ClearGridStatic(ref groundTileManager.StaticGridGroundItems);
		ClearGridStatic(ref platformTileManager.StaticGridPlatformItems);
		ClearGridStatic(ref hazardTileManager.StaticGridHazardItems);
		ClearGridStatic(ref enemyTileManager.StaticGridEnemyItems);
		ClearGridFreeform(ref freeformTileManager.FreeformGridItems, removeEntranceBrazier);

		serializationTileManager.latestSavedFileName = "";
		serializationTileManager.ResetFinalByteArray();

		//If online, send to the other player
		if (NetworkCommunicationController.globalInstance != null)
		{
			if (networkSendBackFlag == true)
				NetworkCommunicationController.globalInstance.SendLevelEditorClearAllGrid();
			
			networkSendBackFlag = true;
		}
	}

}
