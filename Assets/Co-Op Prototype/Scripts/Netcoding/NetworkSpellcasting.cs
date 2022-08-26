using BeardedManStudios.Forge.Networking;
using UnityEngine;

public class NetworkSpellcasting : MonoBehaviour
{
	public bool activated = false;

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		AddSpellWords.globalInstance.AddLinkFracture();

		if (NetworkCommunicationController.globalInstance.IsServer() == false)
		{
			NetworkCommunicationController.globalInstance.ReceivePillarPositionEvent += UpdatePillarPosition;
			if (FindObjectOfType<SpellLink>() != null)
				NetworkCommunicationController.globalInstance.ReceiveDesyncedSphereLinkEvent += FindObjectOfType<SpellLink>().TriggerByWarrior;
		}

	}

	public void Deactivate()
	{
		activated = false;
		
		AddSpellWords.globalInstance.RemoveLinkFracture();

		if (NetworkCommunicationController.globalInstance.IsServer() == false)
		{
			NetworkCommunicationController.globalInstance.ReceivePillarPositionEvent -= UpdatePillarPosition;
			if (FindObjectOfType<SpellLink>() != null)//Added for the error "value does not fall within the expected range" which I dont understand
				NetworkCommunicationController.globalInstance.ReceiveDesyncedSphereLinkEvent -= FindObjectOfType<SpellLink>().TriggerByWarrior;
		}

	}

	public void UpdatePillarPosition(RpcArgs args)
	{
		Vector2 warriorPillarPosition = args.GetNext<Vector2>();

		//Could use the gameobject pillarStorage but w/e
		FinalPillarBehaviour[] allPillars = FindObjectsOfType<FinalPillarBehaviour>();
		for (int i = 0; i < allPillars.Length; i++)
		{
			if (allPillars[i].pillarArrived == false)//This is the only un-updated one!
			{
				allPillars[i].playerPosition = warriorPillarPosition;
				allPillars[i].pillarArrived = true;//Now it can proceed to properly spawn it
				Debug.Log("Found it!" + warriorPillarPosition);
			}
		}
	}

}
