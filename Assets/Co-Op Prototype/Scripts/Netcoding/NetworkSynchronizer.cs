using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using NaughtyAttributes;

public class NetworkSynchronizer : MonoBehaviour
{
	public static NetworkSynchronizer globalInstance;

	[ReadOnly]
	public bool activated = false;

	private PlayerStatsManager playerStatsManager = null;
	private WarriorHealth warriorHealth;
	private WarriorMovement warriorMovement;
	private PlayerManaManager warriorMana;

	public void Start()
	{
		//Setting our singleton
		globalInstance = this;

		playerStatsManager = FindObjectOfType<PlayerStatsManager>();
		warriorHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>();
		warriorMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();
		warriorMana = GameObject.FindGameObjectWithTag("ManaManager").GetComponent<PlayerManaManager>();
	}

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		ReceiveGameState();

		//If warrior, send all his data to synchronize
		if (NetworkCommunicationController.globalInstance.IsServer())
			WarriorSendGameState();

		FindObjectOfType<DialogueManager>().startedLevelCutsceneEvent += LevelCutsceneTriggered;
		NetworkCommunicationController.globalInstance.ReceiveDialogueTriggeredEvent += ReceivedLevelCutscene;

		NetworkCommunicationController.globalInstance.ReceiveOverridePositionEvent += OverridePosition;

