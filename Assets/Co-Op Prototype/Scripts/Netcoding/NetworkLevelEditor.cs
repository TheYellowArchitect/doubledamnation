using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using NaughtyAttributes;

public class NetworkLevelEditor : MonoBehaviour 
{
	//Singleton
	public static NetworkLevelEditor globalInstance;

	public GameObject tileSummonVFX1;
	[ReadOnly]
	public bool activated = false;

	private MasterGridManager masterGrid = null;
	private GridItem tempGridItem;
	private List<GridItem> tempList;
	private List<int> gridIndexValuesList;//Exclusively for deleting ground -_-

	private byte receivedPlacementType;
	private ushort receivedStaticGridX;
	private ushort receivedStaticGridY;
	private ushort receivedStaticGridX2;//Exclusively for platforms
	private float receivedFreeformGridX;
	private float receivedFreeformGridY;
	private byte receivedGridType;
	private ushort receivedIndex;

	void Start () 
	{
		//Setting our singleton
		globalInstance = this;
	}
	
	//Triggered by LevelManager's OnLevelLoad
	public void Activate()
	{
		if (masterGrid == null)
			masterGrid = GameObject.FindGameObjectWithTag("LevelEditorManager").GetComponent<MasterGridManager>();

		if (masterGrid == null)
		{
			Debug.LogError("NetworkLevelEditor cannot find masterGrid!!!");
			return;
		}

		if (activated == false)
			activated = true;
		else
			return;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorCreateStaticTileEvent += CreateStaticTile;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorCreateStaticPlatformEvent += CreateStaticPlatform;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorCreateFreeformTileEvent += CreateFreeformTile;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorRemoveStaticTileEvent += RemoveStaticTile;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorRemoveFreeformTileEvent += RemoveFreeformTile;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorConvertTileToFreeformEvent += ConvertStaticTileToFreeform;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorMoveFreeformTileEvent += MoveFreeformTile;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorMoveFreeformTileFinalizedEvent += MoveFreeformTileFinalized;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorSaveEvent += SaveLevel;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorLoadEvent += LoadLevel;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorPlayEvent += PlayLevel;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorStopEvent += StopLevel;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorClearAllGridEvent += ClearAllLevel;
	}

	//Currently impossible to trigger this lol
	public void DeActivate()
	{
		activated = false;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorCreateStaticTileEvent -= CreateStaticTile;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorCreateStaticPlatformEvent -= CreateStaticPlatform;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorCreateFreeformTileEvent -= CreateFreeformTile;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorRemoveStaticTileEvent -= RemoveStaticTile;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorRemoveFreeformTileEvent -= RemoveFreeformTile;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorConvertTileToFreeformEvent -= ConvertStaticTileToFreeform;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorMoveFreeformTileEvent -= MoveFreeformTile;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorMoveFreeformTileFinalizedEvent -= MoveFreeformTileFinalized;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorSaveEvent -= SaveLevel;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorLoadEvent -= LoadLevel;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorPlayEvent -= PlayLevel;
		NetworkCommunicationController.globalInstance.ReceiveLevelEditorStopEvent -= StopLevel;

		NetworkCommunicationController.globalInstance.ReceiveLevelEditorClearAllGridEvent -= ClearAllLevel;
	}

	public void CreateStaticTile(RpcArgs args)
	{
		receivedPlacementType = args.GetNext<byte>();
		receivedStaticGridX = args.GetNext<ushort>();
		receivedStaticGridY = args.GetNext<ushort>();

		
		if (receivedPlacementType == MasterGridManager.GROUND)
		{
			//CreateSingleTile
			masterGrid.groundTileManager.CreateStaticGround(receivedStaticGridX, receivedStaticGridY);

			//Check adjacent tiles and merge
			masterGrid.groundTileManager.DetermineGroundTileAdjacency(receivedStaticGridX, receivedStaticGridY);
		}
		else if (receivedPlacementType >= MasterGridManager.HOLLOW && receivedPlacementType <= MasterGridManager.SPIKEBOI)
			masterGrid.enemyTileManager.CreateStaticEnemy(receivedStaticGridX, receivedStaticGridY, receivedPlacementType, LevelEditorMenu.isPlayMode);
		else if (receivedPlacementType == MasterGridManager.HAZARD)
			masterGrid.hazardTileManager.CreateStaticHazard(receivedStaticGridX, receivedStaticGridY);
		/* Exit is only freeform, cant be created staticly
		else if (receivedPlacementType == MasterGridManager.EXIT)
		{
			masterGrid.entranceExitTileManager.networkSendBackFlag = false;
			masterGrid.entranceExitTileManager.CreateExit(currentMousePosition.x, currentMousePosition.y);
		}*/
		/*else if (selectedPlacementType == MasterGridManager.ENTRANCE || selectedPlacementType == MasterGridManager.BRAZIER)
		{
			//TODO: Voiceline
			Debug.LogError("Tried to create entrance!");
			return;
		}*/

		//VFX
		Instantiate(tileSummonVFX1, masterGrid.GetGridTileLocation(receivedStaticGridX, receivedStaticGridY), Quaternion.identity);

	}

