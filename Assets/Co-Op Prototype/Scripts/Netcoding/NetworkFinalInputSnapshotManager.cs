using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;

//Ok this is actually different from how warrior/mage works, kinda, since there are 2 input classes instead of one!
//
//The host/warrior, has finalOpponent1 and finalOpponent2
//	finalOpponent1 should by this class, do nothing.
//	finalOpponent2 should by this class, disable local input and use only the one which came (rawInput)
//The client/mage, has finalOpponent1 and finalOpponent2
//	finalOpponent1 should by this class, disable local input and use only the one which came (fullInput)
//	finalOpponent2 should by this class, disable local input and use only the one which came (fullInput)
//
//First of all, disable FinalCharacterInput's Update(), and run GetLeftRightJoystick from HERE, referencing FinalCharacterInput!
//Next, do the design based on the above. Probably, this class, and the other 2, are to be overhead, not for each finalOpponent.
//So, it ends up working like warrior's ;)

//This class is for lockstep btw.
public class NetworkFinalInputSnapshotManager : MonoBehaviour
{
	public static NetworkFinalInputSnapshotManager globalInstance;

	public float inputDelay = 0.04f;
	public Queue<FinalCharacterFullInput> clientInputSnapshotsQueue = new Queue<FinalCharacterFullInput>();
	public bool snapshotListIsEmpty = true;
	public FinalCharacterFullInput latestInputSnapshot;

	public float totalTimePassedSinceNetworkStarted = 0;

	public bool isInputDifferentThanLastInput = false;
	//public bool wasLastFrameAttack = false;

	public float receivedMageMovementX;
	public float receivedMageMovementY;
	public short receivedMageAttack;

	public float lastSentWarriorMovementX;
	public float lastSentWarriorMovementY;
	public short lastSentWarriorAttack;

	public float lastSentMageMovementX;
	public float lastSentMageMovementY;
	public short lastSentMageAttack;

	// Use this for initialization
	public void Activate ()
	{
		globalInstance = this;

		if (NetworkFinalCommunicationController.globalInstance.networkObject.IsServer)
			NetworkFinalCommunicationController.globalInstance.ReceiveRawInputEvent += UpdateHostInput;
		else
			NetworkFinalCommunicationController.globalInstance.ReceiveFinalInputEvent += UpdateFullInput;

	}
	
	public void UpdateHostInput(RpcArgs args)
	{
		receivedMageMovementX = args.GetNext<float>();
		receivedMageMovementY = args.GetNext<float>();
		receivedMageAttack = args.GetNext<short>();
	}

	public void UpdateFullInput(RpcArgs args)
	{
		// This line below should be in pretty much every
		// RPC that takes custom-data types.
		byte[] receivedBytes = args.GetNext<byte[]>();

		clientInputSnapshotsQueue.Enqueue(receivedBytes.ByteArrayToObject<FinalCharacterFullInput>());
	}

