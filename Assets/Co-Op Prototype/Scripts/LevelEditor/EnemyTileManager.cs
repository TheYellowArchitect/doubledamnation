using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTileManager : MonoBehaviour 
{

	[Header("Prefabs")]
	public GameObject hollowLevel1;
	public GameObject satyrLevel1;
	public GameObject centaurLevel1;
	public GameObject cyclopsLevel1;
	public GameObject minotaurLevel1;
	public GameObject harpyLevel1;
	public GameObject spikeboiLevel1;

	[Header("Spawn Offsets")]
	public Vector3 HollowOffset;
	public Vector3 SatyrOffset;
	public Vector3 CentaurOffset;
	public Vector3 CyclopsOffset;
	public Vector3 MinotaurOffset;
	public Vector3 HarpyOffset;
	public Vector3 SpikeboiOffset;

	public List<GridItem> StaticGridEnemyItems = new List<GridItem>();

	private List<GameObject> enemyCorpses = new List<GameObject>();

	private MasterGridManager masterGrid;
	private GridItem tempGridItem;
	private GameObject tempTile;

	// Use this for initialization
	public void Initialize ()
	{
		masterGrid = GetComponent<MasterGridManager>();
	}

	public void CreateStaticEnemy(int Xindex, int Yindex, byte enemyType, bool activateOnSpawn = false)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementType = enemyType;
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(Xindex, Yindex));
		masterGrid.DefaultGridOccupied[Xindex, Yindex] = true;

		//Get the enemy type to spawn as a prefab
		if (enemyType == MasterGridManager.HOLLOW)
			tempTile = hollowLevel1;
		else if (enemyType == MasterGridManager.SATYR)
			tempTile = satyrLevel1;
		else if (enemyType == MasterGridManager.CENTAUR)
			tempTile = centaurLevel1;
		else if (enemyType == MasterGridManager.CYCLOPS)
			tempTile = cyclopsLevel1;
		else if (enemyType == MasterGridManager.MINOTAUR)
			tempTile = minotaurLevel1;
		else if (enemyType == MasterGridManager.HARPY)
			tempTile = harpyLevel1;
		else if (enemyType == MasterGridManager.SPIKEBOI)
			tempTile = spikeboiLevel1;

		//If you spawn an enemy at the middle, he won't touch the ground below him!
		//This ensures all enemies do :)
		Vector3 enemyTypeOffset = new Vector3(0,0,0);
		if (enemyType == MasterGridManager.HOLLOW)
			enemyTypeOffset = HollowOffset;
		else if (enemyType == MasterGridManager.SATYR)
			enemyTypeOffset = SatyrOffset;
		else if (enemyType == MasterGridManager.CENTAUR)
			enemyTypeOffset = CentaurOffset;
		else if (enemyType == MasterGridManager.CYCLOPS)
			enemyTypeOffset = CyclopsOffset;
		else if (enemyType == MasterGridManager.MINOTAUR)
			enemyTypeOffset = MinotaurOffset;
		else if (enemyType == MasterGridManager.HARPY)
			enemyTypeOffset = HarpyOffset;
		else if (enemyType == MasterGridManager.SPIKEBOI)
			enemyTypeOffset = SpikeboiOffset;

		tempTile = Object.Instantiate(tempTile, masterGrid.GetGridTileLocation(Xindex, Yindex) + enemyTypeOffset, Quaternion.identity);

		tempGridItem.gameobject = tempTile;
		tempTile.transform.SetParent(masterGrid.staticHolder);

		StaticGridEnemyItems.Add(tempGridItem);

		LevelManager.globalInstance.AddLevelEditorEnemy(tempGridItem.gameobject);

		if (activateOnSpawn)
			ActivateEnemy(tempTile);
	}

	public void CreateFreeformEnemy(float X, float Y, byte enemyType, bool activateOnSpawn = false)
	{
		tempGridItem = new GridItem();
		tempGridItem.placementType = enemyType;
		tempGridItem.placementLocation = new List<Vector2>();
		tempGridItem.placementLocation.Add(new Vector2(X, Y));

		//Get the enemy type to spawn as a prefab
		if (enemyType == MasterGridManager.HOLLOW)
			tempTile = hollowLevel1;
		else if (enemyType == MasterGridManager.SATYR)
			tempTile = satyrLevel1;
		else if (enemyType == MasterGridManager.CENTAUR)
			tempTile = centaurLevel1;
		else if (enemyType == MasterGridManager.CYCLOPS)
			tempTile = cyclopsLevel1;
		else if (enemyType == MasterGridManager.MINOTAUR)
			tempTile = minotaurLevel1;
		else if (enemyType == MasterGridManager.HARPY)
			tempTile = harpyLevel1;
		else if (enemyType == MasterGridManager.SPIKEBOI)
			tempTile = spikeboiLevel1;

		tempTile = Object.Instantiate(tempTile, new Vector3(X, Y, 0), Quaternion.identity);

		tempGridItem.gameobject = tempTile;
		tempTile.transform.SetParent(masterGrid.freeformHolder);

		masterGrid.freeformTileManager.FreeformGridItems.Add(tempGridItem);

		LevelManager.globalInstance.AddLevelEditorEnemy(tempGridItem.gameobject);

		if (activateOnSpawn)
			ActivateEnemy(tempTile);
	}

	//Toggled on play mode
	public void ActivateEnemies()
	{
		for (int i = 0; i < StaticGridEnemyItems.Count; i++)
			ActivateEnemy(StaticGridEnemyItems[i].gameobject);

		for (int i = 0; i < masterGrid.freeformTileManager.FreeformGridItems.Count; i++)
		{
			if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.CompareTag("Enemy"))
				ActivateEnemy(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject);
		}
	}

	public void ActivateEnemy(GameObject enemy)
	{
		//Disable collider and the sprite
		enemy.GetComponent<BoxCollider2D>().enabled = false;
		enemy.transform.GetChild(1).gameObject.SetActive(false);//child[1] is sprite gameobject. Just disable it altogether

		//Enable actual enemy
		enemy.transform.GetChild(0).gameObject.SetActive(true);
		if (enemy.transform.GetChild(0).gameObject.GetComponent<EnemyPathfinder>().enabled == false)//If dead
			enemy.transform.GetChild(0).gameObject.GetComponent<EnemyPathfinder>().enabled = true;
	}

	public void DisableEnemies()
	{
		for (int i = 0; i < StaticGridEnemyItems.Count; i++)
			DisableEnemy(StaticGridEnemyItems[i].gameobject);

		for (int i = 0; i < masterGrid.freeformTileManager.FreeformGridItems.Count; i++)
		{
			if (masterGrid.freeformTileManager.FreeformGridItems[i].gameobject.CompareTag("Enemy"))
				DisableEnemy(masterGrid.freeformTileManager.FreeformGridItems[i].gameobject);
		}

		//Remove corpses
		for (int i = 0; i < enemyCorpses.Count; i++)
		{
			if (enemyCorpses[i].activeSelf == true)
				enemyCorpses[i].SetActive(false);
		}
	}

	public void DisableEnemy(GameObject enemy)
	{
		//Disable actual enemy
		enemy.transform.GetChild(0).gameObject.GetComponent<EnemyPathfinder>().LevelEditorPause();
		enemy.transform.GetChild(0).gameObject.SetActive(false);

		//Disable collider and the sprite
		enemy.GetComponent<BoxCollider2D>().enabled = true;
		enemy.transform.GetChild(1).gameObject.SetActive(true);//child[1] is sprite gameobject. Just disable it altogether
	}

	public void AddCorpse(GameObject enemyCorpse)
	{
		enemyCorpses.Add(enemyCorpse);
	}
	
}
