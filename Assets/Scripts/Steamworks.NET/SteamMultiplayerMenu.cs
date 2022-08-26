using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamMultiplayerMenu : MonoBehaviour
{
#if !DISABLESTEAMWORKS
	public string gameName = "DoubleDamnation";

	/// <summary>
	/// Flag to indicate that Forge is in the process of connecting to a server/lobby
	/// </summary>
	public bool IsConnecting { get; private set; }

	public bool useMainThreadManagerForRPCs = true;

	public GameObject networkManager = null;

	private NetworkManager mgr = null;
	private NetWorker server;
	private NetWorker client;

	/// <summary>
	/// The Steam ID of the selected lobby to join.
	/// </summary>
	/// <remarks>This value is set by the join menu when a server in the server list is clicked</remarks>
	private CSteamID joiningLobby;
	private CSteamID hostedLobby;

	// Use this for initialization
	public void Init(GameObject prefabNetworkManager)
	{
		networkManager = prefabNetworkManager;

		if (useMainThreadManagerForRPCs)
			Rpc.MainThreadRunner = MainThreadManager.Instance;

		DetermineInviteLobbyCommandLineArgument();
	}


//This means it runs only from Unity, while ofc having Steam open, but then... how to confirm the connection?!
#if DEVELOPMENT_BUILD || UNITY_EDITOR
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3) && Input.GetKey(KeyCode.RightShift))
			Host();

		if (Input.GetKeyDown(KeyCode.F2) && Input.GetKey(KeyCode.RightShift))
			Connect();
	}
