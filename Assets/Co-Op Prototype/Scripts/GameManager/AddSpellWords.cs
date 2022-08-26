using System;
using System.Collections.Generic;
using UnityEngine;

//Spellwords are added onto Word Manager, when conditions are right.
public class AddSpellWords : MonoBehaviour
{
	public static AddSpellWords globalInstance;


	public int deathNumberForModify = 5;
	public int deathNumberForInsanity = 18;


	private WordManager commonWordManager;


	// Use this for initialization
	void Start ()
	{
		globalInstance = this;

		commonWordManager = GetComponent<WordManager>();

		GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DiedEvent += DetermineAddSpellWordOnDeath;

		if (SteamManager.Initialized)
			commonWordManager.AddSpellWord("invite");
	}

	public void DetermineAddSpellWordOnDeath()
	{
		if (PlayerStatsManager.globalInstance.GetTotalDeaths() == deathNumberForModify)
			commonWordManager.AddSpellWord("distort");
		else if (PlayerStatsManager.globalInstance.GetTotalDeaths() == deathNumberForInsanity && SteamManager.Initialized == false)
			commonWordManager.AddSpellWord("insanity");
	}

	//Called by LevelManager
	public void OnLevelLoad()
	{
		if (LevelManager.currentLevel == 1 || LevelManager.currentLevel == 7)
			commonWordManager.AddSpellWord("death");
	}

	public void AddLinkFracture()
	{
		commonWordManager.AddSpellWord("link");
		commonWordManager.AddSpellWord("branch");
		//commonWordManager.AddSpellWord("tear");
	}

	public void RemoveLinkFracture()
	{
		commonWordManager.RemoveSpellWord("link");
		commonWordManager.RemoveSpellWord("branch");
		//commonWordManager.RemoveSpellWord("tear");
	}
	

}