	public void CreateStaticPlatform(RpcArgs args)
	{
		receivedPlacementType = args.GetNext<byte>();
		receivedStaticGridX = args.GetNext<ushort>();
		receivedStaticGridY = args.GetNext<ushort>();
		receivedStaticGridX2 = args.GetNext<ushort>();

		if (receivedPlacementType == MasterGridManager.DEFAULTPLATFORM || receivedPlacementType == MasterGridManager.FALLINGPLATFORM || receivedPlacementType == MasterGridManager.BOOSTPLATFORM)
		{
			masterGrid.platformTileManager.networkSendBackFlag = false;
			masterGrid.platformTileManager.CreateStaticPlatform((int)receivedStaticGridX, (int)receivedStaticGridY, (int)receivedStaticGridX2, receivedPlacementType);
		
			//VFX
			Instantiate(tileSummonVFX1, masterGrid.GetGridTileLocation(receivedStaticGridX, receivedStaticGridY), Quaternion.identity);
		}
	}

	public void CreateFreeformTile(RpcArgs args)
	{
		receivedPlacementType = args.GetNext<byte>();
		receivedFreeformGridX = args.GetNext<float>();
		receivedFreeformGridY = args.GetNext<float>();

		if (receivedPlacementType == MasterGridManager.GROUND)
			masterGrid.groundTileManager.CreateFreeformGround(receivedFreeformGridX, receivedFreeformGridY, 3);
		else if (receivedPlacementType == MasterGridManager.DEFAULTPLATFORM || receivedPlacementType == MasterGridManager.FALLINGPLATFORM || receivedPlacementType == MasterGridManager.BOOSTPLATFORM)
			masterGrid.platformTileManager.CreateFreeformPlatform(receivedFreeformGridX, receivedFreeformGridY, receivedPlacementType);
		else if (receivedPlacementType >= MasterGridManager.HOLLOW && receivedPlacementType <= MasterGridManager.SPIKEBOI)
			masterGrid.enemyTileManager.CreateFreeformEnemy(receivedFreeformGridX, receivedFreeformGridY, receivedPlacementType, LevelEditorMenu.isPlayMode);
		else if (receivedPlacementType == MasterGridManager.HAZARD)
			masterGrid.hazardTileManager.CreateFreeformHazard(receivedFreeformGridX, receivedFreeformGridY, false);
		else if (receivedPlacementType == MasterGridManager.EXIT)
			masterGrid.entranceExitTileManager.CreateExit(receivedFreeformGridX, receivedFreeformGridY);
			
		Instantiate(tileSummonVFX1, new Vector3(receivedFreeformGridX, receivedFreeformGridY, 0), Quaternion.identity);
	}

	public void RemoveStaticTile (RpcArgs args)
	{
		//0 Ground 		(groundTileManager.StaticGridGroundItems)
		//1 Platform 	(platformTileManager.StaticGridPlatformItems)
		//2 Hazard 		(hazardTileManager.StaticGridHazardItems)
		//3 Enemies 	(enemyTileManager.StaticGridEnemyItems)
		receivedGridType = args.GetNext<byte>();
		receivedIndex = args.GetNext<ushort>();

		tempList = masterGrid.GetStaticGridFromEnum(receivedGridType);

		if (tempList == masterGrid.groundTileManager.StaticGridGroundItems)
			gridIndexValuesList = masterGrid.GetGridItemLocationsFromList(receivedIndex, ref masterGrid.groundTileManager.StaticGridGroundItems);
		
		masterGrid.RemoveGridItemFromList(receivedIndex, ref tempList);

		if (tempList == masterGrid.groundTileManager.StaticGridGroundItems)
			masterGrid.hazardTileManager.UpdateHazardAdjacency(gridIndexValuesList.ToArray());
	}

