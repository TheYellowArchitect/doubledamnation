using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using NaughtyAttributes;

public class NetworkEnemyAnimationOverrideController : MonoBehaviour 
{
	public static NetworkEnemyAnimationOverrideController globalInstance;

	[ReadOnly]
	public bool activated = false;

	[Header("Previous animation")]
	public Dictionary<ushort, int> previousAnimationHash = new Dictionary<ushort, int>();
	public Dictionary<ushort, int> currentAnimationHash = new Dictionary<ushort, int>();

	//Animator Animation Hashes
	//Godbless https://stackoverflow.com/questions/34846287/get-name-of-current-animation-state
	public static int idleID = Animator.StringToHash("Idle");
	public static int runID = Animator.StringToHash("Run");
	public static int deathID = Animator.StringToHash("Death");//Doesn't really exist, since death animation is actually hitground/midair lol
	public static int hitMidairID = Animator.StringToHash("Hit Midair");
	public static int hitGroundID = Animator.StringToHash("Hit Ground");
	public static int jumpChargeID = Animator.StringToHash("Jump Charge");
	public static int jumpID = Animator.StringToHash("Jump");
	public static int fallingID = Animator.StringToHash("Falling");
	
	public static int meleeAttackID = Animator.StringToHash("Melee Attack");
	public static int meleeAttackChargeID = Animator.StringToHash("Melee Attack Charge");
	public static int rangedAttackID = Animator.StringToHash("Ranged Attack");
	public static int rangedAttackChargeID = Animator.StringToHash("Ranged Attack Charge");
	public static int meleeAttack2ID = Animator.StringToHash("Melee Attack2");
	public static int meleeAttack2ChargeID = Animator.StringToHash("Melee Attack2 Charge");
	public static int ramAttackID = Animator.StringToHash("Ram Attack");
	public static int ramAttackChargeID = Animator.StringToHash("Ram Attack Charge");

	public const byte IDLE = 0, RUN = 1, JUMP = 2, JUMPCHARGE = 3, FALLING = 4, HITMIDAIR = 5, HITGROUND = 6, MELEEATTACK1 = 7, MELEEATTACK1CHARGE = 8, RANGEDATTACK = 9, RANGEDATTACKCHARGE = 10, MELEEATTACK2 = 11, MELEEATTACK2CHARGE = 12, RAMATTACK = 13, RAMATTACKCHARGE = 14;

	public List<ushort> activatedEnemyListIndex;
	public bool isHost = false;
	public Dictionary<ushort, Animator> activatedAnimatorDictionary = new Dictionary<ushort, Animator>();

	public void Start()
	{
		globalInstance = this;
	}

	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		PrintAnimationHashes();

		isHost = NetworkCommunicationController.globalInstance.IsServer();

