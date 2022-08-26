using NaughtyAttributes;
using UnityEngine;

public class SpellPush : Spell
{

	[Header("Unique Spell Values")]//This header should be a standard for every spell!


	[Header("Outwards Hitbox")]


	[MinValue(0f)]
	[Tooltip("What is the radius that pushes the enemies away from the circle VFX, aka how far does a monster need to be to be pushed.")]
	public float outwardsPushRange;//50

	[Tooltip("What is the knockbackPowerX to push the monster horizontally.")]
	public int outwardsPushPowerX;//27

	[Tooltip("When Insanity, what is the knockbackPowerX to push the monster horizontally.")]
	public int outwardsPushInsanityPowerX;//27

	[Header("Inwards Hitbox")]


	[MinValue(0f)]
	[Tooltip("What is the radius that pushes the enemies away from the circle VFX, aka how far does a monster need to be to be pushed.")]
	public float inwardsPushRange;//50

	[Tooltip("What is the knockbackPowerX to push the monster horizontally.")]
	public int inwardsPushPowerX;//27

	[Tooltip("When Insanity, what is the knockbackPowerX to push the monster horizontally.")]
	public int inwardsPushInsanityPowerX;//27




	[Header("Ripple Visuals")]


	[Tooltip("Friction/Jitteriness/Zigzaginess to add on ripple")]
	public float rippleFriction;//0.9f


	[Tooltip("Strength/Power to add on ripple")]
	public float rippleMaxAmount;//25

	[MinValue(0f)]
	[Tooltip("When Insanity, Friction/Jitteriness/Zigzaginess to add on ripple")]
	public float rippleInsanityFriction;//0.9f

	[MinValue(0f)]
	[Tooltip("When Insanity, Strength/Power to add on ripple")]
	public float rippleInsanityMaxAmount;//25










	[Header("Misc")]

	[MinValue(0f)]
	[Tooltip("What should the difference of midair push be to ground?")]
	public int pushPowerXMidairDelta;

	[Tooltip("The SFX heard when player casts push")]
	public AudioClip spellCastPush;




	private Collider2D[] pushColliders;// Initialized here, so they wont be created upon every ground/jump/anything/collision check.

	private ManaManager playerManaManager;
	private MageBehaviour mageBehaviour;
	private WarriorMovement warriorBehaviour;

	private RipplePostProcessor cameraRipple;
	private GameObject warriorRenderer;


	private int currentPushPowerX;


	// Use this for initialization
	void Start ()
	{
		playerManaManager = GameObject.Find("ManaManager").GetComponent<ManaManager>();
		mageBehaviour = GetComponent<MageBehaviour>();
		warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

		cameraRipple = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RipplePostProcessor>();
		warriorRenderer = GameObject.FindGameObjectWithTag("PlayerRenderer");

	}

	public override void Cast()
	{
		//Use up the mana
		playerManaManager.AddPlayerMana(-1 * manaCost);

		//Does everything ripplevisuals-related.
		DetermineRippleEffect();

		//Does everything hitbox related.
		DeterminePushColliders();

		//Player 2 animation
		mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Push);

		//The VFX
		DetermineVFXSpawn();

