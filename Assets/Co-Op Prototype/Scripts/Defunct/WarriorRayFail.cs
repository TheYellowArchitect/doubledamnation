using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]//Bounding area of our character
public class WarriorController2D : MonoBehaviour 
{
	public LayerMask collisionMask;

	const float skinWidth = 0.015f; //The very corner "bit"/part of the warrior.

	[SerializeField][Range(2, 10)]private int horizontalRayCount = 4;//Rays that check for "physics"
	[SerializeField][Range(2, 10)]private int verticalRayCount = 4;

	private float horizontalRaySpacing;		//The space between horizontal rays.
	private float verticalRaySpacing;		//The space between vertical rays.

	private new BoxCollider2D collider;
	RayCastOrigins raycastOrigins;		//Struct that stores ray positions.

	struct RayCastOrigins
	{
		public Vector2 topLeft, topRight, bottomLeft, bottomRight;
	}

	//When Vector2 math in doubt: https://www.mathisfun.com/algebra/vectors.html

	void Awake()
	{
		collider = GetComponent<BoxCollider2D> ();
	}

	void Start()
	{
		CalculateRaySpacing ();
	}
		
	public void Move(Vector2 velocity)//actually is distance, not velocity.
	{
		UpdateRayCastOrigins ();

		if (velocity.x != 0)
		{HorizontalCollisions (ref velocity);}
		if (velocity.y != 0) 
		{VerticalCollisions (ref velocity);}

		transform.Translate (velocity);
	}

	void VerticalCollisions(ref Vector2 velocity)//ref=pointer
	{
		float directionY = Mathf.Sign (velocity.y);//sign means 1, if greater than 0, else it returns -1
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;

		//Debugging for rays
		for (int i = 0; i < verticalRayCount; i++) 
		{
			Vector2 rayOrigin;//The point/location that the raychecks will start.
			if (directionY==1)
			{rayOrigin = raycastOrigins.topLeft;} 
			else
			{rayOrigin = raycastOrigins.bottomLeft;}

			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			//I will now explain why the above works. rayOrigin is just a point of where the ray will start.
			//First, let's analyze this: "Vector2.right * verticalRaySpacing * i".
			//Vector2.right is (1,0) which means, a point to the right, used not for its properties but because it just shows "RIGHT DIRECTION OK?"
			//Multiplying it by verticalRaySpacing, it now becomes a point, that is distant from origin point, by the space between originpoint and next ray location.
			//Multiplying it by i, simply means it increases proportionately. 
			//Example for this: Multiplying by 0, means the entire Vector2/Point is (0,0). Hence, rayOrigin + (0,0) = rayOrigin, it starts at first ray.
			//Multiplying by 1, means the Vector2/Point is located exactly at first ray. Multiplying by 2, it is located exactly at second ray.
			//The exact ray locationing is achieved because we know the exact distance between rays(verticalRaySpacing).
			//Lastly, "rayOrigin = rayOrigin + someVector2 + anotherVector2".
			//Adding/Summing 2(or 3) Vector2/Points, simply "moves" the point because of simple sum calculation. (2,2) + (1,0) gets it at (3,2).
			//So, after reading this many times, whoever is reading this, u can understand the line of code above. (hopefully, LUL)

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			//RayLength is not fully understood, as in, why we took that length? Search another day plz.
			//Search so i understand hit below. (as in, how we tweak velocity y.
			if (hit)
			{
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;
			}

			//Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);
			Debug.DrawRay (raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.down * 2, Color.red);
		}
	}

	void HorizontalCollisions(ref Vector2 velocity)//ref=pointer
	{
		float directionX = Mathf.Sign (velocity.x);//sign means 1, if greater than 0, else it returns -1
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;

		//Debugging for rays
		for (int i = 0; i < horizontalRayCount; i++) 
		{
			Vector2 rayOrigin;//The point/location that the raychecks will start.
			if (directionX==1)
			{rayOrigin = raycastOrigins.bottomRight;} 
			else
			{rayOrigin = raycastOrigins.bottomLeft;}

			rayOrigin += Vector2.up * (horizontalRaySpacing * i + velocity.y);

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			//RayLength is not fully understood, as in, why we took that length? Search another day plz.
			if (hit)
			{
				velocity.x = (hit.distance - skinWidth) * directionX;
				//Online explanation on the above(why not simply velocity.x=0):
				//99.9% of the time, velocity.x becomes 0 when a player "hits" a wall.
				//If you are moving 10 units per frame, and u detect a wall 5 units away, you want to set your velocity.x=5(hit distance)
				//Therefore, you are actually touching the wall, then every frame after taht, will be setting velocity.x = 0 since u are touching.
				//If you set velocity = 0 immediately when u detect a collision, there will forever be a gap of 5 units between player and the wall.
				rayLength = hit.distance;
			}

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);
		}
	}

	void UpdateRayCastOrigins()
	{
	//So the rays won't fire from the exact edge of the warrior's bounds.

		Bounds bounds = collider.bounds;//bounds = current bounds.
		bounds.Expand (skinWidth * -2);//Actually shrinks it a bit. Takes Vector3.

	
		//Updates ray to not be at exactly on the edge. Possible since we take the edges of SHRUNK bound!
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing()
	{
		Bounds bounds = collider.bounds;//bounds = current bounds.
		bounds.Expand (skinWidth * -2);//Actually shrinks it a bit. Takes Vector3.
		//Multiplying by -2, means an Vector2(1,1) now becomes an (-1,-1) and then *2 -> (-2,-2)
		//Still, how does it convert a float into vector2?

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);//dis math. it just works.
		verticalRaySpacing = bounds.size.x / (verticalRayCount -1);
	}


}
