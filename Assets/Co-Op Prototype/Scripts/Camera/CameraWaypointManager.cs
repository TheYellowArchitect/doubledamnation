using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CameraWaypointManager : MonoBehaviour 
{
	[Header("New Warrior Input")]
		[MinValue(0f)]
		public float timeToActivate;
		public Vector2 location;
		public Vector3 rotation;
		public bool shouldZoom = false;
		public ZoomData zoomSnapshot;

	[Header("Waypoint List")]
		public List<CameraWaypoint> waypointList = new List<CameraWaypoint>();

	//Below should be special for each level!
	[Header("Change this if you want to start somewhere in the level")]
		public Vector3 whereToSpawn = Vector3.zero;

	[Header("Read-Only")]
		public bool hasStartedRoaming;
		public float timeSinceRoamingStarted = 0;
		public int waypointCurrentIndex = -1;
		public Vector2 tempLerpLocation;
		public float lerpTime;//This is timeSinceRoamingStarted - currentLocationTime / nextLocationTime - currentLocationTime

		public List<Transform> previousFocusTargets;
		public MultipleTargetCamera commonMultipleTargetCamera;

	void Start () 
	{
		commonMultipleTargetCamera = GetComponent<MultipleTargetCamera>();
	}
	
	void LateUpdate () 
	{
		if (hasStartedRoaming)
		{
			timeSinceRoamingStarted = timeSinceRoamingStarted + Time.unscaledDeltaTime;

			//If there are any waypoint inputs left
			if (waypointCurrentIndex + 1 < waypointList.Count)
			{

				lerpTime = (timeSinceRoamingStarted - waypointList[waypointCurrentIndex].timeToActivate) / (waypointList[waypointCurrentIndex + 1].timeToActivate - waypointList[waypointCurrentIndex].timeToActivate);
				
				//============================
				//===Lerp Position/Location===
				//============================

				tempLerpLocation = Vector2.Lerp(waypointList[waypointCurrentIndex].location, waypointList[waypointCurrentIndex + 1].location, lerpTime);				
				transform.position = new Vector3(tempLerpLocation.x, tempLerpLocation.y, -1);

				//===================
				//===Lerp Rotation===
				//===================

				transform.rotation = Quaternion.Lerp(waypointList[waypointCurrentIndex].rotation, waypointList[waypointCurrentIndex + 1].rotation, lerpTime);				

				//=============================================
				//===Timer check for moving to next waypoint===
				//=============================================

				//If the next input, has less time than current update/framerate
				if (waypointList[waypointCurrentIndex + 1].timeToActivate <= timeSinceRoamingStarted)
				{
					waypointCurrentIndex++;
					if (waypointList[waypointCurrentIndex].shouldZoom)
						commonMultipleTargetCamera.StartZoom(waypointList[waypointCurrentIndex].zoomSnapshot.zoomValue, waypointList[waypointCurrentIndex].zoomSnapshot.zoomDuration, waypointList[waypointCurrentIndex].zoomSnapshot.intervalBetweenNextZoom, waypointList[waypointCurrentIndex].zoomSnapshot.zoomTarget, waypointList[waypointCurrentIndex].zoomSnapshot.clearFocusTargetsBeforeZoom, waypointList[waypointCurrentIndex].zoomSnapshot.zoomOut);
				}
			}
			else
				transform.position = new Vector3(waypointList[waypointCurrentIndex].location.x, waypointList[waypointCurrentIndex].location.y, -1);
		}
	}

	[Button("Add New Waypoint")]
	public void AddWaypoint()
	{
		CameraWaypoint waypointToAdd = new CameraWaypoint();
		waypointToAdd.timeToActivate = timeToActivate;
		waypointToAdd.location = location;
		waypointToAdd.rotation = Quaternion.Euler(rotation);
		waypointToAdd.shouldZoom = shouldZoom;
		if (waypointToAdd.shouldZoom)
			waypointToAdd.zoomSnapshot = zoomSnapshot;

		waypointList.Add(waypointToAdd);
	}

	[Button("Sort Waypoints")]
	public void SortWaypoints()
	{
		//Sort the list by time!
		waypointList.Sort((x,y) => x.timeToActivate.CompareTo(y.timeToActivate));
	}

	[Button("START")]
	public void StartWaypointRoaming()
	{
		StartCoroutine(StartWaypointRoamingCoroutine());
	}

	public IEnumerator StartWaypointRoamingCoroutine()
	{
		yield return new WaitForSeconds(1.5f);

		hasStartedRoaming = true;
		timeSinceRoamingStarted = 0;
		waypointCurrentIndex = 0;
		

		//Removes all camera focus targets
		previousFocusTargets = commonMultipleTargetCamera.focusTargets;
		commonMultipleTargetCamera.focusTargets.Clear();
		commonMultipleTargetCamera.AddFocusTarget(transform);//Add only camera itself
	}

	[Button("STOP")]
	public void StopTASCoroutine()
	{
		if (hasStartedRoaming == false)
			return;

		hasStartedRoaming = false;
		timeSinceRoamingStarted = 0;
		waypointCurrentIndex = -1;
		

		//Give back the focus targets it had before it started waypoint roaming
		commonMultipleTargetCamera.focusTargets = previousFocusTargets;
	}

	[Button("Delete ALL Waypoints")]
	public void ClearWaypointList()
	{
		waypointList.Clear();
	}
}

