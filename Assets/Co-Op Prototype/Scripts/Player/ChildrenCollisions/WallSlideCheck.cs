using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What this class does, is use the boxColliders (children gameobjects)
/// and with this information, decide what animation to play on wallsliding!\n
/// In short, a state machine.
///
/// Ah also, the walljump hitboxes never touch the ceiling or absolute bottom (ground) hence only the sides!
/// </summary>
public class WallSlideCheck : MonoBehaviour
{

	public BoxCollider2D topWallSlideCollider;
	public BoxCollider2D middleWallSlideCollider;
	public BoxCollider2D bottomWallSlideCollider;

	[Header("Read Only")]
	public bool currentlySideWalled = false;
	public enum wallSlideAnimationState { NinjaFeet, StableFeet, Head, All};
	public enum WallSlidePosition { Top, Middle, Bottom	};
	public wallSlideAnimationState intendedWallSlideAnimation;

	[Header("Flags")]
	public bool isTopSidewalled = false;
	public bool isMiddleSidewalled = false;
	public bool isBottomSidewalled = false;


	//Cache the parent, though tbh it should be the other way but whatever.
	private WarriorMovement warriorBehaviour;

	//A Dictionary of the (touching) GroundColliders
	private Dictionary<int, GameObject> totalSideWallCollisions = new Dictionary<int, GameObject>();

	//One for each collider, for no bugs.
	private Dictionary<int, GameObject> topSideWallCollisions = new Dictionary<int, GameObject>();
	private Dictionary<int, GameObject> middleSideWallCollisions = new Dictionary<int, GameObject>();
	private Dictionary<int, GameObject> bottomSideWallCollisions = new Dictionary<int, GameObject>();

	/*Would-be-used-on-Refactoring variables.
	//Used in triggerEnter and triggerExit as the main dictionary to change/process
	//private  Dictionary<int, GameObject> referencedSideWallCollisions;

	//Used in triggerExit, by checking if the opposite dictionaries than the main/referenced one, are empty or not
	//private bool tempBool1;
	//private bool tempBool2;
	*/



	private void Awake()
	{
		warriorBehaviour = GetComponentInParent<WarriorMovement>();
	}


