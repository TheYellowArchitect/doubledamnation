using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;

public class InputTileManager : MonoBehaviour 
{
	private MasterGridManager masterGrid;
	private GridItem tempGridItem;
	private GameObject tempTile;
	private int tempInt;

	public Camera levelEditorCamera;
	private Camera activeCamera;
	private Vector3 currentCameraViewportPosition;

	[Header("Mouse")]
		[ReadOnly]
		public Vector3 previousMousePosition;
		[ReadOnly]
		public Vector3 currentMousePosition;
		[ReadOnly]
		public int gridX;
		[ReadOnly]
		public int gridY;
		[ReadOnly]
		public GameObject latestLeftClickedGameObject = null;
		[ReadOnly]
		public Vector2 middleMouseClickedPosition = Vector2.zero;

		public byte selectedPlacementType = MasterGridManager.GROUND;
		public byte lastSelectedPlacementType = MasterGridManager.GROUND;

		public GameObject tileSummonVFX1;

	[Header("Debug")]
		public bool showMouseLocation = false;
		public bool showMouseTile = false;
		public bool showMouseViewportLocation = false;

	private RaycastHit2D tempRaycastHit;

	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();
		activeCamera = levelEditorCamera;

		//Disable UI Navigation input
		//So when you move to the right, it won't change selection of the placementType!
		GameObject.FindGameObjectWithTag("EventSystem").GetComponent<EventSystem>().sendNavigationEvents = false;
	}

	// Update is called once per frame
	void Update () 
	{
		DetermineMouseWorldPosition();

		DetermineLeftRightMouseInput();

		DetermineDebugHotkeys();

		DeterminePlacementTypeHotkeys();
	}

	public void LateUpdate()
	{
		//If not dragging with middle mouse click, while at edges, move the camera with edge viewpoint
		if (middleMouseClickedPosition == Vector2.zero)
			masterGrid.cameraTileManager.SetViewportPosition(currentCameraViewportPosition);

		DetermineMiddleMouseInput();
	}

	public void DetermineMouseWorldPosition()
	{
		currentMousePosition = activeCamera.ScreenToWorldPoint(Input.mousePosition);
		if (currentMousePosition != previousMousePosition)
		{
			gridX = (int) (currentMousePosition.x / masterGrid.tileOffset);
			gridY = (int) (currentMousePosition.y / masterGrid.tileOffset);

			DetermineGridExpansion();

			//====================
			//===Viewport stuff===
			//====================

			currentCameraViewportPosition = activeCamera.ScreenToViewportPoint(Input.mousePosition);

			if (showMouseLocation)
				Debug.Log(currentMousePosition);
			if (showMouseTile)
				Debug.Log("Tile currently is: " + gridX + ", " + gridY);
			if (showMouseViewportLocation)
				Debug.Log("Viewport is:" + currentCameraViewportPosition);
				//(0.0,0.0 is bottom left, 1.0,1.0 is top right) -> both are pretty lax aka no in between values eg 0.96f to 1.0f for peak, its 0.9f to 1.0f

			//===================================
			//===Update freeform item position===
			//===   to be the same as mouse   ===
			//===================================
			if (latestLeftClickedGameObject != null)
			{
				NetworkSendFreeformTile(latestLeftClickedGameObject);
				
				masterGrid.freeformTileManager.SetFreeformItemPosition(latestLeftClickedGameObject, currentMousePosition.x, currentMousePosition.y, -0.6f);
			}
			

			
				
		}

		previousMousePosition = currentMousePosition;
	}

	//If near borders of grid, create more of the grid!
	public void DetermineGridExpansion()
	{
		if (gridX > masterGrid.maxTilesSpawned - masterGrid.tileSpawnThresholdOffset || gridY > masterGrid.maxTilesSpawned - masterGrid.tileSpawnThresholdOffset)
			masterGrid.GenerateGridTiles(masterGrid.maxTilesSpawned + masterGrid.tileSpawnCount);
	}

	public void DetermineMiddleMouseInput()
	{
		masterGrid.cameraTileManager.ZoomLevelEditorCamera(Input.GetAxis("Mouse ScrollWheel"));

		//And now we check for middle mouse click!
		if (Input.GetMouseButtonDown(2))
			middleMouseClickedPosition = currentMousePosition;//auto-cast?

		if (Input.GetMouseButtonUp(2))
			middleMouseClickedPosition = Vector2.zero;

		if (middleMouseClickedPosition != Vector2.zero && Input.GetMouseButton(2))
			masterGrid.cameraTileManager.MoveCameraTowards(middleMouseClickedPosition - (Vector2) currentMousePosition);
	}

	public void DetermineLeftRightMouseInput()
	{
		//If on UI (without this, selecting a PlacementType tile, would also create one behind the UI)
		if (currentCameraViewportPosition.x < 0.15f)
			return;

		//If on PlayButton
		if (currentCameraViewportPosition.x < 0.251f && currentCameraViewportPosition.x > 0.1724f)
			if (currentCameraViewportPosition.y < 0.18f && currentCameraViewportPosition.y > 0.034f)
				return;
		//Debug.Log("[X,Y] = " + currentCameraViewportPosition.x + ", " + currentCameraViewportPosition.y);

		//First-Pressed Left-Click (Freeform move)
		if (Input.GetMouseButtonDown(0))
		{
			tempRaycastHit = Physics2D.Raycast(currentMousePosition, Vector2.zero);

			//if (tempRaycastHit != null)
			//Debug.Log("Raycast hit: " + tempRaycastHit.collider.name);

			if (tempRaycastHit.collider != null)
			{
				if (NetworkCommunicationController.globalInstance != null)
					NetworkSendConvertStaticTileToFreeform(tempRaycastHit.collider.gameObject);

				latestLeftClickedGameObject = masterGrid.freeformTileManager.DetermineFreeform(tempRaycastHit.collider);
			}

			
		}

		//Hold Left-Click (Create)
		if (Input.GetMouseButton(0))
		{
			//Raycast to check if empty
			tempRaycastHit = Physics2D.Raycast(activeCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if (tempRaycastHit.collider == null)
			{
				if (GetTileValid() == false)
					return;

				//If tile, try to merge!
				if (selectedPlacementType == MasterGridManager.GROUND)
				{
					//CreateSingleTile
					masterGrid.groundTileManager.CreateStaticGround(gridX, gridY);

					//Check adjacent tiles and merge
					masterGrid.groundTileManager.DetermineGroundTileAdjacency(gridX, gridY);

					//If online, send to the other player
					if (NetworkCommunicationController.globalInstance != null)
						NetworkCommunicationController.globalInstance.SendLevelEditorCreateStaticTile(selectedPlacementType, (ushort) gridX, (ushort) gridY);
				}
				else if (selectedPlacementType == MasterGridManager.DEFAULTPLATFORM || selectedPlacementType == MasterGridManager.FALLINGPLATFORM || selectedPlacementType == MasterGridManager.BOOSTPLATFORM)
					masterGrid.platformTileManager.CreateStaticPlatform(currentMousePosition, selectedPlacementType);
				else if (selectedPlacementType == MasterGridManager.HAZARD)
				{
					masterGrid.hazardTileManager.CreateStaticHazard(gridX, gridY);

					//If online, send to the other player
					if (NetworkCommunicationController.globalInstance != null)
						NetworkCommunicationController.globalInstance.SendLevelEditorCreateStaticTile(selectedPlacementType, (ushort) gridX, (ushort) gridY);
				}
				else if (selectedPlacementType >= MasterGridManager.HOLLOW && selectedPlacementType <= MasterGridManager.SPIKEBOI)
				{
					masterGrid.enemyTileManager.CreateStaticEnemy(gridX, gridY, selectedPlacementType, LevelEditorMenu.isPlayMode);

					//If online, send to the other player
					if (NetworkCommunicationController.globalInstance != null)
						NetworkCommunicationController.globalInstance.SendLevelEditorCreateStaticTile(selectedPlacementType, (ushort) gridX, (ushort) gridY);
				}
				else if (selectedPlacementType == MasterGridManager.ENTRANCE || selectedPlacementType == MasterGridManager.BRAZIER)
				{
					//TODO: Voiceline
					Debug.LogError("Tried to create entrance!");
					return;
				}
				else if (selectedPlacementType == MasterGridManager.EXIT)
				{
					masterGrid.entranceExitTileManager.CreateExit(currentMousePosition.x, currentMousePosition.y);

					//If online, send to the other player
					if (NetworkCommunicationController.globalInstance != null)
						NetworkCommunicationController.globalInstance.SendLevelEditorCreateFreeformTile(selectedPlacementType, currentMousePosition.x, currentMousePosition.y);
				}
					

				//VFX to add feedback per tile, aka feel polished and good per click
				Instantiate(tileSummonVFX1, new Vector3(currentMousePosition.x, currentMousePosition.y, 0), Quaternion.identity);
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			masterGrid.entranceExitTileManager.DetermineCheckpointMovementToEntrance(latestLeftClickedGameObject);

			NetworkSendFreeformTile(latestLeftClickedGameObject, true);

			latestLeftClickedGameObject = null;
		}
			

		//If mouse sidebuttons, freeform placement!
		if (Input.GetMouseButtonDown(3) || Input.GetMouseButtonDown(4))
		{
			//Raycast to check if empty
			tempRaycastHit = Physics2D.Raycast(activeCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if (tempRaycastHit.collider == null)
			{
				if (GetTileValid() == false)
					return;

				//If tile, try to merge!
				if (selectedPlacementType == MasterGridManager.GROUND)
					masterGrid.groundTileManager.CreateFreeformGround(currentMousePosition.x, currentMousePosition.y, 3);//instant 8x8, because otherwise it takes a million clicks! 4x4 freeform is 2 clicks anyway and very autistic to spam, hence this is better
				else if (selectedPlacementType == MasterGridManager.DEFAULTPLATFORM || selectedPlacementType == MasterGridManager.FALLINGPLATFORM || selectedPlacementType == MasterGridManager.BOOSTPLATFORM)
					masterGrid.platformTileManager.CreateFreeformPlatform(currentMousePosition.x , currentMousePosition.y, selectedPlacementType);
				else if (selectedPlacementType == MasterGridManager.HAZARD)
					masterGrid.hazardTileManager.CreateFreeformHazard(currentMousePosition.x, currentMousePosition.y, false);
				else if (selectedPlacementType >= MasterGridManager.HOLLOW && selectedPlacementType <= MasterGridManager.SPIKEBOI)
					masterGrid.enemyTileManager.CreateFreeformEnemy(currentMousePosition.x, currentMousePosition.y, selectedPlacementType, LevelEditorMenu.isPlayMode);
				else if (selectedPlacementType == MasterGridManager.ENTRANCE || selectedPlacementType == MasterGridManager.BRAZIER)
				{
					//TODO: Voiceline
					Debug.LogError("Tried to create entrance!");
					return;
				}
				else if (selectedPlacementType == MasterGridManager.EXIT)
					masterGrid.entranceExitTileManager.CreateExit(currentMousePosition.x, currentMousePosition.y);

				//If online, send to the other player
				if (NetworkCommunicationController.globalInstance != null)
					NetworkCommunicationController.globalInstance.SendLevelEditorCreateFreeformTile(selectedPlacementType, currentMousePosition.x, currentMousePosition.y);

				//VFX to add feedback per tile, aka feel polished and good per click
				Instantiate(tileSummonVFX1, new Vector3(currentMousePosition.x, currentMousePosition.y, 0), Quaternion.identity);
			}
		}

		//Right-Click (Destroy)
		if (Input.GetMouseButtonDown(1))
		{
            tempRaycastHit = Physics2D.Raycast(activeCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

			//If hit something
            if(tempRaycastHit.collider != null)
			{
				int staticGridIndex = -1;

				//=============================================
				//===Find where it belongs, then destroy it.===
				//=============================================

				
				if (tempRaycastHit.collider.gameObject.CompareTag("Ground"))
				{
					staticGridIndex = masterGrid.GetGridItemIndexFromList(tempRaycastHit.collider.gameObject, ref masterGrid.groundTileManager.StaticGridGroundItems);
					if (staticGridIndex != -1)
					{
						List<int> gridIndexValuesList = masterGrid.GetGridItemLocationsFromList(staticGridIndex, ref masterGrid.groundTileManager.StaticGridGroundItems);
					
						masterGrid.RemoveGridItemFromList(staticGridIndex, ref masterGrid.groundTileManager.StaticGridGroundItems);

						masterGrid.hazardTileManager.UpdateHazardAdjacency(gridIndexValuesList.ToArray());

						//If online, send to the other player
						if (NetworkCommunicationController.globalInstance != null)
							NetworkCommunicationController.globalInstance.SendLevelEditorRemoveStaticTile(0, (ushort)staticGridIndex);
					}
				}
				else if (tempRaycastHit.collider.gameObject.CompareTag("Platform"))
				{
					staticGridIndex = masterGrid.GetGridItemIndexFromList(tempRaycastHit.collider.gameObject, ref masterGrid.platformTileManager.StaticGridPlatformItems);
					if (staticGridIndex != -1)
					{
						masterGrid.RemoveGridItemFromList(staticGridIndex, ref masterGrid.platformTileManager.StaticGridPlatformItems);

						//If online, send to the other player
						if (NetworkCommunicationController.globalInstance != null)
							NetworkCommunicationController.globalInstance.SendLevelEditorRemoveStaticTile(1, (ushort)staticGridIndex);
					}
				}
				else if (tempRaycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Hazard"))
				{
					staticGridIndex = masterGrid.GetGridItemIndexFromList(tempRaycastHit.collider.gameObject, ref masterGrid.hazardTileManager.StaticGridHazardItems);
					
					if (staticGridIndex != -1)
					{
						masterGrid.RemoveGridItemFromList(staticGridIndex, ref masterGrid.hazardTileManager.StaticGridHazardItems);

						//If online, send to the other player
						if (NetworkCommunicationController.globalInstance != null)
							NetworkCommunicationController.globalInstance.SendLevelEditorRemoveStaticTile(2, (ushort)staticGridIndex);
					}
						
				}
				else if (tempRaycastHit.collider.gameObject.CompareTag("Enemy"))
				{
					staticGridIndex = masterGrid.GetGridItemIndexFromList(tempRaycastHit.collider.gameObject, ref masterGrid.enemyTileManager.StaticGridEnemyItems);
					if (staticGridIndex != -1)
					{
						masterGrid.RemoveGridItemFromList(staticGridIndex, ref masterGrid.enemyTileManager.StaticGridEnemyItems);

						//If online, send to the other player
						if (NetworkCommunicationController.globalInstance != null)
							NetworkCommunicationController.globalInstance.SendLevelEditorRemoveStaticTile(3, (ushort)staticGridIndex);
					}
				}
				else if (tempRaycastHit.collider.gameObject.layer == LayerMask.NameToLayer("PlayerExclusively"))//or could have used tag "Cutscene"
				{
					//If entrance, voiceline and gtfo
					if (tempRaycastHit.collider.gameObject.GetComponent<TriggerLevelCutscene>() != null)
					{
						latestLeftClickedGameObject = null;
						return;
					}
				}
				else if (tempRaycastHit.collider.gameObject.CompareTag("Brazier"))
				{
					latestLeftClickedGameObject = null;
					return;
				}
				
				//If not static, but freeform
				if (staticGridIndex == -1)
				{
					int freeformGridIndex = masterGrid.GetGridItemIndexFromList(tempRaycastHit.collider.gameObject, ref masterGrid.freeformTileManager.FreeformGridItems);
					if (freeformGridIndex != -1)
					{
						//If online, send to the other player
						if (NetworkCommunicationController.globalInstance != null)
							NetworkCommunicationController.globalInstance.SendLevelEditorRemoveFreeformTile((ushort)freeformGridIndex);

						//If ground, delete any spikes on it
						if (tempRaycastHit.collider.gameObject.CompareTag("Ground"))
						{
							for (int spikeIndex = 0; spikeIndex < tempRaycastHit.collider.gameObject.transform.childCount; spikeIndex++)
								if (tempRaycastHit.collider.gameObject.transform.GetChild(spikeIndex).gameObject.layer == LayerMask.NameToLayer("Hazard"))
									masterGrid.RemoveGridItemFromList(tempRaycastHit.collider.gameObject.transform.GetChild(spikeIndex).gameObject, ref masterGrid.freeformTileManager.FreeformGridItems, true, false);

							//This is needed because by removing^ the spike tiles, the freeformGridIndex of the ground is now different
							//And if you dont do this, you simply get outOfIndex error.
							freeformGridIndex = masterGrid.GetGridItemIndexFromList(tempRaycastHit.collider.gameObject, ref masterGrid.freeformTileManager.FreeformGridItems);		
						}

						masterGrid.RemoveGridItemFromList(freeformGridIndex, ref masterGrid.freeformTileManager.FreeformGridItems, true, false);
					}
					else//probably warrior or something
						Debug.LogError("An item which isn't in any static or freeform list?!" + tempRaycastHit.collider.gameObject.name);
				}

				latestLeftClickedGameObject = null;

			}
		}
	}

	public bool GetTileValid()
	{
		//Check if the tile is within grid bounds
			//[0]
			if (gridX < 0 || gridY < 0)
				return false;
			//[max]
			if (gridX > masterGrid.maxTilesSpawned - 1 || gridY > masterGrid.maxTilesSpawned - 1)
				return false;

		//Check if any tile exists on top of default grid!
			if (masterGrid.DefaultGridOccupied[gridX, gridY] == true)
				return false;

		return true;
	}

	public void DetermineDebugHotkeys()
	{
		if (Input.GetKeyDown(KeyCode.Delete))
			masterGrid.ClearAllGrid(false);//This shouldnt be debug functionality but normal.

		if (GameManager.testing == false)
			return;

		//===================
		//===Display Grids===
		//===================
		
		if (Input.GetKeyDown(KeyCode.G))
			masterGrid.DebugGridItemsFromList(ref masterGrid.groundTileManager.StaticGridGroundItems, "Ground");

		if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.RightShift) == false)
			masterGrid.DebugGridItemsFromList(ref masterGrid.platformTileManager.StaticGridPlatformItems, "Platform");

		if (Input.GetKeyDown(KeyCode.E))
			masterGrid.DebugGridItemsFromList(ref masterGrid.enemyTileManager.StaticGridEnemyItems, "Enemy");

		if (Input.GetKeyDown(KeyCode.H))
			masterGrid.DebugGridItemsFromList(ref masterGrid.hazardTileManager.StaticGridHazardItems, "Hazard");

		if (Input.GetKeyDown(KeyCode.F))
			masterGrid.DebugGridItemsFromList(ref masterGrid.freeformTileManager.FreeformGridItems, "Freeform");
		
		if (Input.GetKeyDown(KeyCode.D))
			masterGrid.DebugOccupiedGridItems();

		//========================
		//===Byte Serialization===
		//========================

		/*
		if (Input.GetKeyDown(KeyCode.S))
			masterGrid.serializationTileManager.LevelSaveWrapper();
		
		if (Input.GetKeyDown(KeyCode.O))
			masterGrid.serializationTileManager.PrintLatestLevel();

		if (Input.GetKeyDown(KeyCode.L))
			masterGrid.serializationTileManager.LoadLatestLevel();

		if (Input.GetKeyDown(KeyCode.B))
			masterGrid.serializationTileManager.PrintFinalByteArray();
		*/

		//==============
		//===Data URI===
		//==============

		if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.RightShift))
			masterGrid.serializationTileManager.ConvertLatestLevelToBase64String();

		//==========
		//===Misc===
		//==========

		//if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.RightShift))
			//masterGrid.entranceExitTileManager.DetermineBrazier();
	}

	//https://docs.unity3d.com/ScriptReference/KeyCode.html
	public void DeterminePlacementTypeHotkeys()
	{
		//Note we start from 1, even though ground is 0, so tl;dr its the original + 1
		if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
			selectedPlacementType = MasterGridManager.GROUND;
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
			selectedPlacementType = MasterGridManager.DEFAULTPLATFORM;
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
			selectedPlacementType = MasterGridManager.FALLINGPLATFORM;
		else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
			selectedPlacementType = MasterGridManager.BOOSTPLATFORM;
		else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
			selectedPlacementType = MasterGridManager.HAZARD;
		else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
			selectedPlacementType = MasterGridManager.HOLLOW;
		else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
			selectedPlacementType = MasterGridManager.SATYR;
		else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
			selectedPlacementType = MasterGridManager.CENTAUR;
		else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
			selectedPlacementType = MasterGridManager.CYCLOPS;
		else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
			selectedPlacementType = MasterGridManager.MINOTAUR;
		else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
			selectedPlacementType = MasterGridManager.HARPY;
		else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadEquals))
			selectedPlacementType = MasterGridManager.SPIKEBOI;

		//Dont want to update the UI each frame ;)
		if (selectedPlacementType != lastSelectedPlacementType)
		{
			lastSelectedPlacementType = selectedPlacementType;

			//Update the UI
			if (LevelEditorMenu.globalInstance != null)
			{
				Debug.Log("Running this with selectedPlacementType == " + (int)selectedPlacementType);
				LevelEditorMenu.globalInstance.CacheButtonColor((int)selectedPlacementType);
			}
		}

		
	}

	//Clicked the top button panels with mouse
	public void DeterminePlacementTypeClick(int placementEnum)
	{
		if (placementEnum == 1)
			selectedPlacementType = MasterGridManager.GROUND;
		else if (placementEnum == 2)
			selectedPlacementType = MasterGridManager.DEFAULTPLATFORM;
		else if (placementEnum == 3)
			selectedPlacementType = MasterGridManager.FALLINGPLATFORM;
		else if (placementEnum == 4)
			selectedPlacementType = MasterGridManager.BOOSTPLATFORM;
		else if (placementEnum == 5)
			selectedPlacementType = MasterGridManager.HAZARD;
		else if (placementEnum == 6)
			selectedPlacementType = MasterGridManager.HOLLOW;
		else if (placementEnum == 7)
			selectedPlacementType = MasterGridManager.SATYR;
		else if (placementEnum == 8)
			selectedPlacementType = MasterGridManager.CENTAUR;
		else if (placementEnum == 9)
			selectedPlacementType = MasterGridManager.CYCLOPS;
		else if (placementEnum == 10)
			selectedPlacementType = MasterGridManager.MINOTAUR;
		else if (placementEnum == 11)
			selectedPlacementType = MasterGridManager.HARPY;
		else if (placementEnum == 12)
			selectedPlacementType = MasterGridManager.SPIKEBOI;
		else if (placementEnum == 13)
			selectedPlacementType = MasterGridManager.ENTRANCE;
		else if (placementEnum == 14)
			selectedPlacementType = MasterGridManager.EXIT;
	}

	public void UpdateActiveCamera(Camera newCamera)
	{
		activeCamera = newCamera;
	}

	public void NetworkSendConvertStaticTileToFreeform(GameObject staticTileGameObject)
	{
		int staticGridIndex = -2;

		if (staticTileGameObject.CompareTag("Ground"))
		{
			staticGridIndex = masterGrid.GetGridItemIndexFromList(staticTileGameObject, ref masterGrid.groundTileManager.StaticGridGroundItems);
			tempInt = 0;
		}
		else if (staticTileGameObject.CompareTag("Platform"))
		{
			staticGridIndex = masterGrid.GetGridItemIndexFromList(staticTileGameObject, ref masterGrid.platformTileManager.StaticGridPlatformItems);
			tempInt = 1;
		}
		else if (staticTileGameObject.layer == LayerMask.NameToLayer("Hazard"))
		{
			staticGridIndex = masterGrid.GetGridItemIndexFromList(staticTileGameObject, ref masterGrid.hazardTileManager.StaticGridHazardItems);
			tempInt = 2;
		}
		else if (staticTileGameObject.CompareTag("Enemy"))
		{
			staticGridIndex = masterGrid.GetGridItemIndexFromList(staticTileGameObject, ref masterGrid.enemyTileManager.StaticGridEnemyItems);
			tempInt = 3;
		}
		else
			tempInt = 4;
			

		if (tempInt != 4 && staticGridIndex > -1)
			NetworkCommunicationController.globalInstance.SendLevelEditorConvertTileToFreeform((ushort)staticGridIndex, (byte)tempInt);
		else if (staticTileGameObject.layer == LayerMask.NameToLayer("Hazard"))//Convert freeform spike tile to single
		{
			staticGridIndex = masterGrid.GetGridItemIndexFromList(staticTileGameObject, ref masterGrid.freeformTileManager.FreeformGridItems);
			tempInt = 4;
			//Debug.Log("Convert hazard index: " + staticGridIndex + "of gridEnum " + tempInt);
			NetworkCommunicationController.globalInstance.SendLevelEditorConvertTileToFreeform((ushort)staticGridIndex, (byte)tempInt);
		}
	}

	public void NetworkSendFreeformTile(GameObject sendingGameObject, bool finalized = false)
	{
		if (NetworkCommunicationController.globalInstance != null && sendingGameObject != null)
		{
			tempInt = masterGrid.GetGridItemIndexFromList(sendingGameObject, ref masterGrid.freeformTileManager.FreeformGridItems);
			//Debug.Log("Sent index is: " + tempInt + "of name: " + sendingGameObject);
			if (tempInt != -1)
			{
				if (finalized == false)
					NetworkCommunicationController.globalInstance.SendLevelEditorMoveFreeformTile((ushort)tempInt, currentMousePosition.x, currentMousePosition.y);
				else
					NetworkCommunicationController.globalInstance.SendLevelEditorMoveFreeformTileFinalized((ushort)tempInt, currentMousePosition.x, currentMousePosition.y);
			}
			else
				Debug.LogError("Online level editor error - While moving gameobject, trying to send its index it says it doesnt exist");
		}

	}
}
