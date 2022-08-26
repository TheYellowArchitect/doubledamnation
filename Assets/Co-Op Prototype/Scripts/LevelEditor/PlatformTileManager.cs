using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTileManager : MonoBehaviour 
{

	[Header("Prefabs")]
	public GameObject platformDefaultLevel1;
	public GameObject platformFallingLevel1;
	public GameObject platformFallingLevel3;

	public List<GridItem> StaticGridPlatformItems = new List<GridItem>();

	private MasterGridManager masterGrid;
	private GridItem tempGridItem;
	private GameObject tempTile;

	private List<FallingPlatform> fallingPlatformList = new List<FallingPlatform>();

	//Instead of adding a new bool parameter to each function, this flag turns true when RPC is received, so it won't send it to the other player!
	public bool networkSendBackFlag = true;

	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();
	}

	//Creating by grid tile indexes, via loading
	public void CreateStaticPlatform(int Xindex1, int Yindex1, int Xindex2, byte platformType)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementType = platformType;
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(Xindex1, Yindex1));
		tempGridItem.placementLocation.Add(new Vector2(Xindex2, Yindex1));
		masterGrid.DefaultGridOccupied[Xindex1, Yindex1] = true;
		masterGrid.DefaultGridOccupied[Xindex2, Yindex1] = true;

		//Get the platform type to spawn as a prefab
		if (platformType == MasterGridManager.DEFAULTPLATFORM)
			tempTile = platformDefaultLevel1;
		else if (platformType == MasterGridManager.FALLINGPLATFORM)
			tempTile = platformFallingLevel1;
		else if (platformType == MasterGridManager.BOOSTPLATFORM)
			tempTile = platformFallingLevel3;

		//Create the platform
		if (Xindex1 < Xindex2)
			tempTile = Object.Instantiate(tempTile, masterGrid.GetGridTileLocation(Xindex1, Yindex1) + new Vector3(masterGrid.tileOffset / 2, 0, 0), Quaternion.identity);
		else
			tempTile = Object.Instantiate(tempTile, masterGrid.GetGridTileLocation(Xindex2, Yindex1) + new Vector3(masterGrid.tileOffset / 2, 0, 0), Quaternion.identity);

		//Store, so it can be reset later
		if (platformType == MasterGridManager.FALLINGPLATFORM || platformType == MasterGridManager.BOOSTPLATFORM)
			StoreFallingPlatform(tempTile.GetComponent<FallingPlatform>());

		tempGridItem.gameobject = tempTile;
		tempTile.transform.SetParent(masterGrid.staticHolder);

		StaticGridPlatformItems.Add(tempGridItem);

		//If online, send to the other player
		if (NetworkCommunicationController.globalInstance != null)
		{
			if (networkSendBackFlag == true)
				NetworkCommunicationController.globalInstance.SendLevelEditorCreateStaticPlatform(platformType, (ushort) Xindex1, (ushort) Yindex1, (ushort) Xindex2);
			
			networkSendBackFlag = true;
		}
	}
	
	//Create by mouse click
	public void CreateStaticPlatform(Vector3 mousePosition, byte platformType)
	{
		//Get approximate tile
		float x = mousePosition.x / masterGrid.tileOffset;
		float y = mousePosition.y / masterGrid.tileOffset;

		//Resolving out of bounds grid edge-case
		if (x < 0.5f)
			x = 0.51f;
		if (y < 0.5f)
			y = 0.51f;

		
		//Get which tile we are
		int indexX = (int) x;
		int indexY = (int) y;
		

		//Get where is the nearest aka furthest from center side, so as to center it
		bool rightSide = false;
		float moduloX = x % 1;
		//If more right than the 50% of this tile, we are obviously towards right of the center
		if (moduloX > 0.5f)
			rightSide = true;

		tempGridItem = new GridItem();
		tempGridItem.placementType = platformType;
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(indexX, indexY));
		masterGrid.DefaultGridOccupied[indexX, indexY] = true;
		if (rightSide)
		{
			tempGridItem.placementLocation.Add(new Vector2(indexX + 1, indexY));
			masterGrid.DefaultGridOccupied[indexX + 1, indexY] = true;
		}
		else
		{
			tempGridItem.placementLocation.Add(new Vector2(indexX - 1, indexY));
			masterGrid.DefaultGridOccupied[indexX - 1, indexY] = true;
		}
		

		//Get the platform type to spawn as a prefab
		if (platformType == MasterGridManager.DEFAULTPLATFORM)
			tempTile = platformDefaultLevel1;
		else if (platformType == MasterGridManager.FALLINGPLATFORM)
			tempTile = platformFallingLevel1;
		else if (platformType == MasterGridManager.BOOSTPLATFORM)
			tempTile = platformFallingLevel3;
		
		//Create the platform
		if (rightSide)
			tempTile = Object.Instantiate(tempTile, masterGrid.GetGridTileLocation(indexX, indexY) + new Vector3(masterGrid.tileOffset / 2, 0, 0), Quaternion.identity);
		else
			tempTile = Object.Instantiate(tempTile, masterGrid.GetGridTileLocation(indexX, indexY) + new Vector3(-masterGrid.tileOffset / 2, 0, 0), Quaternion.identity);

		//Store, so it can be reset later
		if (platformType == MasterGridManager.FALLINGPLATFORM || platformType == MasterGridManager.BOOSTPLATFORM)
			StoreFallingPlatform(tempTile.GetComponent<FallingPlatform>());

		tempGridItem.gameobject = tempTile;
		tempTile.transform.SetParent(masterGrid.staticHolder);

		StaticGridPlatformItems.Add(tempGridItem);

		//If online, send to the other player
		if (NetworkCommunicationController.globalInstance != null)
		{
			if (networkSendBackFlag == true)
			{
				if (rightSide)
					NetworkCommunicationController.globalInstance.SendLevelEditorCreateStaticPlatform(platformType, (ushort) indexX, (ushort) indexY, (ushort)(indexX + 1));
				else
					NetworkCommunicationController.globalInstance.SendLevelEditorCreateStaticPlatform(platformType, (ushort) indexX, (ushort) indexY, (ushort)(indexX - 1));
			}
			
			networkSendBackFlag = true;
		}
	}

	public void CreateFreeformPlatform(float x, float y, byte platformType)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementType = platformType;
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(x, y));

		//Get the platform type to spawn as a prefab
		if (platformType == MasterGridManager.DEFAULTPLATFORM)
			tempTile = platformDefaultLevel1;
		else if (platformType == MasterGridManager.FALLINGPLATFORM)
			tempTile = platformFallingLevel1;
		else if (platformType == MasterGridManager.BOOSTPLATFORM)
			tempTile = platformFallingLevel3;

		//Create the platform
		tempTile = Object.Instantiate(tempTile, new Vector3(x, y, 0), Quaternion.identity);
		
		//Store, so it can be reset later
		if (platformType == MasterGridManager.FALLINGPLATFORM || platformType == MasterGridManager.BOOSTPLATFORM)
			StoreFallingPlatform(tempTile.GetComponent<FallingPlatform>());

		tempGridItem.gameobject = tempTile;
		tempTile.transform.SetParent(masterGrid.freeformHolder);

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);
	}

	public void StoreFallingPlatform(FallingPlatform newFallingPlatform)
	{
		fallingPlatformList.Add(newFallingPlatform);
	}

	public void RestartFallingPlatforms()
	{
		for (int i = 0; i < fallingPlatformList.Count; i++)
			fallingPlatformList[i].ResetPlatform();
	}

	
}
