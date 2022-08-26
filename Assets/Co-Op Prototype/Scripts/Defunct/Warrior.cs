using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(WarriorController2D))]
public class Warrior : MonoBehaviour 
{
	public float movementSpeed = 6;
	float gravity = -20;
	Vector2 velocity;

	WarriorController2D controller; //The script to control him


	void Start () 
	{
		controller = GetComponent<WarriorController2D> ();
	}

	void Update () 
	{
		Vector2 input = new Vector2 (Input.GetAxis ("LeftJoystickHorizontal"), Input.GetAxis ("LeftJoystickVertical"));

		velocity.x = input.x * movementSpeed;
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
}