#endif

	/// <summary>
	/// Handle setting up a host. Called by the host button.
	/// </summary>
	public void Host()
	{
		//If have already hosted
		if (server != null)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(hostedLobby);
			return;
		}

		//If trying to host while connected as client
		if (client != null)
			return;

		// Currently there is a bug in the SteamP2PServer code where the lobby max member count is hard coded to be 5.
		// Until a fix is in place please change line 186 of the SteamP2PServer to read
		//    `m_CreateLobbyResult = SteamMatchmaking.CreateLobby(lobbyType, MaxConnections);`
		server = new SteamP2PServer(2);

		// Don't yet have a way to invite players to lobby. Until then all hosts are set to be public
		//((SteamP2PServer)server).Host(SteamUser.GetSteamID(), isPrivateLobby ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic, OnLobbyReady);
		((SteamP2PServer)server).Host(SteamUser.GetSteamID(), ELobbyType.k_ELobbyTypePublic, OnLobbyReady);

		server.playerTimeout += (player, sender) => { Debug.Log("Player " + player.NetworkId + " timed out"); };

		///Debug.Log("STEAMHOSTED!");

		Connected(server);

	}

	/// <summary>
	/// Sets the lobby to be joined when clicking the accept invite button.
	/// </summary>
	/// <param name="steamId"></param>
	public void SetLobbyToJoin(CSteamID steamId)
	{
		joiningLobby = steamId;
	}

	public void ConnectToLobby(CSteamID steamId)
	{
		joiningLobby = steamId;
		Connect();
	}

	/// <summary>
	/// Handle the connecting the selected lobby/server
	/// </summary>
	public void Connect()
	{
		IsConnecting = true;

		// Need to select a lobby first.
		if (joiningLobby == CSteamID.Nil)
			return;

		Debug.Log("Client, joining Lobby ID:" + joiningLobby);

		Debug.Log("Client name: " + client);
		if (client != null)
			Debug.Log("Binded: " + client.IsBound);

		//Trying to catch edge-case errors eg trying to connect when already connected or when tried to connect while offline
		if (client != null)
		{
			//Already connected, don't even try to connect again (or enjoy errors ;)
			if (client.IsBound)
				return;
			else//tried to connect somewhere without a host
			{
				client = null;

				Debug.Log("Initialized: " + NetworkManager.Instance);
				if (NetworkManager.Instance != null)
					NetworkManager.Instance.Disconnect();
				
			}
				
		}
		
		client = new SteamP2PClient();
		((SteamP2PClient)client).Connect(joiningLobby);

		// Steamworks API calls are async so we need to delay the rest of the networker setup until
		// the local user joins the selected lobby.
		client.bindSuccessful += (networker) => {
			MainThreadManager.Run(() =>
			{
				Connected(client);
			});
		};

		client.bindFailure += sender =>
		{
			MainThreadManager.Run(() =>
			{
				FailedConnection();
			});
		};

		client.disconnected += sender =>
		{
			MainThreadManager.Run(() =>
			{
				FailedConnection();
			});
		};
	}

	/// <summary>
	/// Setup to run after the <see cref="NetWorker"/> has connected
	/// </summary>
	/// <param name="networker"></param>
	public void Connected(NetWorker networker)
	{
		if (!networker.IsBound)
		{
			Debug.LogError("NetWorker failed to bind");
			return;
		}

		//It should NEVER get here, but who knows.
		if (mgr == null && networkManager == null)
		{
			Debug.LogError("A network manager was not provided REEE");
			networkManager = new GameObject("Network Manager");
			mgr = networkManager.AddComponent<NetworkManager>();
		}

		mgr = Instantiate(networkManager).GetComponent<NetworkManager>();
		mgr.Initialize(networker);

		if (networker is IServer)
			NetworkObject.Flush(networker); //Called because we are already in the correct scene!

		///Debug.Log("STEAMCONNECTED!");

		//========


		NetworkManager.Instance.Networker.playerAccepted += ConnectedToClient;
		NetworkManager.Instance.Networker.serverAccepted += ConnectedToServer;
	}

	private void ConnectedToClient(NetworkingPlayer player, NetWorker sender)
	{
		Debug.Log("The server player has connected!");

		//Is it even possible to be Nil, if its here?
		//Whatever, better safe than sorry.
		if (hostedLobby != CSteamID.Nil)
		{
			MainThreadManager.Run(() =>
			{
				PlayerStatsManager.globalInstance.SetSteamLobbyId((ulong)hostedLobby);

				//2 users only inside, 0 is host, 1 is client.
				PlayerStatsManager.globalInstance.SetSteamFriendId((ulong)SteamMatchmaking.GetLobbyMemberByIndex(hostedLobby, 0));
			});
		}
	}

	private void ConnectedToServer(NetWorker sender)
	{
		Debug.Log("The client has connected!");

		NetworkObject.Flush(sender);

		MainThreadManager.Run(() => NetworkManager.Instance.InstantiateNetworkCommunicationController());

		//Is it even possible to be Nil, if its here?
		//Whatever, better safe than sorry.
		if (joiningLobby != CSteamID.Nil)
		{
			MainThreadManager.Run(() =>
			{
				PlayerStatsManager.globalInstance.SetSteamLobbyId((ulong) joiningLobby);

				//2 users only inside, 0 is host, 1 is client.
				PlayerStatsManager.globalInstance.SetSteamFriendId((ulong) SteamMatchmaking.GetLobbyMemberByIndex(joiningLobby, 1));
			});
		}
	}

	/// <summary>
	/// Callback used when a host successfully created a lobby.
	/// Sets lobby metadata that is used on the server browser.
	/// </summary>
	private void OnLobbyReady()
	{
		// If the host has not set a server name then let's use his/her name instead to name the lobby
		string personalName = SteamFriends.GetPersonaName();

		hostedLobby = ((SteamP2PServer) server).LobbyID;

		Debug.Log("Lobby ID is:" + hostedLobby);

		// Set the name of the lobby
		SteamMatchmaking.SetLobbyData(hostedLobby, "name", gameName);

		// Set the unique id of our game so the server list only gets the games with this id -> Is actually the string
		SteamMatchmaking.SetLobbyData(hostedLobby, "dd_gameId", personalName);

		// Set all other game information
		//SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameType", type);
		//SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameMode", mode);
		//SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameDesc", comment);

		SteamFriends.ActivateGameOverlayInviteDialog(hostedLobby);

		#if UNITY_STANDALONE_WIN
			if (DisplayNetwork.globalInstance != null)
				DisplayNetwork.globalInstance.steamLobbyId = hostedLobby;
		#endif
	}

	public void DetermineInviteLobbyCommandLineArgument()
	{
		//From SteamManager.cs
			/*
			string firstArgument = "";
			int secondArgument = 0;//Get argument[0] since there can be many in the command line!
			
			string thirdArgument = "";
			string fourthArgument = "";
			SteamApps.GetLaunchCommandLine(out thirdArgument, 1);
			SteamApps.GetLaunchCommandLine(out fourthArgument, 2);
			
			Debug.Log("Second and Third arguments are: " + thirdArgument + " || " + fourthArgument);

			SteamApps.GetLaunchCommandLine(out firstArgument, secondArgument);
			Debug.Log("First Argument: " + firstArgument + " First Argument Index: " + secondArgument);

			//Was invited
			if (firstArgument.Length > 1)
			{
				//Going from the end (definitely int) towards letter "connect_lobby"
				int i = firstArgument.Length -1;
				while (i > 0)
				{
					//if "y" of "connect_lobby"
					if (System.Char.IsLetter(firstArgument[i]))
						break;
					i--;
				}

				firstArgument = firstArgument.Substring(i);

				Debug.Log("Cropped first argument is: " + firstArgument);
				
				//ulong is unsigned integer 64-bit, aka uint64 in c++
				ulong lobbyID;
				if (ulong.TryParse(firstArgument, out lobbyID))//if number
				{
					Debug.Log("Lobby ID is: " + lobbyID);
					ConnectToLobby((CSteamID)lobbyID);
				}
				else
				{
					firstArgument = firstArgument.Trim();

					Debug.Log("Trimmed first argument is: " + firstArgument);

					if (ulong.TryParse(firstArgument, out lobbyID))//if number
					{
						Debug.Log("Trimmed Lobby ID is: " + lobbyID);
						ConnectToLobby((CSteamID)lobbyID);
					}
					else
					{
						//Remove last character
						firstArgument = firstArgument.Remove(firstArgument.Length - 1);

						Debug.Log("Removedlastchar first argument is: " + firstArgument);

						if (ulong.TryParse(firstArgument, out lobbyID))//if number
						{
							Debug.Log("Removedlastchar Lobby ID is: " + lobbyID);
							ConnectToLobby((CSteamID)lobbyID);
						}
					}
				}

				Debug.Log("ASCII ByteStringSize: " + System.Text.ASCIIEncoding.Unicode.GetByteCount(firstArgument));
				Debug.Log("Unicode ByteStringSize: " + System.Text.ASCIIEncoding.ASCII.GetByteCount(firstArgument));
				//All ways to convert string to Uint64 in Unity
				//https://forum.unity.com/threads/how-to-use-convert-int64-in-unity3d.497209/
			}
			*/

			Debug.Log("Now, going to catch default command args!");

			string[] args = System.Environment.GetCommandLineArgs();
			//0 -> C:\Games\Steam\steamapps\common\Double Damnation\DoubleDamnationSteam.exe
			//1 -> +connect_lobby
			//2 -> 109775242277515446

			ulong lobbyID;
			for (int i = 0; i < args.Length; i++)
			{
				Debug.Log("Arg: " + i + " : " + args[i]);

				if (args[i] == "+connect_lobby")//99% of the cases
				{
					//Debug.Log("+connect_lobby AT INDEX " + i);

					if (ulong.TryParse(args[i+1], out lobbyID))//if number
					{
						//Debug.Log("Lobby ID is: " + lobbyID);
						ConnectToLobby((CSteamID)lobbyID);
						break;
					}
				}
				else if (ulong.TryParse(args[i], out lobbyID))//if number
				{
					//Debug.Log("Number argument detected at index: " + i);
					//Debug.Log("Lobby ID is: " + lobbyID);
					ConnectToLobby((CSteamID)lobbyID);
					break;
				}
			}
				

	}

	/// <summary>
	/// Called when failed to connect to a lobby.
	/// </summary>
	private void FailedConnection()
	{
		if (IsConnecting)
			IsConnecting = false;

		Debug.Log("Failed to connect, reason unknown.");

		if (NetworkManager.Instance != null)
			NetworkManager.Instance.Disconnect();
	}

	//Called when destroyed by NetworkBootWrapper
	void OnDestroy()
	{
		OnApplicationQuit();
	}

	private void OnApplicationQuit()
	{
		//MainThreadManager.Run(DisconnectServer);
		DisconnectServer();

		Destroy(FindObjectOfType<NetworkManager>());
	}

	public void DisconnectServer()
	{
		if (server != null)
			server.Disconnect(true);
	}
#endif
}
