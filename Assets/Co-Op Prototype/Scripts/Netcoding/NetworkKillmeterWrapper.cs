using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using NaughtyAttributes;

public class NetworkKillmeterWrapper : MonoBehaviour 
{
	public static NetworkKillmeterWrapper globalInstance;

	[ReadOnly]
	public bool activated = false;

	public KillMeterManager killMeter;

	// Use this for initialization
	void Start()
	{
		//Setting our singleton
		globalInstance = this;
	}

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		killMeter = GameObject.FindGameObjectWithTag("GameManager").GetComponent<KillMeterManager>();

		//if (NetworkCommunicationController.globalInstance.IsServer() == false)
		NetworkCommunicationController.globalInstance.ReceiveManaKillmeterEvent += DrainAllManaWrapper;

		killMeter.drainedManaEvent += SendDrainMana;

		DetermineClientAllowDrainMana(false);
	}

	public void DeActivate()
	{
		if (activated)
			activated = false;
		else
			return;

		//if (NetworkCommunicationController.globalInstance.IsServer() == false)
		NetworkCommunicationController.globalInstance.ReceiveManaKillmeterEvent -= DrainAllManaWrapper;

		killMeter.drainedManaEvent -= SendDrainMana;
	}

	public void DrainAllManaWrapper(RpcArgs args)
	{
		Debug.Log("Should have all mana drained!");
		killMeter.DrainAllMana();
	}

	public void SendDrainMana()
	{
		if (NetworkCommunicationController.globalInstance.IsServer() && NetworkDamageShare.globalInstance.IsSynchronized())
		{
			Debug.Log("Sent to shut down!");
			NetworkCommunicationController.globalInstance.SendManaKillmeter();
		}
			
	}

	public void DetermineClientAllowDrainMana(bool allow)
	{
		if (killMeter == null)
			return;

		//If client and synchronized, the host sends the killmeter trigger.
        if (NetworkCommunicationController.globalInstance.IsServer() == false)
            killMeter.allowDrainMana = allow;
	}
}
