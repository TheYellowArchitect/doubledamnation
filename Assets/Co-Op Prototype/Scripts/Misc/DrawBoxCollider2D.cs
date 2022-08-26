using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draw.Line() 4 times, once for each side of the BoxCollider2D of this object
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DrawBoxCollider2D : MonoBehaviour
{
	public Color upColor;
	public Color downColor;
	public Color leftColor;
	public Color rightColor;

#if UNITY_EDITOR

	private Vector3 tempPoint1;
	private Vector3 tempPoint2;

	void Update()
	{
		DisplayBoxColliderHitbox();
	}


	void DisplayBoxColliderHitbox()
	{
		if (GameManager.testing == true)
		{
			BoxCollider2D currentBoxCollider2D = GetComponent<BoxCollider2D>();

			//Top-Right to Top-Left
			tempPoint1 = transform.position + new Vector3(currentBoxCollider2D.size.x / 2, currentBoxCollider2D.size.y / 2);

			tempPoint2 = transform.position + new Vector3(-1 * currentBoxCollider2D.size.x / 2, currentBoxCollider2D.size.y / 2);

			Debug.DrawLine(tempPoint1, tempPoint2, upColor);



			//Bottom-Right to Bottom-Left
			tempPoint1 = transform.position + new Vector3(currentBoxCollider2D.size.x / 2, -1 * currentBoxCollider2D.size.y / 2);

			tempPoint2 = transform.position + new Vector3(-1 * currentBoxCollider2D.size.x / 2, -1 * currentBoxCollider2D.size.y / 2);

			Debug.DrawLine(tempPoint1, tempPoint2, downColor);



			//Top-Right to Bottom-Right
			tempPoint1 = transform.position + new Vector3(currentBoxCollider2D.size.x / 2, currentBoxCollider2D.size.y / 2);

			tempPoint2 = transform.position + new Vector3(currentBoxCollider2D.size.x / 2, -1 * currentBoxCollider2D.size.y / 2);

			Debug.DrawLine(tempPoint1, tempPoint2, rightColor);



			//Top-Left to Bottom-Left
			tempPoint1 = transform.position + new Vector3(-1 * currentBoxCollider2D.size.x / 2, currentBoxCollider2D.size.y / 2);

			tempPoint2 = transform.position + new Vector3(-1 * currentBoxCollider2D.size.x / 2, -1 * currentBoxCollider2D.size.y / 2);

			Debug.DrawLine(tempPoint1, tempPoint2, leftColor);

			#region How it used to be. Didn't even display properly LMAO
			/*
			//Diagonal
			//Debug.DrawLine(walljumpTopLeft + new Vector2(transform.position.x, transform.position.y), walljumpBottomRight + new Vector2(transform.position.x, transform.position.y), Color.cyan);

			//Top-Left to Top-Right
			tempPoint1 = new Vector3(currentBoxCollider2D.bounds.min.x, currentBoxCollider2D.bounds.max.y);
			tempPoint2 = new Vector3(currentBoxCollider2D.bounds.max.x, currentBoxCollider2D.bounds.max.y);

				//Add the offset of gameobject and offset of the collider!
				tempPoint1 = tempPoint1 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;
				tempPoint2 = tempPoint2 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;

				Debug.Log("TempPoint1' transform.localPosition and boxoffset is: " + transform.localPosition + " " + (Vector3)currentBoxCollider2D.offset);

			Debug.DrawLine(tempPoint1, tempPoint2, upColor);


			//Bottom-Left to Bottom-Right
			tempPoint1 = new Vector3(currentBoxCollider2D.bounds.min.x, currentBoxCollider2D.bounds.min.y);
			tempPoint2 = new Vector3(currentBoxCollider2D.bounds.max.x, currentBoxCollider2D.bounds.min.y);

				//Add the offset of gameobject and offset of the collider!
				tempPoint1 = tempPoint1 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;
				tempPoint2 = tempPoint2 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;

			Debug.DrawLine(tempPoint1, tempPoint2, downColor);


			//Top-Left to Bottom-Left
			tempPoint1 = new Vector3(currentBoxCollider2D.bounds.min.x, currentBoxCollider2D.bounds.max.y);
			tempPoint2 = new Vector3(currentBoxCollider2D.bounds.min.x, currentBoxCollider2D.bounds.min.y);

				//Add the offset of gameobject and offset of the collider!
				tempPoint1 = tempPoint1 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;
				tempPoint2 = tempPoint2 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;

			Debug.DrawLine(tempPoint1, tempPoint2, leftColor);


			//Top-Right to Bottom-Right
			tempPoint1 = new Vector3(currentBoxCollider2D.bounds.max.x, currentBoxCollider2D.bounds.max.y);
			tempPoint2 = new Vector3(currentBoxCollider2D.bounds.max.x, currentBoxCollider2D.bounds.min.y);

				//Add the offset of gameobject and offset of the collider!
				tempPoint1 = tempPoint1 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;
				tempPoint2 = tempPoint2 + transform.localPosition + (Vector3)currentBoxCollider2D.offset;

			Debug.DrawLine(tempPoint1, tempPoint2, rightColor);

			/*
			/////////////////////
			//Takes the total walljump hitbox, and checks the half left of the half hitbox.
			tempPoint1 = new Vector3(walljumpTopLeft.x - walljumpTopLeft.x / 2, walljumpBottomRight.y);
			Debug.DrawLine(transform.position + walljumpTopLeft, transform.position + tempPoint1, Color.black);
			*/
			#endregion
		}
	}
#endif

}
