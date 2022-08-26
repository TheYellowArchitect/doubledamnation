using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[][\"bool\"][\"bool\"][\"string\"][\"Vector2\"][\"Vector2\"][\"int\"][\"byte[]\"][\"byte\"][\"bool\"][\"double\", \"Vector2\"][\"byte[]\"][\"byte[]\"][\"byte\", \"byte\", \"byte\"][\"int\"][\"int\"][\"int\"][\"int\"][\"int\"][\"int\", \"int\"][\"int\"][\"int\", \"int\", \"int\"][\"ushort\"][][\"int\"][\"Vector2\", \"int\"][\"int\", \"int\", \"int\", \"int\"][\"Vector2\"][][\"Vector2\"][\"int\", \"Vector2\", \"bool\"][][\"Vector2\"][\"ushort\"][\"double\", \"Vector2\"][][\"double\", \"Vector2\", \"ushort\"][\"bool\", \"ushort\"][\"byte\", \"ushort\"][\"byte\", \"ushort\"][\"Vector2\", \"Vector2\", \"ushort\"][\"Vector2\", \"Vector2\", \"ushort\"][][\"byte[]\"][\"byte\", \"ushort\", \"ushort\"][\"byte\", \"float\", \"float\"][\"byte\", \"ushort\"][\"ushort\"][\"ushort\", \"float\", \"float\"][][\"byte[]\"][][][\"byte\", \"ushort\", \"ushort\", \"ushort\"][][\"ushort\", \"byte\"][\"ushort\", \"float\", \"float\"][][\"byte\"][\"short\"][\"byte\"][\"byte\", \"byte\", \"byte\"][\"byte[]\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[][\"spacebarPressed\"][\"dodgerollRight\"][\"mageInputString\"][\"movementInputDirection\"][\"combatInputDirection\"][\"dialogueIndex\"][\"seedState\"][\"buttonClicked\"][\"isOpen\"][\"timestamp\", \"warriorPosition\"][\"receivedInputSnapshot\"][\"shouldMonsterChaseObstructedPlayer\"][\"currentHealth\", \"tempHealth\", \"maxHealth\"][\"maxHealth\"][\"totalDeaths\"][\"currentKills\"][\"totalKills\"][\"totalPowerUps\"][\"warriorInterruptions\", \"mageInteruptions\"][\"levelCutsceneDialogueIndex\"][\"currentHealth\", \"tempHealth\", \"damageTaken\"][\"enemyListIndex\"][][\"currentRunKills\"][\"newPosition\", \"currentRunKills\"][\"currentHealth\", \"tempHealth\", \"currentRunKills\", \"totalKills\"][\"warriorPosition\"][][\"warriorPosition\"][\"cutsceneIndex\", \"magePosition\", \"replyBack\"][][\"hostPillarPosition\"][\"enemyListIndex\"][\"timestamp\", \"warriorPosition\"][][\"timestamp\", \"monsterPosition\", \"enemyListIndex\"][\"facingRight\", \"enemyListIndex\"][\"animationToStart\", \"enemyListIndex\"][\"attackIndex\", \"enemyListIndex\"][\"directionToTarget\", \"spawnPosition\", \"enemyListIndex\"][\"directionToTarget\", \"spawnPosition\", \"enemyListIndex\"][][\"activatedEnemyList\"][\"placementType\", \"gridX\", \"gridY\"][\"placementType\", \"gridX\", \"gridY\"][\"gridEnum\", \"gridIndex\"][\"gridIndex\"][\"gridIndex\", \"gridX\", \"gridY\"][][\"windLevel\"][][][\"placementType\", \"gridX1\", \"gridY\", \"gridX2\"][][\"gridIndex\", \"gridEnum\"][\"gridIndex\", \"gridX\", \"gridY\"][][\"currentMana\"][\"currentLevel\"][\"currentMana\"][\"currentHealth\", \"tempHealth\", \"maxHealth\"][\"deadEnemyIndexList\"]]")]
	public abstract partial class NetworkCommunicationControllerBehavior : NetworkBehavior
	{
		public const byte RPC_RECEIVE_MAGE_SNAPSHOT = 0 + 5;
		public const byte RPC_RECEIVE_MAGE_JUMP = 1 + 5;
		public const byte RPC_RECEIVE_MAGE_DODGEROLL = 2 + 5;
		public const byte RPC_RECEIVE_MAGE_SPELLWORD = 3 + 5;
		public const byte RPC_RECEIVE_WARRIOR_LEFT_JOYSTICK = 4 + 5;
		public const byte RPC_RECEIVE_WARRIOR_RIGHT_JOYSTICK = 5 + 5;
		public const byte RPC_RECEIVE_LEVEL_CUTSCENE_START = 6 + 5;
		public const byte RPC_RECEIVE_RANDOM_SEED = 7 + 5;
		public const byte RPC_RECEIVE_DARKWIND_DISTORTION_CHANGE = 8 + 5;
		public const byte RPC_RECEIVE_WARRIOR_MENU_TOGGLED = 9 + 5;
		public const byte RPC_RECEIVE_WARRIOR_POSITION = 10 + 5;
		public const byte RPC_RECEIVE_WARRIOR_INPUT_SNAPSHOT = 11 + 5;
		public const byte RPC_RECEIVE_MONSTERS_CHASE_OBSTRUCTED_PLAYER = 12 + 5;
		public const byte RPC_RECEIVE_CURRENT_HEALTH = 13 + 5;
		public const byte RPC_RECEIVE_MAX_HEALTH = 14 + 5;
		public const byte RPC_RECEIVE_DEATH_COUNT = 15 + 5;
		public const byte RPC_RECEIVE_CURRENT_KILL_COUNT = 16 + 5;
		public const byte RPC_RECEIVE_TOTAL_KILL_COUNT = 17 + 5;
		public const byte RPC_RECEIVE_POWER_UP_COUNT = 18 + 5;
		public const byte RPC_RECEIVE_INTERRUPTION_COUNT = 19 + 5;
		public const byte RPC_RECEIVE_LEVEL_CUTSCENE_INDEX = 20 + 5;
		public const byte RPC_RECEIVE_TAKE_DAMAGE = 21 + 5;
		public const byte RPC_RECEIVE_MONSTER_PLAYERKILLER = 22 + 5;
		public const byte RPC_RECEIVE_DESYNCHRONIZE = 23 + 5;
		public const byte RPC_RECEIVE_MAGE_SYNCHRONIZE_LINK = 24 + 5;
		public const byte RPC_RECEIVE_WARRIOR_SYNCHRONIZE_LINK = 25 + 5;
		public const byte RPC_RECEIVE_SYNCHRONIZE_POWER_UP = 26 + 5;
		public const byte RPC_RECEIVE_SYNCHRONIZE_DEATH = 27 + 5;
		public const byte RPC_RECEIVE_SYNCHRONIZE_CUTSCENE = 28 + 5;
		public const byte RPC_RECEIVE_OVERRIDE_POSITION = 29 + 5;
		public const byte RPC_RECEIVE_DIALOGUE_TRIGGERED = 30 + 5;
		public const byte RPC_RECEIVE_LEVEL_LOADED = 31 + 5;
		public const byte RPC_RECEIVE_PILLAR_POSITION = 32 + 5;
		public const byte RPC_RECEIVE_MONSTER_KILLED = 33 + 5;
		public const byte RPC_RECEIVE_DESYNCED_POSITION = 34 + 5;
		public const byte RPC_RECEIVE_DESYNCED_SPHERE_LINK = 35 + 5;
		public const byte RPC_RECEIVE_MONSTER_POSITION = 36 + 5;
		public const byte RPC_RECEIVE_MONSTER_FACING = 37 + 5;
		public const byte RPC_RECEIVE_MONSTER_ANIMATION_START = 38 + 5;
		public const byte RPC_RECEIVE_MONSTER_ATTACK_ACTION = 39 + 5;
		public const byte RPC_RECEIVE_MONSTER_RANGED_ATTACK = 40 + 5;
		public const byte RPC_RECEIVE_HARPY_RANGED_ATTACK = 41 + 5;
		public const byte RPC_RECEIVE_CLEAR_SYNCHRONIZED_MONSTERS = 42 + 5;
		public const byte RPC_RECEIVE_ADDED_SYNCHRONIZED_MONSTERS = 43 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_CREATE_STATIC_TILE = 44 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_CREATE_FREEFORM_TILE = 45 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_REMOVE_STATIC_TILE = 46 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_REMOVE_FREEFORM_TILE = 47 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_MOVE_FREEFORM_TILE = 48 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_SAVE = 49 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_LOAD = 50 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_PLAY = 51 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_STOP = 52 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_CREATE_STATIC_PLATFORM = 53 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_CLEAR_ALL_GRID = 54 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_CONVERT_TILE_TO_FREEFORM = 55 + 5;
		public const byte RPC_RECEIVE_LEVEL_EDITOR_MOVE_FREEFORM_TILE_FINALIZED = 56 + 5;
		public const byte RPC_RECEIVE_MANA_KILLMETER = 57 + 5;
		public const byte RPC_RECEIVE_CURRENT_MANA = 58 + 5;
		public const byte RPC_RECEIVE_CURRENT_LEVEL = 59 + 5;
		public const byte RPC_RECEIVE_OVERRIDE_CURRENT_MANA = 60 + 5;
		public const byte RPC_RECEIVE_OVERRIDE_CURRENT_HEALTH = 61 + 5;
		public const byte RPC_RECEIVE_DEAD_MONSTERS = 62 + 5;
		
		public NetworkCommunicationControllerNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkCommunicationControllerNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("ReceiveMageSnapshot", ReceiveMageSnapshot);
			networkObject.RegisterRpc("ReceiveMageJump", ReceiveMageJump, typeof(bool));
			networkObject.RegisterRpc("ReceiveMageDodgeroll", ReceiveMageDodgeroll, typeof(bool));
			networkObject.RegisterRpc("ReceiveMageSpellword", ReceiveMageSpellword, typeof(string));
			networkObject.RegisterRpc("ReceiveWarriorLeftJoystick", ReceiveWarriorLeftJoystick, typeof(Vector2));
			networkObject.RegisterRpc("ReceiveWarriorRightJoystick", ReceiveWarriorRightJoystick, typeof(Vector2));
			networkObject.RegisterRpc("ReceiveLevelCutsceneStart", ReceiveLevelCutsceneStart, typeof(int));
			networkObject.RegisterRpc("ReceiveRandomSeed", ReceiveRandomSeed, typeof(byte[]));
			networkObject.RegisterRpc("ReceiveDarkwindDistortionChange", ReceiveDarkwindDistortionChange, typeof(byte));
			networkObject.RegisterRpc("ReceiveWarriorMenuToggled", ReceiveWarriorMenuToggled, typeof(bool));
			networkObject.RegisterRpc("ReceiveWarriorPosition", ReceiveWarriorPosition, typeof(double), typeof(Vector2));
			networkObject.RegisterRpc("ReceiveWarriorInputSnapshot", ReceiveWarriorInputSnapshot, typeof(byte[]));
			networkObject.RegisterRpc("ReceiveMonstersChaseObstructedPlayer", ReceiveMonstersChaseObstructedPlayer, typeof(byte[]));
			networkObject.RegisterRpc("ReceiveCurrentHealth", ReceiveCurrentHealth, typeof(byte), typeof(byte), typeof(byte));
			networkObject.RegisterRpc("ReceiveMaxHealth", ReceiveMaxHealth, typeof(int));
			networkObject.RegisterRpc("ReceiveDeathCount", ReceiveDeathCount, typeof(int));
			networkObject.RegisterRpc("ReceiveCurrentKillCount", ReceiveCurrentKillCount, typeof(int));
			networkObject.RegisterRpc("ReceiveTotalKillCount", ReceiveTotalKillCount, typeof(int));
			networkObject.RegisterRpc("ReceivePowerUpCount", ReceivePowerUpCount, typeof(int));
			networkObject.RegisterRpc("ReceiveInterruptionCount", ReceiveInterruptionCount, typeof(int), typeof(int));
			networkObject.RegisterRpc("ReceiveLevelCutsceneIndex", ReceiveLevelCutsceneIndex, typeof(int));
			networkObject.RegisterRpc("ReceiveTakeDamage", ReceiveTakeDamage, typeof(int), typeof(int), typeof(int));
			networkObject.RegisterRpc("ReceiveMonsterPlayerkiller", ReceiveMonsterPlayerkiller, typeof(ushort));
			networkObject.RegisterRpc("ReceiveDesynchronize", ReceiveDesynchronize);
			networkObject.RegisterRpc("ReceiveMageSynchronizeLink", ReceiveMageSynchronizeLink, typeof(int));
			networkObject.RegisterRpc("ReceiveWarriorSynchronizeLink", ReceiveWarriorSynchronizeLink, typeof(Vector2), typeof(int));
			networkObject.RegisterRpc("ReceiveSynchronizePowerUp", ReceiveSynchronizePowerUp, typeof(int), typeof(int), typeof(int), typeof(int));
			networkObject.RegisterRpc("ReceiveSynchronizeDeath", ReceiveSynchronizeDeath, typeof(Vector2));
			networkObject.RegisterRpc("ReceiveSynchronizeCutscene", ReceiveSynchronizeCutscene);
			networkObject.RegisterRpc("ReceiveOverridePosition", ReceiveOverridePosition, typeof(Vector2));
			networkObject.RegisterRpc("ReceiveDialogueTriggered", ReceiveDialogueTriggered, typeof(int), typeof(Vector2), typeof(bool));
			networkObject.RegisterRpc("ReceiveLevelLoaded", ReceiveLevelLoaded);
			networkObject.RegisterRpc("ReceivePillarPosition", ReceivePillarPosition, typeof(Vector2));
			networkObject.RegisterRpc("ReceiveMonsterKilled", ReceiveMonsterKilled, typeof(ushort));
			networkObject.RegisterRpc("ReceiveDesyncedPosition", ReceiveDesyncedPosition, typeof(double), typeof(Vector2));
			networkObject.RegisterRpc("ReceiveDesyncedSphereLink", ReceiveDesyncedSphereLink);
			networkObject.RegisterRpc("ReceiveMonsterPosition", ReceiveMonsterPosition, typeof(double), typeof(Vector2), typeof(ushort));
			networkObject.RegisterRpc("ReceiveMonsterFacing", ReceiveMonsterFacing, typeof(bool), typeof(ushort));
			networkObject.RegisterRpc("ReceiveMonsterAnimationStart", ReceiveMonsterAnimationStart, typeof(byte), typeof(ushort));
			networkObject.RegisterRpc("ReceiveMonsterAttackAction", ReceiveMonsterAttackAction, typeof(byte), typeof(ushort));
			networkObject.RegisterRpc("ReceiveMonsterRangedAttack", ReceiveMonsterRangedAttack, typeof(Vector2), typeof(Vector2), typeof(ushort));
			networkObject.RegisterRpc("ReceiveHarpyRangedAttack", ReceiveHarpyRangedAttack, typeof(Vector2), typeof(Vector2), typeof(ushort));
			networkObject.RegisterRpc("ReceiveClearSynchronizedMonsters", ReceiveClearSynchronizedMonsters);
			networkObject.RegisterRpc("ReceiveAddedSynchronizedMonsters", ReceiveAddedSynchronizedMonsters, typeof(byte[]));
			networkObject.RegisterRpc("ReceiveLevelEditorCreateStaticTile", ReceiveLevelEditorCreateStaticTile, typeof(byte), typeof(ushort), typeof(ushort));
			networkObject.RegisterRpc("ReceiveLevelEditorCreateFreeformTile", ReceiveLevelEditorCreateFreeformTile, typeof(byte), typeof(float), typeof(float));
			networkObject.RegisterRpc("ReceiveLevelEditorRemoveStaticTile", ReceiveLevelEditorRemoveStaticTile, typeof(byte), typeof(ushort));
			networkObject.RegisterRpc("ReceiveLevelEditorRemoveFreeformTile", ReceiveLevelEditorRemoveFreeformTile, typeof(ushort));
			networkObject.RegisterRpc("ReceiveLevelEditorMoveFreeformTile", ReceiveLevelEditorMoveFreeformTile, typeof(ushort), typeof(float), typeof(float));
			networkObject.RegisterRpc("ReceiveLevelEditorSave", ReceiveLevelEditorSave);
			networkObject.RegisterRpc("ReceiveLevelEditorLoad", ReceiveLevelEditorLoad, typeof(byte[]));
			networkObject.RegisterRpc("ReceiveLevelEditorPlay", ReceiveLevelEditorPlay);
			networkObject.RegisterRpc("ReceiveLevelEditorStop", ReceiveLevelEditorStop);
			networkObject.RegisterRpc("ReceiveLevelEditorCreateStaticPlatform", ReceiveLevelEditorCreateStaticPlatform, typeof(byte), typeof(ushort), typeof(ushort), typeof(ushort));
			networkObject.RegisterRpc("ReceiveLevelEditorClearAllGrid", ReceiveLevelEditorClearAllGrid);
			networkObject.RegisterRpc("ReceiveLevelEditorConvertTileToFreeform", ReceiveLevelEditorConvertTileToFreeform, typeof(ushort), typeof(byte));
			networkObject.RegisterRpc("ReceiveLevelEditorMoveFreeformTileFinalized", ReceiveLevelEditorMoveFreeformTileFinalized, typeof(ushort), typeof(float), typeof(float));
			networkObject.RegisterRpc("ReceiveManaKillmeter", ReceiveManaKillmeter);
			networkObject.RegisterRpc("ReceiveCurrentMana", ReceiveCurrentMana, typeof(byte));
			networkObject.RegisterRpc("ReceiveCurrentLevel", ReceiveCurrentLevel, typeof(short));
			networkObject.RegisterRpc("ReceiveOverrideCurrentMana", ReceiveOverrideCurrentMana, typeof(byte));
			networkObject.RegisterRpc("ReceiveOverrideCurrentHealth", ReceiveOverrideCurrentHealth, typeof(byte), typeof(byte), typeof(byte));
			networkObject.RegisterRpc("ReceiveDeadMonsters", ReceiveDeadMonsters, typeof(byte[]));

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId)){
					uint newId = obj.NetworkId + 1;
					ProcessOthers(gameObject.transform, ref newId);
				}
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new NetworkCommunicationControllerNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkCommunicationControllerNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMageSnapshot(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMageJump(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMageDodgeroll(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMageSpellword(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveWarriorLeftJoystick(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveWarriorRightJoystick(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelCutsceneStart(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveRandomSeed(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDarkwindDistortionChange(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveWarriorMenuToggled(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveWarriorPosition(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveWarriorInputSnapshot(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonstersChaseObstructedPlayer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveCurrentHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMaxHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDeathCount(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveCurrentKillCount(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveTotalKillCount(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceivePowerUpCount(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveInterruptionCount(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelCutsceneIndex(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveTakeDamage(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterPlayerkiller(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDesynchronize(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMageSynchronizeLink(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveWarriorSynchronizeLink(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveSynchronizePowerUp(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveSynchronizeDeath(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveSynchronizeCutscene(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveOverridePosition(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDialogueTriggered(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelLoaded(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceivePillarPosition(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterKilled(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDesyncedPosition(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDesyncedSphereLink(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterPosition(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterFacing(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterAnimationStart(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterAttackAction(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveMonsterRangedAttack(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveHarpyRangedAttack(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveClearSynchronizedMonsters(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveAddedSynchronizedMonsters(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorCreateStaticTile(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorCreateFreeformTile(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorRemoveStaticTile(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorRemoveFreeformTile(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorMoveFreeformTile(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorSave(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorLoad(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorPlay(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorStop(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorCreateStaticPlatform(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorClearAllGrid(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorConvertTileToFreeform(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveLevelEditorMoveFreeformTileFinalized(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveManaKillmeter(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveCurrentMana(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveCurrentLevel(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveOverrideCurrentMana(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveOverrideCurrentHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceiveDeadMonsters(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}