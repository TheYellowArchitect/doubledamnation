using UnityEngine;

public class WaypointMovement : MonoBehaviour
{
	public float speed;

	/* Too complicated this way, since weird shapes.
	[Header("Limits/Corners")]
	public Vector2 topRight;
	public Vector2 topLeft;
	public Vector2 bottomRight;
	public Vector2 bottomLeft;
	*/

	//x is minimum, y is maximum
	[Header("X = Minimum, Y = Maximum")]
	public Vector2 horizontalBounds;
	public Vector2 verticalBounds;

	public Vector2 previousPosition;
	public Vector2 targetPosition;

	public float lerpRate;

	private float tempLerpX;
	private float tempLerpY;

	void Start()
	{
		lerpRate = 0;
		previousPosition = transform.localPosition;
		DeterminePositionWithinBounds();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//If hasn't reached destination yet
		if (lerpRate < 1)
		{
			lerpRate = lerpRate + Time.deltaTime * speed;

			transform.localPosition = Vector3.Lerp(previousPosition, targetPosition, lerpRate);
		}
		else
		{
			lerpRate = 0;
			previousPosition = targetPosition;
			DeterminePositionWithinBounds();
		}


	}

	public void DeterminePositionWithinBounds()
	{
		targetPosition = new Vector2(Random.Range(horizontalBounds.x, horizontalBounds.y), Random.Range(verticalBounds.x, verticalBounds.y));
	}
}
