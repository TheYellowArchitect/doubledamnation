using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellUp : Spell
{
	[Header("Unique Spell Values")]//This header should be a standard for every spell!
	[Tooltip("Pillar gameobject that is spawned on level 1")]
	public GameObject prefabPillarEarth;

	[Tooltip("Pillar gameobject that is spawned on level 3")]
	public GameObject prefabPillarFire;

	[Tooltip("Pillar gameobject that is spawned on level 3")]
	public GameObject prefabPillarWind;

	private GameObject spawnedPillarGameObject;//Not used anywhere lmao
	private ManaManager playerManaManager;
	private MageBehaviour mageBehaviour;
	//private WarriorMovement warriorBehaviour;


	// Use this for initialization
	void Start ()
	{
		playerManaManager = GameObject.Find("ManaManager").GetComponent<ManaManager>();
		mageBehaviour = GetComponent<MageBehaviour>();
		//warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();
	}
	
	// Update is called once per frame
	public override void Cast ()
	{
		//Use up the mana
		playerManaManager.AddPlayerMana(-1 * manaCost);

		//The pillar behaviour.Start() does its stuff, its own responsibilities so only instantiation is needed.
		if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
			spawnedPillarGameObject = Instantiate(prefabPillarEarth);
		else if (LevelManager.currentLevel == 2)
			spawnedPillarGameObject = Instantiate(prefabPillarFire);
		else if (LevelManager.currentLevel > 2)
			spawnedPillarGameObject = Instantiate(prefabPillarWind);

		//Player 2 animation
		mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Support);

		//SFX
		PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.UpSpell);
	}
}


#region old Support/Up. Janky/Clunky asf. Perhaps in the future add it as a new spell? An emerging rock? (make it work always reach ur feet vertically as fast, never missing, and not slowing down)
	/*
    //Use up the mana
    SetMana(-1 * EarthPillarManaCost);

    //Cast a ray from the player, towards the Ground
    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 200, WhatIsGround);

    if (hit.collider == null)
        Debug.LogError("Pillar raycasting down, WAS NULL");

    //No need to check if hit.collider == null, since it will always hit the ground.

    // How pillar support works
    /* =======
     * Prefab contains: 1 boxcollider2d (Trigger), 1 boxcollider2d(collider)(Initially disabled), 1 kinematic rigidbody, 1 sprite renderer, 1 pillarBehaviour(Script)
     * Spawn prefab on: transform.position + new Vector3(0, hit.distance * 3, 0);
     * Then, give it velocity, towards up.
     * IEnumerator/Timer to kill it after X seconds. Kill = play crumbling animation.
     * For VFX (later stuff): OnTriggerEnter2D: if ground -> VFX DOWNWARDS DUST
     *                        OnTriggerExit2D: if ground -> VFX UPWARDS DUST
     * On player collision (triggerEnter) -> Enable collider(Box2D), disable Trigger(Box2D)

    //Change on the above. trigger collider + ground, instead of trigger and swapping. VFX is by child trigger of pillar. Right now, undone.

    if (LevelManager.currentLevel < 3)
        tempGameObject = Instantiate(prefabSupportProjectileFire);
    else if (LevelManager.currentLevel == 3)
        tempGameObject = Instantiate(prefabSupportProjectileWind);


    tempGameObject.GetComponent<SupportProjectileBehaviour>().InitializeValues(transform.position, hit.distance, !midair);


    //Player 2 animation
    mageBehaviour.CastSpell(MageBehaviour.SpellAnimation.Support);

    //The VFX!
    */
#endregion