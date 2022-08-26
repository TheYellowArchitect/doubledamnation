using TMPro;
using UnityEngine;

public class DisplayNetwork : MonoBehaviour
{
	public static DisplayNetwork globalInstance;
	//Should show:
		//Ping
		//Desync
		//Bandwidth

	private TextMeshProUGUI networkUI;
	private NetworkPinger pinger;
	private NetworkDamageShare synchronizer;

	public Steamworks.CSteamID steamLobbyId;
	//^This shouldnt be called once only on the host, needing to have UI open while hosting.
	//This should be a callback when this object becomes active imo, and callback SteamMultiplayerMenu or sth.

	void Awake()
	{
		globalInstance = this;
	}

	// Use this for initialization
	void Start ()
	{
		networkUI = GetComponent<TextMeshProUGUI>();
		pinger = FindObjectOfType<NetworkPinger>();
		synchronizer = FindObjectOfType<NetworkDamageShare>();
	}
	
	// Update is called once per frame
	void Update ()//Why not LateUpdate()?
	{
		//If not online, gtfo
		//Tbh, you could just create this on connection so as to not bloat, but whatever.
		if (NetworkCommunicationController.globalInstance == null)
			return;

		networkUI.text = "Ping: " + pinger.ping + "\nSynced: " + synchronizer.IsSynchronized() + "\nBandwidth In: " + NetworkCommunicationController.globalInstance.networkObject.Networker.BandwidthIn + "\nBandwidth Out: " + NetworkCommunicationController.globalInstance.networkObject.Networker.BandwidthOut;

		//If Steam
		if (SteamManager.Initialized)
			networkUI.text = networkUI.text + "\n" + "LobbyID: " + steamLobbyId;
	}
}
