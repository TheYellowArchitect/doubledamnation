using NaughtyAttributes;
using UnityEngine;

public class SpellHealth : Spell
{

	//[Header("Unique Spell Values")]//This header should be a standard for every spell!

	private WarriorHealth warriorHealthScript;

	void Start()
	{
		warriorHealthScript = GetComponent<MageBehaviour>().warriorHealthScript;
	}

	public override void Cast()
	{
		warriorHealthScript.StartDamagedHPIndication();//Animation included ;)
	}

}
