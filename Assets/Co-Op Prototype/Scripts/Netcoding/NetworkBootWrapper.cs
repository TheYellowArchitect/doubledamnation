using BeardedManStudios.Forge.Networking;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class NetworkBootWrapper : MonoBehaviour
{
	//Singleton
	public static NetworkBootWrapper globalInstance;

	//Prefab
	public GameObject networkManagerPrefab;
	public GameObject multiplayerMenuPrefab;
	public GameObject steamMultiplayerMenuPrefab;

	public bool playOffline = false;
	public bool hasDisconnected = false;//Flag used to not trigger disconnect event more than once -_-

	[Header("Automatically Host")]
	public bool automaticallyLocallyHostConnect = false;
	public float timeToHost = 10f;
	public float timeToConnect = 15f;

	private GameObject multiplayerMenuInstance;

	// Use this for initialization
	void Start ()
	{
		if (playOffline == false)
		{
			globalInstance = this;

			CreateAndStartMultiplayerMenu();
		}
	}

	public void CreateAndStartMultiplayerMenu()
	{
		//If Steam
		if (SteamManager.Initialized == true)
		{
			multiplayerMenuInstance = GameObject.Instantiate(steamMultiplayerMenuPrefab, transform);

			multiplayerMenuInstance.GetComponent<SteamMultiplayerMenu>().Init(networkManagerPrefab);				
		}
		else//If Local
		{
			multiplayerMenuInstance = GameObject.Instantiate(multiplayerMenuPrefab, transform);

			multiplayerMenuInstance.GetComponent<MultiplayerMenu>().Init(networkManagerPrefab);

#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
			if (automaticallyLocallyHostConnect)
				multiplayerMenuInstance.GetComponent<MultiplayerMenu>().AutomaticallyLocallyHostConnect(timeToHost, timeToConnect);
#endif
		}


		//hasDisconnected = false;//This is set at NetworkCommunicationController.FinalizeNetworkBehaviour()
	}

}