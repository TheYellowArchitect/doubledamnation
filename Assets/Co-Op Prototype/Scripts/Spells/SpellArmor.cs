using NaughtyAttributes;
using UnityEngine;

public class SpellArmor :  Spell
{

	[Header("Unique Spell Values")]//This header should be a standard for every spell!

	[MinValue(0f)]
	[Tooltip("What is the duration of invulnerability from using armor spell, in seconds.")]
	public float armorInvulnerabilityDuration;//2


	private WarriorHealth warriorHealthScript;
	private ManaManager playerManaManager;
	private MageBehaviour mageBehaviour;
	private AetherMage aetherBehaviour;


	// Use this for initialization
	void Start ()
	{
		warriorHealthScript = GetComponent<MageBehaviour>().warriorHealthScript;
		playerManaManager = GameObject.Find("ManaManager").GetComponent<ManaManager>();
		mageBehaviour = GetComponent<MageBehaviour>();
		aetherBehaviour = GetComponent<AetherMage>();
	}
	
	public override void Cast()
	{
		//Use up the mana
		playerManaManager.AddPlayerMana(-1 * manaCost);

		//Activate Invulnerability
		warriorHealthScript.ActivateArmorSpellInvulnerability(armorInvulnerabilityDuration);

		//Player 2 animation
		mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Armor);

		aetherBehaviour.CastAether();
	}
}
