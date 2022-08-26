using BeardedManStudios.Forge.Networking;
using UnityEngine;
using NaughtyAttributes;

public class NetworkDamageShare : MonoBehaviour
{
	//Singleton
	public static NetworkDamageShare globalInstance;

	[ReadOnly]
	public bool activated = false;

	private WarriorHealth warriorHealth;

	private bool isSynchronized = true;
	private EnemyBehaviour tempEnemyBehaviour;
	private int healthDifference;

	public void Start()
	{
		//Setting our singleton
		globalInstance = this;

		warriorHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>();
	}

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		ActivateDamageEvents();

		warriorHealth.PoweredUpNotifyOtherPlayerEvent += PoweredUp;
		warriorHealth.DiedNotifyOtherPlayerEvent += LocalDeathTriggered;
		EnemyBehaviour.playerKillerEvent += SendPlayerKiller;
		NetworkCommunicationController.globalInstance.ReceiveMonsterPlayerKillerEvent += ReceivePlayerKiller;
		NetworkCommunicationController.globalInstance.ReceiveDesynchronizeEvent += DesynchronizeFullyWrapper;
		NetworkCommunicationController.globalInstance.ReceiveSynchronizePowerUpEvent += SynchronizeFullyWrapper;
		NetworkCommunicationController.globalInstance.ReceiveMageSynchronizeLinkEvent += MageSynchronizeLinkSpellEffect;
		NetworkCommunicationController.globalInstance.ReceiveWarriorSynchronizeLinkEvent += WarriorSynchronizeLinkSpellEffect;
		NetworkCommunicationController.globalInstance.ReceiveSynchronizePowerUpEvent += ReceivedPowerUp;
		NetworkCommunicationController.globalInstance.ReceiveSynchronizeDeathEvent += ReceivedDeath;

