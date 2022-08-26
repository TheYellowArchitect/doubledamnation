using BeardedManStudios.Forge.Networking;
using UnityEngine;

public class NetworkToggleMenu : MonoBehaviour
{
	public bool activated = false;

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		PauseMenu.globalInstance.pauseNotifyOtherPlayerEvent += NotifyGamePaused;
		PauseMenu.globalInstance.resumeNotifyOtherPlayerEvent += NotifyGameResumed;
		NetworkCommunicationController.globalInstance.ReceiveWarriorMenuToggledEvent += ReceiveMenuToggle;
	}

	public void DeActivate()
	{
		activated = false;

		PauseMenu.globalInstance.pauseNotifyOtherPlayerEvent -= NotifyGamePaused;
		PauseMenu.globalInstance.resumeNotifyOtherPlayerEvent -= NotifyGameResumed;
		NetworkCommunicationController.globalInstance.ReceiveWarriorMenuToggledEvent -= ReceiveMenuToggle;
	}

	public void NotifyGamePaused()
	{
		NetworkCommunicationController.globalInstance.SendWarriorMenuToggled(true);
	}

	public void NotifyGameResumed()
	{
		NetworkCommunicationController.globalInstance.SendWarriorMenuToggled(false);
		Debug.Log("Sent!");
	}

	public void ReceiveMenuToggle(RpcArgs args)
	{
		bool isOpen = args.GetNext<bool>();
		Debug.Log("isOpen: " + isOpen + "    GameIsPaused: " + PauseMenu.GameIsPaused);
		if (isOpen)//is paused
		{
			if (PauseMenu.GameIsPaused == false)
				PauseMenu.globalInstance.Pause(true, !NetworkCommunicationController.globalInstance.IsServer(), false);
		}
		else
		{
			if (PauseMenu.GameIsPaused == true)
				PauseMenu.globalInstance.Resume(false);
		}
	}
}
