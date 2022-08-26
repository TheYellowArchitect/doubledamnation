using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamRichPresence : MonoBehaviour
{
#if !DISABLESTEAMWORKS
	public static SteamRichPresence globalInstance;

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;

		if (SteamManager.Initialized == false)
		{
			gameObject.SetActive(false);
			return;
		}

		//Debug.Log("Does this work, if yes, what is the gameobject name: " + gameObject.name);
		//Yes it does work, its on ActiveGameScene actually, on gameobject "SteamRichPresence"

		//One of the 2 below shows the game properly
		//SteamFriends.SetRichPresence( "status", "Status!" );
		//SteamFriends.SetRichPresence("steam_display", "Testerino");
		//https://partner.steamgames.com/doc/api/ISteamFriends#SetRichPresence

		//This makes join button
		SteamFriends.SetRichPresence("connect", "Join!");//No idea if this works or does anything of value. Maybe it helps in join bs idk
	}
#endif
}
