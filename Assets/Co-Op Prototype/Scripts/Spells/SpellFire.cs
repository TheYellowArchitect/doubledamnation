using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellFire : Spell
{

	[Header("Unique Spell Values")]//This header should be a standard for every spell!
	[Tooltip("Fireball gameobject that shall be spawned in Level1")]
	public GameObject prefabFireballProjectile1;

	[Tooltip("Fireball gameobject that shall be spawned in Level2")]
	public GameObject prefabFireballProjectile2;

	[Tooltip("Fireball gameobject that shall be spawned in Level3")]
	public GameObject prefabFireballProjectile3;

	[Tooltip("The SFX heard when player casts fireball")]
	public AudioClip spellCastFireball;

	private Vector2 facingVector;
	private Vector3 fireballOffsetSpawnPoint;
	private Vector3 fireballRotation;
	private GameObject spawnedFireballGameObject;
	private ProjectileDamageTrigger spawnedProjectileDamageTrigger;

	private ManaManager playerManaManager;
	private MageBehaviour mageBehaviour;
	private WarriorMovement warriorBehaviour;

	// Use this for initialization
	void Start ()
	{
		playerManaManager = GameObject.Find("ManaManager").GetComponent<ManaManager>();
		mageBehaviour = GetComponent<MageBehaviour>();
		warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();
	}
	
	public override void Cast()
	{
		//Use up the mana
		playerManaManager.AddPlayerMana(-1 * manaCost);


		//Player 2 animation
		mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Fire);

		//facingVector, fireballRotation, fireballOffsetSpawnPoint
		SetFireballRotationValues();

		CreateFireball(transform.position);
	}

	public void SetFireballRotationValues()
	{
		//Fireball's direction, left or right? depends on player facing.
		if (warriorBehaviour.GetIsWallsliding())
		{
			if (warriorBehaviour.GetFacingLeft())
			{
				facingVector = Vector2.right;

				fireballRotation = Vector3.zero;

				fireballOffsetSpawnPoint = new Vector3(2.5f, -0.7f, 0);//-0.7f on Y, because otherwise it will hit a wall if doing it at the top of a wallslide.
			}
				
			else
			{
				facingVector = Vector2.left;

				fireballRotation = new Vector3(0, 0, 180);

				fireballOffsetSpawnPoint = new Vector3(-2.5f, -0.7f, 180);
			}

		}
		else
		{
			if (warriorBehaviour.GetFacingLeft())
			{
				facingVector = Vector2.left;

				fireballRotation = new Vector3(0, 0, 180);

				fireballOffsetSpawnPoint = new Vector3(-2.5f, 0, 180);
			}
			else
			{
				facingVector = Vector2.right;

				fireballRotation = Vector3.zero;

				fireballOffsetSpawnPoint = new Vector3(2.5f, 0, 0);
			}
		}
	}

	public void SetFireballRotationValues(Vector2 targetFacing, Vector3 targetRotation, Vector3 targetOffsetSpawnPoint)
	{
		facingVector = targetFacing;
		fireballRotation = targetRotation;
		fireballOffsetSpawnPoint = targetOffsetSpawnPoint;
	}

	public GameObject CreateFireball(Vector3 targetSpawnPoint, bool playCreateFireballSFX = true)
	{
		//SFX "fireball summoning/launching"
		if (playCreateFireballSFX)
			PlayerSoundManager.globalInstance.PlayClip(spellCastFireball, PlayerSoundManager.AudioSourceName.SpellCast1);


		//Spawn the fireball (prefab)//fkin hell, move all of this to magebehaviour already ffs
		if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
			spawnedFireballGameObject = Instantiate(prefabFireballProjectile1);
		else if (LevelManager.currentLevel == 2)
			spawnedFireballGameObject = Instantiate(prefabFireballProjectile2);
		else// if (LevelManager.currentLevel > 2)
			spawnedFireballGameObject = Instantiate(prefabFireballProjectile3);

		
		//Insanity damage boost
		if (InsanityIntensityManager.globalInstance.GetInsanityActive() == true)
		{
			spawnedProjectileDamageTrigger = spawnedFireballGameObject.GetComponent<ProjectileDamageTrigger>();
			spawnedProjectileDamageTrigger.damagePower++;
		}

		if (fireballRotation != Vector3.zero)
			spawnedFireballGameObject.transform.Rotate(fireballRotation);

		//Set variables to the fireball, and let it run wild :P
		spawnedFireballGameObject.GetComponent<ProjectileDamageTrigger>().InitializeValues
		(facingVector, targetSpawnPoint + fireballOffsetSpawnPoint, -2);

		//DesyncSphere Link Detection
		if (NetworkCommunicationController.globalInstance != null)
			spawnedFireballGameObject.AddComponent<NetworkFireballDesyncSphere>();

		return spawnedFireballGameObject;
	}
}
