using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/*Useful Byte Manipulation Links:
	[]https://www.dotnetperls.com/tobase64string (Bless.)
	[]https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/how-to-convert-a-byte-array-to-an-int
	[]https://docs.microsoft.com/en-us/dotnet/api/system.bitconverter?view=net-6.0
	[]https://stackoverflow.com/questions/2660232/convert-2-bytes-to-a-number
	[]https://docs.microsoft.com/en-us/dotnet/api/system.convert.frombase64string?view=net-6.0
*/

public class SerializationTileManager : MonoBehaviour 
{
	private MasterGridManager masterGrid;

	public byte[] test = new byte[5];

	private string levelsFolderPath;
	private string tempLevelFileName;
	public string latestSavedFileName;

	public string levelFilenameTitle = "_";
	public string levelFilenameStringURILoadedTitle = "StringURI";
	public string levelFilenameExtension = "wind";

	//I use this to start the level from here, instead of [0][0]
	//Could be [350][295] up to [500][350]. This would mean its fully 256 optimized and each tile costs 1 byte, instead of 2.
	public int minimumTileX;
	public int minimumTileY;

	public int currentByteIndex = 0;
	public byte[] finalByteArray = new byte[1000];
	public byte[] tempByteArray;

	public const byte NEXTSECTIONBUFFER = 255;
	public const float FREEFORMSECTIONBUFFER = 5200f;

	private Vector3 tempVector3;

	//Instead of adding a new bool parameter to each function, this flag turns true when RPC is received, so it won't send it to the other player!
	public bool networkSendBackFlag = true;

	// Use this for initialization
	public void Initialize ()
	{
		masterGrid = GetComponent<MasterGridManager>();

		Array.Resize(ref finalByteArray, 10);

		//Setting the screenshotFolderPath, tl;dr: the game's folder -> Screenshots folder
		levelsFolderPath = Application.persistentDataPath + "/Levels";
	}

	public void LevelSaveWrapper(string levelTitle = "")
	{
		//If Levels folder doesnt exist, create that folder
		if (Directory.Exists(levelsFolderPath) == false)
			Directory.CreateDirectory(levelsFolderPath);

		//If no title, then enumerate and +1 the filename, aka procedurally generated
		if (levelTitle == "")
		{
			//Now we got to get proper naming so we don't override the filename by default.
			//To do this, we just enumerate the levels inside the folder and give it +1 in naming.

			//So, first we get the number of levels inside the folder
			int levelsFolderCount = Directory.GetFiles(levelsFolderPath, "*." + levelFilenameExtension).Length;

			//And now we make the name.
			tempLevelFileName = levelFilenameTitle + (levelsFolderCount + 1) + "." + levelFilenameExtension;
		}
		else
			tempLevelFileName = levelTitle + "." + levelFilenameExtension;

		//Get masterGrid.Lists<> and set them into bytes, so as to save them.
		//This manipulates and sets finalByteArray
		SerializeBytes();

		latestSavedFileName = levelsFolderPath + "/" + tempLevelFileName;

		//If .NET 6.0, Asyncwrite. But not worth the upgrade (no insanity mode, obsolete code aka refactor, new bugs etc etc)
		File.WriteAllBytes(latestSavedFileName, finalByteArray);

		Debug.Log("Saving at path: " + latestSavedFileName);
	}


	public void SerializeBytes()
	{
		finalByteArray[0] = DetermineHeaderByte();

		finalByteArray[1] = (byte) GameObject.FindGameObjectWithTag("Canvas").GetComponent<LevelEditorMenu>().GetHealth();

		currentByteIndex = 2;

		DetermineMinimumTileOffset();

		AddShortToFinalByteArray(minimumTileX);

		AddShortToFinalByteArray(minimumTileY);

		currentByteIndex = 6;

		//====================================
		//===256 Optimization (1 byte/tile)===
		//====================================

		//Ground
		Set256Section(ref masterGrid.groundTileManager.StaticGridGroundItems, MasterGridManager.GROUND);
		
		//Platforms
			//Default Platform
			Set256Section(ref masterGrid.platformTileManager.StaticGridPlatformItems, MasterGridManager.DEFAULTPLATFORM);

			//Falling Platform
			Set256Section(ref masterGrid.platformTileManager.StaticGridPlatformItems, MasterGridManager.FALLINGPLATFORM);

			//Level3BoostPlatform
			Set256Section(ref masterGrid.platformTileManager.StaticGridPlatformItems, MasterGridManager.BOOSTPLATFORM);

		//Hazard
		Set256Section(ref masterGrid.hazardTileManager.StaticGridHazardItems, MasterGridManager.HAZARD);

		//Enemies
			//Hollow
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.HOLLOW);

