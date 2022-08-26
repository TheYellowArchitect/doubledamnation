using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

//TODO: Refactor heavily by stretching the middle instead of spawning infinite of them and stretching at the end hackishly!!!
public class FinalPillarBehaviour : MonoBehaviour
{
	[Header("Offsets")]

	[Tooltip("How much is needed to perfectly connect Top with middle?\nHappens only once, obviously")]
	public float pillarTopMiddleOffsetY = 0.73f;

	[Tooltip("How much is needed to perfectly connect 2 middle pieces?")]
	public float pillarMiddleMiddleOffsetY = 2.84f;

	[Tooltip("How much to the right must middle pillar be pushed, to align with top?")]
	public float pillarTopMiddleOffsetX = 0.011f;

	[Tooltip("This shouldn't be hardcoded, but it's done for convenience. Literally BoxCollider2D.size.y * parentLocalScale.y")]
	public float pillarMiddleHeight;



	[Header("Visual-Related(VFX Mostly)")]

	[Tooltip ("How quickly should each pillar part die?")]
	public float timePerPillarPartSpellDeath;//0.2f

	[Tooltip ("This is added because otherwise, ")]
	public float timeAddedToSpellDeathPerPillar;//-0.008f

	[Tooltip ("When a pillar is erected, what is the default/minimum number of particles to spawn?")]
	public int defaultParticlesAmountToSpawn;

	[Tooltip ("When a huge pillar is erected, of say, 20 middle pillar parts, the default particles will never cover it!")]
	public int particlesAmountToSpawnPerPillar;



	[Header("Misc")]

	[Tooltip("All old pillars used to be true. But the new pillar is to stay and become part of the level :)")]
	public bool hasDeathTimer = false;

	[ShowIf("hasDeathTimer")]
	[Tooltip("After hitting player, when does it fall?")]
	public float fallTime = 1;

	[ShowIf("hasDeathTimer")]
	[Tooltip("After spawning, when does it die?")]
	public float deathTime = 2;

	[ShowIf("hasDeathTimer")]
	[Tooltip("How fast should it fall?(speed)")]
	public float fallingSpeed;

	[Tooltip("When does it actually spawn the very frame summoned(it doesn't spawn right under the player, and collision is ignored at first)")]
	public int initSpawnPositionOffset = 2048;//Literally trying to spawn it as far away as possible instead of directly under player's feet, kinda hackish but works

	[Tooltip("How far away is the player from the top pillar, so as the pillar spawns right below his feet")]
	public float playerVerticalOffset;

	[Tooltip("When raycasting down, where does the pillar emerge from?")]
	public LayerMask WhatIsGround;

	[Tooltip("How far away should it raycast downwards?")]
	public float raycastMaxDistance = 999f;

	[Tooltip("Middle pillar gameobject")]
	public GameObject middlePillarPrefab;

	[Tooltip("Transform of top pillar")]
	public Transform topPillarTransform;

	[Tooltip ("The gameobject which holds the pillarflame VFX")]
	public GameObject deathFlameVFXGameObject;

	[Tooltip ("The particle system which is to play when topPillar dies")]
	public ParticleSystem topPillarDeathVFX;

	[Tooltip("This script makes all platforms ignored, note that it is a child!")]
	public FinalPillarIgnorePlatforms ignorePlatformsBehaviour;

	[Tooltip("So every new pillar spawns behind old ones!")]
	public static int lastPillarLayer = 45;//used to be playable -3 (and middle -4)


	//Used just for falling, so you dont give speed to each child!
	private Rigidbody2D commonRigidbody;
	public Vector3 playerPosition;
	private ParticleSystem deathFlameVFX;

	private float positionY;

	private GameObject spawnedMiddlePillar;
	private float bottomPillarDistanceFromGround;
	private float groundHitHeight;//From raycast, so small ground won't have issues.

	//For ignoring platforms
	private List<BoxCollider2D> childrenPillarColliders;
	public List<GameObject> childrenPillarGameObjects;

	private Vector2 bottomPillarPoint;
	private Vector2 previousPillarPosition;

	private bool hasStartedDying = false;

	//Netcoding exclusive
	public bool pillarArrived = false;