	public void ProcessFinalInput(ref float warriorMovementX, ref float warriorMovementY, ref short warriorAttackDirection, ref float mageMovementX, ref float mageMovementY, ref short mageAttackDirection)
	{
		//Since this runs every Update(), it's good to keep this timer here
		totalTimePassedSinceNetworkStarted = totalTimePassedSinceNetworkStarted + Time.unscaledDeltaTime;

		//If warrior
		if (NetworkFinalCommunicationController.globalInstance.IsServer())
		{
			//Warrior input is to be not touched at all
			//BUT, mage input is to be replaced by the one which came (and perma-disabled btw)
			mageMovementX = receivedMageMovementX;
			mageMovementY = receivedMageMovementY;
			mageAttackDirection = receivedMageAttack;

			//Reset the mage attack so it won't do it again with bad luck timing -_-
			//receivedMageAttack = 0;


			//And then send the full input.
				//Here, we check if the input is different than the last one we sent, and if different, we send it!
				//Ofc, we cache the current input, so next time, we know the last one ;)
				isInputDifferentThanLastInput = false;

				if (lastSentWarriorMovementX != warriorMovementX)
				{
					lastSentWarriorMovementX = warriorMovementX;

					isInputDifferentThanLastInput = true;
				}

				if (lastSentWarriorMovementY != warriorMovementY)
				{
					lastSentWarriorMovementY = warriorMovementY;

					isInputDifferentThanLastInput = true;
				}

				if (lastSentWarriorAttack != warriorAttackDirection)
				{
					lastSentWarriorAttack = warriorAttackDirection;

					isInputDifferentThanLastInput = true;
				}

				//Mage's inputs must be checked as well!
				if (lastSentMageMovementX != mageMovementX)
				{
					lastSentMageMovementX = mageMovementX;

					isInputDifferentThanLastInput = true;
				}

				if (lastSentMageMovementY != mageMovementY)
				{
					lastSentMageMovementY = mageMovementY;

					isInputDifferentThanLastInput = true;
				}

				if (lastSentMageAttack != mageAttackDirection)
				{
					lastSentMageAttack = mageAttackDirection;

					isInputDifferentThanLastInput = true;
				}

				//And if the input is different, create a snapshot, and send it!
				if (isInputDifferentThanLastInput)
					NetworkFinalCommunicationController.globalInstance.SendFinalInput(new FinalCharacterFullInput
					{ 	hostMovementX = warriorMovementX,
						hostMovementY = warriorMovementY,
						hostAttackDirection = warriorAttackDirection,
						clientMovementX = mageMovementX,
						clientMovementY = mageMovementY,
						clientAttackDirection = mageAttackDirection,
						timestamp = totalTimePassedSinceNetworkStarted
					});
		}
		else//If Mage
		{
			//First of all, send the input to warrior
				//Here, we check if the input is different than the last one we sent, and if different, we send it!
				//Ofc, we cache the current input, so next time, we know the last one ;)
				isInputDifferentThanLastInput = false;

				if (lastSentMageMovementX != mageMovementX)
				{
					lastSentMageMovementX = mageMovementX;

					isInputDifferentThanLastInput = true;
				}

				if (lastSentMageMovementY != mageMovementY)
				{
					lastSentMageMovementY = mageMovementY;

					isInputDifferentThanLastInput = true;
				}

				if (lastSentMageAttack != mageAttackDirection)
				{
					lastSentMageAttack = mageAttackDirection;

					isInputDifferentThanLastInput = true;
				}

				if (isInputDifferentThanLastInput)
					NetworkFinalCommunicationController.globalInstance.SendRawInput(mageMovementX, mageMovementY, mageAttackDirection);

			//Then process input based on the one(s) sent!
			//If some input snapshot exists, update our input with it!
				if (clientInputSnapshotsQueue.Count != 0)
				{
					//If first input, get the time there!
					if (clientInputSnapshotsQueue.Count == 1 && snapshotListIsEmpty == true)
					{
						snapshotListIsEmpty = false;

						totalTimePassedSinceNetworkStarted = clientInputSnapshotsQueue.Peek().timestamp;
					}

					if (totalTimePassedSinceNetworkStarted >= clientInputSnapshotsQueue.Peek().timestamp + inputDelay)
					{
						//Cache the input to execute, and by dequeuing, the list is pushed one to the left!
						latestInputSnapshot = clientInputSnapshotsQueue.Dequeue();

						if (clientInputSnapshotsQueue.Count == 0 && snapshotListIsEmpty == false)
							snapshotListIsEmpty = true;
					}

				}

			//Update the input based on the host's inputs
				warriorMovementX = latestInputSnapshot.hostMovementX;
				warriorMovementY = latestInputSnapshot.hostMovementY;
				warriorAttackDirection = latestInputSnapshot.hostAttackDirection;

				mageMovementX = latestInputSnapshot.clientMovementX;
				mageMovementY = latestInputSnapshot.clientMovementY;
				mageAttackDirection = latestInputSnapshot.clientAttackDirection;

			//Reset attack directions
				//latestInputSnapshot.hostAttackDirection = 0;
				//latestInputSnapshot.clientAttackDirection = 0;
		}
	}


}