	//==========
	public void SideWallEnterTrigger(Collider2D wallToRegister, WallSlidePosition triggeredWallSlideType)
	{
		#region Level3Platform Bugfix
		//Rant of a bug that wasted me 1.5 days, and also made this system un-clean, below:
		//Fuck me in the past for doing this kind of shit.
		//So, I literally refactor WallSlide hitbox, and for some fucking reason
		//even though layer masks are the same and hitbox too, it tries to wallslide on platforms of level 3
		//Now, here is the real bug. Level 3 platforms are and **ALWAYS WERE** GROUND, YOU FUCKING RETARD.
		//But for some unknown reason, player never wallslided onto them, with the exact same layer masks as here wtf?!
		//And I didn't think of Ground layer mask bs, because I never got the wallsliding past in the bug...
		//There are many workarounds, none of which is easy to fix at its core
		//Have "Ground" for atopGround collisions and "Ground" for nextToGround collisions.
		//Retard. Now I have to make this 2-line hack
		//inb4 "bruh you wrote a wall of text and now this class looks awful, the 2 lines aint got shit on your rant"
		//This rant is a reminder for me in the far future to remember that this was always a bug
		//wallslide on platforms should have happened in old versions, but seeing it worked, I didn't even think why it shouldn't.
		//Level 3 platforms are good and idk how you will keep expanding them with the above, but at least the above help.
		//tl;dr: Try tinkering with GroundCollision gameobject layer, with platform one-way component layers...
		//if sidewall and atopwall could take more than 1 layer mask, then you could solve this and any further retarded moments.
		//
		//If you want to confirm the mystery of this bug
		//go to version 300sth, and see for yourself. And compare the code with this very version, 340
		if (wallToRegister.CompareTag("Platform"))
			return;

		#endregion


		#region ProcessingEnterDictionariesRefactored
		/*
		//Picking what dictionary to update.
		//This is very clean compared to the alternative without using the below variable
		//However, it seems Unity doesn't like ref variables lmao.
		//Gotta use old-school pointers, that's not clean. Anyhow, commented, enjoy I guess.

		if (triggeredWallSlideType == WallSlidePosition.Top)
			referencedSideWallCollisions = ref topSideWallCollisions;
		else if (triggeredWallSlideType == WallSlidePosition.Middle)
			referencedSideWallCollisions = ref middleSideWallCollisions;
		else//if (triggeredWallSlideType == WallSlidePosition.Bottom)
			referencedSideWallCollisions = ref bottomSideWallCollisions;


		//If not registered
		if (referencedSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
		{
			referencedSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

			//If the sidewall is unregistered
			if (totalSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
			{
				totalSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

				//Update Warrior's Collision directory, since it is this' clone ;)
				warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
			}
		}
		*/
		#endregion

		#region ProcessingEnterDictionariesOriginal
		//=====
		//LOCAL WALLSLIDING DICTIONARY
		if (triggeredWallSlideType == WallSlidePosition.Top)
		{
			//If not registered
			if (topSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
			{
				topSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

				//If the sidewall is unregistered
				if (totalSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
				{
					totalSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

					//Update Warrior's Collision directory, since it is this' clone ;)
					warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
				}
			}
		}
		else if (triggeredWallSlideType == WallSlidePosition.Middle)
		{
			//If not registered
			if (middleSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
			{
				middleSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

				//If the sidewall is unregistered
				if (totalSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
				{
					totalSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

					//Update Warrior's Collision directory, since it is this' clone ;)
					warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
				}
			}
		}
		else//if (triggeredWallSlideType == WallSlidePosition.Bottom)
		{
			//If not registered
			if (bottomSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
			{
				bottomSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

				//If the sidewall is unregistered
				if (totalSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
				{
					totalSideWallCollisions.Add(wallToRegister.GetInstanceID(), wallToRegister.gameObject);

					//Update Warrior's Collision directory, since it is this' clone ;)
					warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
				}
			}
		}
		//=====
		#endregion



		//If first time wallsliding
		if (currentlySideWalled == false && totalSideWallCollisions.Count > 0)
		{
			currentlySideWalled = true;

			warriorBehaviour.SetSideWalled(true);
		}

		//Turn on the flag linked to the collider which just exited
		DetermineWallSlideFlags(triggeredWallSlideType, true);

		//Having gotten the flags, it sets the variable: intendedWallSlideAnimation, which will be passed onto WarriorMovement
		DetermineWallSlideAnimation();

		//Notify warrior to get the proper wallslide state
		warriorBehaviour.UpdateWallSlideAnimationState(intendedWallSlideAnimation);


	}
	//==========


	//==========
	public void SideWallExitTrigger(Collider2D wallToRegister, WallSlidePosition triggeredWallSlideType)
	{
		#region ProcessingExitDictionariesRefactored
		//Picking what dictionary to update.
		//This is very clean compared to the alternative I think but not sure.
		//Because of my uncertainty, I comment out the old part, so as to see the OG KISS logic.
		//If you are confused on how this works, see SideWallEnterTrigger()
		//Actually fuck it, I will keep the old one because it is obvious.
		//This commented out thing is shorter and cleaner in the long-term but not obvious on how it works.


		/*
		if (triggeredWallSlideType == WallSlidePosition.Top)
		{
			referencedSideWallCollisions = ref topSideWallCollisions;

			//Check if the other 2 wallsliders are empty
			tempBool1 = middleSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID());
			tempBool2 = bottomSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID());
		}
		else if (triggeredWallSlideType == WallSlidePosition.Middle)
		{
			referencedSideWallCollisions = ref middleSideWallCollisions;

			//Check if the other 2 wallsliders are empty
			tempBool1 = topSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID());
			tempBool2 = bottomSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID());
		}
		else//if (triggeredWallSlideType == WallSlidePosition.Bottom)
		{
			referencedSideWallCollisions = ref bottomSideWallCollisions;

			//Check if the other 2 wallsliders are empty
			tempBool1 = topSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID());
			tempBool2 = middleSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID());
		}


		//If not registered
		if (referencedSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()))
		{
			referencedSideWallCollisions.Remove(wallToRegister.GetInstanceID());

			//If non-present too in other 2 wallslidetypes, remove from total
			if (tempBool1 == false && tempBool1 == false)
			{
				totalSideWallCollisions.Remove(wallToRegister.GetInstanceID());

				//Update Warrior's Collision directory, since it is this' clone ;)
				warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
			}
		}
		*/
		#endregion

		#region ProcessingExitDictionariesOriginal
		if (triggeredWallSlideType == WallSlidePosition.Top)
		{
			//If not registered
			if (topSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()))
			{
				topSideWallCollisions.Remove(wallToRegister.GetInstanceID());

				//If non-present too in other 2 wallslidetypes, remove from total
				if (middleSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false && bottomSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
				{
					totalSideWallCollisions.Remove(wallToRegister.GetInstanceID());

					//Update Warrior's Collision directory, since it is this' clone ;)
					warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
				}
			}
		}
		else if (triggeredWallSlideType == WallSlidePosition.Middle)
		{
			//If not registered
			if (middleSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()))
			{
				middleSideWallCollisions.Remove(wallToRegister.GetInstanceID());

				//If non-present too in other 2 wallslidetypes, remove from total
				if (bottomSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false && topSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
				{
					totalSideWallCollisions.Remove(wallToRegister.GetInstanceID());

					//Update Warrior's Collision directory, since it is this' clone ;)
					warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
				}
			}
		}
		else//if (triggeredWallSlideType == WallSlidePosition.Bottom)
		{
			//If not registered
			if (bottomSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()))
			{
				bottomSideWallCollisions.Remove(wallToRegister.GetInstanceID());

				//If non-present too in other 2 wallslidetypes, remove from total
				if (middleSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false && topSideWallCollisions.ContainsKey(wallToRegister.GetInstanceID()) == false)
				{
					totalSideWallCollisions.Remove(wallToRegister.GetInstanceID());

					//Update Warrior's Collision directory, since it is this' clone ;)
					warriorBehaviour.SetWallSlideCollisionDictionary(totalSideWallCollisions);
				}
			}
		}
		#endregion

		//Empty list
		if (totalSideWallCollisions.Count == 0)
		{
			currentlySideWalled = false;

			warriorBehaviour.SetSideWalled(false);
		}

		//Turn off the flag linked to the collider which just exited
		DetermineWallSlideFlags(triggeredWallSlideType, false);

		//Having gotten the flags, it sets the variable: intendedWallSlideAnimation, which will be passed onto WarriorMovement
		DetermineWallSlideAnimation();

		//Notify warrior to get the proper wallslide state
		warriorBehaviour.UpdateWallSlideAnimationState(intendedWallSlideAnimation);


	}


	public void ClearCollisions()
	{
		totalSideWallCollisions.Clear();

		topSideWallCollisions.Clear();
		middleSideWallCollisions.Clear();
		bottomSideWallCollisions.Clear();
	}

	//==========

	//by using triggeredWallSlideType, it does the same logic, but changes 2 variables really.
	//[X]SideWallCollisions.Count, and is[X]Sidewalled
	public void DetermineWallSlideFlags(WallSlidePosition triggeredWallSlideType, bool enteredWallSlide)
	{

		switch (triggeredWallSlideType)
		{
			case (WallSlidePosition.Top):

				//If exiting wallslide, check how many walls are registered
				if (enteredWallSlide == false && topSideWallCollisions.Count == 0)
					isTopSidewalled = false;
				else
					isTopSidewalled = true;
				break;


			case (WallSlidePosition.Middle):

				//If exiting wallslide, check how many walls are registered
				if (enteredWallSlide == false && middleSideWallCollisions.Count == 0)
					isMiddleSidewalled = false;
				else
					isMiddleSidewalled = true;
				break;


			case (WallSlidePosition.Bottom):

				//If exiting wallslide, check how many walls are registered
				if (enteredWallSlide == false && bottomSideWallCollisions.Count == 0)
					isBottomSidewalled = false;
				else
					isBottomSidewalled = true;
				break;
		}

		/* Old. Bugged when more than 1 wall per collider
		switch (triggeredWallSlideType)
		{
			case (WallSlidePosition.Top):
				isTopSidewalled = enteredWallSlide;
				break;
			case (WallSlidePosition.Middle):
				isMiddleSidewalled = enteredWallSlide;
				break;
			case (WallSlidePosition.Bottom):
				isBottomSidewalled = enteredWallSlide;
				break;
		}*/
	}

	/// <summary>
	/// Given the isXSidewalled flags, determines the animation
	/// This is to make a mini state machine
	/// </summary>
	public void DetermineWallSlideAnimation()
	{
		//3 Flags, so 7 possible combinations THEORITICALLY.

		//Original wallslide -> hand + 2 feet on wall
		if (isTopSidewalled && isBottomSidewalled)
			intendedWallSlideAnimation = wallSlideAnimationState.All;
		//Hand extending to head wallslide
		else if (isTopSidewalled && isMiddleSidewalled == false && isBottomSidewalled == false)
			intendedWallSlideAnimation = wallSlideAnimationState.Head;
		//Ninja Wallslide -> Both feet diagonally clutch
		else if (isBottomSidewalled && isMiddleSidewalled == false && isTopSidewalled == false)
			intendedWallSlideAnimation = wallSlideAnimationState.NinjaFeet;
		//Original Wallslide without the hand on the wall -> no hand + 2 feet on wall
		else if (isBottomSidewalled && isMiddleSidewalled && isTopSidewalled == false)
			intendedWallSlideAnimation = wallSlideAnimationState.StableFeet;

		//===
		//The below are "should have animations" if I wanted to go ultra polish but m8, I have missed release date by MONTHS! already!
		//===

			else if(isBottomSidewalled == false && isMiddleSidewalled && isTopSidewalled == false)
				intendedWallSlideAnimation = wallSlideAnimationState.All;

				//^Normally this would be for middle only, and the hand must stretch diagonally downwards instead of diagonally upwards
				// and the one leg to be on its position (the upper one) while the downwards is literally hanging
				// such a boring edge-case to implement, now that I'm 100% done with this... so yeah, rip.
			else if (isBottomSidewalled == false && isMiddleSidewalled && isTopSidewalled)
				intendedWallSlideAnimation = wallSlideAnimationState.All;

				//^When you slide downwards, see the above missing animation, aka hanging leg, but the hand is diagonally upwards like vanilla
	}
}
