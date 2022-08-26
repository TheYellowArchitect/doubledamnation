using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

//RESPONSIBLE FOR FLIP AND RANGEDATTACKS
public class NetworkEnemyActionController : MonoBehaviour 
{
	public static NetworkEnemyActionController globalInstance;

	private NetworkEnemyPositionInterpolationController commonNetworkEnemyPositionInterpolationController;
	private EnemyBehaviour tempEnemyBehaviour;
	private EnemyRangedAttack tempEnemyRangedAttack;
	private HarpyAuraAttack tempHarpyRangedAttack;

	public List<ushort> activatedEnemyListIndex;
	
	void Start () 
	{
		globalInstance = this;

		commonNetworkEnemyPositionInterpolationController = GetComponent<NetworkEnemyPositionInterpolationController>();
		activatedEnemyListIndex = commonNetworkEnemyPositionInterpolationController.activatedEnemyListIndex;
	}

	public void Activate()
	{
		NetworkCommunicationController.globalInstance.ReceiveMonsterFacingEvent += DetermineFacingFlip;
		NetworkCommunicationController.globalInstance.ReceiveMonsterRangedAttackEvent += FinishedRangeAttack;
		NetworkCommunicationController.globalInstance.ReceiveHarpyRangedAttackEvent += FinishedHarpyRangeAttack;
	}

	public void DeActivate()
	{
		NetworkCommunicationController.globalInstance.ReceiveMonsterFacingEvent -= DetermineFacingFlip;
		NetworkCommunicationController.globalInstance.ReceiveMonsterRangedAttackEvent -= FinishedRangeAttack;
		NetworkCommunicationController.globalInstance.ReceiveHarpyRangedAttackEvent -= FinishedHarpyRangeAttack;
	}

	//Sent at the start of connection on NetworkSynchronizer.Activate()
	public void SendAllMonstersFacing()
	{
		for (int i = 0; i < activatedEnemyListIndex.Count; i++)
			NetworkCommunicationController.globalInstance.SendMonsterFacingUnreliable(LevelManager.globalInstance.GetEnemyBehaviourFromIndex(activatedEnemyListIndex[i]).GetFacingRight(), activatedEnemyListIndex[i]);
	}

	public void DetermineFacingFlip(RpcArgs args)
	{
		bool receivedFacingRight = args.GetNext<bool>();
		ushort receivedMonsterIndex = args.GetNext<ushort>();

		//Iterate among all cached enemies, to confirm the index exists in the first place
		bool alreadyRegistered = false;
		int i = 0;
		for (; i < activatedEnemyListIndex.Count; i++)
		{
			if (activatedEnemyListIndex[i] == receivedMonsterIndex)
			{
				alreadyRegistered = true;
				break;
			}
		}

		//If not registered, register it
		if (alreadyRegistered == false)
		{
			commonNetworkEnemyPositionInterpolationController.AddEnemyIndex(receivedMonsterIndex, LevelManager.globalInstance.GetEnemyRigidbodyFromIndex(receivedMonsterIndex));
			alreadyRegistered = true;
			i = activatedEnemyListIndex.Count - 1;
		}

		tempEnemyBehaviour = LevelManager.globalInstance.GetEnemyBehaviourFromIndex(activatedEnemyListIndex[i]);

		if (tempEnemyBehaviour != null)
		{
			//If facing received, is opposite of current
			if ((tempEnemyBehaviour.GetFacingRight() == true && receivedFacingRight == false) || (tempEnemyBehaviour.GetFacingRight() == false && receivedFacingRight == true))
				tempEnemyBehaviour.FlipSprite();
		}
		//else, remove enemybehaviour? When is it null? dead enemy or straight up non-existent?

		
	}
	
	public void FinishedRangeAttack(RpcArgs args)
	{
		Vector2 receivedDirectionToTarget = args.GetNext<Vector2>();
		Vector2 receivedSpawnPosition = args.GetNext<Vector2>();
		ushort receivedMonsterIndex = args.GetNext<ushort>();

		tempEnemyRangedAttack = LevelManager.globalInstance.enemyList[receivedMonsterIndex].GetComponent<EnemyRangedAttack>();
		if (tempEnemyRangedAttack != null)
			tempEnemyRangedAttack.CompleteNetworkClientAttack(receivedDirectionToTarget, receivedSpawnPosition);
	}

	public void FinishedHarpyRangeAttack(RpcArgs args)
	{
		Vector2 receivedDirectionToTarget = args.GetNext<Vector2>();
		Vector2 receivedSpawnPosition = args.GetNext<Vector2>();
		ushort receivedMonsterIndex = args.GetNext<ushort>();

		tempHarpyRangedAttack = LevelManager.globalInstance.enemyList[receivedMonsterIndex].GetComponentInChildren<HarpyAuraAttack>();
		if (tempHarpyRangedAttack != null)
			tempHarpyRangedAttack.ShootProjectile(receivedDirectionToTarget, receivedSpawnPosition);
	}
}
