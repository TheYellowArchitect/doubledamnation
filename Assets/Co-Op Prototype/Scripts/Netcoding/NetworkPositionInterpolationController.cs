using System.Collections.Generic;
using System.Collections;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using NaughtyAttributes;

public class NetworkPositionInterpolationController : MonoBehaviour
{
	public static NetworkPositionInterpolationController globalInstance;

	[ReadOnly]
	public bool activated = false;

	public WarriorMovement warriorBehaviour;

	[Header("Position Snapshot")]

	[Tooltip("0.02 seconds is 50 milliseconds")]
	public double timePerSnapshot = 0.020d;

	[Tooltip("[0] is current, [1] is the one we interpolate towards, [2] and [3] are the 2 next ones")]
	public List<PositionSnapshot> snapshots = new List<PositionSnapshot>();

	private double timePassedSinceLastPosition = 0d;
	private double timePassedSinceLastPositionTotal = 0d;
	public Vector2 lerpResultPosition;
	public double lerpRate;

	//So you won't spam with "Idle" snapshots, but only send new positions!
	public Vector2 previouslySentSnapshot = Vector2.zero;
	public Vector2 currentWarriorSnapshot;

	public bool hasActivatedOnce = false;

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

		/*
		if (hasActivatedOnce == false && NetworkCommunicationController.globalInstance.IsServer() == false)
		{
			hasActivatedOnce = true;
		}*/

		NetworkCommunicationController.globalInstance.ReceiveWarriorPositionEvent += UpdateLatestWarriorPosition;

		if (warriorBehaviour != null)
			warriorBehaviour.isGrounded += TouchedGround;
	}

	public void DeActivate()
	{
		activated = false;

		NetworkCommunicationController.globalInstance.ReceiveWarriorPositionEvent -= UpdateLatestWarriorPosition;

		if (warriorBehaviour != null)
			warriorBehaviour.isGrounded -= TouchedGround;

		ClearSnapshots();
	}

	[Button("Clear Snapshots")]
	public void ClearSnapshots()
	{
		timePassedSinceLastPosition = 0d;
		timePassedSinceLastPositionTotal = 0d;
		snapshots.Clear();
	}

	public void ClearSnapshots(float timeToTrigger)
	{
		StartCoroutine(ClearSnapshotsCoroutine(timeToTrigger));
	}

	//Invoke uses string, aka rip performance
	//Though, could have a static abstract function which gets float and function as parameter. Ah well.
	public IEnumerator ClearSnapshotsCoroutine(float timeToTrigger)
	{
		yield return new WaitForSeconds(timeToTrigger);
		ClearSnapshots();
	}

	public void UpdateLatestWarriorPosition(RpcArgs args)
	{
		double receivedSnapshotTime = args.GetNext<double>();
		Vector2 receivedWarriorPosition = args.GetNext<Vector2>();

		snapshots.Add(new PositionSnapshot { timestamp = receivedSnapshotTime, warriorPosition = receivedWarriorPosition});

		//If 2nd snapshot, put the timestamp to start properly!
		if (snapshots.Count == 2)
			timePassedSinceLastPosition = snapshots[0].timestamp;


	}

	//MasterInputManager enters the input and expects us to do the rest.
	//Called each frame/Update()!
	public void EnterMasterInput(WarriorInputData tempWarriorInputData, MageInputData tempMageInputData)
	{
		//========================================
		//===If warrior -> Send Position       ===
		//===If mage    -> Interpolate Position===
		//========================================

		//Debug.Log("Time Passed " + timePassedSinceLastPosition + "   " + timePassedSinceLastPositionTotal);

		//If warrior -> Send position
		if (NetworkCommunicationController.globalInstance.IsServer())
		{
			//Debug.Log("Time.unscaledDeltaTime: " + Time.unscaledDeltaTime);
			currentWarriorSnapshot = warriorBehaviour.transform.position.ToVector2();

			//Update the timestamps below, and send by timer.
			timePassedSinceLastPosition = timePassedSinceLastPosition + Time.unscaledDeltaTime;
			timePassedSinceLastPositionTotal = timePassedSinceLastPositionTotal + Time.unscaledDeltaTime;
			if ((timePassedSinceLastPosition >= timePerSnapshot || timePassedSinceLastPositionTotal == 0d)) //&& currentWarriorSnapshot != previouslySentSnapshot)
			{
				timePassedSinceLastPosition = 0;
				NetworkCommunicationController.globalInstance.SendWarriorPositionUnreliable(currentWarriorSnapshot, timePassedSinceLastPositionTotal);
				previouslySentSnapshot = currentWarriorSnapshot;
			}
		}
		else
		{
			InterpolateMageToFinalWarriorPosition();
			timePassedSinceLastPosition = timePassedSinceLastPosition + Time.unscaledDeltaTime;
		}
	}

	public void InterpolateMageToFinalWarriorPosition()
	{

		if (snapshots.Count < 2)
		{
			Debug.LogWarning("Not enough snapshots, to do any interpolation.");
			return;
		}

		while (snapshots.Count > 2 && timePassedSinceLastPosition > snapshots[1].timestamp)
		{
			snapshots.RemoveAt(0);
		}

		/*while (snapshots.Count > 2 && timePassedSinceLastPosition > snapshots[0].timestamp && snapshots[1].timestamp - snapshots[0].timestamp > timePerSnapshot + NetworkPinger.globalInstance.GetPingMilliseconds())
		{
			timePassedSinceLastPosition = snapshots[1].timestamp;
			snapshots.RemoveAt(0);
		}*/
			

		//Debug.Log("Snapshots: " + snapshots.Count);
		//Debug.Log("Final Warrior Snapshot: " + snapshots[snapshots.Count - 1].timestamp + " X " + snapshots[snapshots.Count - 1].warriorPosition.x);

		lerpRate = (timePassedSinceLastPosition - snapshots[0].timestamp) / (snapshots[1].timestamp - snapshots[0].timestamp);
		lerpResultPosition = Vector2.Lerp(snapshots[0].warriorPosition, snapshots[1].warriorPosition, (float)lerpRate);

		//Debug.Log("snapshots[0] and [1] timestamp: " + snapshots[0].timestamp + " " + snapshots[1].timestamp);

		/*if (lerpRate > 1 && snapshots.Count == 2)
			snapshots.Clear();//Useful for resetting the timestamp on the 2nd snapshot*/

		//Debug.Log("lerpRate: " + lerpRate);

		//This is for a VERY rare bug (only on high pings) where lerpResultPosition is NaN...
		if (float.IsNaN(lerpResultPosition.x))
			return;

		//Take the interpolated position and put it onto the warrior
		warriorBehaviour.transform.position = new Vector3(lerpResultPosition.x, lerpResultPosition.y, 0);
		//warriorBehaviour.GetComponent<Rigidbody2D>().position = new Vector3(lerpResultPosition.x, lerpResultPosition.y, 0);
		//warriorBehaviour.GetComponent<Rigidbody2D>().MovePosition(lerpResultPosition);

	}

	/// <summary>
	/// This exists so player won't bounce without magic circle -_-
	/// </summary>
	public void TouchedGround()
	{
		//Shouldn't this be unreliable?
		NetworkCommunicationController.globalInstance.SendWarriorPositionUnreliable(warriorBehaviour.transform.position.ToVector2(), timePassedSinceLastPositionTotal);
	}

}