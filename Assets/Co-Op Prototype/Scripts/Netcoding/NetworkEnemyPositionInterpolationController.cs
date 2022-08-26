using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using NaughtyAttributes;

///See NetworkPositionInterpolationController
//Same logic, but instead of just 1 (Warrior), it is for all Monsters.
//Hence, it is the same as NetworkPositionInterpolationController but individual logic now has a forloop list wrapper ;)
public class NetworkEnemyPositionInterpolationController : MonoBehaviour
{
	public static NetworkEnemyPositionInterpolationController globalInstance;

	[ReadOnly]
	public bool activated = false;

	public List<ushort> activatedEnemyListIndex = new List<ushort>();

	private NetworkEnemyAnimationOverrideController commonNetworkEnemyAnimationOverrideController;

	[Header("Position Snapshot")]

	[Tooltip("0.02 seconds is 50 milliseconds")]
	public double timePerSnapshot = 0.020d;

	//[Tooltip("[0] is current, [1] is the one we interpolate towards, [2] and [3] are the 2 next ones, and so on")]
	public Dictionary<ushort, List<PositionSnapshot>> snapshotDictionary = new Dictionary<ushort, List<PositionSnapshot>>();
	public Dictionary<ushort, Rigidbody2D> activatedRigidbodyDictionary = new Dictionary<ushort, Rigidbody2D>();

	private Dictionary<ushort, double> timePassedSinceLastPosition = new Dictionary<ushort, double>();
	private Dictionary<ushort, double> timePassedSinceLastPositionTotal = new Dictionary<ushort, double>();

	private Dictionary<ushort, float[]> duplicateSnapshotDictionary = new Dictionary<ushort, float[]>();

	public Vector2 lerpResultPosition;
	public double lerpRate;

	//Hacky flag to trigger ClientRestartEnemies() on the first time they connect
	private bool clientInitialMonstersSynced = false;

	private int latestDebugPrintCount = -1;
	private int currentDebugPrintCount;

	// Use this for initialization
	void Awake()
	{
		//Setting our singleton
		globalInstance = this;

		commonNetworkEnemyAnimationOverrideController = GetComponent<NetworkEnemyAnimationOverrideController>();
		commonNetworkEnemyAnimationOverrideController.activatedEnemyListIndex = activatedEnemyListIndex;
	}

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		NetworkCommunicationController.globalInstance.ReceiveMonsterPositionEvent += UpdateLatestMonsterPosition;
		NetworkCommunicationController.globalInstance.ReceiveClearSynchronizedMonstersEvent += ReceivedClearEnemyIndexes;

		NetworkCommunicationController.globalInstance.ReceiveAddedSynchronizedMonstersEvent += AddEnemyIndexListFromClient;

