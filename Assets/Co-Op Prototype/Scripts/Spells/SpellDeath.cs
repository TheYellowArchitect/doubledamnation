using NaughtyAttributes;
using UnityEngine;

public class SpellDeath : Spell
{

	//[Header("Unique Spell Values")]//This header should be a standard for every spell!

	private WarriorHealth warriorHealthScript;

	//This is for windLevelBoss scene/creditBosses, after you win, you shouldnt be able to die.
	//Don't remove death spellword, having the voiceline is far more impactful.
	public bool deathSpellwordEasterEggEnabled = false;
	public bool deathSpellwordEasterEggActivated = false;

	public void Start()
	{
		warriorHealthScript = GetComponent<MageBehaviour>().warriorHealthScript;
	}

	public override void Cast()
	{
		if (deathSpellwordEasterEggEnabled == false)
			warriorHealthScript.DieSimple();
		else if (deathSpellwordEasterEggActivated == false)//"You thought you could escape?!" and deny death ;)
		{
			deathSpellwordEasterEggActivated = true;

			GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(14);
		}

	}
}