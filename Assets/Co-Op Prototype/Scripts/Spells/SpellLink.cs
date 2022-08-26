using BeardedManStudios.Forge.Networking;
using NaughtyAttributes;
using UnityEngine;

public class SpellLink : Spell
{
	[Header("Unique Spell Values")]//This header should be a standard for every spell!

	[MinValue(0f)]
	[Tooltip("What is the radius that kills enemies?")]
	public float killRange;//50


	private Collider2D[] pushColliders;// Initialized here, so they wont be created upon every ground/jump/anything/collision check.
	private GameObject warriorRenderer;
	private WarriorMovement warriorBehaviour;
	private ManaManager playerManaManager;


	void Start()
	{
		warriorRenderer = GameObject.FindGameObjectWithTag("PlayerRenderer");
		warriorBehaviour = FindObjectOfType<WarriorMovement>();
		playerManaManager = GameObject.Find("ManaManager").GetComponent<ManaManager>();
	}

	//By having it need some mana instead of instant, it promotes playing a bit desynced to kill enemies!
	public override void Cast()
	{
		MageCastLink();
	}

	public void MageCastLink()
	{
		//Use up the mana
		playerManaManager.AddPlayerMana(-1 * manaCost);

		//If Mage + Desynchronized
		if (NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized() == false)
			NetworkSynchronizer.globalInstance.MageSynchronizeLink();
	}

	public void LocalCastEffect()
	{
		//Kill everything around player
		DetermineAreaKill();

		VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MageLink, transform.position, transform.parent.gameObject);
	}

	//If I have time to polish, one of the billion things I would do, is check if going through enemyList to see playerDistance, is more performant
	public void DetermineAreaKill()
	{
		//If paused, dont do any damage to monsters duh
		if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
			return;

		//Get all the enemies within range
		pushColliders = Physics2D.OverlapCircleAll(warriorRenderer.transform.position, killRange, warriorBehaviour.WhatIsDamageable | 1 << LayerMask.NameToLayer("Default"));

		//Iterate through every "enemy"
		for (int i = 0; i < pushColliders.Length; i++)
		{
			if (pushColliders[i].gameObject.CompareTag("Enemy"))
				pushColliders[i].gameObject.GetComponent<EnemyBehaviour>().TakeDamage(48, warriorRenderer.transform.position, 0, 15, 0.8f, false);//The hitstun multiplier is more or less "for how much pushed?"
			else if (pushColliders[i].gameObject.CompareTag("EnemyProjectile"))
				pushColliders[i].gameObject.GetComponent<ProjectileDamageTrigger>().Death();
		}
	}

	//Triggered by WarriorMovement when in the hitbox it detects "DesyncSphere" gameobject, aka network/online for sure.
	public void TriggerByWarrior(RpcArgs args)
	{
		MageCastLink();
	}

	//Shows the radius(es) of "Push"
#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		//Draw outwards push range
		UnityEditor.Handles.color = new Color(0f, 0f, 1.0f, 0.1f);
		UnityEditor.Handles.DrawSolidDisc(transform.parent.position, Vector3.back, killRange);
	}
#endif
}