		//Activated enemies register here, not the reverse.
		//if (warriorBehaviour != null)
			//warriorBehaviour.isGrounded += TouchedGround;
	}

	public void DeActivate()
	{
		activated = false;

		NetworkCommunicationController.globalInstance.ReceiveMonsterPositionEvent -= UpdateLatestMonsterPosition;
		NetworkCommunicationController.globalInstance.ReceiveClearSynchronizedMonstersEvent -= ReceivedClearEnemyIndexes;

		NetworkCommunicationController.globalInstance.ReceiveAddedSynchronizedMonstersEvent -= AddEnemyIndexListFromClient;

		//Deactivated enemies register here, not the reverse.
		//if (warriorBehaviour != null)
			//warriorBehaviour.isGrounded -= TouchedGround;
	}

	public void UpdateLatestMonsterPosition(RpcArgs args)
	{
		double receivedSnapshotTime = args.GetNext<double>();
		Vector2 receivedMonsterPosition = args.GetNext<Vector2>();
		ushort receivedMonsterIndex = args.GetNext<ushort>();

		//If host sent monster snapshot, before the client even has registered it, register it via client
		if (snapshotDictionary.ContainsKey(receivedMonsterIndex) == false)
			AddEnemyIndex(receivedMonsterIndex, LevelManager.globalInstance.GetEnemyRigidbodyFromIndex(receivedMonsterIndex));

		snapshotDictionary[receivedMonsterIndex].Add(new PositionSnapshot { timestamp = receivedSnapshotTime, warriorPosition = receivedMonsterPosition});

		//If 2nd snapshot, put the timestamp to start properly!
		if (snapshotDictionary[receivedMonsterIndex].Count == 2)
			timePassedSinceLastPosition[receivedMonsterIndex] = snapshotDictionary[receivedMonsterIndex][0].timestamp;
	}

	public void Update()
	{
		if (activated == false)
			return;

		//If disconnected but somehow still not updated (heavy bug with packets)
		if (NetworkCommunicationController.globalInstance == null)
		{
			Debug.LogError("Weird bizzaro bug where disconnection did not happen smoothly. NetworkCommunicationController is null.");
			return;
		}

		DebugPrintSnapshotList();

		if (activatedEnemyListIndex.Count > 0 && NetworkDamageShare.globalInstance.IsSynchronized())
			ProcessMonsterPositions();
	}
	
	public void DebugPrintSnapshotList()
	{
		currentDebugPrintCount = snapshotDictionary.Count;
		if (currentDebugPrintCount != latestDebugPrintCount)
		{
			Debug.Log("Enemies Registered: " + currentDebugPrintCount);

			int i = 0;
			foreach(KeyValuePair<ushort, Rigidbody2D> entry in activatedRigidbodyDictionary)
			{
				if (entry.Value != null)
					Debug.Log("Enemy" + i + " Collider's Name: " + entry.Value.gameObject.name);
				i++;
			}
		}
		latestDebugPrintCount = currentDebugPrintCount;
	}

	//Called each frame/Update()!
	public void ProcessMonsterPositions()
	{
		//=======================================
		//===If host   -> Send Position       ===
		//===If client -> Interpolate Position===
		//=======================================

		//If host -> Send position
		if (NetworkCommunicationController.globalInstance.IsServer())
		{
			//Debug.Log("Time.unscaledDeltaTime: " + Time.unscaledDeltaTime);

			//Iterate for each monster (i is picked enemy index)
			ushort i;
			for (int j = 0; j < activatedEnemyListIndex.Count; j++)
			{
				i = activatedEnemyListIndex[j];

				if (snapshotDictionary.ContainsKey(i) == false)
				{
					Debug.LogError("Host has Key " + i + " which doesn't exist!");
					//RemoveEnemyIndex(i);
					//i--;
					continue;
				}

				//Update the timestamps below, and send by timer.
				timePassedSinceLastPosition[i] = timePassedSinceLastPosition[i] + Time.unscaledDeltaTime;
				timePassedSinceLastPositionTotal[i] = timePassedSinceLastPositionTotal[i] + Time.unscaledDeltaTime;
				if (timePassedSinceLastPosition[i] >= timePerSnapshot || timePassedSinceLastPositionTotal[i] == 0d)
				{
					timePassedSinceLastPosition[i] = 0;
					//Debug.Log(duplicateSnapshotDictionary[i]);

					//Get current snapshot, and check if its different than last one
					//The goal of this is to not send idle-movement snapshots and overflow the bandwidth
						//Get current position
						duplicateSnapshotDictionary[i][0] = activatedRigidbodyDictionary[i].transform.position.ToVector2().x;
						duplicateSnapshotDictionary[i][1] = activatedRigidbodyDictionary[i].transform.position.ToVector2().y;

						//If current position is different than previous position, send the snapshot
						if (duplicateSnapshotDictionary[i][0] != duplicateSnapshotDictionary[i][2] || duplicateSnapshotDictionary[i][1] != duplicateSnapshotDictionary[i][3])
						{
							NetworkCommunicationController.globalInstance.SendMonsterPositionUnreliable(activatedRigidbodyDictionary[i].transform.position.ToVector2(), timePassedSinceLastPositionTotal[i], i);
							//activatedRigidbodyDictionary[i].transform.position.ToVector2();
							Debug.Log("SENDING"+i);

							//Cache the "previous position"
							duplicateSnapshotDictionary[i][2] = duplicateSnapshotDictionary[i][0];
							duplicateSnapshotDictionary[i][3] = duplicateSnapshotDictionary[i][1];
						}
							

						
				}
			}

			
			
			
		}
		else//InterpolateMonsterToFinalMonsterPosition
		{
			//Iterate for each monster (i is picked enemy index)
			ushort i;
			for (int j = 0; j < activatedEnemyListIndex.Count; j++)
			{
				i = activatedEnemyListIndex[j];

				if (snapshotDictionary.ContainsKey(i) == false)
					continue;

				//Not enough snapshots to do any interpolation, just gtfo
				if (snapshotDictionary[i].Count < 2)
					continue;

				while (snapshotDictionary[i].Count > 2 && timePassedSinceLastPosition[i] > snapshotDictionary[i][1].timestamp)
				{
					snapshotDictionary[i].RemoveAt(0);
				}

				//Debug.Log("Monster's Snapshot Count: " +snapshotDictionary[i].Count);
				//Debug.Log("Monster Snapshot: " + snapshotDictionary[i][snapshotDictionary[i].Count - 1].timestamp + " X " + snapshotDictionary[i][snapshotDictionary[i].Count - 1].warriorPosition.x);

				lerpRate = (timePassedSinceLastPosition[i] - snapshotDictionary[i][0].timestamp) / (snapshotDictionary[i][1].timestamp - snapshotDictionary[i][0].timestamp);
				lerpResultPosition = Vector2.Lerp(snapshotDictionary[i][0].warriorPosition, snapshotDictionary[i][1].warriorPosition, (float)lerpRate);

				//This is for a VERY rare bug (only on high pings) where lerpResultPosition is NaN...
				if (float.IsNaN(lerpResultPosition.x))
					continue;

				//Having calculated the interpolated position, place it onto the monster (assuming it exists ofc and isnt removed)
				if (activatedRigidbodyDictionary[i] != null)
				{
					activatedRigidbodyDictionary[i].transform.position = new Vector3(lerpResultPosition.x, lerpResultPosition.y, 0);					

					timePassedSinceLastPosition[i] = timePassedSinceLastPosition[i] + Time.unscaledDeltaTime;
				}
				else
					RemoveEnemyIndex(i);//If it still errors, just comment this out and let it be detected+removed elsewhere
				
			}
			
		}
	}





	public void AddEnemyIndex(ushort monsterIndexToAdd, Rigidbody2D monsterRigidbodyToAdd)
	{
		//Debug.Log("Enemy Index ADDED " + monsterIndexToAdd);

		if (snapshotDictionary.ContainsKey(monsterIndexToAdd))
		{
			Debug.LogWarning("Already added");
			return;
		}

		activatedEnemyListIndex.Add(monsterIndexToAdd);
		activatedRigidbodyDictionary.Add(monsterIndexToAdd, monsterRigidbodyToAdd);
		snapshotDictionary.Add(monsterIndexToAdd, new List<PositionSnapshot>());

		timePassedSinceLastPosition.Add(monsterIndexToAdd, 0d);
		timePassedSinceLastPositionTotal.Add(monsterIndexToAdd, 0d);

		duplicateSnapshotDictionary.Add(monsterIndexToAdd, new float[4]{-1, -1, -1, -1});
		duplicateSnapshotDictionary[monsterIndexToAdd][2] = activatedRigidbodyDictionary[monsterIndexToAdd].transform.position.ToVector2().x;
		duplicateSnapshotDictionary[monsterIndexToAdd][3] = activatedRigidbodyDictionary[monsterIndexToAdd].transform.position.ToVector2().y;

		commonNetworkEnemyAnimationOverrideController.AddAnimator(monsterIndexToAdd);
		//Normally, should return short e.g. -1 and then remove if -1 instead of nothing/end
	}

	public void RemoveEnemyIndex(ushort monsterIndexToRemove)
	{
		if (snapshotDictionary.ContainsKey(monsterIndexToRemove))
		{
			snapshotDictionary.Remove(monsterIndexToRemove);
			activatedRigidbodyDictionary.Remove(monsterIndexToRemove);
			activatedEnemyListIndex.Remove(monsterIndexToRemove);

			timePassedSinceLastPosition.Remove(monsterIndexToRemove);
			timePassedSinceLastPositionTotal.Remove(monsterIndexToRemove);

			duplicateSnapshotDictionary.Remove(monsterIndexToRemove);

			commonNetworkEnemyAnimationOverrideController.RemoveAnimator(monsterIndexToRemove);
		}
		else
			Debug.LogWarning("Tried to remove a monster index which is already removed: " + monsterIndexToRemove);


	}

	public void ClearAllEnemyIndexes()
	{
		while (activatedEnemyListIndex.Count > 0)
			RemoveEnemyIndex(activatedEnemyListIndex[activatedEnemyListIndex.Count - 1]);
	}

	public void ReceivedClearEnemyIndexes(RpcArgs args)
	{
		ClearAllEnemyIndexes();
	}

	public void SendEntireEnemyIndexList()
	{
		if (activatedEnemyListIndex.Count < 1)
			return;//Don't send empty list across the network!

		NetworkCommunicationController.globalInstance.SendAddedSynchronizedMonsters(activatedEnemyListIndex);
	}

	public void AddEnemyIndexListFromClient(RpcArgs args)
	{
		// This line below should be in pretty much every RPC that takes custom-data types.
        byte[] receivedBytes = args.GetNext<byte[]>();

		List<ushort> tempList = receivedBytes.ByteArrayToObject<List<ushort>>();

		bool replaceList = true;

		//If lists are the same, don't empty and refill
		if (tempList.Count == activatedEnemyListIndex.Count)
		{
			replaceList = false;
			for (int i = 0; i < tempList.Count; i++)
			{
				if (tempList[i] != activatedEnemyListIndex[i])
				{
					replaceList = true;
					break;
				}
			}

			if (replaceList == false)
				return;
		}

		Debug.Log("Adding EnemyIndexList from Client, Count: " + tempList.Count);
		
		//Clear current list
		ClearAllEnemyIndexes();

		//Create a brand new list
		for (int i = 0; i < tempList.Count; i++)
			AddEnemyIndex(tempList[i], LevelManager.globalInstance.GetEnemyRigidbodyFromIndex(tempList[i]));

		if (NetworkCommunicationController.globalInstance.IsServer() == false && clientInitialMonstersSynced == false)
		{
			clientInitialMonstersSynced = true;

			ClientRestartEnemies();

			NetworkCommunicationController.globalInstance.networkObject.ClearRpcBuffer();
		}
	}

	//When desync happens, checks if coroutineRunning == false, and restarts coroutine properly
	public void ClientRestartEnemies()
	{
		EnemyBehaviour tempEnemyBehaviour;

		for (int i = 0; i < activatedEnemyListIndex.Count; i++)
		{
			tempEnemyBehaviour = LevelManager.globalInstance.GetEnemyBehaviourFromIndex(activatedEnemyListIndex[i]);
			if (tempEnemyBehaviour.IsCoroutineRunning() == false && tempEnemyBehaviour.gameObject.activeSelf)//gotta check if activeSelf in edge-case monster dies before this RPC arrives
				tempEnemyBehaviour.StartCoroutine(tempEnemyBehaviour.UpdateBehaviour());
		}
	}

	public void ResetEnemiesVelocity()
	{
		for (int i = 0; i < activatedEnemyListIndex.Count; i++)
		{
			//If active enemy behaviour
			if (LevelManager.globalInstance.GetEnemyBehaviourFromIndex(activatedEnemyListIndex[i]).enabled)
				LevelManager.globalInstance.GetEnemyRigidbodyFromIndex(activatedEnemyListIndex[i]).velocity = Vector2.zero;
		}
	}

}
