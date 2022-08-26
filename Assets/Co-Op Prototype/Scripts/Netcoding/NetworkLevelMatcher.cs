using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using NaughtyAttributes;

//It is destroyed once its job is done
public class NetworkLevelMatcher : MonoBehaviour 
{
	public static NetworkLevelMatcher globalInstance;

	[ReadOnly]
	public bool activated = false;

	public static short receivedCurrentLevel;
	
	public void Activate()
	{
		//Debug.Log("Activated?");

		if (activated == false)
			activated = true;
		else
			return;

		NetworkCommunicationController.globalInstance.ReceiveCurrentLevelEvent += ReceivedOtherPlayerLevel;
		globalInstance = this;
		NetworkCommunicationController.globalInstance.SendCurrentLevel();
	}

	public void DeActivate()
	{
		activated = false;

		NetworkCommunicationController.globalInstance.ReceiveCurrentLevelEvent -= ReceivedOtherPlayerLevel;
		globalInstance = null;
	}

	//Equal level means proceed to connect normally.
	//Different level means the lowest one skips current level and re-sends rightafter.
	public void ReceivedOtherPlayerLevel(RpcArgs args)
	{
		receivedCurrentLevel = args.GetNext<short>();
		Debug.Log("ReceivedLevel: " + receivedCurrentLevel);

		//If insta-invite aka have played before, insta-skip intro (see LevelManager.NetworkLevelMatcherCallback's final condition which is for here ;)
		if (receivedCurrentLevel == LevelManager.currentLevel && LevelManager.currentLevel == -1)
		{
			GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().SkipAllDialogues();//This automatically skips intro ;)
		}

		//If same level, finalize instantly, aka best case scenario (e.g. both at tutorial area)
		else if (receivedCurrentLevel == LevelManager.currentLevel)
		{
			GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().DontSkipAllDialogues(1.5f);
			NetworkCommunicationController.globalInstance.FinalizeNetworkStart();
			Debug.Log("Just Finalized Network!");
			DeActivate();
		}

		//If greater level, we must skip levels until we reach it, and then depend on levelmanager.callback
		else if (receivedCurrentLevel > LevelManager.currentLevel)
		{
			GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().SkipAllDialogues();//This automatically skips intro ;)
			if (LevelManager.currentLevel > -1)
				SceneManagerScript.globalInstance.LoadTargetLevel(receivedCurrentLevel);
			else//Intro boat (-1)
				GameObject.FindGameObjectWithTag("IntroManager").GetComponent<IntroManager>().levelToLoadAfter = receivedCurrentLevel;
		}
		//else if (receivedCurrentLevel < LevelManager.currentLevel)
			//do nothing.
	}

}
