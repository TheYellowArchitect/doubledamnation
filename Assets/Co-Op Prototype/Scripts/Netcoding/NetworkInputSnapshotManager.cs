using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using NaughtyAttributes;

public class NetworkInputSnapshotManager : MonoBehaviour
{
	public static NetworkInputSnapshotManager globalInstance;

	[ReadOnly]
	public bool activated = false;

	public bool rejectMageLocalVelocity = true;

	[Header("Mage Snapshot Variables")]
	//100 ms, for now at least.
	public float inputDelay = 0.04f;
	public Queue<InputTrackingData> warriorInputSnapshotsQueue = new Queue<InputTrackingData>();
	public bool snapshotListIsEmpty = true;
	public InputTrackingData receivedInputTrackingData;
	public InputTrackingData latestInputSnapshot;
	public bool wasLastFrameDodgeroll = false;

	[Header("Warrior Send Snapshot Variables")]
	public bool isWarriorInputDifferentThanLastInput = false;
	public Vector2 lastSentLeftJoystick = Vector2.zero;
	public Vector2 lastSentRightJoystick = Vector2.zero;
	public bool lastSentMageJump = false;
	public bool lastSentDodgeroll = false;
	public bool lastSentDodgerollRight = false;
	public string lastSentSpellstring = "";

	[Header("Warrior Get Mage Inputs Variables")]
	public bool receivedJump = false;
	//public bool cachedJump = false;
	public bool receivedDodgeroll = false;
	public bool receivedDodgerollRight = false;
	public string receivedSpellword;
	public string addedSpellword;//cant find better name for this smh.

	public bool mageLastSentJump = false;

	public bool startCountingNetworkTimer = false;
	public float totalTimePassedSinceNetworkStarted = 0;






