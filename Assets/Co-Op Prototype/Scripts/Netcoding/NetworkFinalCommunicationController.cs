using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class NetworkFinalCommunicationController : NetworkFinalCommunicationControllerBehavior
{
	public static NetworkFinalCommunicationController globalInstance;

	public Action<RpcArgs> ReceiveRawInputEvent;//Sent by client onto host
	public Action<RpcArgs> ReceiveFinalInputEvent;//Confirmed by the host and filled with his own and sent back



	//Notifies everyone and everything, that it is setup on the network, ready to work!
	protected override void NetworkStart()
	{
		base.NetworkStart();

		MainThreadManager.Run(() => Debug.Log("Final Network Started!"));

		globalInstance = this;

		//Set this gameobject to become a child of NetworkManager GameObject, containing MultiplayerMenu.cs
		transform.SetParent(GameObject.FindObjectOfType<MultiplayerMenu>().transform);

		FindObjectOfType<NetworkFinalInputSnapshotManager>().Activate();

		/*interpolationController = GameObject.FindObjectOfType<NetworkPositionInterpolationController>();
		inputSnapshotManager = GameObject.FindObjectOfType<NetworkInputSnapshotManager>();
		synchronizer = GameObject.FindObjectOfType<NetworkSynchronizer>();
		pinger = GameObject.FindObjectOfType<NetworkPinger>();
		damageSharer = GameObject.FindObjectOfType<NetworkDamageShare>();
		spellcasting = FindObjectOfType<NetworkSpellcasting>();
		menuToggle = FindObjectOfType<NetworkToggleMenu>();*/

		//ActivateNetworkBehaviour();

		//Smooth disconnection when host/client connection is lost, for whatever reason.
		NetworkManager.Instance.Networker.disconnected += Disconnect;
		NetworkManager.Instance.Networker.playerDisconnected += Disconnect;

		//Latency simulation!
		//networkObject.Networker.LatencySimulation = 5000;
		//NetworkManager.Instance.Networker.LatencySimulation = 5000;
	}

	/// <summary>
	/// Runs on host when you close host
	/// Runs on client when you close host
	/// Runs on client when you close client
	/// </summary>
	/// <param name="sender"></param>
	public void Disconnect(NetWorker sender)
	{
		if (NetworkBootWrapper.globalInstance.hasDisconnected == false)
		{
			MainThreadManager.Run(() =>
			{
				//DeActivateNetworkBehaviour();

				Destroy(FindObjectOfType<MultiplayerMenu>());

				//Create the next (inactive) MultiplayerMenu, so you can re-host/re-connect ;)
				NetworkBootWrapper.globalInstance.hasDisconnected = true;
				NetworkBootWrapper.globalInstance.CreateAndStartMultiplayerMenu();

				networkObject.Destroy();//Also calls Destroy(this);
			});
		}

	}

	//Runs on host when you close host
	//Runs on host when you close client
	public void Disconnect(NetworkingPlayer player, NetWorker sender)
	{
		if (NetworkBootWrapper.globalInstance.hasDisconnected == false)
		{
			MainThreadManager.Run(() =>
			{
				//DeActivateNetworkBehaviour();

				Destroy(FindObjectOfType<MultiplayerMenu>());
				//FindObjectOfType<MultiplayerMenu>().DisconnectServer();

				//Create the next (inactive) MultiplayerMenu, so you can re-host/re-connect ;)
				NetworkBootWrapper.globalInstance.hasDisconnected = true;
				NetworkBootWrapper.globalInstance.CreateAndStartMultiplayerMenu();

				networkObject.Destroy();//Also calls Destroy(this);
			});
		}
	}

	public void SendRawInput(float movementX, float movementY, short attackDirection)
	{
		networkObject.SendRpc(RPC_RECEIVE_RAW_INPUT, Receivers.Others, movementX, movementY, attackDirection);
	}

	public void SendFinalInput(FinalCharacterFullInput snapshotToSend)
	{
		networkObject.SendRpc(RPC_RECEIVE_FINAL_INPUT, Receivers.Others, snapshotToSend.ObjectToByteArray());
	}

	public override void ReceiveRawInput(RpcArgs args)
	{
		if (ReceiveRawInputEvent != null)
			ReceiveRawInputEvent(args);
	}

	public override void ReceiveFinalInput(RpcArgs args)
	{
		if (ReceiveFinalInputEvent != null)
			ReceiveFinalInputEvent(args);
	}



	public bool IsServer()
	{
		return networkObject.IsServer;
	}


}
