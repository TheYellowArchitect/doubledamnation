using UnityEngine;

[System.Serializable]
public struct CameraWaypoint
{
	public float timeToActivate;
	public Vector2 location;
	public Quaternion rotation;
	public bool shouldZoom;
	public ZoomData zoomSnapshot;
}