		if (warriorMovement != null)
			warriorMovement.startedRevive += StartedRevive;
	}

	public void DeActivate()
	{
		activated = false;

		//Unregisters events
		UnReceiveGameState();

		FindObjectOfType<DialogueManager>().startedLevelCutsceneEvent -= LevelCutsceneTriggered;
		NetworkCommunicationController.globalInstance.ReceiveDialogueTriggeredEvent -= ReceivedLevelCutscene;

		NetworkCommunicationController.globalInstance.ReceiveOverridePositionEvent -= OverridePosition;

		if (warriorMovement != null)
			warriorMovement.startedRevive -= StartedRevive;
	}

	public void OverridePosition(RpcArgs args)
	{
		//Debug.Log("IsServer() " + );
		//Debug.Log("IsSynchronized() " + NetworkDamageShare.globalInstance.IsSynchronized());

		//if (NetworkCommunicationController.globalInstance.IsServer() && NetworkDamageShare.globalInstance.IsSynchronized())
			//return;
			
		warriorMovement.transform.position = args.GetNext<Vector2>();
	}

	/// <summary>
	/// These are triggered only by mage, so warrior never has these functions running for him, since he sends with Receivers.Others
	/// </summary>
	public void ReceiveGameState()
	{
		NetworkCommunicationController.globalInstance.ReceiveRandomSeedEvent += UpdateSeed;
		NetworkCommunicationController.globalInstance.ReceiveMonstersChaseObstructedPlayerEvent += UpdateChaseObstructionMonsters;
		NetworkCommunicationController.globalInstance.ReceiveDeadMonstersEvent += UpdateDeadMonsters;

		NetworkCommunicationController.globalInstance.ReceiveCurrentHealthEvent += UpdateWarriorCurrentHealth;
		NetworkCommunicationController.globalInstance.ReceiveMaxHealthEvent += UpdateWarriorMaxHealth;
		NetworkCommunicationController.globalInstance.ReceiveCurrentManaEvent += UpdateCurrentMana;
		NetworkCommunicationController.globalInstance.ReceiveOverrideCurrentManaEvent += OverrideCurrentMana;
		NetworkCommunicationController.globalInstance.ReceiveOverrideCurrentHealthEvent += OverrideCurrentHealth;

		NetworkCommunicationController.globalInstance.ReceiveDeathCountEvent += UpdateDeathCount;
		NetworkCommunicationController.globalInstance.ReceiveCurrentKillCountEvent += UpdateCurrentKillCount;
		NetworkCommunicationController.globalInstance.ReceiveTotalKillCountEvent += UpdateTotalKillCount;

		NetworkCommunicationController.globalInstance.ReceivePowerUpCountEvent += UpdatePowerUpCount;
		NetworkCommunicationController.globalInstance.ReceiveInterruptionCountEvent += UpdateInterruptions;
		NetworkCommunicationController.globalInstance.ReceiveLevelCutsceneIndexEvent += UpdateLevelCutsceneIndex;
	}

	/// <summary>
	/// For when disconnect happens
	/// </summary>
	public void UnReceiveGameState()
	{
		NetworkCommunicationController.globalInstance.ReceiveRandomSeedEvent -= UpdateSeed;
		NetworkCommunicationController.globalInstance.ReceiveMonstersChaseObstructedPlayerEvent -= UpdateChaseObstructionMonsters;

		NetworkCommunicationController.globalInstance.ReceiveCurrentHealthEvent -= UpdateWarriorCurrentHealth;
		NetworkCommunicationController.globalInstance.ReceiveMaxHealthEvent -= UpdateWarriorMaxHealth;
		NetworkCommunicationController.globalInstance.ReceiveCurrentManaEvent -= UpdateCurrentMana;
		NetworkCommunicationController.globalInstance.ReceiveOverrideCurrentManaEvent -= OverrideCurrentMana;
		NetworkCommunicationController.globalInstance.ReceiveOverrideCurrentHealthEvent -= OverrideCurrentHealth;

		NetworkCommunicationController.globalInstance.ReceiveDeathCountEvent -= UpdateDeathCount;
		NetworkCommunicationController.globalInstance.ReceiveCurrentKillCountEvent -= UpdateCurrentKillCount;
		NetworkCommunicationController.globalInstance.ReceiveTotalKillCountEvent -= UpdateTotalKillCount;

		NetworkCommunicationController.globalInstance.ReceivePowerUpCountEvent -= UpdatePowerUpCount;
		NetworkCommunicationController.globalInstance.ReceiveInterruptionCountEvent -= UpdateInterruptions;
		NetworkCommunicationController.globalInstance.ReceiveLevelCutsceneIndexEvent -= UpdateLevelCutsceneIndex;
	}

	public void WarriorSendGameState()
	{
		//Synchronize Monsters
		SendMonsters();

		//Synchronize Health
		SendHealthOverride();

		//Synchronize Mana
		SendManaOverride();

		//Synchronize Deaths
		SendDeaths();

		//Synchronize Kills
		SendKills();

		//Synchronize Dialogues
		SendDialogues();
	}

	public void SendMonsters()
	{
		NetworkCommunicationController.globalInstance.SendRandomSeed(playerStatsManager.GetRandomSeed());

		NetworkCommunicationController.globalInstance.SendMonstersChaseObstructedPlayer(LevelManager.globalInstance.enemyChaseObstructedPlayer);

		NetworkCommunicationController.globalInstance.SendDeadMonsters(LevelManager.globalInstance.GetDeadEnemiesIndex());

		LevelManager.globalInstance.AddActiveEnemiesToNetworkIndex();
		NetworkEnemyPositionInterpolationController.globalInstance.SendEntireEnemyIndexList();

		NetworkEnemyActionController.globalInstance.SendAllMonstersFacing();//Could just send those who are not default facing -_-
	}

	public void SendHealth()
	{
		NetworkCommunicationController.globalInstance.SendCurrentHealth(warriorHealth.CurrentHealth, warriorHealth.TempHP, warriorHealth.MaxHealth);

		//NetworkCommunicationController.globalInstance.SendMaxHealth(warriorHealth.MaxHealth);
	}

	public void SendHealthOverride()
	{
		NetworkCommunicationController.globalInstance.SendOverrideCurrentHealth(warriorHealth.CurrentHealth, warriorHealth.TempHP, warriorHealth.MaxHealth);
	}

	public void SendManaOverride()
	{
		NetworkCommunicationController.globalInstance.SendOverrideCurrentMana(warriorMana.GetCurrentMana());
	}

	public void SendMana()
	{
		NetworkCommunicationController.globalInstance.SendCurrentMana(warriorMana.GetCurrentMana());
	}

	public void SendDeaths()
	{
		NetworkCommunicationController.globalInstance.SendDeathCount(playerStatsManager.GetTotalDeaths());

		//TODO: Should have deaths per level
	}

	public void SendKills()
	{
		NetworkCommunicationController.globalInstance.SendCurrentKillCount(playerStatsManager.GetCurrentKills());

		NetworkCommunicationController.globalInstance.SendTotalKillCount(playerStatsManager.GetTotalKills());
	}

	public void SendDialogues()
	{
		NetworkCommunicationController.globalInstance.SendPowerUpCount(playerStatsManager.GetTotalPowerUps());

		NetworkCommunicationController.globalInstance.SendInterruptionCount(playerStatsManager.GetInterruptionsByWarrior(), playerStatsManager.GetInterruptionsByMage());

		NetworkCommunicationController.globalInstance.SendLevelCutsceneIndex(LevelManager.levelCutsceneDialogueIndex);
	}

	public void MageSynchronizeLink()
	{
		Debug.Log("Sending synchronisation link");

		NetworkSynchronizer.globalInstance.SendDialogues();

		//NetworkCommunicationController.globalInstance.SendOverridePosition(warriorHealth.transform.position.ToVector2(), true);//Position is sent along the Link RPC
		NetworkPositionInterpolationController.globalInstance.snapshots.Clear();//Removing all snapshots so it won't bug

		//Mage's active indexes override warrior's
		NetworkEnemyPositionInterpolationController.globalInstance.SendEntireEnemyIndexList();

		NetworkDamageShare.globalInstance.SynchronizeFully();
		FindObjectOfType<SpellLink>().LocalCastEffect();

		Debug.Log("Kills mage about to send" + PlayerStatsManager.globalInstance.GetCurrentKills());

		//Send to the other player to deal with the HP properly.
		NetworkCommunicationController.globalInstance.SendWarriorSynchronizeLink(warriorHealth.transform.position.ToVector2(), PlayerStatsManager.globalInstance.GetCurrentKills());
	}

	//This happens ONLY to the client/mage
	public void UpdateSeed(RpcArgs args)
	{
		// This line below should be in pretty much every
		// RPC that takes custom-data types.
		byte[] receivedBytes = args.GetNext<byte[]>();

		// We convert receivedBytes to the original object of data-type <Random.State>
		UnityEngine.Random.State randomSeedState = receivedBytes.ByteArrayToObject<UnityEngine.Random.State>();



		//Set the seed
		Random.state = randomSeedState;

		//Cache/store the seed
		PlayerStatsManager.globalInstance.SetRandomSeed(randomSeedState);
	}

	public void UpdateChaseObstructionMonsters(RpcArgs args)
	{
		// This line below should be in pretty much every
		// RPC that takes custom-data types.
		byte[] receivedBytes = args.GetNext<byte[]>();

		// We convert receivedBytes to the original object of data-type <List<bool>>
		List<bool> shouldMonsterChaseObstructedPlayer = receivedBytes.ByteArrayToObject<List<bool>>();

		FindObjectOfType<LevelManager>().SetChaseObstructedPlayerWrapper(shouldMonsterChaseObstructedPlayer);
	}

	public void UpdateDeadMonsters(RpcArgs args)
	{
		// This line below should be in pretty much every
		// RPC that takes custom-data types.
		byte[] receivedBytes = args.GetNext<byte[]>();

		// We convert receivedBytes to the original object of data-type <List<bool>>
		List<ushort> deadMonstersIndexList = receivedBytes.ByteArrayToObject<List<ushort>>();

		
		for (int i = 0; i < deadMonstersIndexList.Count; i++)
		{
			//Debug.Log("Dead: " + deadMonstersIndexList[i]);

			PlayerStatsManager.globalInstance.DecreaseKillCount();//tfw Die() is Interface-locked and cannot have more parameters ffs.

			LevelManager.globalInstance.GetEnemyBehaviourFromIndex(deadMonstersIndexList[i]).Die();
		}
			

	}

	public void UpdateWarriorCurrentHealth(RpcArgs args)
	{
		int receivedCurrentHealth = (int) args.GetNext<byte>();
		int receivedTempHP = (int) args.GetNext<byte>();
		int receivedMaxHealth = (int) args.GetNext<byte>();

		UpdateWarriorCurrentHealth(receivedCurrentHealth, receivedTempHP, receivedMaxHealth);
	}

	public void UpdateWarriorCurrentHealth(int receivedCurrentHealth, int receivedTempHP, int receivedMaxHealth)
	{
		//The greatest health overrides the other, hence desync ftw for "scouting" aka clearing forward areas then relinking.
		if (receivedCurrentHealth > warriorHealth.CurrentHealth)
			warriorHealth.CurrentHealth = receivedCurrentHealth;

		if (receivedTempHP > warriorHealth.TempHP)
			warriorHealth.TempHP = receivedTempHP;

		if (receivedMaxHealth > warriorHealth.MaxHealth)
			warriorHealth.MaxHealth = receivedMaxHealth;
	}

	public void UpdateWarriorMaxHealth(RpcArgs args)
	{
		warriorHealth.MaxHealth = args.GetNext<int>();
	}

	public void UpdateCurrentMana(RpcArgs args)
	{
		byte receivedMana = args.GetNext<byte>();

		//Debug.Log("Received: " + receivedMana + " current: " + warriorMana.GetCurrentMana());

		//If "link" gave 4 mana to the other player, I would obviously make this < instead of >
		//But since "link" resets to 0 mana, it doesnt break the game (perma-instakills by spamming branch/link)
		//Now, its yet another reason to branch and resync :D
		if ((int)receivedMana > warriorMana.GetCurrentMana())
			warriorMana.SetPlayerCurrentMana(receivedMana);
	}

	public void OverrideCurrentMana(RpcArgs args)
	{
		warriorMana.SetPlayerCurrentMana(args.GetNext<byte>());
	}

	public void OverrideCurrentHealth(RpcArgs args)
	{
		warriorHealth.CurrentHealth = (int) args.GetNext<byte>();
		warriorHealth.TempHP = (int) args.GetNext<byte>();
		warriorHealth.MaxHealth = (int) args.GetNext<byte>();
	}

	public void UpdateDeathCount(RpcArgs args)
	{
		playerStatsManager.SetTotalDeaths(args.GetNext<int>());
	}

	public void UpdateCurrentKillCount(RpcArgs args)
	{
		playerStatsManager.SetCurrentKills(args.GetNext<int>());
	}

	public void UpdateTotalKillCount(RpcArgs args)
	{
		playerStatsManager.SetTotalKills(args.GetNext<int>());
	}

	public void UpdateInterruptions(RpcArgs args)
	{
		playerStatsManager.SetInterruptionsByWarrior(args.GetNext<int>());
		playerStatsManager.SetInterruptionsByMage(args.GetNext<int>());
	}

	public void UpdatePowerUpCount(RpcArgs args)
	{
		playerStatsManager.SetTotalPowerUps(args.GetNext<int>());
	}

	public void UpdateLevelCutsceneIndex(RpcArgs args)
	{
		LevelManager.levelCutsceneDialogueIndex = args.GetNext<int>();
	}

	public void LevelCutsceneTriggered()
	{
		NetworkCommunicationController.globalInstance.SendDialogueTriggered(LevelManager.levelCutsceneDialogueIndex, warriorHealth.transform.position);

		if (NetworkCommunicationController.globalInstance.IsServer())
		{
			//Clears all enemy indexes locally, then sends to client to do the same.
			NetworkEnemyPositionInterpolationController.globalInstance.ClearAllEnemyIndexes();
			NetworkCommunicationController.globalInstance.SendClearSynchronizedMonsters();
		}

		//If (mage + start of level) -> delete snapshots
		//if (NetworkCommunicationController.globalInstance.IsServer() == false && LevelManager.levelCutsceneDialogueIndex % 2 == 0 && LevelManager.levelCutsceneDialogueIndex < 10)
			//NetworkPositionInterpolationController.globalInstance.snapshots.Clear();//Removing all snapshots so it won't bug

		if (NetworkDamageShare.globalInstance.IsSynchronized() == false)
		{
			//If mage, clear snapshots ((sending position above should make warrior take mage's position))
			if (NetworkCommunicationController.globalInstance.IsServer() == false)
				NetworkPositionInterpolationController.globalInstance.snapshots.Clear();//Removing all snapshots so it won't bug

			NetworkDamageShare.globalInstance.SynchronizeFully();
		}

	}

	public void ReceivedLevelCutscene(RpcArgs args)
	{
		int possibleFireLevelGateLockCutsceneIndex = args.GetNext<int>();
		Vector2 magePosition = args.GetNext<Vector2>();//I don't even know what this is for, but could be useful.
		bool replyBack = args.GetNext<bool>();

		//Fire level lock
		if (possibleFireLevelGateLockCutsceneIndex == 5)//5 is to get into fire level exit. This means the other player who sent has activated it.
		{
			//If there is a lock on end gate, unlock it (lock = requirement/condition to enter beforehand)
			if (GameObject.Find("End").GetComponent<KillLockedEndGate>() != null)
				GameObject.Find("End").GetComponent<KillLockedEndGate>().UnlockEndGate();
		}

		Debug.Log("This should trigger for both players.");

		if (NetworkDamageShare.globalInstance.IsSynchronized() == false)
		{
			//If warrior, go at mage's position
			if (NetworkCommunicationController.globalInstance.IsServer())
				warriorHealth.transform.position = magePosition;

			NetworkDamageShare.globalInstance.SynchronizeFully();
		}
	}

	/// <summary>
	/// This is for mage so he won't bug after reviving
	/// </summary>
	public void StartedRevive()
	{
		if (NetworkCommunicationController.globalInstance.IsServer() == false)
			NetworkPositionInterpolationController.globalInstance.snapshots.Clear();//Removing all snapshots so it won't bug

		if (NetworkDamageShare.globalInstance.IsSynchronized() == false)
			NetworkDamageShare.globalInstance.SynchronizeFully();
	}

}