	// Use this for initialization
	void Start ()
	{
		commonRigidbody = GetComponent<Rigidbody2D>();

		deathFlameVFX = deathFlameVFXGameObject.GetComponent<ParticleSystem>();

		playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

		GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DiedEvent += ResetLayer;

		if (GameObject.FindGameObjectWithTag("PillarStorage") != null)
			transform.SetParent(GameObject.FindGameObjectWithTag("PillarStorage").transform);

		pillarMiddleHeight = 0.65f * transform.localScale.y;
		//----------

		//Spawn far away ((hacky but works))
		transform.position = new Vector3(playerPosition.x - initSpawnPositionOffset, playerPosition.y - initSpawnPositionOffset, 0.1f);//0.1 so its behind lava flame VFX

		if (hasDeathTimer)
		{
			Invoke("Fall", fallTime);
			Invoke("TimerDeath", deathTime);
		}

		//VFX! (rising rocks/pebbles)

		StartCoroutine(PlaceRightBelowPlayerOnNextFrame());

	}

	public void Fall()
	{
		commonRigidbody.velocity = new Vector2(0, -fallingSpeed);
	}

	public void TimerDeath()
	{
		Destroy(gameObject);
	}


	public IEnumerator PlaceRightBelowPlayerOnNextFrame()
	{
		//if offline
		if (NetworkCommunicationController.globalInstance == null)
			yield return new WaitForFixedUpdate();
		else//online
		{
			if (NetworkDamageShare.globalInstance.IsSynchronized())
			{
				//If warrior, send the pillar and continue normally
				if (NetworkCommunicationController.globalInstance.IsServer())
				{
					NetworkCommunicationController.globalInstance.SendPillarPosition(new Vector2(playerPosition.x, playerPosition.y));
					yield return new WaitForFixedUpdate();
				}
				else//if mage, eternally loop here as you wait for NetworkSpellcasting to update pillarArrived, and playerPosition.
				{
					while (pillarArrived == false)//Notified by NetworkSpellcasting
						yield return new WaitForEndOfFrame();
				}
			}
			/*
			else
			{
				NetworkCommunicationController.globalInstance.SendPillarPosition(new Vector2(playerPosition.x, playerPosition.y));
				yield return new WaitForFixedUpdate();
			}
			*/

			
		}

		//tbh, instead of this, there should be "rising" for 0.15 seconds.
		//Also helps in raising player if grounded, instead of disabling it like currently. After release I guess
		//^Nope. 0.15 seconds means fast movement wont be able to use the pillar easily, hence clunky.

		//Confirm raycast is possible, aka player is not out of playable bounds
			// Cast a ray straight down.
			RaycastHit2D confirmCastHit = Physics2D.Raycast(playerPosition, Vector2.down, raycastMaxDistance, WhatIsGround);

			if (confirmCastHit.collider == null)
			{
				Debug.LogError("Cannot detect ground below... Where are you?...");
				Destroy(gameObject);
				yield break;
			}

		PlaceTopPillarRightBelowPlayerFeet();

		SpawnAppropriateMiddlePillars();

		SetChildrenPillarList();

		InitPillarIgnorePlatforms();

		lastPillarLayer--;

		//layer order 1 is backgroundClay -_-
		//Would go to another layer entirely, but muh lava...
		if (lastPillarLayer < 2)
			ResetLayer();
	}

	public void PlaceTopPillarRightBelowPlayerFeet()
	{
		//Place Top Pillar
		//Get difference/offset.
		positionY = playerPosition.y - playerVerticalOffset;

		//Place top pillar right under player's feet
		topPillarTransform.position = new Vector3(playerPosition.x, positionY);

		//Get z to 0, so it won't look bad at lava fire VFX
		topPillarTransform.localPosition = new Vector3(topPillarTransform.localPosition.x, topPillarTransform.localPosition.y, 0);

		//Sort it in layer so its always in front! (middle pillars also get this, with lastPillarLayer - 1 ;)
		topPillarTransform.GetComponent<SpriteRenderer>().sortingOrder = lastPillarLayer;

		//Place the death VFX on the exact same position
		topPillarDeathVFX.transform.position = topPillarTransform.position;
	}