	private WarriorMovement warriorBehaviour;
	private MageBehaviour mageBehaviour;
	private WordManager wordManager;//what a hack.

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;
	}

	/// <summary>
	/// Called by NetworkCommunicationController, when network has started, aka connection is made and all!
	/// </summary>
	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		NetworkCommunicationController.globalInstance.ReceiveMageJumpEvent += UpdateLatestMageJump;
		NetworkCommunicationController.globalInstance.ReceiveMageDodgerollEvent += UpdateLatestMageDodgeroll;
		NetworkCommunicationController.globalInstance.ReceiveMageSpellwordEvent += UpdateLatestMageSpellword;

		NetworkCommunicationController.globalInstance.ReceiveWarriorInputSnapshotEvent += UpdateLatestWarriorInputSnapshot;

		warriorBehaviour = GameObject.FindObjectOfType<WarriorMovement>();
		mageBehaviour = GameObject.FindObjectOfType<MageBehaviour>();

		//bloat below
		wordManager = GameObject.FindObjectOfType<WordManager>();
	}

	public void DeActivate()
	{
		activated = false;

		NetworkCommunicationController.globalInstance.ReceiveMageJumpEvent -= UpdateLatestMageJump;
		NetworkCommunicationController.globalInstance.ReceiveMageDodgerollEvent -= UpdateLatestMageDodgeroll;
		NetworkCommunicationController.globalInstance.ReceiveMageSpellwordEvent -= UpdateLatestMageSpellword;

		NetworkCommunicationController.globalInstance.ReceiveWarriorInputSnapshotEvent -= UpdateLatestWarriorInputSnapshot;
	}

	public void ProcessMasterInput(InputTrackingData currentInputSnapshot)
	{
		//Since this runs every Update(), it's good to keep this timer here
		totalTimePassedSinceNetworkStarted = totalTimePassedSinceNetworkStarted + Time.unscaledDeltaTime;

		//If warrior
		if (NetworkCommunicationController.globalInstance.IsServer())
		{
			WarriorUpdateInputsBasedOnMage(ref currentInputSnapshot);

			WarriorSendInputs(currentInputSnapshot);

			warriorBehaviour.Movement(currentInputSnapshot);

			mageBehaviour.DetermineSpell(currentInputSnapshot.mageInputData.finishedSpellwordString);
		}

		//If mage
		else
		{
			//return;//UNCOMMENT IF YOU WANT TO CHECK HOW PURE POSITION INTERPOLATION GOES

			MageUpdateInputsBasedOnWarriorSnapshots();

			MageSendInputs(currentInputSnapshot.mageInputData);

			warriorBehaviour.Movement(latestInputSnapshot);

			mageBehaviour.DetermineSpell(currentInputSnapshot.mageInputData.finishedSpellwordString);//Yes. This runs locally.
		}

	}

	public void WarriorUpdateInputsBasedOnMage(ref InputTrackingData currentInputSnapshot)
	{
		//Reset mage's snapshot, so local inputs won't work!!
		//Reset, so our local input won't override the other online player's!
		currentInputSnapshot.mageInputData.aboutToJump = false;
		currentInputSnapshot.mageInputData.aboutToDodgeroll = false;
		currentInputSnapshot.mageInputData.dodgerollRight = false;
		currentInputSnapshot.mageInputData.finishedSpellwordString = "";

		//Update his inputs based on mage's
		currentInputSnapshot.mageInputData.aboutToJump = receivedJump;
		currentInputSnapshot.mageInputData.aboutToDodgeroll = receivedDodgeroll;
		currentInputSnapshot.mageInputData.dodgerollRight = receivedDodgerollRight;
		currentInputSnapshot.mageInputData.finishedSpellwordString = receivedSpellword;

		if (receivedSpellword != "")
		{
			wordManager.ClearActiveWord();

			//addedSpellword = addedSpellword + receivedSpellword;
			addedSpellword = receivedSpellword;

			for (int i = 0; i < addedSpellword.Length; i++)
				wordManager.TypeLetter(addedSpellword[i]);

			//Check if the spell finished
			foreach (Word pickedWord in wordManager.spells)
				if (pickedWord.thisWord.CompareTo(addedSpellword) == 0)//If the same strings
				{
					Debug.Log("Spellword repeating stopped");
					addedSpellword = "";
					//lastSentSpellstring = "";
				}

		}




		//TODO: PlayerStatsManager.globalInstance.NetworkUpdateLatestWarriorLeftJoystick(cachedLeftJoystick);
		//TODO: But for the entire snapshot I guess. Mostly for mage's inputs. And for mage, for the entire snapshot.

		//Reset Mage's flags
		receivedDodgeroll = false;
		receivedDodgerollRight = false;
		receivedSpellword = "";
	}

	public void MageUpdateInputsBasedOnWarriorSnapshots()
	{
		//If some input snapshot exists, update our input with it!
		if (warriorInputSnapshotsQueue.Count != 0)
		{
			//If first input, get the time there!
			if (warriorInputSnapshotsQueue.Count == 1 && snapshotListIsEmpty == true)
			{
				snapshotListIsEmpty = false;

				totalTimePassedSinceNetworkStarted = warriorInputSnapshotsQueue.Peek().deltaTimeFromLastRegister;
			}

			if (totalTimePassedSinceNetworkStarted >= warriorInputSnapshotsQueue.Peek().deltaTimeFromLastRegister + inputDelay)
			{
				//Cache the input to execute, and by dequeuing, the list is pushed one to the left!
				latestInputSnapshot = warriorInputSnapshotsQueue.Dequeue();

				if (warriorInputSnapshotsQueue.Count == 0 && snapshotListIsEmpty == false)
					snapshotListIsEmpty = true;
			}

			//Toggle Flag -> Solves the double-dash bug
				if (wasLastFrameDodgeroll)
				{
					latestInputSnapshot.mageInputData.aboutToDodgeroll = false;

					wasLastFrameDodgeroll = false;
				}

				if (wasLastFrameDodgeroll == false && latestInputSnapshot.mageInputData.aboutToDodgeroll == true)
					wasLastFrameDodgeroll = true;
		}
	}

	[Button("Print Snapshots")]
	public void PrintInputSnapshots()
	{
		Queue<InputTrackingData> debugInputSnapshotsQueue = new Queue<InputTrackingData>(warriorInputSnapshotsQueue);

		Debug.Log("TotalTimePassedSinceNetworkStarted is: " + totalTimePassedSinceNetworkStarted + "\nSnapshot Count: " + debugInputSnapshotsQueue.Count);

		while (debugInputSnapshotsQueue.Count > 0)
		{
			//Cache the input to execute, and by dequeuing, the list is pushed one to the left!
			latestInputSnapshot = debugInputSnapshotsQueue.Dequeue();

			Debug.Log("Time: " + latestInputSnapshot.deltaTimeFromLastRegister + " JoystickX: " + latestInputSnapshot.warriorInputData.movementInputDirection.x + " JoystickY: " + latestInputSnapshot.warriorInputData.movementInputDirection.y);
		}

	}

	public void WarriorSendInputs(InputTrackingData currentInputSnapshot)
	{
		//Here, we check if the input is different than the last one we sent, and if different, we send it!
		//Ofc, we cache the current input, so next time, we know the last one ;)
		isWarriorInputDifferentThanLastInput = false;

		if (lastSentLeftJoystick != currentInputSnapshot.warriorInputData.movementInputDirection)
		{
			lastSentLeftJoystick = currentInputSnapshot.warriorInputData.movementInputDirection;

			isWarriorInputDifferentThanLastInput = true;
		}

		if (lastSentRightJoystick != currentInputSnapshot.warriorInputData.combatInputDirection)
		{
			lastSentRightJoystick = currentInputSnapshot.warriorInputData.combatInputDirection;

			isWarriorInputDifferentThanLastInput = true;
		}

		if (lastSentMageJump != currentInputSnapshot.mageInputData.aboutToJump)
		{
			lastSentMageJump = currentInputSnapshot.mageInputData.aboutToJump;

			isWarriorInputDifferentThanLastInput = true;
		}

		if (lastSentDodgeroll != currentInputSnapshot.mageInputData.aboutToDodgeroll)
		{
			lastSentDodgeroll = currentInputSnapshot.mageInputData.aboutToDodgeroll;
			lastSentDodgerollRight = currentInputSnapshot.mageInputData.dodgerollRight;

			isWarriorInputDifferentThanLastInput = true;
		}

		if (lastSentSpellstring != currentInputSnapshot.mageInputData.finishedSpellwordString)
		{
			lastSentSpellstring = currentInputSnapshot.mageInputData.finishedSpellwordString;

			isWarriorInputDifferentThanLastInput = true;
		}

		//And if the input is different, timestamp the snapshot, and send it!
		if (isWarriorInputDifferentThanLastInput)
		{
			//Timestamp it
			currentInputSnapshot.deltaTimeFromLastRegister = totalTimePassedSinceNetworkStarted;

			//Send it!
			NetworkCommunicationController.globalInstance.SendWarriorInputSnapshot(currentInputSnapshot);
		}

	}

	public void MageSendInputs(MageInputData currentInputSnapshotMageInput)
	{
		//If jump is different -> Send it to warrior
		if (mageLastSentJump != currentInputSnapshotMageInput.aboutToJump)
		{
			mageLastSentJump = currentInputSnapshotMageInput.aboutToJump;

			Debug.Log("Sent mage jump!");

			NetworkCommunicationController.globalInstance.SendMageJump(mageLastSentJump);
		}

		//If dodgerollKeyIsDown -> Send it to warrior, along with the direction
		if (currentInputSnapshotMageInput.aboutToDodgeroll == true)
			NetworkCommunicationController.globalInstance.SendMageDodgeroll(currentInputSnapshotMageInput.dodgerollRight);

		//If spell string is not null -> Send it to warrior
		if (currentInputSnapshotMageInput.finishedSpellwordString != "" && currentInputSnapshotMageInput.finishedSpellwordString != null)
			NetworkCommunicationController.globalInstance.SendMageSpellword(currentInputSnapshotMageInput.finishedSpellwordString);

	}

	/// <summary>
	/// Only mage gets this!
	/// </summary>
	/// <param name="args"></param>
	public void UpdateLatestWarriorInputSnapshot(RpcArgs args)
	{
		// This line below should be in pretty much every
		// RPC that takes custom-data types.
		byte[] receivedBytes = args.GetNext<byte[]>();

		// We convert receivedBytes to the original object of data-type <InputTrackingData>
		receivedInputTrackingData = receivedBytes.ByteArrayToObject<InputTrackingData>();

		//Reminder. The timestamp of the warrior's time is on deltaTimeRegistered field.

		warriorInputSnapshotsQueue.Enqueue(receivedInputTrackingData);
	}

	public void UpdateLatestMageJump(RpcArgs args)
	{
		receivedJump = args.GetNext<bool>();
	}

	public void UpdateLatestMageDodgeroll(RpcArgs args)
	{
		receivedDodgeroll = true;
		receivedDodgerollRight = args.GetNext<bool>();
	}

	public void UpdateLatestMageSpellword(RpcArgs args)
	{
		receivedSpellword = args.GetNext<string>();
	}

	[Button("Clear Snapshots")]
	public void ClearSnapshots()
	{
		warriorInputSnapshotsQueue.Clear();
		snapshotListIsEmpty = true;
		//totalTimePassedSinceNetworkStarted = 0;
	}

	/*Documentation of what I want to achieve:
		Warrior remembers/caches the last sent input, so as to confirm he won't send duplicate (the mage does NOT check if duplicate via reception)

		warrior has a variable: timePassedSinceNetworkStartedWarrior; -> which he does time.deltaTime; to increase
		mage has a variable: timePassedSinceNetworkStartedMage; -> which he does time.deltaTime; to increase

		Mage's inputs pass to warrior like the old system.
		Warrior's inputs are sent instantly, no snapshot timer or w/e. -> Here is where the check of difference happens btw

	*/
}
