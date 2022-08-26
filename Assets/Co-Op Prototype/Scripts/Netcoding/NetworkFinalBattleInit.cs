using UnityEngine;

/*Here is how the entire final battle is to work with netcoding (le documentation-driven programming has arrived!)

	NetworkFinalBattleInit, listens for level load event, and waits to trigger on final level
	It also checks if vanilla ending aka a duel happens ;)

	OnTrigger -> NetworkCommunicationController.DeActivate all subscriptions ((dont destroy anything cuz bugs and that's bloat))
			  -> CreatePrefabInstance (NetworkCommunicationFinalBattleController) ((See multiplayermenu))
			  -> ^Based on the booleans here, it creates the appropriate controller, as a component (add component)
			        ^The default is equal (position interpolation but no host)
			        ^Poslerp is pure position interpolation + dmg by host
			        ^Lockstep is host decides input, pretty much.
			        If more than 1 of the above booleans is detected, warning/error plz.
*/

public class NetworkFinalBattleInit : MonoBehaviour
{
	[Header("What netcoding design do you wish?")]
	//Default
	public bool equalPositionInterpolationController = true;

	//Everything runs by host (position + dmg)
	public bool hostPositionInterpolationController = false;

	//Everything runs by host, but netcoding data is purely input
	public bool hostLockstepController = false;

	//Fired by FinalBattleManager (exists only on final level) and it means no alternative endings exist
	public void Activate()
	{
		//Stop pretty much all communication
		NetworkCommunicationController.globalInstance.DeActivateNetworkBehaviour();

		//Makes its own controller, so I won't bloat NetworkCommunicationController
		if (NetworkCommunicationController.globalInstance.IsServer() == false)//This if is needed so only 1 controller spawns instead of 2 (if mage or host doesnt matter)
			BeardedManStudios.Forge.Networking.Unity.NetworkManager.Instance.InstantiateNetworkFinalCommunicationController();

		//Create the netcoding design based on booleans. Put the appropriate controller next to FinalCharacterBehaviour ;)
		if (equalPositionInterpolationController)
		{

		}
		else if (hostPositionInterpolationController)
		{

		}
		else if (hostLockstepController)
			FindObjectOfType<FinalBattleManager>().gameObject.AddComponent<NetworkFinalInputSnapshotManager>();
		else
			Debug.LogError("No netcoding design controller is selected. Please fix.");
	}

}