	public void SpawnAppropriateMiddlePillars()
	{
		//The - ((blabla)) works like this:
		//topPillarTransform.SpriteRenderer.size.y is the height of the gameobject.
		//Dividing that by 2, we get how much distance is needed from center (transform.position.y) to bottom point.
		//BUT! SpriteRenderer doesnt know about localscale! So we multiply by father's scaling.
		bottomPillarPoint = new Vector2(topPillarTransform.position.x, topPillarTransform.position.y - ((topPillarTransform.GetComponent<SpriteRenderer>().size.y * transform.localScale.x) / 2));

		//Check if first middle pillar needs to be spawned
		GetDistanceYFromPoint(bottomPillarPoint);

		//Set Middle Pillar right below Top Pillar
		if (bottomPillarDistanceFromGround > pillarMiddleHeight)
		{
			spawnedMiddlePillar = Instantiate(middlePillarPrefab, this.transform);

			spawnedMiddlePillar.transform.localPosition = new Vector3(topPillarTransform.localPosition.x + pillarTopMiddleOffsetX, topPillarTransform.localPosition.y + pillarTopMiddleOffsetY, 0);

			spawnedMiddlePillar.GetComponent<SpriteRenderer>().sortingOrder = lastPillarLayer - 1;
		}
		else//Check if there is a gap.
		{
			//Gap detected, fill it up with a weird middle pillar
			if (bottomPillarDistanceFromGround != 0)
			{
				spawnedMiddlePillar = Instantiate(middlePillarPrefab, this.transform);

				spawnedMiddlePillar.transform.localPosition = new Vector3(topPillarTransform.localPosition.x + pillarTopMiddleOffsetX, topPillarTransform.localPosition.y + pillarTopMiddleOffsetY, 0);

				spawnedMiddlePillar.GetComponent<SpriteRenderer>().sortingOrder = lastPillarLayer - 1;

				//Edge-case, of very small ground!
				if (groundHitHeight < 6f)
				{
					spawnedMiddlePillar.transform.localScale = new Vector3(spawnedMiddlePillar.transform.localScale.x, 0.8f, spawnedMiddlePillar.transform.localScale.z);

					//Changing position too since making scale less than 1, creates gap upwards -_-
					spawnedMiddlePillar.transform.localPosition = spawnedMiddlePillar.transform.localPosition + new Vector3(0, 0.07f, 0);
				}

			}

			return;
		}

		//Set Middle Pillars bellow Middle Pillars
		while (true)
		{
			//0.02f so it wont collide with its edgeY!
			bottomPillarPoint = new Vector2(spawnedMiddlePillar.transform.position.x, spawnedMiddlePillar.transform.position.y - (pillarMiddleHeight / 2) - 0.02f);

			//Check if first middle pillar needs to be spawned
			GetDistanceYFromPoint(bottomPillarPoint);

			//Set Middle Pillar right below Top Pillar
			if (bottomPillarDistanceFromGround > pillarMiddleHeight)
			{
				previousPillarPosition = spawnedMiddlePillar.transform.localPosition;

				spawnedMiddlePillar = Instantiate(middlePillarPrefab, this.transform);

				spawnedMiddlePillar.transform.localPosition = new Vector3(previousPillarPosition.x, previousPillarPosition.y + pillarMiddleMiddleOffsetY, 0);

				spawnedMiddlePillar.GetComponent<SpriteRenderer>().sortingOrder = lastPillarLayer - 1;
			}
			else//Final/Bottom pillar above the ground detected! Time to pull the stretch hack!
			{
				if (groundHitHeight > 6f)
					spawnedMiddlePillar.transform.localScale = new Vector3(spawnedMiddlePillar.transform.localScale.x, spawnedMiddlePillar.transform.localScale.y * 3, spawnedMiddlePillar.transform.localScale.z);
				else//Edge-case of very small ground!
					spawnedMiddlePillar.transform.localScale = new Vector3(spawnedMiddlePillar.transform.localScale.x, spawnedMiddlePillar.transform.localScale.y * 2.5f, spawnedMiddlePillar.transform.localScale.z);
				return;
			}

		}


		//When falling, if trigger2D, destroy.
	}

	//Wrapper/Overloader
	public void GetDistanceYFromPoint(float x, float y)
	{
		GetDistanceYFromPoint(new Vector2(x,y));
	}

	public void GetDistanceYFromPoint(Vector2 originPoint)
	{
		// Cast a ray straight down.
		RaycastHit2D resultHit = Physics2D.Raycast(originPoint, Vector2.down, raycastMaxDistance, WhatIsGround);

		Debug.DrawRay(originPoint, Vector2.down * 300, Color.magenta, 3);

		// If it hits something...
		if (resultHit.collider != null)
		{
			// Calculate the distance from the origin point
			bottomPillarDistanceFromGround = Mathf.Abs(resultHit.point.y - originPoint.y);
			float groundTopPointY = resultHit.point.y;
			groundHitHeight = resultHit.collider.bounds.size.y;

			Debug.Log("Collision is: " + resultHit.collider.name);
			Debug.Log("Distance is: " + bottomPillarDistanceFromGround);
			Debug.Log("groundTopPointY is: " + groundTopPointY);

			Debug.Log("GroundHeight is: " + groundHitHeight);
			//float floatHeight = 3;//How far above should the player hover?
			//float heightError = floatHeight - distance;
		}
	}

