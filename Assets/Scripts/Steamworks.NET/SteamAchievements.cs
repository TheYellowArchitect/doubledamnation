using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamAchievements : MonoBehaviour
{
#if !DISABLESTEAMWORKS
	public static SteamAchievements globalInstance;

	// Use this for initialization
	//Consider Project Settings -> Script Execution Order -> So this always runs after SteamManager
	void Start ()
	{
		globalInstance = this;

		if (SteamManager.Initialized == false)
		{
			gameObject.SetActive(false);
			return;
		}

		UnlockAchievement("Achievement_0");
	}

	public void UnlockAchievement(string achievementString)
	{
		Debug.Log(SteamFriends.GetPersonaName());//This shows your steam username

		SteamUserStats.SetAchievement(achievementString);
		SteamUserStats.StoreStats();//Without this, the achievement won't be unlocked - it won't reach the steam server.
	}
#endif

}
