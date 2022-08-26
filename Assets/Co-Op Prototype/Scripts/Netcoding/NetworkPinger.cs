using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class NetworkPinger : MonoBehaviour
{
	public static NetworkPinger globalInstance;

	public GameObject pingLabelUI;
	public Text pingValue;

	[ReadOnly]
	public bool activated = false;

	public uint ping = 0;
	public float timeBetweenPings = 2f;
	public float pingTimePassed = 0;

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;
	}

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		NetworkCommunicationController.globalInstance.networkObject.Networker.onPingPong += GetPing;
	}

	public void DeActivate()
	{
		activated = false;

		NetworkCommunicationController.globalInstance.networkObject.Networker.onPingPong -= GetPing;
	}

	public void Update()
	{
		//if Online and mage
		if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && activated)
		{
			pingTimePassed = pingTimePassed + Time.unscaledDeltaTime;

			if (pingTimePassed > timeBetweenPings)
			{
				pingTimePassed = 0;

				if (NetworkManager.Instance == null)
				{
					Debug.LogError("Caught it at null, it would continue below!");
					return;
				}

				NetworkCommunicationController.globalInstance.networkObject.Networker.Ping();
			}
		}
	}

	public void GetPing(double pingReceived, NetWorker player)
	{
		ping = (uint)pingReceived;

		NetworkCommunicationController.globalInstance.SetPing(pingValue, ping.ToString(), pingLabelUI);
	}

	public float GetPingMilliseconds()
	{
		return ping / 1000;
	}
}