		//Register to animation updates if client
		if (isHost == false)
			NetworkCommunicationController.globalInstance.ReceiveMonsterAnimationStartEvent += OverrideAnimation;
	}

	public void DeActivate()
	{
		activated = false;

		if (isHost == false)
			NetworkCommunicationController.globalInstance.ReceiveMonsterAnimationStartEvent -= OverrideAnimation;
	}

	//Host
	public void Update()
	{
		//If online and host, check each frame for new animations!
		if (activated && activatedEnemyListIndex.Count > 0 && isHost && NetworkDamageShare.globalInstance.IsSynchronized())
		{
			for (int i = 0; i < activatedEnemyListIndex.Count; i++)
			{
				//"Animator is not playing an AnimatorController" bugfix (the inactive enemy should be cleared next frame by other functions)
				//It would also lead to an error, since the animationHash is 0
				if (LevelManager.globalInstance.enemyList[activatedEnemyListIndex[i]] == null || LevelManager.globalInstance.enemyList[activatedEnemyListIndex[i]].activeSelf == false)
					continue;

				currentAnimationHash[activatedEnemyListIndex[i]] = activatedAnimatorDictionary[activatedEnemyListIndex[i]].GetCurrentAnimatorStateInfo(0).shortNameHash;

				//Animation changed
				if (currentAnimationHash[activatedEnemyListIndex[i]] != previousAnimationHash[activatedEnemyListIndex[i]])
					NetworkCommunicationController.globalInstance.SendMonsterAnimationStart( AnimationHashToByte(currentAnimationHash[activatedEnemyListIndex[i]]), activatedEnemyListIndex[i]);

				previousAnimationHash[activatedEnemyListIndex[i]] = currentAnimationHash[activatedEnemyListIndex[i]];
			}
		}
	}

	//Client
	public void OverrideAnimation(RpcArgs args)
	{
		byte receivedAnimationID = args.GetNext<byte>();
		ushort receivedMonsterIndex = args.GetNext<ushort>();

		//Edge-case where animator is dead or sth but hasnt been removed yet idk, but it does happen once in a while
		if (activatedAnimatorDictionary.ContainsKey(receivedMonsterIndex) == false)
			return;

		activatedAnimatorDictionary[receivedMonsterIndex].Play(ByteToHash(receivedAnimationID));
	}

	public void AddAnimator(ushort enemyIndex)
	{
		if (LevelManager.globalInstance.GetEnemyAnimatorFromIndex(enemyIndex) != null)
		{
			activatedAnimatorDictionary.Add(enemyIndex, LevelManager.globalInstance.GetEnemyAnimatorFromIndex(enemyIndex));
			previousAnimationHash.Add(enemyIndex, idleID);
			currentAnimationHash.Add(enemyIndex, idleID);
		}
	}

	public void RemoveAnimator(ushort enemyIndex)
	{
		if (previousAnimationHash.ContainsKey(enemyIndex))
		{
			previousAnimationHash.Remove(enemyIndex);
			currentAnimationHash.Remove(enemyIndex);
			activatedAnimatorDictionary.Remove(enemyIndex);
		}
	}

	public byte AnimationHashToByte(int animationHash)
	{
		if (animationHash == idleID)
			return IDLE;
		else if (animationHash == runID)
			return RUN;
		else if (animationHash == hitMidairID)
			return HITMIDAIR;
		else if (animationHash == hitGroundID)
			return HITGROUND;
		else if (animationHash == jumpChargeID)
			return JUMPCHARGE;
		else if (animationHash == jumpID)
			return JUMP;
		else if (animationHash == fallingID)
			return FALLING;
		else if (animationHash == meleeAttackID)
			return MELEEATTACK1;
		else if (animationHash == meleeAttackChargeID)
			return MELEEATTACK1CHARGE;
		else if (animationHash == rangedAttackID)
			return RANGEDATTACK;
		else if (animationHash == rangedAttackChargeID)
			return RANGEDATTACKCHARGE;
		else if (animationHash == meleeAttack2ID)
			return MELEEATTACK2;
		else if (animationHash == meleeAttack2ChargeID)
			return MELEEATTACK2CHARGE;
		else if (animationHash == ramAttackID)
			return RAMATTACK;
		else if (animationHash == ramAttackChargeID)
			return RAMATTACKCHARGE;
		else
		{
			Debug.LogError("UNEXPECTED STATE: " + animationHash);
			PrintAnimationHashes();
			return FALLING;
		}
	}

	public int ByteToHash(byte constantByte)
	{
		if (constantByte == IDLE)
			return idleID;
		else if (constantByte == RUN)
			return runID;
		else if (constantByte == HITMIDAIR)
			return hitMidairID;
		else if (constantByte == HITGROUND)
			return hitGroundID;
		else if (constantByte == JUMPCHARGE)
			return jumpChargeID;
		else if (constantByte == JUMP)
			return jumpID;
		else if (constantByte == FALLING)
			return fallingID;
		else if (constantByte == MELEEATTACK1)
			return meleeAttackID;
		else if (constantByte == MELEEATTACK1CHARGE)
			return meleeAttackChargeID;
		else if (constantByte == RANGEDATTACK)
			return rangedAttackID;
		else if (constantByte == RANGEDATTACKCHARGE)
			return rangedAttackChargeID;
		else if (constantByte == MELEEATTACK2)
			return meleeAttack2ID;
		else if (constantByte == MELEEATTACK2CHARGE)
			return meleeAttack2ChargeID;
		else if (constantByte == RAMATTACK)
			return ramAttackID;
		else if (constantByte == RAMATTACKCHARGE)
			return ramAttackChargeID;
		else
		{
			Debug.LogError("UNEXPECTED BYTE STATE: " + constantByte);
			return fallingID;
		}
	}

	public void PrintAnimationHashes()
	{
		Debug.Log("============================");
		Debug.Log("IdleID: " + idleID);
		Debug.Log("RunID: " + runID);
		Debug.Log("HitMidairID: " + hitMidairID);
		Debug.Log("HitGroundID: " + hitGroundID);
		Debug.Log("JumpChargeID: " + jumpChargeID);
		Debug.Log("JumpID: " + jumpID);
		Debug.Log("FallingID: " + fallingID);
		Debug.Log("MeleeAttackID: " + meleeAttackID);
		Debug.Log("MeleeAttackChargeID: " + meleeAttackChargeID);
		Debug.Log("RangedAttackID: " + rangedAttackID);
		Debug.Log("RangedAttackChargeID: " + rangedAttackChargeID);
		Debug.Log("MeleeAttack2ID: " + meleeAttack2ID);
		Debug.Log("MeleeAttack2ChargeID: " + meleeAttack2ChargeID);
		Debug.Log("RamAttackID: " + ramAttackID);
		Debug.Log("RamAttackChargeID: " + ramAttackChargeID);
		Debug.Log("============================");
	}
}
