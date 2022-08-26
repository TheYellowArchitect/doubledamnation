using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController1 : MonoBehaviour 
{
	public float movementSpeed = 10f;
	public float airSpeed = 10f;//this is multiplied in calculations.
	public float crouchingSpeed = 10f;
	public bool airControl = true;
	public LayerMask WhatIsGround;

	private bool facingRight = true;
	private bool midair = false;
	private bool crouching = false;
	private float moveX;
	private Vector2 inputDirection;
	private new Rigidbody2D rigidbody;
	//private Animator animator;		  // Reference to the warrior's animator component.
	private Transform GroundCheck;    // A position marking where to check if the player is grounded. aka where player's feet are.
	private Transform CeilingCheck;   // A position marking where to check for ceilings. aka where player's head is.
	//private GameObject gamemanager;
	//Both Children GameObjects of player/warrior, not his components!

	public const float standRadius = 0.1f; //Used to check if player can stand up after crouching
	public const float jumpPower = 150f;    //Used for the very first jump.
	const float GroundedRadius = 0.2f;     //Radius of the overlap circle to determine if grounded, at player's feet(sphere)
	const float CeilingRadius = .01f;	   //Radius of the overlap circle to determine if the player can stand up

	private void Awake() //is private necessary though? :wonder:
	{
		//animator = GetComponent<Animator> ();
		rigidbody = GetComponent<Rigidbody2D> ();
		inputDirection = Vector2.zero;

        //Children game objects. Could have used BoxCollider2Ds but ah well.
        GroundCheck = transform.Find("GroundCheck");  
		CeilingCheck = transform.Find("CeilingCheck");

        //gamemanager = GameObject.FindGameObjectWithTag("GameManager");
    }
		

	void Start () 
	{
        StartCoroutine(Physics());
	}

	void Update ()
    {
        ShowVelocity ();
	}


	//For physics/rigidbody/animations
	void FixedUpdate () 
	{
		PlayerMove ();//Coroutines for smooth velocity?
	}

    ///
    public void Movement(Vector2 inputDirection, bool byPlayer)
    {
        
    }
    ///

    IEnumerator Physics()
    {
        yield return new WaitForSeconds(2);
    }


	void PlayerMove()
	//Remake this with coroutines.
	{
		//GroundCheck: The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		midair=true;


		Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, WhatIsGround);
		                         //Function OverlapCircleAll takes Vector2 point/location, float radius, int layerMask returns Collider2D[]
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders [i].gameObject != gameObject)//any gameobject except this/player/warrior's collider.
			{
				midair = false;
				break;
			}
		}

		//CrouchCheck (remake pl0x)
		if (crouching)
		{
			if (true)//replace with input for crouching.
			{
				// If the character has a ceiling preventing them from standing up, keep them crouching
				if (Physics2D.OverlapCircle(CeilingCheck.position, CeilingRadius, WhatIsGround))
				{
					crouching = true;
				}
			}

		}

		//JumpCheck
		if (inputDirection.y > 0.5 && (inputDirection.x < 0.5 || inputDirection.x > -0.5)) //Jump Input Detected
		{
			if (!midair)
				Jump (inputDirection);
		}
        
		//Apply Physics
		if (midair && airControl == false) 
		{
			//Nothing, nothing to add.
		}
		else if (crouching)
		{
			rigidbody.velocity = new Vector2 (inputDirection.x * crouchingSpeed, rigidbody.velocity.y);
			//Y axis needed for diagonal crouches.
			//rigidbody.AddForce (inputDirection * crouchingSpeed);
		}
		else if (midair)//since air control, use coroutines in the future, thanks.
		{
			rigidbody.velocity = new Vector2 (inputDirection.x * airSpeed, rigidbody.velocity.y);
			//rigidbody.AddForce (inputDirection * airSpeed);
		}
		else //in the ground
		{
			rigidbody.velocity = new Vector2 (inputDirection.x * movementSpeed, rigidbody.velocity.y);
			//rigidbody.AddForce (inputDirection * movementSpeed);
		}
        
	}

	void Jump(Vector2 jumpVector)
	{
		//Happens on the very frame that the jump is detected.
		rigidbody.AddForce (jumpVector * jumpPower);//Gravity scale:2 , jumpPower:150f, seems perfect.
		midair = true;
	}

	void FlipPlayer()//shouldnt happen, player should always face the enemy.
	{
		facingRight = !facingRight;
		Vector2 localscale = GetComponent<Transform> ().localScale;
		localscale.x *= -1;
		transform.localScale = localscale;
	}

	void ShowVelocity()
	{
		if (GameManager.testing == true) 
			Debug.DrawRay (transform.localPosition, rigidbody.velocity/3, Color.blue);// dividing by 3 cuz the ray is too big.
	}
}