	public void SetChildrenPillarList()
	{
		childrenPillarGameObjects = new List<GameObject>();

		//Iterate without caring about child indexes aka bug prone!
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).gameObject.layer == LayerMask.NameToLayer("Ground"))
				childrenPillarGameObjects.Add(transform.GetChild(i).gameObject);
		}
	}

	public void InitPillarIgnorePlatforms()
	{
		childrenPillarColliders = new List<BoxCollider2D>();

		for (int i = 0; i < childrenPillarGameObjects.Count; i++)
			childrenPillarColliders.Add(childrenPillarGameObjects[i].GetComponent<BoxCollider2D>());

		ignorePlatformsBehaviour.SetColliders(childrenPillarColliders);
	}

	public void StartSpellDeathCoroutine()
	{
		if (hasStartedDying == false)
		{
			hasStartedDying = true;

			StartCoroutine(SpellDeathCoroutine());
		}

	}

	public IEnumerator SpellDeathCoroutine()
	{
		//If has middle pillar
		/*if (childrenPillarGameObjects.Count > 1)
			bottomPillarPoint = new Vector2(spawnedMiddlePillar.transform.position.x, spawnedMiddlePillar.transform.position.y - ((pillarMiddleHeight * spawnedMiddlePillar.transform.localScale.y) / 2) - 0.02f);
		else
			bottomPillarPoint = new Vector2(topPillarTransform.position.x, topPillarTransform.position.y - ((topPillarTransform.GetComponent<SpriteRenderer>().size.y * transform.localScale.y) / 2));
		*/
		//If has middle pillar
		if (childrenPillarGameObjects.Count > 1)
			bottomPillarPoint = new Vector2(spawnedMiddlePillar.transform.position.x, spawnedMiddlePillar.transform.position.y);
		else
			bottomPillarPoint = new Vector2(topPillarTransform.position.x, topPillarTransform.position.y);


		deathFlameVFXGameObject.transform.position = bottomPillarPoint;

		//Move the VFX properly on lava -_-
		deathFlameVFXGameObject.transform.localPosition = new Vector3(deathFlameVFXGameObject.transform.localPosition.x, deathFlameVFXGameObject.transform.localPosition.y, deathFlameVFXGameObject.transform.localPosition.z + 2);


		//Particle System Emission stuff
			//Get the emission module to code straight into the particle system!
			//((I legit do not know what variable type var is, but it just works...))
			var vfxEmissionModule = deathFlameVFX.emission;

			//Get enough particles to cover all the middle pillars!
			for (int i = 1; i < childrenPillarGameObjects.Count; i++)
				defaultParticlesAmountToSpawn = defaultParticlesAmountToSpawn + particlesAmountToSpawnPerPillar;

			ParticleSystem.Burst finalBurst = new ParticleSystem.Burst();
			finalBurst.time = 0f;
			finalBurst.minCount = (short)defaultParticlesAmountToSpawn;
			finalBurst.maxCount = (short)defaultParticlesAmountToSpawn;
			finalBurst.cycleCount = 1;
			finalBurst.repeatInterval = 0;

			vfxEmissionModule.SetBurst(0, finalBurst);


		deathFlameVFX.Play();

		int pillarIndexCounter = childrenPillarGameObjects.Count;

		//Go downwards, and destroy the bottom child every 0.X seconds (fits with the VFX)
		//Goes from Count to 0, instead of 0++ to Count, because it is a list.
		while (pillarIndexCounter != 0)//2 is platform detector and VFX
		{
			//Wait a bit so VFX covers the part fully. Which we will delete ;)
			yield return new WaitForSeconds(timePerPillarPartSpellDeath);

			//Increase timer per pillar
			timePerPillarPartSpellDeath = timePerPillarPartSpellDeath + timeAddedToSpellDeathPerPillar;

			//Destroy the part, as it should be covered fully by VFX by now.
			Destroy(childrenPillarGameObjects[pillarIndexCounter - 1]);

			//Go to pillar part above it
			pillarIndexCounter--;
		}

		topPillarDeathVFX.Play();

		//Wait so the particles dissipate completely
		yield return new WaitForSeconds(deathFlameVFX.duration);

		//We are done here, so we delete the entire pillar.
		Destroy(gameObject);

	}

	/*
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Keypad9))
			StartSpellDeathCoroutine();
	}*/

	//Gotta reset because it goes behind background at some point LUL
	public void ResetLayer()
	{
		lastPillarLayer = 45;
	}
}
