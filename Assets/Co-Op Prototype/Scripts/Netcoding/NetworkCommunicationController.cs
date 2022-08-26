using System;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCommunicationController : NetworkCommunicationControllerBehavior
{

	//Checking if online or not ;)
	//if (NetworkCommunicationController.globalInstance == null)
	public static NetworkCommunicationController globalInstance;

	//public Action NetworkStartEvent;

	public Action<RpcArgs> ReceiveMageJumpEvent;
	public Action<RpcArgs> ReceiveMageDodgerollEvent;
	public Action<RpcArgs> ReceiveMageSpellwordEvent;

	public Action<RpcArgs> ReceiveWarriorRightJoystickEvent;
	public Action<RpcArgs> ReceiveWarriorLeftJoystickEvent;
	public Action<RpcArgs> ReceiveWarriorPositionEvent;
	public Action<RpcArgs> ReceiveWarriorInputSnapshotEvent;

	public Action<RpcArgs> ReceiveCurrentHealthEvent;
	public Action<RpcArgs> ReceiveMaxHealthEvent;
	public Action<RpcArgs> ReceiveDeathCountEvent;
	public Action<RpcArgs> ReceiveCurrentKillCountEvent;
	public Action<RpcArgs> ReceiveTotalKillCountEvent;
	public Action<RpcArgs> ReceivePowerUpCountEvent;
	public Action<RpcArgs> ReceiveInterruptionCountEvent;
	public Action<RpcArgs> ReceiveLevelCutsceneIndexEvent;
	public Action<RpcArgs> ReceivePillarPositionEvent;
	public Action<RpcArgs> ReceiveMonsterKilledEvent;
	public Action<RpcArgs> ReceiveDesyncedPositionEvent;
	public Action<RpcArgs> ReceiveDesyncedSphereLinkEvent;

	public Action<RpcArgs> ReceiveLevelEditorCreateStaticTileEvent;
	public Action<RpcArgs> ReceiveLevelEditorCreateStaticPlatformEvent;
	public Action<RpcArgs> ReceiveLevelEditorCreateFreeformTileEvent;
	public Action<RpcArgs> ReceiveLevelEditorRemoveStaticTileEvent;
	public Action<RpcArgs> ReceiveLevelEditorRemoveFreeformTileEvent;
	public Action<RpcArgs> ReceiveLevelEditorMoveFreeformTileEvent;
	public Action<RpcArgs> ReceiveLevelEditorMoveFreeformTileFinalizedEvent;
	public Action<RpcArgs> ReceiveLevelEditorConvertTileToFreeformEvent;
	public Action<RpcArgs> ReceiveLevelEditorSaveEvent;
	public Action<RpcArgs> ReceiveLevelEditorLoadEvent;
	public Action<RpcArgs> ReceiveLevelEditorPlayEvent;
	public Action<RpcArgs> ReceiveLevelEditorStopEvent;
	public Action<RpcArgs> ReceiveLevelEditorClearAllGridEvent;

	public Action<RpcArgs> ReceiveRandomSeedEvent;
	public Action<RpcArgs> ReceiveMonstersChaseObstructedPlayerEvent;
	public Action<RpcArgs> ReceiveDeadMonstersEvent;
	public Action<RpcArgs> ReceiveCurrentLevelEvent;

	public Action<RpcArgs> ReceiveTakeDamageEvent;
	public Action<RpcArgs> ReceiveMonsterPlayerKillerEvent;
	public Action<RpcArgs> ReceiveMonsterPositionEvent;
	public Action<RpcArgs> ReceiveMonsterFacingEvent;
	public Action<RpcArgs> ReceiveMonsterAnimationStartEvent;
	public Action<RpcArgs> ReceiveMonsterAttackActionEvent;
	public Action<RpcArgs> ReceiveMonsterRangedAttackEvent;
	public Action<RpcArgs> ReceiveHarpyRangedAttackEvent;
	public Action<RpcArgs> ReceiveClearSynchronizedMonstersEvent;
	public Action<RpcArgs> ReceiveAddedSynchronizedMonstersEvent;
	public Action<RpcArgs> ReceiveManaKillmeterEvent;
	public Action<RpcArgs> ReceiveCurrentManaEvent;
	public Action<RpcArgs> ReceiveOverrideCurrentManaEvent;
	public Action<RpcArgs> ReceiveOverrideCurrentHealthEvent;

	public Action<RpcArgs> ReceiveDesynchronizeEvent;
	public Action<RpcArgs> ReceiveMageSynchronizeLinkEvent;
	public Action<RpcArgs> ReceiveWarriorSynchronizeLinkEvent;
	public Action<RpcArgs> ReceiveSynchronizePowerUpEvent;
	public Action<RpcArgs> ReceiveSynchronizeDeathEvent;
	public Action<RpcArgs> ReceiveSynchronizeCutsceneEvent;
	public Action<RpcArgs> ReceiveOverridePositionEvent;
	public Action<RpcArgs> ReceiveDialogueTriggeredEvent;
	public Action<RpcArgs> ReceiveWarriorMenuToggledEvent;
	public Action<RpcArgs> ReceiveDarkwindDistortionChangeEvent;
	public Action<RpcArgs> ReceiveLevelLoadedEvent;

	public GameObject networkManagerGameObject;

	public NetworkPositionInterpolationController interpolationController;
	public NetworkEnemyPositionInterpolationController enemyInterpolationController;
	public NetworkEnemyAnimationOverrideController enemyAnimatorOverrideController;
	public NetworkEnemyActionController enemyActionController;
	public NetworkInputSnapshotManager inputSnapshotManager;
	public NetworkSynchronizer synchronizer;
	public NetworkDesyncSphereInterpolation desyncSphere;
	public NetworkPinger pinger;
	public NetworkDamageShare damageSharer;
	public NetworkSpellcasting spellcasting;
	public NetworkToggleMenu menuToggle;
	public NetworkLevelEditor levelEditor;
	public NetworkKillmeterWrapper killmeterMana;
	public NetworkDarkwindDistortionManager darkwindDistortioner;
	public NetworkLevelMatcher levelMatcher;

	public static bool hasFinalizedConnection = false;


	//Notifies everyone and everything, that it is setup on the network, ready to work!
	protected override void NetworkStart()
	{
		base.NetworkStart();

		MainThreadManager.Run(() => Debug.Log("Network Started!"));

		globalInstance = this;

		//Set this gameobject to become a child of NetworkManager GameObject, containing MultiplayerMenu.cs
		if (SteamManager.Initialized == false)
			transform.SetParent(GameObject.FindObjectOfType<MultiplayerMenu>().transform);
		else
			transform.SetParent(GameObject.FindObjectOfType<SteamMultiplayerMenu>().transform);

		networkManagerGameObject = GameObject.FindGameObjectWithTag("NetworkManager");
			interpolationController = networkManagerGameObject.GetComponent<NetworkPositionInterpolationController>();
			enemyInterpolationController = networkManagerGameObject.GetComponent<NetworkEnemyPositionInterpolationController>();
			enemyAnimatorOverrideController = networkManagerGameObject.GetComponent<NetworkEnemyAnimationOverrideController>();
			enemyActionController = networkManagerGameObject.GetComponent<NetworkEnemyActionController>();
			inputSnapshotManager = networkManagerGameObject.GetComponent<NetworkInputSnapshotManager>();
			synchronizer = networkManagerGameObject.GetComponent<NetworkSynchronizer>();
			desyncSphere = networkManagerGameObject.GetComponent<NetworkDesyncSphereInterpolation>();
			pinger = networkManagerGameObject.GetComponent<NetworkPinger>();
			damageSharer = networkManagerGameObject.GetComponent<NetworkDamageShare>();
			spellcasting = networkManagerGameObject.GetComponent<NetworkSpellcasting>();
			menuToggle = networkManagerGameObject.GetComponent<NetworkToggleMenu>();
			levelEditor = networkManagerGameObject.GetComponent<NetworkLevelEditor>();
			killmeterMana = networkManagerGameObject.GetComponent<NetworkKillmeterWrapper>();
			darkwindDistortioner = networkManagerGameObject.GetComponent<NetworkDarkwindDistortionManager>();
			levelMatcher = networkManagerGameObject.GetComponent<NetworkLevelMatcher>();

		//Smooth disconnection when host/client connection is lost, for whatever reason.
		NetworkManager.Instance.Networker.disconnected += Disconnect;
		NetworkManager.Instance.Networker.playerDisconnected += Disconnect;

		//This will trigger FinalizeNetworkStart(), when LevelManager.currentLevel are equal for both players.
		levelMatcher.Activate();
	}

	//Delays until both players are on the same level.
	//Instead of checking each frame, works by callback from NetworkLevelMatcher
	public void FinalizeNetworkStart()
	{
		hasFinalizedConnection = true;

		ActivateNetworkBehaviour();
	}

	public void ActivateNetworkBehaviour()
	{
		interpolationController.Activate();
		enemyInterpolationController.Activate();
		enemyAnimatorOverrideController.Activate();
		enemyActionController.Activate();
		inputSnapshotManager.Activate();
		synchronizer.Activate();
		pinger.Activate();
		damageSharer.Activate();
		spellcasting.Activate();
		menuToggle.Activate();
		killmeterMana.Activate();
		darkwindDistortioner.Activate();

		if (LevelManager.currentLevel == 7)
			levelEditor.Activate();

		//Clears the jitter
		if (IsServer() == false)
			interpolationController.ClearSnapshots(4f);

		NetworkBootWrapper.globalInstance.hasDisconnected = false;

		PlayerStatsManager.globalInstance.SetPlayedAsHost(IsServer());
		PlayerStatsManager.globalInstance.SetPlayedAsClient(!IsServer());
	}

	public void DeActivateNetworkBehaviour()
	{
		interpolationController.DeActivate();
		enemyInterpolationController.DeActivate();
		enemyAnimatorOverrideController.DeActivate();
		enemyActionController.DeActivate();
		inputSnapshotManager.DeActivate();
		synchronizer.DeActivate();
		desyncSphere.DeActivate();
		pinger.DeActivate();
		damageSharer.DeActivate();
		spellcasting.Deactivate();
		menuToggle.DeActivate();
		killmeterMana.DeActivate();
		darkwindDistortioner.DeActivate();

		if (LevelManager.currentLevel == 7)
			levelEditor.DeActivate();
	}

	/// <summary>
	/// Runs on host when you close host
	/// Runs on client when you close host
	/// Runs on client when you close client
	/// </summary>
	/// <param name="sender"></param>
	public void Disconnect(NetWorker sender)
	{
		if (NetworkBootWrapper.globalInstance.hasDisconnected == false)
		{
			MainThreadManager.Run(() =>
			{
				if (hasFinalizedConnection)
					DeActivateNetworkBehaviour();

				Destroy(FindObjectOfType<MultiplayerMenu>());

				//Create the next (inactive) MultiplayerMenu, so you can re-host/re-connect ;)
				NetworkBootWrapper.globalInstance.hasDisconnected = true;
				NetworkBootWrapper.globalInstance.CreateAndStartMultiplayerMenu();

				globalInstance = null;

				networkObject.Destroy();//Also calls Destroy(this);
			});
		}

	}

	//Runs on host when you close host
	//Runs on host when you close client
	public void Disconnect(NetworkingPlayer player, NetWorker sender)
	{
		if (NetworkBootWrapper.globalInstance.hasDisconnected == false)
		{
			MainThreadManager.Run(() =>
			{
				DeActivateNetworkBehaviour();

				//Set this gameobject to become a child of NetworkManager GameObject, containing MultiplayerMenu.cs
				if (SteamManager.Initialized == false)
					Destroy(FindObjectOfType<MultiplayerMenu>());
				else
					Destroy(FindObjectOfType<SteamMultiplayerMenu>());

				//Create the next (inactive) MultiplayerMenu, so you can re-host/re-connect ;)
				NetworkBootWrapper.globalInstance.hasDisconnected = true;
				NetworkBootWrapper.globalInstance.CreateAndStartMultiplayerMenu();

				networkObject.Destroy();//Also calls Destroy(this);
			});
		}
	}

	//Had to make this because I cannot access MainThreadManager via NetworkPinger, and cannot check if.activeSelf, nor place text
	public void SetPing(Text pingValue, string valueToPlace, GameObject pingUI)
	{
		MainThreadManager.Run(() =>
		{
			if (pingUI.activeSelf)
				pingValue.text = valueToPlace;
		});
	}

	//=================
	//===Global Gets===
	//=================
	public bool IsServer()
	{
		return networkObject.IsServer;
	}

	//===========================
	//===Local Notifiers Below===
	//===========================

	//===Mage===

	public void SendMageJump(bool spacebarPressed)
	{
		networkObject.SendRpc(RPC_RECEIVE_MAGE_JUMP, Receivers.Others, spacebarPressed);
	}

	public void SendMageDodgeroll(bool dodgerollRight)
	{
		networkObject.SendRpc(RPC_RECEIVE_MAGE_DODGEROLL, Receivers.Others, dodgerollRight);
	}

	public void SendMageSpellword(string spellword)
	{
		networkObject.SendRpc(RPC_RECEIVE_MAGE_SPELLWORD, Receivers.Others, spellword);
	}

	//===Warrior===

	public void SendWarriorRightJoystick(Vector2 combatInputDirection)
	{
		networkObject.SendRpc(RPC_RECEIVE_WARRIOR_RIGHT_JOYSTICK, Receivers.Others, combatInputDirection);
	}

	public void SendWarriorLeftJoystick(Vector2 movementInputDirection)
	{
		networkObject.SendRpc(RPC_RECEIVE_WARRIOR_LEFT_JOYSTICK, Receivers.Others, movementInputDirection);
	}

	public void SendWarriorPositionUnreliable(Vector2 warriorPosition, double timePassedSinceLastPositionTotal)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_WARRIOR_POSITION, Receivers.Others, timePassedSinceLastPositionTotal, new Vector2(warriorPosition.x, warriorPosition.y));
	}

	public void SendWarriorPositionReliable(Vector2 warriorPosition, double timePassedSinceLastPositionTotal)
	{
		networkObject.SendRpc(RPC_RECEIVE_WARRIOR_POSITION, Receivers.Others, timePassedSinceLastPositionTotal, new Vector2(warriorPosition.x, warriorPosition.y));
	}

	public void SendWarriorInputSnapshot(InputTrackingData sentInputSnapshot)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_WARRIOR_INPUT_SNAPSHOT, Receivers.Others, sentInputSnapshot.ObjectToByteArray());
	}

	public void SendWarriorMenuToggled(bool isOpen)
	{
		networkObject.SendRpc(RPC_RECEIVE_WARRIOR_MENU_TOGGLED, Receivers.Others, isOpen);
	}

	//public void Send

	public void SendDarkwindDistortionChange(DarkwindMenu.ButtonClicked buttonClicked)
	{
		networkObject.SendRpc(RPC_RECEIVE_DARKWIND_DISTORTION_CHANGE, Receivers.Others, (byte)buttonClicked);
	}

	//===Health===

	public void SendCurrentHealth(int currentHealth, int tempHP, int maxHealth)
	{
		networkObject.SendRpc(RPC_RECEIVE_CURRENT_HEALTH, Receivers.Others, (byte) currentHealth, (byte) tempHP, (byte) maxHealth);
	}

	public void SendMaxHealth(int maxHealth)
	{
		networkObject.SendRpc(RPC_RECEIVE_MAX_HEALTH, Receivers.Others, maxHealth);
	}

	//===DeathKills===
	public void SendDeathCount(int deathCount)
	{
		networkObject.SendRpc(RPC_RECEIVE_DEATH_COUNT, Receivers.Others, deathCount);
	}

	public void SendCurrentKillCount(int currentKills)
	{
		networkObject.SendRpc(RPC_RECEIVE_CURRENT_KILL_COUNT, Receivers.Others, currentKills);
	}

	public void SendTotalKillCount(int totalKills)
	{
		networkObject.SendRpc(RPC_RECEIVE_TOTAL_KILL_COUNT, Receivers.Others, totalKills);
	}

	public void SendPowerUpCount(int powerUps)
	{
		networkObject.SendRpc(RPC_RECEIVE_POWER_UP_COUNT, Receivers.Others, powerUps);
	}

	//===Monsters ===
	public void SendMonsterPositionUnreliable(Vector2 monsterPosition, double timePassedSinceLastPositionTotal, ushort enemyListIndex)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_MONSTER_POSITION, Receivers.Others, timePassedSinceLastPositionTotal, new Vector2(monsterPosition.x, monsterPosition.y), enemyListIndex);
	}

	public void SendMonsterFacingUnreliable(bool facingRight, ushort enemyListIndex)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_MONSTER_FACING, Receivers.Others, facingRight, enemyListIndex);
	}

	public void SendMonsterFacing(bool facingRight, ushort enemyListIndex)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_MONSTER_FACING, Receivers.Others, facingRight, enemyListIndex);
	}

	public void SendMonsterAnimationStart(byte animationID, ushort enemyListIndex)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_MONSTER_ANIMATION_START, Receivers.Others, animationID, enemyListIndex);
	}

	public void SendMonsterRangedAttack(Vector2 directionToTarget, Vector2 spawnPosition, ushort enemyListIndex)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_MONSTER_RANGED_ATTACK, Receivers.Others, directionToTarget, spawnPosition, enemyListIndex);
	}

	/*public void SendMonsterRangedAttack(ushort enemyListIndex)
	{
		networkObject.SendRpc(RPC_RECEIVE_MONSTER_RANGED_ATTACK, Receivers.Others, enemyListIndex);
	}*/

	public void SendHarpyRangedAttack(Vector2 directionToTarget, Vector2 spawnPosition, ushort enemyListIndex)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_HARPY_RANGED_ATTACK, Receivers.Others, directionToTarget, spawnPosition, enemyListIndex);
	}

	//Note this isn't RPC unreliable!
	public void SendClearSynchronizedMonsters()
	{
		networkObject.SendRpc(RPC_RECEIVE_CLEAR_SYNCHRONIZED_MONSTERS, Receivers.Others);
	}

	public void SendAddedSynchronizedMonsters(List<ushort> activatedEnemyListIndex)
	{
		//Debug.Log("Size is: " + activatedEnemyListIndex.ObjectToByteArray().Length);//206 for many tutorial enemies, which is normal serialization.
		networkObject.SendRpc(RPC_RECEIVE_ADDED_SYNCHRONIZED_MONSTERS, Receivers.Others, activatedEnemyListIndex.ObjectToByteArray());
	}

	public void SendManaKillmeter()
	{
		networkObject.SendRpc(RPC_RECEIVE_MANA_KILLMETER, Receivers.Others);
	}

	public void SendCurrentMana(int mana)
	{
		networkObject.SendRpc(RPC_RECEIVE_CURRENT_MANA, Receivers.Others, (byte) mana);
	}

	public void SendOverrideCurrentMana(int mana)
	{
		networkObject.SendRpc(RPC_RECEIVE_OVERRIDE_CURRENT_MANA, Receivers.Others, (byte) mana);
	}

	public void SendOverrideCurrentHealth(int currentHealth, int tempHP, int maxHealth)
	{
		networkObject.SendRpc(RPC_RECEIVE_OVERRIDE_CURRENT_HEALTH, Receivers.Others, (byte) currentHealth, (byte) tempHP, (byte) maxHealth);
	}

	//===Damage Share===
	public void SendTakeDamage(int currentHealth, int tempHP, int damageTaken)
	{
		networkObject.SendRpc(RPC_RECEIVE_TAKE_DAMAGE, Receivers.Others, currentHealth, tempHP, damageTaken);
	}

	public void SendMonsterPlayerKiller(ushort enemyListIndex)
	{
		networkObject.SendRpc(RPC_RECEIVE_MONSTER_PLAYERKILLER, Receivers.Others, enemyListIndex);
	}

	public void SendDesynchronize()
	{
		networkObject.SendRpc(RPC_RECEIVE_DESYNCHRONIZE, Receivers.All);
	}

	//===Synchronization===
	public void SendMageSynchronizeLink(int currentRunKills)
	{
		networkObject.SendRpc(RPC_RECEIVE_MAGE_SYNCHRONIZE_LINK, Receivers.Others, currentRunKills);
	}

	public void SendWarriorSynchronizeLink(Vector2 magePosition, int currentRunKills)
	{
		networkObject.SendRpc(RPC_RECEIVE_WARRIOR_SYNCHRONIZE_LINK, Receivers.Others, magePosition, currentRunKills);
	}

	public void SendSynchronizePowerUp(int currentHealth, int tempHealth, int currentRunKills, int totalKills)
	{
		networkObject.SendRpc(RPC_RECEIVE_SYNCHRONIZE_POWER_UP, Receivers.Others, currentHealth, tempHealth, currentRunKills, totalKills);
	}

	public void SendSynchronizeDeath(Vector2 warriorPosition)
	{
		networkObject.SendRpc(RPC_RECEIVE_SYNCHRONIZE_DEATH, Receivers.Others, warriorPosition);
	}

	public void SendOverridePosition(Vector2 warriorPosition)
	{
		networkObject.SendRpc(RPC_RECEIVE_OVERRIDE_POSITION, Receivers.Others, warriorPosition);
	}

	public void SendDialogueTriggered(int dialogueIndex, Vector2 warriorPosition, bool replyBack = false)
	{
		networkObject.SendRpc(RPC_RECEIVE_DIALOGUE_TRIGGERED, Receivers.Others, dialogueIndex, warriorPosition, replyBack);
	}

	public void SendLevelLoaded()
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_LOADED, Receivers.Others);
	}

	public void SendPillarPosition(Vector2 pillarPosition)
	{
		networkObject.SendRpc(RPC_RECEIVE_PILLAR_POSITION, Receivers.Others, pillarPosition);
	}

	public void SendMonsterKilled(ushort enemyListIndex)
	{
		networkObject.SendRpc(RPC_RECEIVE_MONSTER_KILLED, Receivers.Others, enemyListIndex);
	}

	public void SendDesyncedPosition(Vector2 warriorPosition, double timestamp)
	{
		networkObject.SendRpc(RPC_RECEIVE_DESYNCED_POSITION, Receivers.Others, timestamp, warriorPosition);
	}

	public void SendDesyncedSphereLink()
	{
		networkObject.SendRpc(RPC_RECEIVE_DESYNCED_SPHERE_LINK, Receivers.Others);
	}

	//===Level Editor===

	public void SendLevelEditorCreateStaticTile(byte tileType, ushort gridX, ushort gridY)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_CREATE_STATIC_TILE, Receivers.Others, tileType, gridX, gridY);
	}

	public void SendLevelEditorCreateStaticPlatform(byte tileType, ushort gridX1, ushort gridY, ushort gridX2)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_CREATE_STATIC_PLATFORM, Receivers.Others, tileType, gridX1, gridY, gridX2);
	}

	public void SendLevelEditorCreateFreeformTile(byte tileType, float gridX, float gridY)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_CREATE_FREEFORM_TILE, Receivers.Others, tileType, gridX, gridY);
	}

	public void SendLevelEditorRemoveStaticTile(byte gridType, ushort gridIndex)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_REMOVE_STATIC_TILE, Receivers.Others, gridType, gridIndex);
	}
	
	public void SendLevelEditorRemoveFreeformTile(ushort gridIndex)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_REMOVE_FREEFORM_TILE, Receivers.Others, gridIndex);
	}

	public void SendLevelEditorMoveFreeformTile(ushort gridIndex, float newX, float newY)
	{
		networkObject.SendRpcUnreliable(RPC_RECEIVE_LEVEL_EDITOR_MOVE_FREEFORM_TILE, Receivers.Others, gridIndex, newX, newY);
	}

	public void SendLevelEditorMoveFreeformTileFinalized(ushort gridIndex, float newX, float newY)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_MOVE_FREEFORM_TILE_FINALIZED, Receivers.Others, gridIndex, newX, newY);
	}

	public void SendLevelEditorConvertTileToFreeform(ushort gridIndex, byte gridType)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_CONVERT_TILE_TO_FREEFORM, Receivers.Others, gridIndex, gridType);
	}

	public void SendLevelEditorSave()
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_SAVE, Receivers.Others);
	}

	public void SendLevelEditorLoad(byte[] windLevel)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_LOAD, Receivers.Others, windLevel);
	}

	public void SendLevelEditorPlay()
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_PLAY, Receivers.Others);
	}

	public void SendLevelEditorStop()
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_STOP, Receivers.Others);
	}

	public void SendLevelEditorClearAllGrid()
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_EDITOR_CLEAR_ALL_GRID, Receivers.Others);
	}
	

	//===Story/Dialogue===
	public void SendInterruptionCount(int warriorInterruptions, int mageInterruptions)
	{
		networkObject.SendRpc(RPC_RECEIVE_INTERRUPTION_COUNT, Receivers.Others, warriorInterruptions, mageInterruptions);
	}

	public void SendLevelCutsceneIndex(int levelCutsceneIndex)
	{
		networkObject.SendRpc(RPC_RECEIVE_LEVEL_CUTSCENE_INDEX, Receivers.Others, levelCutsceneIndex);
	}

	//===Misc Init===
	public void SendRandomSeed(UnityEngine.Random.State randomSeedState)
	{
		networkObject.SendRpc(RPC_RECEIVE_RANDOM_SEED, Receivers.Others, randomSeedState.ObjectToByteArray());
	}

	public void SendMonstersChaseObstructedPlayer(List<bool> enemyChaseObstructedPlayerList)
	{
		networkObject.SendRpc(RPC_RECEIVE_MONSTERS_CHASE_OBSTRUCTED_PLAYER, Receivers.Others, enemyChaseObstructedPlayerList.ObjectToByteArray());
	}

	public void SendDeadMonsters(List<ushort> deadMonstersIndexList)
	{
		networkObject.SendRpc(RPC_RECEIVE_DEAD_MONSTERS, Receivers.Others, deadMonstersIndexList.ObjectToByteArray());
	}

	public void SendCurrentLevel()
	{
		networkObject.SendRpc(RPC_RECEIVE_CURRENT_LEVEL, Receivers.Others, (short)LevelManager.currentLevel);
	}

	//Receivers.All, so it triggers both this player and the other ;)
	public void SendCurrentLevelBoth()
	{
		networkObject.SendRpc(RPC_RECEIVE_CURRENT_LEVEL, Receivers.All, (short)LevelManager.currentLevel);
	}






	//================
	//===RPCs Below===
	//================

	//===Mage===

	public override void ReceiveMageJump(RpcArgs args)
	{
		if (ReceiveMageJumpEvent != null)
			ReceiveMageJumpEvent(args);
	}

	public override void ReceiveMageDodgeroll(RpcArgs args)
	{
		if (ReceiveMageDodgerollEvent != null)
			ReceiveMageDodgerollEvent(args);
	}

	public override void ReceiveMageSpellword(RpcArgs args)
	{
		if (ReceiveMageSpellwordEvent != null)
			ReceiveMageSpellwordEvent(args);
	}

	//===Warrior===

	public override void ReceiveWarriorRightJoystick(RpcArgs args)
	{
		if (ReceiveWarriorRightJoystickEvent != null)
			ReceiveWarriorRightJoystickEvent(args);
	}

	public override void ReceiveWarriorLeftJoystick(RpcArgs args)
	{
		if (ReceiveWarriorLeftJoystickEvent != null)
			ReceiveWarriorLeftJoystickEvent(args);
	}

	public override void ReceiveWarriorPosition(RpcArgs args)
	{
		if (ReceiveWarriorPositionEvent != null)
			ReceiveWarriorPositionEvent(args);
	}

	public override void ReceiveWarriorInputSnapshot(RpcArgs args)
	{
		if (ReceiveWarriorInputSnapshotEvent != null)
			ReceiveWarriorInputSnapshotEvent(args);
	}

	public override void ReceiveWarriorMenuToggled(RpcArgs args)
	{
		if (ReceiveWarriorMenuToggledEvent != null)
			ReceiveWarriorMenuToggledEvent(args);
	}

	public override void ReceiveDarkwindDistortionChange(RpcArgs args)
	{
		if (ReceiveDarkwindDistortionChangeEvent != null)
			ReceiveDarkwindDistortionChangeEvent(args);
	}

	//===Health===
	public override void ReceiveCurrentHealth(RpcArgs args)
	{
		if (ReceiveCurrentHealthEvent != null)
			ReceiveCurrentHealthEvent(args);
	}

	public override void ReceiveMaxHealth(RpcArgs args)
	{
		if (ReceiveMaxHealthEvent != null)
			ReceiveMaxHealthEvent(args);
	}

	//===DeathKills===
	public override void ReceiveDeathCount(RpcArgs args)
	{
		if (ReceiveDeathCountEvent != null)
			ReceiveDeathCountEvent(args);
	}

	public override void ReceiveCurrentKillCount(RpcArgs args)
	{
		if (ReceiveCurrentKillCountEvent != null)
			ReceiveCurrentKillCountEvent(args);
	}

	public override void ReceiveTotalKillCount(RpcArgs args)
	{
		if (ReceiveTotalKillCountEvent != null)
			ReceiveTotalKillCountEvent(args);
	}

	public override void ReceivePowerUpCount(RpcArgs args)
	{
		if (ReceivePowerUpCountEvent != null)
			ReceivePowerUpCountEvent(args);
	}

	//===Story/Dialogue===
	public override void ReceiveInterruptionCount(RpcArgs args)
	{
		if (ReceiveInterruptionCountEvent != null)
			ReceiveInterruptionCountEvent(args);
	}

	public override void ReceiveLevelCutsceneIndex(RpcArgs args)
	{
		if (ReceiveLevelCutsceneIndexEvent != null)
			ReceiveLevelCutsceneIndexEvent(args);
	}

	//===Monsters===
	public override void ReceiveMonsterPosition(RpcArgs args)
	{
		if (ReceiveMonsterPositionEvent != null)
			ReceiveMonsterPositionEvent(args);
	}

	public override void ReceiveMonsterFacing(RpcArgs args)
	{
		if (ReceiveMonsterFacingEvent != null)
			ReceiveMonsterFacingEvent(args);
	}

	public override void ReceiveMonsterAnimationStart(RpcArgs args)
	{
		if (ReceiveMonsterAnimationStartEvent != null)
			ReceiveMonsterAnimationStartEvent(args);
	}

	public override void ReceiveMonsterAttackAction(RpcArgs args)
	{
		if (ReceiveMonsterAttackActionEvent != null)
			ReceiveMonsterAttackActionEvent(args);
	}

	public override void ReceiveMonsterRangedAttack(RpcArgs args)
	{
		if (ReceiveMonsterRangedAttackEvent != null)
			ReceiveMonsterRangedAttackEvent(args);
	}

	public override void ReceiveHarpyRangedAttack(RpcArgs args)
	{
		if (ReceiveHarpyRangedAttackEvent != null)
			ReceiveHarpyRangedAttackEvent(args);
	}

	public override void ReceiveClearSynchronizedMonsters(RpcArgs args)
	{
		if (ReceiveClearSynchronizedMonstersEvent != null)
			ReceiveClearSynchronizedMonstersEvent(args);
	}

	public override void ReceiveAddedSynchronizedMonsters(RpcArgs args)
	{
		if (ReceiveAddedSynchronizedMonstersEvent != null)
			ReceiveAddedSynchronizedMonstersEvent(args);
	}

	public override void ReceiveManaKillmeter(RpcArgs args)
	{
		if (ReceiveManaKillmeterEvent != null)
			ReceiveManaKillmeterEvent(args);
	}

	public override void ReceiveCurrentMana(RpcArgs args)
	{
		if (ReceiveCurrentManaEvent != null)
			ReceiveCurrentManaEvent(args);
	}

	public override void ReceiveOverrideCurrentMana(RpcArgs args)
	{
		if (ReceiveOverrideCurrentManaEvent != null)
			ReceiveOverrideCurrentManaEvent(args);
	}

	public override void ReceiveOverrideCurrentHealth(RpcArgs args)
	{
		if (ReceiveOverrideCurrentHealthEvent != null)
			ReceiveOverrideCurrentHealthEvent(args);
	}

	//===Damage Share===
	public override void ReceiveTakeDamage(RpcArgs args)
	{
		if (ReceiveTakeDamageEvent != null)
			ReceiveTakeDamageEvent(args);
	}

	public override void ReceiveMonsterPlayerkiller(RpcArgs args)
	{
		if (ReceiveMonsterPlayerKillerEvent != null)
			ReceiveMonsterPlayerKillerEvent(args);
	}

	public override void ReceiveDesynchronize(RpcArgs args)
	{
		if (ReceiveDesynchronizeEvent != null)
			ReceiveDesynchronizeEvent(args);
	}

	//===Synchronization===
	public override void ReceiveMageSynchronizeLink(RpcArgs args)
	{
		if (ReceiveMageSynchronizeLinkEvent != null)
			ReceiveMageSynchronizeLinkEvent(args);
	}

	public override void ReceiveWarriorSynchronizeLink(RpcArgs args)
	{
		if (ReceiveWarriorSynchronizeLinkEvent != null)
			ReceiveWarriorSynchronizeLinkEvent(args);
	}

	public override void ReceiveSynchronizePowerUp(RpcArgs args)
	{
		if (ReceiveSynchronizePowerUpEvent != null)
			ReceiveSynchronizePowerUpEvent(args);
	}

	public override void ReceiveSynchronizeDeath(RpcArgs args)
	{
		if (ReceiveSynchronizeDeathEvent != null)
			ReceiveSynchronizeDeathEvent(args);
	}

	public override void ReceiveSynchronizeCutscene(RpcArgs args)
	{
		if (ReceiveSynchronizeCutsceneEvent != null)
			ReceiveSynchronizeCutsceneEvent(args);
	}

	public override void ReceiveOverridePosition(RpcArgs args)
	{
		if (ReceiveOverridePositionEvent != null)
			ReceiveOverridePositionEvent(args);
	}

	public override void ReceiveDialogueTriggered(RpcArgs args)//Vector2 WarriorPosition
	{
		if (ReceiveDialogueTriggeredEvent != null)
			ReceiveDialogueTriggeredEvent(args);
	}

	public override void ReceiveLevelLoaded(RpcArgs args)
	{
		if (ReceiveLevelLoadedEvent != null)
			ReceiveLevelLoadedEvent(args);
	}

	public override void ReceivePillarPosition(RpcArgs args)
	{
		if (ReceivePillarPositionEvent != null)
			ReceivePillarPositionEvent(args);
	}

	public override void ReceiveMonsterKilled(RpcArgs args)
	{
		if (ReceiveMonsterKilledEvent != null)
			ReceiveMonsterKilledEvent(args);
	}

	public override void ReceiveDesyncedPosition(RpcArgs args)
	{
		if (ReceiveDesyncedPositionEvent != null)
			ReceiveDesyncedPositionEvent(args);
	}

	public override void ReceiveDesyncedSphereLink(RpcArgs args)
	{
		if (ReceiveDesyncedSphereLinkEvent != null)
			ReceiveDesyncedSphereLinkEvent(args);
	}




	//===Level Editor===
	public override void ReceiveLevelEditorCreateStaticTile(RpcArgs args)
	{
		if (ReceiveLevelEditorCreateStaticTileEvent != null)
			ReceiveLevelEditorCreateStaticTileEvent(args);
	}

	public override void ReceiveLevelEditorCreateStaticPlatform(RpcArgs args)
	{
		if (ReceiveLevelEditorCreateStaticPlatformEvent != null)
			ReceiveLevelEditorCreateStaticPlatformEvent(args);
	}

	public override void ReceiveLevelEditorCreateFreeformTile(RpcArgs args)
	{
		if (ReceiveLevelEditorCreateFreeformTileEvent != null)
			ReceiveLevelEditorCreateFreeformTileEvent(args);
	}

	public override void ReceiveLevelEditorRemoveStaticTile(RpcArgs args)
	{
		if (ReceiveLevelEditorRemoveStaticTileEvent != null)
			ReceiveLevelEditorRemoveStaticTileEvent(args);
	}

	public override void ReceiveLevelEditorRemoveFreeformTile(RpcArgs args)
	{
		if (ReceiveLevelEditorRemoveFreeformTileEvent != null)
			ReceiveLevelEditorRemoveFreeformTileEvent(args);
	}

	public override void ReceiveLevelEditorMoveFreeformTile(RpcArgs args)
	{
		if (ReceiveLevelEditorMoveFreeformTileEvent != null)
			ReceiveLevelEditorMoveFreeformTileEvent(args);
	}

	public override void ReceiveLevelEditorMoveFreeformTileFinalized(RpcArgs args)
	{
		if (ReceiveLevelEditorMoveFreeformTileFinalizedEvent != null)
			ReceiveLevelEditorMoveFreeformTileFinalizedEvent(args);
	}

	public override void ReceiveLevelEditorConvertTileToFreeform(RpcArgs args)
	{
		if (ReceiveLevelEditorConvertTileToFreeformEvent != null)
			ReceiveLevelEditorConvertTileToFreeformEvent(args);
	}

	public override void ReceiveLevelEditorSave(RpcArgs args)
	{
		if (ReceiveLevelEditorSaveEvent != null)
			ReceiveLevelEditorSaveEvent(args);
	}

	public override void ReceiveLevelEditorLoad(RpcArgs args)
	{
		if (ReceiveLevelEditorLoadEvent != null)
			ReceiveLevelEditorLoadEvent(args);
	}

	public override void ReceiveLevelEditorPlay(RpcArgs args)
	{
		if (ReceiveLevelEditorPlayEvent != null)
			ReceiveLevelEditorPlayEvent(args);
	}

	public override void ReceiveLevelEditorStop(RpcArgs args)
	{
		if (ReceiveLevelEditorStopEvent != null)
			ReceiveLevelEditorStopEvent(args);
	}

	public override void ReceiveLevelEditorClearAllGrid(RpcArgs args)
	{
		if (ReceiveLevelEditorClearAllGridEvent != null)
			ReceiveLevelEditorClearAllGridEvent(args);
	}

	//===Misc Init===

	public override void ReceiveRandomSeed(RpcArgs args)
	{
		if (ReceiveRandomSeedEvent != null)
			ReceiveRandomSeedEvent(args);
	}

	public override void ReceiveMonstersChaseObstructedPlayer(RpcArgs args)
	{
		if (ReceiveMonstersChaseObstructedPlayerEvent != null)
			ReceiveMonstersChaseObstructedPlayerEvent(args);
	}

	public override void ReceiveDeadMonsters(RpcArgs args)
	{
		if (ReceiveDeadMonstersEvent != null)
			ReceiveDeadMonstersEvent(args);
	}

	public override void ReceiveLevelCutsceneStart(RpcArgs args)
	{

	}

	public override void ReceiveCurrentLevel(RpcArgs args)
	{
		if (ReceiveCurrentLevelEvent != null)
			ReceiveCurrentLevelEvent(args);
	}



















	//TODO: Unknown usage below
	public override void ReceiveMageSnapshot(RpcArgs args)
	{

	}

}