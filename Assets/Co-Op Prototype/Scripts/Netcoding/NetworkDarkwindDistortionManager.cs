using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class NetworkDarkwindDistortionManager : MonoBehaviour 
{
	public static NetworkDarkwindDistortionManager globalInstance;

	public DarkwindMenu darkwindMenu;

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;
	}
	
	public void Activate()
	{
		NetworkCommunicationController.globalInstance.ReceiveDarkwindDistortionChangeEvent += ReceivedButtonClick;

		darkwindMenu.Reset();
	}

	public void DeActivate()
	{
		NetworkCommunicationController.globalInstance.ReceiveDarkwindDistortionChangeEvent -= ReceivedButtonClick;
	}

	public void ReceivedButtonClick(RpcArgs args)
	{
		darkwindMenu.DetermineButtonClicked(args.GetNext<byte>());
	}
}
