using System.Collections;
using UnityEngine;

#region Vision of Final Insanity Mode
/*
Things for a fully polished insanity mode.

First of all, a menu. Changing friction, maxamount, timebetweenripples, with a slider of sorts. Add push insanity values as well.
On this menu, you should also be able to put the ripple locations. Random or not, or on-screen, click with a mouse for where they should happen.
On mouse thingy, even have a waypoint and curve, so you can click 2 locations or more and have waypoint.
In that menu, the insanity presets should be able to hotkey'd into any Fbutton. (F2 for example)

Do you understand now why I rushed the fuck out of this? I just want it to work, and the above is way too much work.
Given full focus it should take a week. A WEEK! And I haven't been full focus in a year or so. Too hard when I'm bored.
*/
#endregion
//TODO: ChangeInsanityToggle, so the toggle is called here which calls its brother component (and returning the boolean value of insanityActive)
public class InsanityIntensityManager : MonoBehaviour
{
	public static InsanityIntensityManager globalInstance;

	private InsanityToggle commonInsanityToggle;//This is bloat.
	private RipplePostProcessor cameraRippleBehaviour;
	private Transform warriorTransform;

	/*public float tempFriction = 25;
	public float tempMaxAmount = 0.9f;
	public float tempTimeBetweenRipples = 0.05f;*/

	public float targetFriction;
	public float targetMaxAmount;//Never fucking forget: 1.0+ means eternal ripple, no timer needed.
	public float timeBetweenRipples;

	public bool isInsanityActive = false;
	public bool isCoroutineRunning = false;//Flag to determine if a coroutine should start.

	public enum RippleType { SingleCentral, SingleRandomCentral, DoubleCentral, DoubleRandomCentral, TriangleCentral, SingleUnderCenter};
	public RippleType targetRippleType;

	private Vector3 tempReleaseVector = new Vector3(0,-48);

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;

		commonInsanityToggle = GetComponent<InsanityToggle>();

		cameraRippleBehaviour = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RipplePostProcessor>();

		warriorTransform = GameObject.FindGameObjectWithTag("Player").transform;