			//Satyr
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.SATYR);

			//Centaur
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.CENTAUR);

			//Cyclops
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.CYCLOPS);

			//Minotaur
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.MINOTAUR);

			//Harpy
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.HARPY);

			//Spikeboi
			Set256Section(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.SPIKEBOI);

		//==================================
		//===Purely Static (2 bytes/tile)===
		//==================================
		//Place those which are greater than minimumX/Y + 254

		//Ground
		SetDefaultStaticSection(ref masterGrid.groundTileManager.StaticGridGroundItems, MasterGridManager.GROUND);

		//Platforms
			//Default Platform
			SetDefaultStaticSection(ref masterGrid.platformTileManager.StaticGridPlatformItems, MasterGridManager.DEFAULTPLATFORM);

			//Falling Platform
			SetDefaultStaticSection(ref masterGrid.platformTileManager.StaticGridPlatformItems, MasterGridManager.FALLINGPLATFORM);

			//Level3BoostPlatform
			SetDefaultStaticSection(ref masterGrid.platformTileManager.StaticGridPlatformItems, MasterGridManager.BOOSTPLATFORM);

		//Hazard
		SetDefaultStaticSection(ref masterGrid.hazardTileManager.StaticGridHazardItems, MasterGridManager.HAZARD);

		//Enemies
			//Hollow
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.HOLLOW);

			//Satyr
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.SATYR);

			//Centaur
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.CENTAUR);

			//Cyclops
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.CYCLOPS);

			//Minotaur
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.MINOTAUR);

			//Harpy
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.HARPY);

			//Spikeboi
			SetDefaultStaticSection(ref masterGrid.enemyTileManager.StaticGridEnemyItems, MasterGridManager.SPIKEBOI);


		//=============================
		//===Freeform (4 bytes/tile)===
		//=============================
		//Place those non-static freeform tiles which are decimal and unrestricted (greater than 0 though)

		//Ground
			SetFreeformGroundSection("4x4");//Single
			SetFreeformGroundSection("4x8");//Vertical
			SetFreeformGroundSection("8x4");//Horizontal
			SetFreeformGroundSection("RepeatingFat");//aka double
			//^A crazy idea I had is to use their collider size, instead of stringname.
			//Crazy because deserializing that, I would have to GetComponent<Collider> of all freeform ground objects, exclusively for deserialization. Bloat!

		//Platforms
			//Default Platform
			SetFreeformSection(MasterGridManager.DEFAULTPLATFORM);

			//Falling Platform
			SetFreeformSection(MasterGridManager.FALLINGPLATFORM);

			//Level3BoostPlatform
			SetFreeformSection(MasterGridManager.BOOSTPLATFORM);

		//Hazard
			SetFreeformHazardSection(false);
			SetFreeformHazardSection(true);

		//Enemies
			//Hollow
			SetFreeformSection(MasterGridManager.HOLLOW);

			//Satyr
			SetFreeformSection(MasterGridManager.SATYR);

			//Centaur
			SetFreeformSection(MasterGridManager.CENTAUR);

			//Cyclops
			SetFreeformSection(MasterGridManager.CYCLOPS);

			//Minotaur
			SetFreeformSection(MasterGridManager.MINOTAUR);

			//Harpy
			SetFreeformSection(MasterGridManager.HARPY);

			//Spikeboi
			SetFreeformSection(MasterGridManager.SPIKEBOI);

		//=====================
		//=== Entrance Exit ===
		//=====================

		//StartPos
			SetFreeformSection(MasterGridManager.ENTRANCE, false);

		//Darkwind Brazier
			SetFreeformSection(MasterGridManager.BRAZIER, false);

		//EndPos (infinite in numbers, doesn't even need the FREEFORMBUFFER, hence removed later)
			SetFreeformSection(MasterGridManager.EXIT, false);

		//=====================
		//===Trim the ending===
		//=====================
		Array.Resize(ref finalByteArray, currentByteIndex + 1);
	}

	public void Set256Section(ref List<GridItem> GridListItems, byte acceptedPlacementType)
	{

		//Iterate all tiles of that gridlist
		for (int i = 0; i < GridListItems.Count; i++)
		{
			if (GridListItems[i].placementType == acceptedPlacementType && IsWithin256(GridListItems[i]))
			{
				//Iterate each tile's location (e.g. doubletile aka 4x4, covers 4 vector2s, so 4 iterations)
				for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
				{
					if (currentByteIndex + 4 > finalByteArray.Length)
						Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

					finalByteArray[currentByteIndex++] = (byte) (GridListItems[i].placementLocation[j].x - minimumTileX);
					finalByteArray[currentByteIndex++] = (byte) (GridListItems[i].placementLocation[j].y - minimumTileY);
				}

				//For platforms, remove the second Y, since it is redundant!
				if (GridListItems[i].placementType == MasterGridManager.DEFAULTPLATFORM || GridListItems[i].placementType == MasterGridManager.FALLINGPLATFORM || GridListItems[i].placementType == MasterGridManager.BOOSTPLATFORM)
					currentByteIndex--;
			}

		}

		if (currentByteIndex + 2 > finalByteArray.Length)
			Array.Resize(ref finalByteArray, finalByteArray.Length * 2);
		finalByteArray[currentByteIndex++] = NEXTSECTIONBUFFER;
	}

	//This checks if adding the offset, it won't go beyond 1 byte
	public bool IsWithin256(GridItem tile)
	{
		for (int i = 0; i < tile.placementLocation.Count; i++)
		{
			if ((int) tile.placementLocation[i].x - minimumTileX > (int) NEXTSECTIONBUFFER)
				return false;

			if ((int) tile.placementLocation[i].y - minimumTileY > (int) NEXTSECTIONBUFFER)
				return false;
		}

		//Debug.Log("Tile 0: " + tile.placementLocation[0].x);

		return true;
	}

	public void SetDefaultStaticSection(ref List<GridItem> GridListItems, byte acceptedPlacementType)
	{
		//Iterate all tiles of that gridlist
		for (int i = 0; i < GridListItems.Count; i++)
		{
			if (GridListItems[i].placementType == acceptedPlacementType && IsWithin256(GridListItems[i]) == false)
			{
				//Iterate each tile's location (e.g. doubletile aka 4x4, covers 4 vector2s, so 4 iterations)
				for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
				{
					if (currentByteIndex + 8 > finalByteArray.Length)
						Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

					AddShortToFinalByteArray((int)GridListItems[i].placementLocation[j].x - minimumTileX);
					AddShortToFinalByteArray((int)GridListItems[i].placementLocation[j].y - minimumTileY);
				}

				//For platforms, remove the second Y, since it is redundant!
				if (GridListItems[i].placementType == MasterGridManager.DEFAULTPLATFORM || GridListItems[i].placementType == MasterGridManager.FALLINGPLATFORM || GridListItems[i].placementType == MasterGridManager.BOOSTPLATFORM)
					currentByteIndex = currentByteIndex - 2;
			}

		}

		AddShortToFinalByteArray((ushort)NEXTSECTIONBUFFER * 10);
	}

	public void AddShortToFinalByteArray(int value)
	{
		byte[] locationBytes = new byte[2];

		locationBytes = BitConverter.GetBytes((ushort)value);

		//Debug.Log("Location Value is: " + value);
		//Debug.Log("Location Bytes[0] is " + locationBytes[0]);
		//Debug.Log("Location Bytes[1] is " + locationBytes[1]);

		if (currentByteIndex + 4 > finalByteArray.Length)
			Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

		finalByteArray[currentByteIndex++] = locationBytes[0];
		finalByteArray[currentByteIndex++] = locationBytes[1];
	}

	//Since freeform saves only 1 Vector2 position
	//You got to cache/save its single/vertical/horizontal/double in some other way
	public void SetFreeformGroundSection(string nameDifference, bool includeEndBuffer = true)
	{
		//Iterate all ground tiles of freeform gridlist
		for (int i = 0; i < masterGrid.freeformTileManager.FreeformGridItems.Count; i++)
		{
			if (masterGrid.freeformTileManager.FreeformGridItems[i].placementType == MasterGridManager.GROUND)
			{
				if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.name.Contains(nameDifference))
				{
					if (currentByteIndex + 16 > finalByteArray.Length)
						Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[0].x);
					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[0].y);
				}
			}
		}

		//End the section with a proper buffer
		if (includeEndBuffer)
			AddFloatToFinalByteArray(FREEFORMSECTIONBUFFER);
	}

	public void SetFreeformSection(byte acceptedPlacementType, bool includeEndBuffer = true)
	{
		//Iterate all tiles of that gridlist
		for (int i = 0; i < masterGrid.freeformTileManager.FreeformGridItems.Count; i++)
		{
			if (masterGrid.freeformTileManager.FreeformGridItems[i].placementType == acceptedPlacementType)
			{
				for (int j = 0; j < masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation.Count; j++)
				{
					if (currentByteIndex + 16 > finalByteArray.Length)
						Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[j].x);
					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[j].y);
				}
			}

		}

		if (includeEndBuffer)
			AddFloatToFinalByteArray(FREEFORMSECTIONBUFFER);
	}

	public void SetFreeformHazardSection(bool attachedToGround, bool includeEndBuffer = true)
	{
		//Iterate all ground tiles of freeform gridlist
		for (int i = 0; i < masterGrid.freeformTileManager.FreeformGridItems.Count; i++)
		{
			if (masterGrid.freeformTileManager.FreeformGridItems[i].placementType == MasterGridManager.HAZARD)
			{
				//If spike is NOT attached to any ground
				if (attachedToGround == false && masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.parent == masterGrid.freeformHolder.transform)
				{
					if (currentByteIndex + 16 > finalByteArray.Length)
						Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[0].x);
					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[0].y);
				}

				//If spike is attached to any ground
				else if (attachedToGround == true && masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.transform.parent != masterGrid.freeformHolder.transform)
				{
					if (currentByteIndex + 16 > finalByteArray.Length)
						Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[0].x);
					AddFloatToFinalByteArray(masterGrid.freeformTileManager.FreeformGridItems[i].placementLocation[0].y);
				}
			}
		}

		//End the section with a proper buffer
		if (includeEndBuffer)
			AddFloatToFinalByteArray(FREEFORMSECTIONBUFFER);
	}

	public void AddFloatToFinalByteArray(float value)
	{
		byte[] locationBytes = new byte[4];

		locationBytes = BitConverter.GetBytes(value);

		if (currentByteIndex + 8 > finalByteArray.Length)
			Array.Resize(ref finalByteArray, finalByteArray.Length * 2);

		finalByteArray[currentByteIndex++] = locationBytes[0];
		finalByteArray[currentByteIndex++] = locationBytes[1];
		finalByteArray[currentByteIndex++] = locationBytes[2];
		finalByteArray[currentByteIndex++] = locationBytes[3];
	}

	//=============

	//Should be a utility function, unrelated to this class imo
	public string GetLatestFileName(string path)
	{
		string[] fileNames = Directory.GetFiles(path);
		int latestWrittenIndex = 0;

		FileInfo pickedFileInfo;
		System.DateTime pickedLastModifiedDate;
		System.DateTime latestLastModifiedDate = new FileInfo(fileNames[0]).LastWriteTime;

		for (int i = 1; i < fileNames.Length; i++)
		{
			pickedFileInfo = new FileInfo(fileNames[i]);
			pickedLastModifiedDate = pickedFileInfo.LastWriteTime;

			//If this file is written later than
			if (pickedLastModifiedDate > latestLastModifiedDate)
			{
				latestLastModifiedDate = pickedLastModifiedDate;
				latestWrittenIndex = i;
			}

		}

		return fileNames[latestWrittenIndex];
	}

	public string GetLevelsFolderPath()
	{
		return levelsFolderPath;
	}

	public string GetLatestLevelFileName()
	{
		return GetLatestFileName(levelsFolderPath);
	}

	public void LoadLatestLevel()
	{
		LoadLevel(GetLatestLevelFileName());
	}

	public void LoadLevel(string targetfilenamepath)
	{
		//Get filename of latest level!
		tempLevelFileName = targetfilenamepath;

		Debug.Log("Target Filename: " + tempLevelFileName);

		//If the level we load, is already loaded, give a warning to the user.
		if (latestSavedFileName == targetfilenamepath)
		{
			//TODO: Just a voiceline with subs. No popup box.
			//"We are already here!"
			Debug.LogError("The level you are loading is already loaded.");
			return;
		}

		LoadLevel(File.ReadAllBytes(@tempLevelFileName));
	}

	public void LoadLevel(byte[] levelByteArray)
	{
		Debug.Log("BEGINNING LOADING");

		byte sectionBufferIndex = 0;
		Debug.Log("Byte 0 = " + levelByteArray[0] + " (Header)");
		GameObject.FindGameObjectWithTag("Canvas").GetComponent<LevelEditorMenu>().SetHealth( (int) levelByteArray[1]);
		Debug.Log("Byte 1 = " + levelByteArray[1] + " (Health)");
		ushort minimumOffsetX = BitConverter.ToUInt16(levelByteArray, 2);
		ushort minimumOffsetY = BitConverter.ToUInt16(levelByteArray, 4);
		Debug.Log("Byte 2 = " + minimumOffsetX +"(MinimumX)");
		Debug.Log("Byte 3 = " + minimumOffsetY +"(MinimumY)");
		Debug.Log("New Section: " + GetSectionName(sectionBufferIndex));

		//=================
		//===256 Default===
		//=================

		int byteArrayIndex = 6;
		while (sectionBufferIndex < 12)//256 section
		{

			if ((byte) levelByteArray[byteArrayIndex] == NEXTSECTIONBUFFER)
			{
				Debug.Log("New Section: " + GetSectionName(sectionBufferIndex + 1));
				sectionBufferIndex++;
			}
			else
			{
				Debug.Log("Byte " + byteArrayIndex + ", " + (byteArrayIndex+1) + " = [" + (levelByteArray[byteArrayIndex] + minimumOffsetX) + "][" + (levelByteArray[byteArrayIndex+1] + minimumOffsetY) + "] (" + GetSectionName(sectionBufferIndex) + ")");

				//If this tile is on the expanded grid, aka further than initialTilesCount
				//Then expand the grid!
				//And add +1 because [length - 1], so otherwise it will always create all tiles before the one we want -_-
				if (levelByteArray[byteArrayIndex] + minimumOffsetX + 1 > masterGrid.maxTilesSpawned)
					masterGrid.GenerateGridTiles(levelByteArray[byteArrayIndex] + minimumOffsetX + 1);
				if (levelByteArray[byteArrayIndex+1] + minimumOffsetY + 1 > masterGrid.maxTilesSpawned)
					masterGrid.GenerateGridTiles(levelByteArray[byteArrayIndex+1] + minimumOffsetY + 1);


				if (sectionBufferIndex == MasterGridManager.GROUND)
				{
					masterGrid.groundTileManager.CreateStaticGround(levelByteArray[byteArrayIndex] + minimumOffsetX, levelByteArray[byteArrayIndex+1] + minimumOffsetY);
					masterGrid.groundTileManager.DetermineGroundTileAdjacency(levelByteArray[byteArrayIndex] + minimumOffsetX, levelByteArray[byteArrayIndex+1] + minimumOffsetY);
				}
				else if (sectionBufferIndex > MasterGridManager.GROUND && sectionBufferIndex < MasterGridManager.BOOSTPLATFORM)
				{
					masterGrid.platformTileManager.CreateStaticPlatform(levelByteArray[byteArrayIndex] + minimumOffsetX, levelByteArray[byteArrayIndex+1] + minimumOffsetY, levelByteArray[byteArrayIndex+2] + minimumOffsetX, sectionBufferIndex);
					byteArrayIndex = byteArrayIndex + 1;//Because a platform has 3 X,Y, and hence for next tileobject, we need to go +3 in total
				}
				else if (sectionBufferIndex == MasterGridManager.HAZARD)
					masterGrid.hazardTileManager.CreateStaticHazard(levelByteArray[byteArrayIndex] + minimumOffsetX, levelByteArray[byteArrayIndex+1] + minimumOffsetY);
				else if (sectionBufferIndex > MasterGridManager.HAZARD && sectionBufferIndex < 12)
					masterGrid.enemyTileManager.CreateStaticEnemy(levelByteArray[byteArrayIndex] + minimumOffsetX, levelByteArray[byteArrayIndex+1] + minimumOffsetY, sectionBufferIndex);

				byteArrayIndex = byteArrayIndex + 1;//This is in addition of the ending +1, so **only when** a 256tile is made (instead of buffercheck), it does +2
			}

			byteArrayIndex = byteArrayIndex + 1;
		}

		//====================
		//===Static Default===
		//====================

		while(sectionBufferIndex < 24)//staticdefault section
		{

			if (BitConverter.ToUInt16(levelByteArray, byteArrayIndex) == (ushort)NEXTSECTIONBUFFER * 10)
			{
				Debug.Log("New Section: " + GetSectionName(sectionBufferIndex-12 + 1));
				sectionBufferIndex++;
			}
			else
			{
				Debug.Log("Byte " + byteArrayIndex + "," + (byteArrayIndex+1) + "," + (byteArrayIndex+2) + "," + (byteArrayIndex+3) + " = [" + (BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + minimumOffsetX) + "][" + (BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + minimumOffsetY) + "] (" + GetSectionName(sectionBufferIndex-12) + ")");

				//If this tile is on the expanded grid, aka further than initialTilesCount
				//Then expand the grid!
				//And add +1 because [length - 1], so otherwise it will always create all tiles before the one we want -_-
				if (BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + minimumOffsetX + 1 > masterGrid.maxTilesSpawned)
					masterGrid.GenerateGridTiles(BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + minimumOffsetX + 1);
				if (BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + minimumOffsetY + 1 > masterGrid.maxTilesSpawned)
					masterGrid.GenerateGridTiles(BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + minimumOffsetY + 1);


				if (sectionBufferIndex-12 == MasterGridManager.GROUND)
				{
					masterGrid.groundTileManager.CreateStaticGround(BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + minimumOffsetX, BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + minimumOffsetY);
					masterGrid.groundTileManager.DetermineGroundTileAdjacency(BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + minimumOffsetX, BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + minimumOffsetY);
				}
				else if (sectionBufferIndex-12 > MasterGridManager.GROUND && sectionBufferIndex-12 < MasterGridManager.BOOSTPLATFORM)
				{
					masterGrid.platformTileManager.CreateStaticPlatform(BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + (ushort) minimumOffsetX, BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + (ushort) minimumOffsetY, BitConverter.ToUInt16(levelByteArray, byteArrayIndex+4) + (ushort) minimumOffsetX, (byte)((int)sectionBufferIndex-12));
					byteArrayIndex = byteArrayIndex + 2;//Because a platform has 3 X,Y, and hence for next tileobject, we need to go +6 in total
				}
				else if (sectionBufferIndex-12 == MasterGridManager.HAZARD)
					masterGrid.hazardTileManager.CreateStaticHazard(BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + minimumOffsetX, BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + minimumOffsetY);
				else if (sectionBufferIndex-12 > MasterGridManager.HAZARD && sectionBufferIndex-12 < 12)
					masterGrid.enemyTileManager.CreateStaticEnemy(BitConverter.ToUInt16(levelByteArray, byteArrayIndex) + (ushort) minimumOffsetX, BitConverter.ToUInt16(levelByteArray, byteArrayIndex+2) + (ushort) minimumOffsetY, (byte)((int)sectionBufferIndex-12));

				byteArrayIndex = byteArrayIndex + 2;
			}

			byteArrayIndex = byteArrayIndex + 2;
		}

		//==============
		//===Freeform===
		//==============

		//When you enter here, sectionBufferIndex is 24
		while(sectionBufferIndex < 40 && byteArrayIndex < levelByteArray.Length)
		{
			if (BitConverter.ToSingle(levelByteArray, byteArrayIndex) == FREEFORMSECTIONBUFFER)
			{
				Debug.Log("New Section: " + GetFreeformSectionName(sectionBufferIndex + 1));
				sectionBufferIndex++;
			}
			else
			{
				Debug.Log("Byte " + byteArrayIndex + "," + (byteArrayIndex+1) + "," + (byteArrayIndex+2) + "," + (byteArrayIndex+3) + "," + (byteArrayIndex+4) + "," + (byteArrayIndex+5) + "," + (byteArrayIndex+6) + "," + (byteArrayIndex+7) + " = [" + BitConverter.ToSingle(levelByteArray, byteArrayIndex) + "][" + BitConverter.ToSingle(levelByteArray, byteArrayIndex+4) + "] (" + GetFreeformSectionName(sectionBufferIndex) + ")");

				if (sectionBufferIndex >= 24 && sectionBufferIndex < 28)
					masterGrid.groundTileManager.CreateFreeformGround(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), (byte)((int)sectionBufferIndex - 24));
				else if (sectionBufferIndex >= 28 && sectionBufferIndex < 31)
					masterGrid.platformTileManager.CreateFreeformPlatform(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), (byte)((int)sectionBufferIndex - 27));
				else if (sectionBufferIndex == 31)
					masterGrid.hazardTileManager.CreateFreeformHazard(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), false);
				else if (sectionBufferIndex == 32)
					masterGrid.hazardTileManager.CreateFreeformHazard(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), true);
				else if (sectionBufferIndex > 32 && sectionBufferIndex < 40)
					masterGrid.enemyTileManager.CreateFreeformEnemy(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), (byte)((int)sectionBufferIndex - 28));

				byteArrayIndex = byteArrayIndex + 4;
			}

			byteArrayIndex = byteArrayIndex + 4;
			Debug.Log("sectionBufferIndex = " + sectionBufferIndex);
		}

		//=====================================
		//=== Entrance Exit ===
		//=====================================

		//StartPos
			masterGrid.entranceExitTileManager.CreateEntrance(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4));
			tempVector3 = new Vector3(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), 0);//used for moving the player to entrance

		//Move the index forwards: 4 bytes for X of entrance, 4 bytes for Y of entrance.
			byteArrayIndex = byteArrayIndex + 8;

		//Darkwind Brazier
			masterGrid.entranceExitTileManager.CreateBrazier(new Vector3(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4), 0));

		//Move the index forwards: 4 bytes for X of entrance, 4 bytes for Y of entrance.
			byteArrayIndex = byteArrayIndex + 8;

		//EndPos (infinite in numbers, doesn't even need the FREEFORMBUFFER since its the end of the byte array)
		Debug.Log("Array Length is: " + levelByteArray.Length + "(Note that the final index possible is [" + (levelByteArray.Length-1) + "])");
		while (byteArrayIndex < levelByteArray.Length - 1)//remove that -1 and it goes for one more loop, even though there are literally no bytes to access
		{
			Debug.Log("Loaded Exit ByteArray Index is: " + byteArrayIndex);

			masterGrid.entranceExitTileManager.CreateExit(BitConverter.ToSingle(levelByteArray, byteArrayIndex), BitConverter.ToSingle(levelByteArray, byteArrayIndex+4));
			byteArrayIndex = byteArrayIndex + 8;
		}

		Debug.Log("Finished loading. The end of the byte array is reached: [" + byteArrayIndex + "], of .Length: " + levelByteArray.Length);

		//================================
		//=== Move Warrior to Entrance ===
		//================================

		//Move checkpoint at entrance
		GameObject.FindGameObjectWithTag("Checkpoint").transform.position = tempVector3 + new Vector3(0, -2, 0);

		//Set warriorCheckpoint by the above checkpoint
		LevelManager.globalInstance.SetWarriorCheckpoint();

		//Move warrior to the entrance but with offset
		GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().ResetAtLevelStart();

		//If online, send to the other player
		if (NetworkCommunicationController.globalInstance != null)
		{
			if (networkSendBackFlag == true)
				NetworkCommunicationController.globalInstance.SendLevelEditorLoad(levelByteArray);
			
			networkSendBackFlag = true;
		}
	}

	public string GetSectionName(int sectionIndex)
	{
		if (sectionIndex == MasterGridManager.GROUND)
			return "Ground";
		else if (sectionIndex == MasterGridManager.DEFAULTPLATFORM)
			return "Default Platform";
		else if (sectionIndex == MasterGridManager.FALLINGPLATFORM)
			return "Falling Platform";
		else if (sectionIndex == MasterGridManager.BOOSTPLATFORM)
			return "Level3BoostPlatform";
		else if (sectionIndex == MasterGridManager.HAZARD)
			return "Hazard";
		else if (sectionIndex == MasterGridManager.HOLLOW)
			return "Hollow";
		else if (sectionIndex == MasterGridManager.SATYR)
			return "Satyr";
		else if (sectionIndex == MasterGridManager.CENTAUR)
			return "Centaur";
		else if (sectionIndex == MasterGridManager.CYCLOPS)
			return "Cyclops";
		else if (sectionIndex == MasterGridManager.MINOTAUR)
			return "Minotaur";
		else if (sectionIndex == MasterGridManager.HARPY)
			return "Harpy";
		else if (sectionIndex == MasterGridManager.SPIKEBOI)
			return "Spikeboi";
		else if (sectionIndex == 12)
			return "END";
		
		return "null";
	}

	public string GetFreeformSectionName(int sectionIndex)
	{
		if (sectionIndex >= 24 && sectionIndex <= 27)
			return "Ground";
		else if (sectionIndex == 28)
			return "Default Platform";
		else if (sectionIndex == 29)
			return "Falling Platform";
		else if (sectionIndex == 30)
			return "Level3BoostPlatform";
		else if (sectionIndex == 31 || sectionIndex == 32)
			return "Hazard";
		else if (sectionIndex == 33)
			return "Hollow";
		else if (sectionIndex == 34)
			return "Satyr";
		else if (sectionIndex == 35)
			return "Centaur";
		else if (sectionIndex == 36)
			return "Cyclops";
		else if (sectionIndex == 37)
			return "Minotaur";
		else if (sectionIndex == 38)
			return "Harpy";
		else if (sectionIndex == 39)
			return "Spikeboi";
		else if (sectionIndex == 40)
			return "END";
		
		return "null";
	}
	
	/*
	Title isnt put onto the serialization but the filename!
	[Header]
		[8 bits/booleans] -> 0b in C# 7.0 (e.g. 0x is for hexadecimal), otherwise bitmath time on a byte.
			isOriginalFormat -> In case you do some hype optimization in the future
			bloodTimer -> Level 2 bloodtimer thing
			bool2 -> and forward to bool8 are left for future choices. Leave at least 2 empty, for booleans of 4 choices eg color
			bool3
			bool4
			bool5
			bool6
			bool7
	[offsetX256] (1 byte)
	[offsetY256] (1 byte)
	[is256Optimized section]~12 sections
		[GroundTiles] (2 bytes each)
		(1 byte gap)
		[PlatformTiles]
			[DefaultPlatforms] (2 bytes each)
			(1 byte gap)
			[FallingPlatforms] (2 bytes each)
			(1 byte gap)
			[Level3Platforms] (2 bytes each)
		(1 byte gap)
		[HazardTiles] (2 bytes each)
		(1 byte gap)
		[EnemyTiles]
			[Hollow] (2 bytes each)
			(1 byte gap)
			[Centaur] (2 bytes each)
			(1 byte gap)
			...
		(1 byte gap)
	[2D Array Positioning]~12 sections
		[GroundTiles] (4 bytes each)
		(2 byte gap)
		[PlatformTiles]
			[DefaultPlatforms] (4 bytes each)
			(2 byte gap)
			[FallingPlatforms] (4 bytes each)
			(2 byte gap)
			[Level3Platforms] (4 bytes each)
			(2 byte gap)
		[HazardTiles] (4 bytes each)
		(2 byte gap)
		[EnemyTiles]
			[Hollow] (4 bytes each)
			(2 byte gap)
			[Centaur] (4 bytes each)
			(2 byte gap)
			...
		(2 byte gap)
	[Freeform Positioning]~16 sections, see below
		[GroundTiles]
			[4x4] (8 bytes each)
			(4 bytes gap)
			[4x8] (8 bytes each)
			(4 bytes gap)
			[8x4] (8 bytes each)
			(4 bytes gap)
			[8x8] (8 bytes each)
			(4 bytes gap)
		[Platform Tiles]
			[DefaultPlatforms] (8 bytes each)
			(4 byte gap)
			[FallingPlatforms] (8 bytes each)
			(4 byte gap)
			[Level3Platforms] (8 bytes each)
			(4 byte gap)
		[Hazard Tiles]
			[Unattached Spikes] (8 bytes each)
			(4 byte gap)
			[Attached Spikes] (8 bytes each)
			(4 byte gap)
		[Enemies] (see above sections, aka Vector2, aka 4 bytes each, with each enemy section)
	TODO:
	[Health] (1 byte)
	[StartPosition] (4 bytes, freeform style)
	[EndPosition] (4 bytes, freeform style)//As many until 4 byte gap
	(4 byte gap)
	FIN
	*/

	
	public byte DetermineHeaderByte()
	{
		/*[8 bits/booleans] -> 0b in C# 7.0 (e.g. 0x is for hexadecimal), otherwise bitmath time on a byte.
			0 isOriginalFormat -> In case you do some hype optimization in the future
			1 is256Optimized -> See above tl;dr converts static positioning to 1 byte per element instead of 2.
			2 bloodTimer -> Level 2 bloodtimer thing
			3 bool3 -> and forward to bool8 are left for future choices. Leave at least 2 empty, for booleans of 4 choices eg color
			4 bool4
			5 bool5
			6 bool6
			7 bool7
			8 bool8

		See https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators
		Aka you just add 00000000, and place 1 for each of the above being true. Then just add them to 0.
		For example, is256Optimized is & 00000010
		For bool8, it is & 10000000

		Sad I cannot straight up do bit-math, using 0b, and gotta do this if/else bs, but at least its for the far future lol
		https://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte
		https://www.dotnetperls.com/set-bit-zero
		*/

		return 0;
	}

	public void DetermineMinimumTileOffset()
	{
		minimumTileX = MasterGridManager.MAX_DEFAULTGRID;
		minimumTileY = MasterGridManager.MAX_DEFAULTGRID;

		//Ground
		SetMinimum(ref masterGrid.groundTileManager.StaticGridGroundItems);

		//Platforms
		SetMinimum(ref masterGrid.platformTileManager.StaticGridPlatformItems);

		//Hazards
		SetMinimum(ref masterGrid.hazardTileManager.StaticGridHazardItems);

		//Hazards
		SetMinimum(ref masterGrid.enemyTileManager.StaticGridEnemyItems);

		Debug.Log("Minimums are: " + minimumTileX + ", " + minimumTileY);
	}

	public void SetMinimum(ref List<GridItem> GridListItems)
	{
		int pickedTileX;
		int pickedTileY;

		for (int i = 0; i < GridListItems.Count; i++)
		{
			for (int j = 0; j < GridListItems[i].placementLocation.Count; j++)
			{
				pickedTileX = (int) GridListItems[i].placementLocation[j].x;
				pickedTileY = (int) GridListItems[i].placementLocation[j].y;

				if (pickedTileX < minimumTileX)
					minimumTileX = pickedTileX;
				
				if (pickedTileY < minimumTileY)
					minimumTileY = pickedTileY;
			}
		}
	}

	public void PrintFinalByteArray()
	{
		//Iterate byte array and print each byte
		for (int i = 0; i < finalByteArray.Length; i++)
			Debug.Log("Byte " + i + " = " + finalByteArray[i]);
	}

	//======================================================================================================================================================================
	//===Data URI===========================================================================================================================================================
	//======================================================================================================================================================================
	//===Linked .wind Filetype Links:
	//===	"If you're talking about a game build being able to open files in Windows that are double-clicked, I believe you can manage this with a combination of "file associations" (a simple registry key saying "open this file type with my application") and CommandLineArguments https://docs.unity3d.com/Manual/CommandLineArguments.html
	//===	https://answers.unity.com/questions/807765/assign-file-type-to-a-game.html
	//===	https://stackoverflow.com/questions/3057576/how-to-launch-an-application-from-a-browser
	//===	https://superuser.com/questions/29717/associate-a-file-type-with-a-specific-program
	//===	https://www.autohotkey.com/board/topic/71831-application-url-launch-local-application-from-browser/
	//===
	//===	https://answers.unity.com/questions/373079/how-do-i-access-command-prompt-from-unity.html -> Needed for the language and shift trick too.
	//==========================================================================================================================================================
	//===Maximum URL length is 2048 (2 MB)======================================================================================================================
	//==========================================================================================================================================================
	//==="data:level/wind;base64,[ACTUAL DATA]"=================================================================================================================
	//===	See examples from https://en.wikipedia.org/wiki/Data_URI_scheme
	//===	And ofc when you parse it in-game, you remove everything before [ACTUAL DATA]
	//===
	//===	As for why level/wind? Because the web is infiltrating more of daily life and becoming more bloated.
	//===	Mayhaps one day, you can load levels on a browser, but to be realistic, seen far more simplistically, almost excel-like.
	//===	But I feel the .wind format I made is excellent for any kind of 2D game. Could even add slopes (same way as platforms)
	//============================================================================================================================================================
	//============================================================================================================================================================
	
	public string ConvertLatestLevelToBase64String()
	{
		tempByteArray = File.ReadAllBytes(GetLatestLevelFileName());

		string encodedLevelString = Convert.ToBase64String(tempByteArray);

		Debug.Log("Latest Level encoded in the following string: " + encodedLevelString);

		return encodedLevelString;
	}

	public void ConvertBase64StringToByteArray(string encodedLevelString)
	{
		tempByteArray = Convert.FromBase64String(encodedLevelString);


		//===============================
		//===Determine Proper Filename===
		//===============================

			//Now we got to get proper naming so we don't override the filename by default.
			//To do this, we just enumerate the levels inside the folder and give it +1 in naming.

			//So, first we get the number of levels inside the folder
			int levelsFolderCount = Directory.GetFiles(levelsFolderPath, "*." + levelFilenameExtension).Length;

			//And now we make the name.
			tempLevelFileName = levelFilenameStringURILoadedTitle + (levelsFolderCount + 1) + "." + levelFilenameExtension;


		//If .NET 6.0, Asyncwrite. But not worth the upgrade (no insanity mode, obsolete code aka refactor, new bugs etc etc)
		File.WriteAllBytes(levelsFolderPath + "/" + tempLevelFileName, tempByteArray);

		Debug.Log("Saving loaded string-to-bytes level as: " + levelsFolderPath + "/" + "STRINGURIL." + levelFilenameExtension);
	}

	public Byte[] ResetFinalByteArray()
	{
		finalByteArray = new Byte[1000];
		return finalByteArray;
	}
}