	public void RemoveFreeformTile (RpcArgs args)
	{
		receivedIndex = args.GetNext<ushort>();

		tempGridItem = masterGrid.freeformTileManager.FreeformGridItems[receivedIndex];

		//If GROUND tile, delete any spikes on it first
		if (tempGridItem.placementType == MasterGridManager.GROUND)
		{
			for (int spikeIndex = 0; spikeIndex < tempGridItem.gameobject.transform.childCount; spikeIndex++)
			{
				if (tempGridItem.gameobject.transform.GetChild(spikeIndex).gameObject.layer == LayerMask.NameToLayer("Hazard"))
					masterGrid.RemoveGridItemFromList(tempGridItem.gameobject.transform.GetChild(spikeIndex).gameObject, ref masterGrid.freeformTileManager.FreeformGridItems, true, false);
			}

			//This is needed because by removing^ the spike tiles, the freeformGridIndex of the ground is now different
			//And if you dont do this, you simply get outOfIndex error.
			receivedIndex = (ushort) masterGrid.GetGridItemIndexFromList(tempGridItem.gameobject, ref masterGrid.freeformTileManager.FreeformGridItems);		
		}
			
		masterGrid.RemoveGridItemFromList((int)receivedIndex, ref masterGrid.freeformTileManager.FreeformGridItems, true, false);
	}

	public void ConvertStaticTileToFreeform(RpcArgs args)
	{
		receivedIndex = args.GetNext<ushort>();
		receivedGridType = args.GetNext<byte>();

		masterGrid.freeformTileManager.DetermineFreeform(masterGrid.GetStaticGridFromEnum(receivedGridType)[receivedIndex].gameobject.GetComponent<Collider2D>());
	}

	public void MoveFreeformTile(RpcArgs args)
	{
		receivedIndex = args.GetNext<ushort>();
		receivedFreeformGridX = args.GetNext<float>();
		receivedFreeformGridY = args.GetNext<float>();

		//Debug.Log("Received Index: " + receivedIndex + " of gameobject name in here which is: " + masterGrid.freeformTileManager.FreeformGridItems[receivedIndex].gameobject.name);

		if (receivedIndex > -1 && receivedIndex < masterGrid.freeformTileManager.FreeformGridItems.Count)
			masterGrid.freeformTileManager.SetFreeformItemPosition(masterGrid.freeformTileManager.FreeformGridItems[receivedIndex].gameobject, receivedFreeformGridX, receivedFreeformGridY, -0.6f);
	}

	public void MoveFreeformTileFinalized(RpcArgs args)
	{
		MoveFreeformTile(args);

		receivedIndex = args.GetNext<ushort>();
		receivedFreeformGridX = args.GetNext<float>();
		receivedFreeformGridY = args.GetNext<float>();

		if (receivedIndex > -1 && receivedIndex < masterGrid.freeformTileManager.FreeformGridItems.Count)
			masterGrid.entranceExitTileManager.DetermineCheckpointMovementToEntrance(masterGrid.freeformTileManager.FreeformGridItems[receivedIndex].gameobject);
	}

	public void SaveLevel(RpcArgs args)
	{
		LevelEditorMenu.globalInstance.networkSendBackFlag = false;
		LevelEditorMenu.globalInstance.SaveLevel();
	}

	public void LoadLevel(RpcArgs args)
	{
		masterGrid.serializationTileManager.networkSendBackFlag = false;
		masterGrid.serializationTileManager.LoadLevel(args.GetNext<byte[]>());
	}

	public void PlayLevel(RpcArgs args)
	{
		if (LevelEditorMenu.isPlayMode == false)
		{
			LevelEditorMenu.globalInstance.networkSendBackFlag = false;
			LevelEditorMenu.globalInstance.TogglePlayMode();
		}	
	}

	public void StopLevel(RpcArgs args)
	{
		if (LevelEditorMenu.isPlayMode == true)
		{
			LevelEditorMenu.globalInstance.networkSendBackFlag = false;
			LevelEditorMenu.globalInstance.TogglePlayMode();
		}
	}

	public void ClearAllLevel(RpcArgs args)
	{
		masterGrid.networkSendBackFlag = false;
		masterGrid.ClearAllGrid(false);
	}
}
