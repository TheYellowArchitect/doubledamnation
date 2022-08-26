using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif


public class SteamInviteCallbacks : MonoBehaviour
{
#if !DISABLESTEAMWORKS
	private Callback<GameLobbyJoinRequested_t> callbackLobbyJoinRequest;
	private Callback<LobbyInvite_t> callbackLobbyInvite;

	private void Start()
	{
		if (SteamManager.Initialized == false)
		{
			gameObject.SetActive(false);
			return;
		}

		callbackLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
		callbackLobbyInvite = Callback<LobbyInvite_t>.Create(OnLobbyInvite);
	}

	private void OnDestroy()
	{
		callbackLobbyJoinRequest = null;
	}

	/// <summary>
	/// Handle the lobby join requests when already in game for an invite
	/// </summary>
	/// <param name="result"></param>
	private void OnLobbyJoinRequested(GameLobbyJoinRequested_t result)
	{
		// TODO: make sure join requests can be accepted if already playing.
		//       that will require setting the lobby id somewhere else and disconnecting from the game first.
		FindObjectOfType<SteamMultiplayerMenu>().ConnectToLobby(result.m_steamIDLobby);
	}

	private void OnLobbyInvite(LobbyInvite_t invite)
	{
		Debug.Log("Should close the overlay now :)");
	}
#endif
}