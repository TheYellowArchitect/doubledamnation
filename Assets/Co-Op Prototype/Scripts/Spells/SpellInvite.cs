using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInvite : Spell
{
	public SteamMultiplayerMenu steamMultiplayerMenu = null;
	private MageBehaviour mageBehaviour = null;

	void Start ()
	{
		if (SteamManager.Initialized == false)
			return;

		mageBehaviour = GetComponent<MageBehaviour>();

		GetSteamMultiplayerMenu();
	}
	
	public override void Cast()
	{
		if (SteamManager.Initialized == false)
			return;

		if (mageBehaviour != null)
			mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Health);

		if (steamMultiplayerMenu == null)
			GetSteamMultiplayerMenu();

		if (steamMultiplayerMenu != null)
			steamMultiplayerMenu.Host();
	}

	public void GetSteamMultiplayerMenu()
	{
		steamMultiplayerMenu = GameObject.FindGameObjectWithTag("NetworkManager").GetComponentInChildren<SteamMultiplayerMenu>();

		if (steamMultiplayerMenu == null)
		{
			Debug.LogError("Didn't find steam multiplayer menu under NetworkManager GameObject");
			steamMultiplayerMenu = FindObjectOfType<SteamMultiplayerMenu>();
		}

		if (steamMultiplayerMenu == null)
			Debug.LogError("SteamMultiplayerMenu simply does not exist.");
	}
}
