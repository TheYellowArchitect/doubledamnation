using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTileManager : MonoBehaviour 
{
	[Header("Camera")]
	public Camera levelEditorCamera;

	[Header("MovementZoom Variables")]
	//This is how much each frame the camera moves
	public float edgeMovePower = 5f;
	public float edgeZoomMultiplier = 0.3f;
	public float zoomOutPower = 3f;
	public float dragPower = 1f;//No idea why it bugs on this value being greater than 2

	private MasterGridManager masterGrid;
	private GameObject warriorCamera;
	private Transform backgroundTransform;

	//Remove this, and the very first zoom on entering a level, becomes offset and camera gtfos
	private bool hasZoomedBefore = false;
	
	// Use this for initialization
	public void Initialize () 
	{
		masterGrid = GetComponent<MasterGridManager>();
		warriorCamera = GameObject.Find("CameraHolder");
		CacheBackground();
		DisableWarriorCamera();
	}

	public void CacheBackground()
	{
		backgroundTransform = warriorCamera.transform.GetChild(0).GetChild(0);
	}
	
	//By mouse edges ofc
	public void SetViewportPosition(Vector3 currentViewportPosition)
	{
		//In case there are 2 cameras and you destroy one (usually debugging lol)
		if (levelEditorCamera == null)
			levelEditorCamera = Camera.main;

		//&& is for editor window, where values go under 0.0 and over 1.0, and you dont want the viewport to move while alt-tabbed ;)

		if (currentViewportPosition.x < 0.01f && currentViewportPosition.x > -0.1f)
			levelEditorCamera.transform.position = levelEditorCamera.transform.position + new Vector3(edgeMovePower * levelEditorCamera.orthographicSize * edgeZoomMultiplier * Time.deltaTime * -1, 0, 0);
		else if (currentViewportPosition.x > 0.99f && currentViewportPosition.x < 1.1f)
			levelEditorCamera.transform.position = levelEditorCamera.transform.position + new Vector3(edgeMovePower * levelEditorCamera.orthographicSize *  edgeZoomMultiplier * Time.deltaTime, 0, 0);

		if (currentViewportPosition.y < 0.01f && currentViewportPosition.y > -0.1f)
			levelEditorCamera.transform.position = levelEditorCamera.transform.position + new Vector3(0, edgeMovePower * levelEditorCamera.orthographicSize *  edgeZoomMultiplier * Time.deltaTime * -1, 0);
		else if (currentViewportPosition.y > 0.99f && currentViewportPosition.y < 1.1f)
			levelEditorCamera.transform.position = levelEditorCamera.transform.position + new Vector3(0, edgeMovePower * levelEditorCamera.orthographicSize *  edgeZoomMultiplier * Time.deltaTime, 0);
	
		//Debug.Log("Current Viewport Position: " + currentViewportPosition);

		EnsureCameraGridBounds();
	}

	public void ZoomLevelEditorCamera(float scrollwheelRolling)
	{
		if (scrollwheelRolling == 0)
			return;

		levelEditorCamera.orthographicSize = levelEditorCamera.orthographicSize + scrollwheelRolling * zoomOutPower * -1;

		//Optimally, this would check for aspect ratio, since 5:4 can reach like 130 without clipping background POG
		if (levelEditorCamera.orthographicSize > 96)
			levelEditorCamera.orthographicSize = 96;

		//Could check if less than 0, but its a cool easter egg feature to mirror the level I guess
	}

	public void MoveCameraTowards(Vector2 mouseDragDifference)
	{
		levelEditorCamera.transform.position = levelEditorCamera.transform.position + new Vector3(mouseDragDifference.x * dragPower, mouseDragDifference.y * dragPower, 0);
	
		EnsureCameraGridBounds();
	}

	//If XY less than 0 in world space, or greater than maxTilesSpawned * tileOffset (5.12), then make that the peak
	//If it jitters in the far future, just add it manually on the above, on any Vector3 to assign and only at the end say levelEditorCamera.transform.position = finalCalculatedVector; instead of re-changing it
	public void EnsureCameraGridBounds()
	{
		if (levelEditorCamera.transform.position.x < 0)
			levelEditorCamera.transform.position = new Vector3(0, levelEditorCamera.transform.position.y, levelEditorCamera.transform.position.z);
		else if (levelEditorCamera.transform.position.x > masterGrid.maxTilesSpawned * masterGrid.tileOffset)
			levelEditorCamera.transform.position = new Vector3(masterGrid.maxTilesSpawned * masterGrid.tileOffset, levelEditorCamera.transform.position.y, levelEditorCamera.transform.position.z);

		if (levelEditorCamera.transform.position.y < 0)
			levelEditorCamera.transform.position = new Vector3(levelEditorCamera.transform.position.x, 0, levelEditorCamera.transform.position.z);
		else if (levelEditorCamera.transform.position.y > masterGrid.maxTilesSpawned * masterGrid.tileOffset)
			levelEditorCamera.transform.position = new Vector3(levelEditorCamera.transform.position.x, masterGrid.maxTilesSpawned * masterGrid.tileOffset, levelEditorCamera.transform.position.z);			

	}

	public void DisableWarriorCamera()
	{
		//Move background
		levelEditorCamera.gameObject.SetActive(true);
		backgroundTransform.SetParent(levelEditorCamera.transform, true);
		backgroundTransform.localPosition = Vector3.zero;
		if (hasZoomedBefore == false)
			hasZoomedBefore = true;
		else
			levelEditorCamera.transform.position = warriorCamera.transform.position;//Relocate camera to warrior camera, so relocating the camera instantly somewhere else won't feel jarring/snappy to the user
		warriorCamera.SetActive(false);

		//So as to still be able to place things while in play mode
		masterGrid.inputTileManager.UpdateActiveCamera(levelEditorCamera.GetComponent<Camera>());
	}

	public void EnableWarriorCamera()
	{
		//Move background
		warriorCamera.SetActive(true);
		backgroundTransform.SetParent(warriorCamera.transform.GetChild(0), true);
		backgroundTransform.localPosition = Vector3.zero;
		levelEditorCamera.gameObject.SetActive(false);

		//So as to still be able to place things while in play mode
		masterGrid.inputTileManager.UpdateActiveCamera(warriorCamera.transform.GetChild(0).gameObject.GetComponent<Camera>());
	}

	public Transform GetWarriorCameraTransform()
	{
		return warriorCamera.transform;
	}

	public Transform GetLevelEditorCameraTransform()
	{
		return levelEditorCamera.transform;
	}
}