		warriorHealth.gameObject.GetComponent<WarriorMovement>().justKilledEnemy += SendMonsterKilled;//If its for any monster put it on damage events!
		NetworkCommunicationController.globalInstance.ReceiveMonsterKilledEvent += ReceivedMonsterKilled;
	}

	public void ActivateDamageEvents()
	{
		warriorHealth.TookDamageEvent += SendTookDamage;

		NetworkCommunicationController.globalInstance.ReceiveTakeDamageEvent += TakeDamage;
	}

	public void DeActivate()
	{
		activated = false;

		DeActivateDamageEvents();

		warriorHealth.PoweredUpNotifyOtherPlayerEvent -= PoweredUp;
		warriorHealth.DiedNotifyOtherPlayerEvent -= LocalDeathTriggered;
		EnemyBehaviour.playerKillerEvent -= SendPlayerKiller;
		NetworkCommunicationController.globalInstance.ReceiveMonsterPlayerKillerEvent -= ReceivePlayerKiller;
		NetworkCommunicationController.globalInstance.ReceiveDesynchronizeEvent -= DesynchronizeFullyWrapper;
		NetworkCommunicationController.globalInstance.ReceiveSynchronizePowerUpEvent -= SynchronizeFullyWrapper;
		NetworkCommunicationController.globalInstance.ReceiveMageSynchronizeLinkEvent -= MageSynchronizeLinkSpellEffect;
		NetworkCommunicationController.globalInstance.ReceiveWarriorSynchronizeLinkEvent -= WarriorSynchronizeLinkSpellEffect;
		NetworkCommunicationController.globalInstance.ReceiveSynchronizePowerUpEvent -= ReceivedPowerUp;
		NetworkCommunicationController.globalInstance.ReceiveSynchronizeDeathEvent -= ReceivedDeath;

		warriorHealth.gameObject.GetComponent<WarriorMovement>().justKilledEnemy -= SendMonsterKilled;
		NetworkCommunicationController.globalInstance.ReceiveMonsterKilledEvent -= ReceivedMonsterKilled;
	}

	public void DeActivateDamageEvents()
	{
		warriorHealth.TookDamageEvent -= SendTookDamage;

		NetworkCommunicationController.globalInstance.ReceiveTakeDamageEvent -= TakeDamage;
	}

	public void SendTookDamage(int damageTaken)
	{
		NetworkCommunicationController.globalInstance.SendTakeDamage(warriorHealth.CurrentHealth, warriorHealth.TempHP, damageTaken);
	}

	public void SendPlayerKiller(ushort enemyListIndex)
	{
		NetworkCommunicationController.globalInstance.SendMonsterPlayerKiller(enemyListIndex);
	}

	public void ReceivePlayerKiller(RpcArgs args)
	{
		//This is from the LevelManager.enemyList[i], with i being this variable
		ushort monsterEnemyListIndex = args.GetNext<ushort>();

		if (LevelManager.globalInstance.enemyList[monsterEnemyListIndex] != null)
			LevelManager.globalInstance.enemyList[monsterEnemyListIndex].GetComponent<EnemyBehaviour>().AddPlayerKillerHP(2, false);

		/*
		//If monster is alive
		if (LevelManager.globalInstance.enemyList[monsterEnemyListIndex] != null)
		{
			LevelManager.globalInstance.enemyList[monsterEnemyListIndex].GetComponent<EnemyBehaviour>().AddPlayerKillerHP(2, false);

			Debug.Log("|NOT NULL!|");
		}
		else//if dead
		{
			LevelManager.globalInstance.enemyList[monsterEnemyListIndex].SetActive(true);
			LevelManager.globalInstance.enemyList[monsterEnemyListIndex].GetComponent<EnemyBehaviour>().AddPlayerKillerHP(2, false);
			LevelManager.globalInstance.enemyList[monsterEnemyListIndex].SetActive(false);//Shouldn't it by itself disable itself?

			Debug.Log("|NULL!|");
		}*/


	}

	public void TakeDamage(RpcArgs args)
	{
		int currentHealthBeforeDamage = args.GetNext<int>();
		int tempHealthBeforeDamage = args.GetNext<int>();
		int damageTakenBeforeDamage = args.GetNext<int>();

		//If Mage
		if (NetworkCommunicationController.globalInstance.IsServer() == false)
		{
			//If synced perfectly before damage
			if (currentHealthBeforeDamage == warriorHealth.CurrentHealth && tempHealthBeforeDamage == warriorHealth.TempHP)
				warriorHealth.TakeDamage(damageTakenBeforeDamage, warriorHealth.transform.position);
			//If client has taken damage from same source as warrior in client, but also receives host warrior's damage source
			//Aka if invulnerable and 1 damage away in health
			else if (warriorHealth.GetInvulnerability() == true)
			{
				healthDifference = warriorHealth.CurrentHealth + warriorHealth.TempHP - currentHealthBeforeDamage - tempHealthBeforeDamage;
				if (healthDifference == -1)
				{
					Debug.LogWarning("Would have desynced but invulnerable and 1 hp difference");
				}
				else
					NetworkCommunicationController.globalInstance.SendDesynchronize();
			}
			else//DESYNC -> Because hp is already different by one
				NetworkCommunicationController.globalInstance.SendDesynchronize();
		}
		else//warrior, desync (since all dmg should first happen by warrior)
		{
			Debug.Log("currentHealthBeforeDamage is: " + currentHealthBeforeDamage);
			Debug.Log("currentHealth is: " + warriorHealth.CurrentHealth);
			Debug.Log("damage is: " + damageTakenBeforeDamage);

			Debug.Log("tempHealthBeforeDamage is: " + tempHealthBeforeDamage);
			Debug.Log("tempHealth is: " + warriorHealth.TempHP);

			healthDifference = warriorHealth.CurrentHealth + warriorHealth.TempHP - currentHealthBeforeDamage - tempHealthBeforeDamage;

			if (healthDifference != -1)
				NetworkCommunicationController.globalInstance.SendDesynchronize();


		}

	}

	public void DesynchronizeFully()
	{
		if (isSynchronized == false)
			return;

		isSynchronized = false;

		Debug.Log("Desynchronize!");

		//Now, remove damage share and position share
		DeActivateDamageEvents();
		NetworkPositionInterpolationController.globalInstance.DeActivate();
		NetworkEnemyActionController.globalInstance.DeActivate();
		NetworkKillmeterWrapper.globalInstance.DetermineClientAllowDrainMana(true);
		//NetworkSynchronizer.globalInstance.acceptCurrentHealthFlag = true;
		NetworkDesyncSphereInterpolation.globalInstance.Activate();

		if (NetworkCommunicationController.globalInstance.IsServer() == false)
		{
			NetworkEnemyPositionInterpolationController.globalInstance.ClientRestartEnemies();
			NetworkEnemyPositionInterpolationController.globalInstance.ResetEnemiesVelocity();//So enemies won't slide on desync

			//PlayerStatsManager.globalInstance.SetCurrentKills(0);
		}
		else
			//Synchronize kills on desync, just in case.
			NetworkSynchronizer.globalInstance.SendKills();

		//SFX
		PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Desync); 
	}

	public void SynchronizeFully()
	{
		//Maybe bugs things idk
		if (isSynchronized == true)
			return;
		
		isSynchronized = true;

		Debug.Log("Resynchronized!");

		//TODO: SFX of linking plz, sounding like a chain

		//Now, re-enable damage share, and position share
		ActivateDamageEvents();
		NetworkPositionInterpolationController.globalInstance.Activate();
		NetworkEnemyActionController.globalInstance.Activate();
		NetworkCommunicationController.globalInstance.SendCurrentHealth(warriorHealth.CurrentHealth, warriorHealth.TempHP, warriorHealth.MaxHealth);
		NetworkSynchronizer.globalInstance.SendMana();
		NetworkKillmeterWrapper.globalInstance.DetermineClientAllowDrainMana(false);
		NetworkDesyncSphereInterpolation.globalInstance.DeActivate();

		if (NetworkCommunicationController.globalInstance.IsServer() == false)//So enemies won't jitter by having velocity conflicting repositioning!
			NetworkEnemyPositionInterpolationController.globalInstance.ResetEnemiesVelocity();

		//Pillars dont spawn 2 times per cast while desynced, so when resynced they must be killed or real desync happens
		LevelManager.globalInstance.DeletePillars();

		//SFX
		PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Resync); 
	}

	public void SynchronizeFullyWrapper(RpcArgs args)
	{
		SynchronizeFully();
	}

	public void DesynchronizeFullyWrapper(RpcArgs args)
	{
		DesynchronizeFully();
	}


	public void PoweredUp()
	{
		NetworkCommunicationController.globalInstance.SendSynchronizePowerUp(warriorHealth.CurrentHealth, warriorHealth.TempHP, PlayerStatsManager.globalInstance.GetCurrentKills(), PlayerStatsManager.globalInstance.GetTotalKills());

		NetworkSynchronizer.globalInstance.SendDialogues();

		Debug.Log("HELLO " + PlayerStatsManager.globalInstance.GetCurrentKills());

		//If desynchronized
		if (IsSynchronized() == false)
		{
			//Debug.Log("Helloooo " + NetworkCommunicationController.globalInstance.IsServer());

			if (NetworkCommunicationController.globalInstance.IsServer() == false)
			{
				NetworkCommunicationController.globalInstance.SendOverridePosition(warriorHealth.transform.position.ToVector2());
				NetworkPositionInterpolationController.globalInstance.snapshots.Clear();//Removing all snapshots so it won't bug
			}

			NetworkEnemyPositionInterpolationController.globalInstance.SendEntireEnemyIndexList();

			SynchronizeFully();
		}

	}

	//Doesn't even get called/registered. However, SynchronizeFully() is sent to other player when PowerUp happens!
	public void ReceivedPowerUp(RpcArgs args)
	{
		int receivedCurrentHealth = args.GetNext<int>();
		int receivedTempHealth = args.GetNext<int>();

		int receivedCurrentKills = args.GetNext<int>();
		int receivedTotalKills = args.GetNext<int>();

		warriorHealth.RewardTempHPAesthetics();

		NetworkSynchronizer.globalInstance.UpdateWarriorCurrentHealth(receivedCurrentHealth, receivedTempHealth, warriorHealth.MaxHealth);
		/*		
		//The one who sent it, has already ran PowerUp
		Debug.Log("StartHealth is: " + (warriorHealth.CurrentHealth + warriorHealth.TempHP));

		//Now, replace health with the one sent
		
		Debug.Log("Received Current Health is: " + receivedCurrentHealth);
		Debug.Log("Received Temp Health is: " + receivedTempHealth);
		warriorHealth.CurrentHealth = receivedCurrentHealth;
		warriorHealth.TempHP = receivedTempHealth;

		Debug.Log("EndHealth is: " + (warriorHealth.CurrentHealth + warriorHealth.TempHP));
		*/

		//Add kill difference
		//DetermineKillGapPowerUp(args.GetNext<int>());
		//Set total kills
		PlayerStatsManager.globalInstance.SetCurrentKills(receivedCurrentKills);
		PlayerStatsManager.globalInstance.SetTotalKills(receivedTotalKills);

		//If desynchronized
		if (IsSynchronized() == false)
			SynchronizeFully();

	}

	public void WarriorSynchronizeLinkSpellEffect(RpcArgs args)
	{
		Debug.Log("Sending synchronisation link");

		//Place on position of mage before doing anything (e.g. killing everything around you)
		warriorHealth.transform.position = args.GetNext<Vector2>();

		SynchronizeFully();
		FindObjectOfType<SpellLink>().LocalCastEffect();

		//Debug.Log("Kills warrior about to change " + PlayerStatsManager.globalInstance.GetCurrentKills());
		//DetermineKillGapLink(args.GetNext<int>());
		//Debug.Log("Kills warrior after seeing mage's kills" + PlayerStatsManager.globalInstance.GetCurrentKills());
	}

	public void MageSynchronizeLinkSpellEffect(RpcArgs args)
	{
		//DetermineKillGapLink(args.GetNext<int>());
	}

	

	public void ReceivedMonsterKilled(RpcArgs args)
	{
		//This is from the LevelManager.enemyList[i], with i being this variable
		ushort monsterEnemyListIndex = args.GetNext<ushort>();

		tempEnemyBehaviour = LevelManager.globalInstance.enemyList[monsterEnemyListIndex].GetComponent<EnemyBehaviour>();

		//Debug.Log("Receivedd: " + monsterEnemyListIndex);

		if (tempEnemyBehaviour == null)
		{
			Debug.LogError("ReceivedMonsterKilled has literally no <EnemyBehaviour>, how can this be? " + monsterEnemyListIndex);
			return;
		}

		//Debug.Log("Start Kills " + PlayerStatsManager.globalInstance.GetCurrentKills());

		//Debug.Log("Synchronized? " + NetworkDamageShare.globalInstance.IsSynchronized() + " TriggersPowerUp? " + PlayerStatsManager.globalInstance.TriggersPowerUp(2));

		//To avoid triggering powerup twice while desynced
		if (NetworkDamageShare.globalInstance.IsSynchronized() == false && PlayerStatsManager.globalInstance.TriggersPowerUp(2))
		{
			PlayerStatsManager.globalInstance.SetCurrentKills(PlayerStatsManager.globalInstance.GetCurrentKills() - 2);
			//Debug.Log("Current kills: " + PlayerStatsManager.globalInstance.GetCurrentKills());
		}
			

		//If monster is alive
		if (tempEnemyBehaviour.transform.gameObject.activeSelf && tempEnemyBehaviour.isDying == false)
			tempEnemyBehaviour.Die();
		else//Just remove from index, to make sure its removed
			NetworkEnemyPositionInterpolationController.globalInstance.RemoveEnemyIndex(monsterEnemyListIndex);	

		//Debug.Log("End Kills " + PlayerStatsManager.globalInstance.GetCurrentKills());
	}

	//If monster has playerkiller buff, it sends regardless
	//Else, send only if synchronized!
	public void SendMonsterKilled(ushort enemyListIndex)
	{
		NetworkCommunicationController.globalInstance.SendMonsterKilled(enemyListIndex);
	}


	public void LocalDeathTriggered()
	{
		NetworkCommunicationController.globalInstance.SendSynchronizeDeath(warriorHealth.transform.position.ToVector2());

		if (NetworkCommunicationController.globalInstance.IsServer())
		{
			//Clears all enemy indexes locally, then sends to client to do the same.
			NetworkEnemyPositionInterpolationController.globalInstance.ClearAllEnemyIndexes();
			NetworkCommunicationController.globalInstance.SendClearSynchronizedMonsters();
		}
	}

	public void ReceivedDeath(RpcArgs args)
	{
		//If desynchronized
		if (IsSynchronized() == false)
		{
			SynchronizeFully();

			if (NetworkCommunicationController.globalInstance.IsServer())
				warriorHealth.transform.position = args.GetNext<Vector2>();
		}

		//If not already dying, die.
		if (warriorHealth.GetComponent<WarriorMovement>().startDying == false)
			warriorHealth.DieByNetwork();

	}

	//Used in warrior movement btw
	public bool IsSynchronized()
	{
		return isSynchronized;
	}
}
