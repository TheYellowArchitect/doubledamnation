using System.Collections.Generic;
using System.IO;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMenu : MonoBehaviour
{
	/*public InputField ipAddress = null;
	public InputField portNumber = null;*/
	public bool changeSceneOnConnect = false;
	public string masterServerHost = string.Empty;
	public ushort masterServerPort = 15940;
	public string natServerHost = string.Empty;
	public ushort natServerPort = 15941;

	private GameObject networkManager;
	private NetworkManager mgr = null;
	private NetWorker server;
	private NetWorker client;

	public bool useMainThreadManagerForRPCs = true;

	public bool getLocalNetworkConnections = false;

	//==
	public string ipAddress = "127.0.0.1";
	public string portNumber = "15937";

	public void Init(GameObject prefabNetworkManager)
	{
		/*
		ipAddress.text = "127.0.0.1";
		portNumber.text = "15937";


		for (int i = 0; i < ToggledButtons.Length; ++i)
		{
			Button btn = ToggledButtons[i].GetComponent<Button>();
			if (btn != null)
				_uiButtons.Add(btn);
		}*/
		networkManager = prefabNetworkManager;

		// Do any firewall opening requests on the operating system
		NetWorker.PingForFirewall((ushort)15937);

		if (useMainThreadManagerForRPCs)
			Rpc.MainThreadRunner = MainThreadManager.Instance;

		if (getLocalNetworkConnections)
		{
			NetWorker.localServerLocated += LocalServerLocated;
			NetWorker.RefreshLocalUdpListings((ushort)15937);
		}
	}

#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
	public void AutomaticallyLocallyHostConnect(float hostConnectionTime, float clientConnectionTime)
	{
		List<string> directoryList = new List<string>(Directory.GetDirectories(Directory.GetCurrentDirectory()));

		if (ParrelSync.ClonesManager.IsClone())
			Invoke("Connect", clientConnectionTime);
		else
			Invoke("Host", hostConnectionTime);		
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3) && Input.GetKey(KeyCode.RightShift))
			Host();

		if (Input.GetKeyDown(KeyCode.F2) && Input.GetKey(KeyCode.RightShift))
			Connect();
	}
#endif

	private void LocalServerLocated(NetWorker.BroadcastEndpoints endpoint, NetWorker sender)
	{
		Debug.Log("Found endpoint: " + endpoint.Address + ":" + endpoint.Port);
	}

	public void Connect()
	{
		/*
		ushort port;
		if(!ushort.TryParse(portNumber, out port))
		{
			Debug.LogError("The supplied port number is not within the allowed range 0-" + ushort.MaxValue);
		    	return;
		}

		NetWorker client;

		client = new UDPClient();
		if (natServerHost.Trim().Length == 0)
			((UDPClient)client).Connect(ipAddress, (ushort)port);
		else
			((UDPClient)client).Connect(ipAddress, (ushort)port, natServerHost, natServerPort);
		*/

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

		client = new UDPClient();
		if (natServerHost.Trim().Length == 0)
			((UDPClient)client).Connect(ipAddress, (ushort)15937);
		else
			((UDPClient)client).Connect(ipAddress, (ushort)15937, natServerHost, natServerPort);

		Connected(client);
	}

	public void Host()
	{
		//If have already hosted
		if (server != null)
			return;

		//If trying to host while connected as client
		if (client != null)
			return;
		

		server = new UDPServer(2);

		if (natServerHost.Trim().Length == 0)
			((UDPServer)server).Connect(ipAddress, (ushort)15937);
		else
			((UDPServer)server).Connect(port: (ushort)15937, natHost: natServerHost, natPort: natServerPort);

		server.playerTimeout += (player, sender) =>
		{
			Debug.Log("Player " + player.NetworkId + " timed out");
		};
		//LobbyService.Instance.Initialize(server);

		Connected(server);

		Debug.Log("Hosted!");
	}



	private void TestLocalServerFind(NetWorker.BroadcastEndpoints endpoint, NetWorker sender)
	{
		Debug.Log("Address: " + endpoint.Address + ", Port: " + endpoint.Port + ", Server? " + endpoint.IsServer);
	}

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
		mgr.Initialize(networker, masterServerHost, masterServerPort);

		if (networker is IServer)
		{
			if (changeSceneOnConnect)
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
			else
				NetworkObject.Flush(networker); //Called because we are already in the correct scene!
		}

		//===



		NetworkManager.Instance.Networker.playerAccepted += ConnectedToClient;
		NetworkManager.Instance.Networker.serverAccepted += ConnectedToServer;
	}

	private void ConnectedToClient(NetworkingPlayer player, NetWorker sender)
	{
		/*Debug.Log("Pending objects: " + NetworkManager.Instance.pendingNetworkObjects.Count);

		Dictionary<Int32, NetworkObject> tempDictionary = NetworkManager.Instance.pendingNetworkObjects;
		foreach(NetworkObject item in tempDictionary.Values)
			Debug.Log("Pending: " + item.Rpcs.Count);*/

		Debug.Log("The server player has connected!");

		//MainThreadManager.Run(() => NetworkManager.Instance.InstantiateNetworkCommunicationController());
	}

	private void ConnectedToServer(NetWorker sender)
	{
		/*Debug.Log("Pending objects: " + NetworkManager.Instance.pendingNetworkObjects.Count);*/

		Debug.Log("The client has connected!");

		NetworkObject.Flush(sender);

		MainThreadManager.Run(() => NetworkManager.Instance.InstantiateNetworkCommunicationController());
	}

	//Called when destroyed by NetworkBootWrapper
	void OnDestroy()
	{
		OnApplicationQuit();
	}

	private void OnApplicationQuit()
	{
		//if (getLocalNetworkConnections)
		if (true)
			NetWorker.EndSession();

		MainThreadManager.Run(DisconnectServer);
		//DisconnectServer();

		Destroy(FindObjectOfType<NetworkManager>());
	}

	public void DisconnectServer()
	{
		if (server != null)
			server.Disconnect(true);
	}


}