		//F7 defaults below, so player is amazed when he triggers it.
		InitDefaultInsanity();
	}
	
	public void InitDefaultInsanity()
	{
		targetFriction = 50;
		targetMaxAmount = 0.1f;
		timeBetweenRipples = 0.0006f;

		targetRippleType = RippleType.SingleUnderCenter;
	}

	public void ToggleIntensity(bool active)
	{
		isInsanityActive = active;

		//Start ripple with timer
		if (isInsanityActive == true && isCoroutineRunning == false)
			StartCoroutine(ContinuousRippleDelayed());

		//When de-activating insanity, turning off the values helps in no visual bugs
		if (isInsanityActive == false && isCoroutineRunning == true)
			cameraRippleBehaviour.enabled = false;

	}
	
	// Update is called once per frame
	void Update ()
	{
		//Just checks/sets the level of intensity
		if (isInsanityActive)
			DetermineFKeysIntensity();
	}

	//TODO: Should decouple ripple location timer change, should be by waypoint imo
	IEnumerator ContinuousRippleDelayed()
	{
		while (isInsanityActive)
		{
			isCoroutineRunning = true;

			//If insanity was toggled off, cameraRippleBehaviour should be disabled. Enable it again.
			if (cameraRippleBehaviour.enabled == false)
				cameraRippleBehaviour.enabled = true;

			//Give it the ripple values set by F keys
			cameraRippleBehaviour.SetValues(targetMaxAmount, targetFriction);

			///Change ripple values here!
			///cameraRippleBehaviour.Friction = tempFriction;
			///cameraRippleBehaviour.MaxAmount = tempMaxAmount;

			//This is spagghetti because it is theoritically unfinished.
			//What it does is depending on rippleType, do rippleEffect on the position it expects.
			DetermineSpagghettiRippleType();

			yield return new WaitForSeconds(timeBetweenRipples);
		}

		isCoroutineRunning = false;
	}

	//Default is 25, 0.9f
	//inb4 switch-case fags popup here
	//if you read this, you may not like it, but the below is clean.
	//((Not that I had to rush this shit lmao))
	public void DetermineFKeysIntensity()
	{
		/*
		if (Input.GetKeyDown(KeyCode.F12))
		{
			targetFriction = 0;
			targetMaxAmount = 0;
			timeBetweenRipples = 100;
		}
		else if (Input.GetKeyDown(KeyCode.F11))
		{
            targetFriction = 50;
            targetMaxAmount = 0.95f;
            timeBetweenRipples = 0.1f;
		}
		else if (Input.GetKeyDown(KeyCode.F10))
		{
            targetFriction = 40;
            targetMaxAmount = 0.8f;
            timeBetweenRipples = 0.1f;
		}*/
		//Original Insanity
		if (Input.GetKeyDown(KeyCode.F8))
		{
            targetFriction = 10;
            targetMaxAmount = 0;
            timeBetweenRipples = 0.1f;
		}
		//Original Insanity + No ZigZags
		else if (Input.GetKeyDown(KeyCode.F7))
		{
            targetFriction = 50;
            targetMaxAmount = 0.1f;
            timeBetweenRipples = 0.0006f;

			targetRippleType = RippleType.SingleUnderCenter;
		}
		//Original Insanity + F1/Default
		else if (Input.GetKeyDown(KeyCode.F6))
		{
			targetFriction = 1.1f;
			targetMaxAmount = 0.4f;
			timeBetweenRipples = 0.002f;

			targetRippleType = RippleType.SingleUnderCenter;
		}
		//Uberstrong wavespam Insanity
		else if (Input.GetKeyDown(KeyCode.F5))
		{
            targetFriction = 50;
            targetMaxAmount = 1f;
            timeBetweenRipples = 0.03f;

			targetRippleType = RippleType.SingleCentral;
		}
		//Strong wavespam Insanity
		else if (Input.GetKeyDown(KeyCode.F4))
		{
            targetFriction = 5;
            targetMaxAmount = 0.9f;
            timeBetweenRipples = 0.05f;

			targetRippleType = RippleType.SingleCentral;
		}
		//Slight wavespam Insanity
		else if (Input.GetKeyDown(KeyCode.F3))
		{
            targetFriction = 10;
            targetMaxAmount = 0.9f;
            timeBetweenRipples = 0.02f;

			targetRippleType = RippleType.SingleCentral;
		}
		//has more friction than default (ground/enemies should be easier to distinguish not sure tho since 0 friction)
		else if (Input.GetKeyDown(KeyCode.F2))
		{
            targetFriction = 0;
            targetMaxAmount = 0.95f;
            timeBetweenRipples = 0.02f;

			targetRippleType = RippleType.SingleUnderCenter;
		}
		//Default Insanity (aka perfectly visually playable)
		else if (Input.GetKeyDown(KeyCode.F1))
		{
			targetFriction = 1;
			targetMaxAmount = 0.9f;
			timeBetweenRipples = 0.02f;

			targetRippleType = RippleType.SingleUnderCenter;

			//Alternative looking the same, is 50, 0.9, 0.002

			//Unknown differences: 1.1, 0.99, 0.002
		}

	}

	public void DetermineSpagghettiRippleType()
	{
		//SingleCentral forever. Others shake the screen too much as the point changes each frame.
		//So the only way others can work is by
		if (targetRippleType == RippleType.SingleCentral)
			cameraRippleBehaviour.RippleEffect(Camera.main.WorldToScreenPoint(warriorTransform.position));
		else if (targetRippleType == RippleType.SingleRandomCentral)
		{
			float tempX = Random.Range(0, Screen.width / 4);
			float tempY = Random.Range(0, Screen.height / 4);
			Vector3 centralVector = Camera.main.WorldToScreenPoint(Camera.main.transform.position);

			Vector3 tempVector = centralVector + new Vector3(tempX, tempY, 0);

			cameraRippleBehaviour.RippleEffect(tempVector);

			Debug.Log("Player Position is: " + Camera.main.WorldToScreenPoint(Camera.main.transform.position));
			Debug.Log("Random Position is: " + tempVector);

			/*float tempX = Random.Range(0, Screen.width);
            float tempY = Random.Range(0, Screen.height);

            Vector3 tempVector = new Vector3(tempX, tempY, 0);

            cameraRippleBehaviour.RippleEffect(tempVector);

            Debug.Log("Player Position is: " + Camera.main.WorldToScreenPoint(Camera.main.transform.position));
            Debug.Log("Random Position is: " + tempVector);*/
		}
		else if (targetRippleType == RippleType.DoubleRandomCentral)
		{
			float tempX = Random.Range(0, Screen.width / 4);
			float tempY = Random.Range(0, Screen.height / 4);
			Vector3 centralVector = Camera.main.WorldToScreenPoint(Camera.main.transform.position);

			Vector3 tempVector = centralVector + new Vector3(tempX, tempY, 0);

			cameraRippleBehaviour.RippleEffect(tempVector);

			tempX = Random.Range(0, Screen.width / 4);
			tempY = Random.Range(0, Screen.height / 4);

			tempVector = centralVector + new Vector3(tempX, tempY, 0);

			cameraRippleBehaviour.RippleEffect(tempVector);

			Debug.Log("Player Position is: " + Camera.main.WorldToScreenPoint(Camera.main.transform.position));
			Debug.Log("Random Position is: " + tempVector);
		}
		else if (targetRippleType == RippleType.DoubleCentral)
		{
			Vector3 centralVector = Camera.main.WorldToScreenPoint(Camera.main.transform.position);

			cameraRippleBehaviour.RippleEffect(centralVector + new Vector3(50, 100));
			cameraRippleBehaviour.RippleEffect(centralVector + new Vector3(-50, -100));
		}
		else if (targetRippleType == RippleType.TriangleCentral)
		{
			Vector3 centralVector = Camera.main.WorldToScreenPoint(Camera.main.transform.position);

			cameraRippleBehaviour.RippleEffect(centralVector + new Vector3(0, 150));
			cameraRippleBehaviour.RippleEffect(centralVector + new Vector3(-50, -100));
			cameraRippleBehaviour.RippleEffect(centralVector + new Vector3(-50, -100));

			//^The above doesn't work. R E T A R D.
			//Because the 3rd overrides the rest.
		}
		else if (targetRippleType == RippleType.SingleUnderCenter)
			cameraRippleBehaviour.RippleEffect(Camera.main.WorldToScreenPoint(warriorTransform.position) + tempReleaseVector);
	}

	public bool GetInsanityActive()
	{
		return isInsanityActive;
	}
}