		//SFX
		PlayerSoundManager.globalInstance.PlayClip(spellCastPush, PlayerSoundManager.AudioSourceName.SpellCast1, 1, 1);
	}

	/// <summary>
	/// 1. Checks if LevelEditor and stopped/paused
	/// 2. Checks if RipplePostProcessor is active. If not, activates it.
	/// 3. Sets values of RipplePostProcessor
	/// 4. Ripples the RipplePostProcessor
	/// </summary>
	public void DetermineRippleEffect()
	{
		if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
			return;

		//Check if disabled
		if (cameraRipple.enabled == false)
			cameraRipple.enabled = true;

		//Changing the RipplePostProcessor Values to "default" cuz insanity activated means the values are different!
		if (InsanityIntensityManager.globalInstance.GetInsanityActive() == false)
			cameraRipple.SetValues(rippleMaxAmount, rippleFriction);
		else
			cameraRipple.SetValues(rippleInsanityMaxAmount, rippleInsanityFriction);

		//When you move it to mage behaviour, put the component as a starting/private variable. In other words, cache it to save performance.
		cameraRipple.RippleEffect(Camera.main.WorldToScreenPoint(warriorRenderer.transform.position));
	}

	/// <summary>
	/// What it does is draws/gets the colliders of enemies nearby
	/// Depending on their distance, pushes them away (via takeDamage 0 dmg)
	/// And if insanity changes the values a bit.
	/// </summary>
	public void DeterminePushColliders()
	{
		//===Outwards===
		//If not insanity, normal push power, otherwise insanity push power
		/*if (InsanityIntensityManager.globalInstance.GetInsanityActive() == false)
			currentPushPowerX = outwardsPushPowerX;
		else
			currentPushPowerX = outwardsPushInsanityPowerX;

		//If midair, push more
		if (warriorBehaviour.GetMidair() == true)
			currentPushPowerX = currentPushPowerX + pushPowerXMidairDelta;


		//Outwards
		PushColliders(outwardsPushRange, currentPushPowerX);*/



		//This used to be after all the logic, but I guess moving the order shouldn't create any new bug
		DestroyPillars (outwardsPushRange);

		//If level editor pause mode, enemies dont have proper colliders, nullreferenceexception error -_-
		if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
			return;

		if (InsanityIntensityManager.globalInstance.GetInsanityActive() == false)
			PushColliders(outwardsPushRange, currentPushPowerX, false);
		else
			PushColliders(outwardsPushRange, currentPushPowerX, true);



		//===Inwards===

		//If not insanity, normal push power, otherwise insanity push power
		/*if (InsanityIntensityManager.globalInstance.GetInsanityActive() == false)
			currentPushPowerX = inwardsPushPowerX;
		else
			currentPushPowerX = inwardsPushInsanityPowerX;

		//If midair, push more
		if (warriorBehaviour.GetMidair() == true)
			currentPushPowerX = currentPushPowerX + pushPowerXMidairDelta;*/

		//And inwards
		PushColliders(inwardsPushRange, currentPushPowerX, true);
	}

	public void PushColliders(float pushRange, int pushPowerX, bool strongPushKnockback)
	{
		//Get all the enemies within range
		//colliders = Physics2D.OverlapCircleAll(transform.position, pushRange, WhatIsDamageable | LayerMask.NameToLayer("Default"));
		pushColliders = Physics2D.OverlapCircleAll(warriorRenderer.transform.position, pushRange, warriorBehaviour.WhatIsDamageable | 1 << LayerMask.NameToLayer("Default"));

		//Iterate through every "enemy", to push him back, BACK - TO THE PIT!
		for (int i = 0; i < pushColliders.Length; i++)
		{
			//Note:
			// Enemy dmg works differently from player: hitstun is a multiplier instead of flat timer, and knockbackPowerXY is ignored.
			// By giving transform.position, it gets the angle by itself.
			// Applying damage on an enemy, but without the damage , does work just for dat knockback effect.

			//Confirm it is not a hazard.(or y'know change hazard layer HMMM)
			if (pushColliders[i].gameObject.CompareTag("Enemy"))
				pushColliders[i].gameObject.GetComponent<EnemyBehaviour>().TakeDamage(0, warriorRenderer.transform.position, pushPowerX, 15, 0.8f, strongPushKnockback);//The hitstun multiplier is more or less "for how much pushed?"
			else if (pushColliders[i].gameObject.CompareTag("EnemyProjectile"))
				pushColliders[i].gameObject.GetComponent<ProjectileDamageTrigger>().Deflect(warriorBehaviour.WhatIsDamageable);
		}
	}

	public void DetermineVFXSpawn()
	{
		if (warriorBehaviour.GetSidewalled() == false)
		{
			if (warriorBehaviour.GetMidair())
			{
				VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushMidair, warriorRenderer.transform.position + new Vector3(0, 3.4f, 0), gameObject);
				VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushMidair, warriorRenderer.transform.position + new Vector3(0, 3.4f, 0), gameObject);
				VFXManager.globalInstance.GetLastCreatedVFX().GetComponent<SpriteRenderer>().flipX = true;
			}
			else
			{
				VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushGround, warriorRenderer.transform.position + new Vector3(0, 3.4f, 0));
				VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushGround, warriorRenderer.transform.position + new Vector3(0, 3.4f, 0));
				VFXManager.globalInstance.GetLastCreatedVFX().GetComponent<SpriteRenderer>().flipX = true;
			}
		}
		else//spagghetti, all for proper visuals on wall collision
		{
			//VFX spawning looks towards the right

			//if warrior looks towards the right, then spawn only to the left
			if (warriorBehaviour.GetIsWallslidingLeft())
			{
				if (warriorBehaviour.GetMidair())
					VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushMidair, warriorRenderer.transform.position + new Vector3(2.655f, 2.4f, 0), gameObject);
				else
					VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushGround, warriorRenderer.transform.position + new Vector3(2.655f, 3.4f, 0));
			}
			else
			{

				if (warriorBehaviour.GetMidair())
				{
					VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushMidair, warriorRenderer.transform.position + new Vector3(-2.655f, 2.4f, 0), gameObject);
					VFXManager.globalInstance.GetLastCreatedVFX().GetComponent<SpriteRenderer>().flipX = true;
				}
				else
				{
					VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PushGround, warriorRenderer.transform.position + new Vector3(-2.655f, 3.4f, 0));
					VFXManager.globalInstance.GetLastCreatedVFX().GetComponent<SpriteRenderer>().flipX = true;
				}


			}

		}
	}

	/// <summary>
	/// If any pillar is in range, DESTROY IT!
	/// </summary>
	public void DestroyPillars(float pushRange)
	{
		pushColliders = Physics2D.OverlapCircleAll(warriorRenderer.transform.position, pushRange, 1 << LayerMask.NameToLayer("Ground"));

		//Iterate through every pillar detected
		for (int i = 0; i < pushColliders.Length; i++)
		{
			//Confirm it is a pillar and not ground
			if (pushColliders[i].gameObject.CompareTag("Pillar"))
				if (pushColliders[i].transform.parent.GetComponent<FinalPillarBehaviour>() != null)//If middle or top pillar
					pushColliders[i].transform.parent.GetComponent<FinalPillarBehaviour>().StartSpellDeathCoroutine();
				else//if the small parts of top pillar -_-
					pushColliders[i].transform.parent.parent.GetComponent<FinalPillarBehaviour>().StartSpellDeathCoroutine();
		}
	}

	//Shows the radius(es) of "Push"
#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		//Draw outwards push range
		UnityEditor.Handles.color = new Color(0f, 0f, 1.0f, 0.1f);
		UnityEditor.Handles.DrawSolidDisc(transform.parent.position, Vector3.back, outwardsPushRange);

		//Draw inwards push range
		UnityEditor.Handles.color = new Color(0.3f, 0.5f, 1.0f, 0.1f);
		UnityEditor.Handles.DrawSolidDisc(transform.parent.position, Vector3.back, inwardsPushRange);
	}
#endif
}
