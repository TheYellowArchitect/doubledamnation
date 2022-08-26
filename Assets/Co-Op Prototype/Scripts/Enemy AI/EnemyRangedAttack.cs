using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyRangedAttack : BaseEnemyAttack
{
    [Header("Ranged Attack Properties")]
    [Required]
    [Tooltip("The projectile to \"spawn\" upon attack.")]
    public GameObject prefabProjectile;

    //So it won't have anything inside.(hitboxes :P )
    public override void Start()
    {
        /*
        commonBehaviour = GetComponent<EnemyBehaviour>();
        if (commonBehaviour == null)
            Debug.LogError("No behaviour detected on this enemy");
        */
    }
    

    public override void StartAttack()
    {
        base.StartAttack();

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.RANGED_ATTACK_CHARGE;
        commonBehaviour.NewStateTransition();
    }

    public override void CompleteAttack()
    {
        if (interrupted)
        {
            ResetAttackTimers();
            return;
        }

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.RANGED_ATTACK;
        commonBehaviour.NewStateTransition();

        GameObject tempProjectile = Instantiate(prefabProjectile);
        tempProjectile.GetComponent<ProjectileDamageTrigger>().InitializeValues(commonBehaviour.GetDirectionToPlayer(), transform.position, (short) commonBehaviour.enemyListIndex);

        Debug.Log("Completed the Ranged Attack");

        finishAttackActive = true;
        //Invoke("FinishAttack", recoveryTime);

        //If online and host and synchronized, send the ranged attack to client
        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() && NetworkDamageShare.globalInstance.IsSynchronized())
            NetworkCommunicationController.globalInstance.SendMonsterRangedAttack(commonBehaviour.GetDirectionToPlayer(), transform.position, commonBehaviour.enemyListIndex);
    }

    //Invoked by NetworkEnemyActionController
    public void CompleteNetworkClientAttack(Vector2 directionToPlayer, Vector2 spawnPosition)
    {
        GameObject tempProjectile = Instantiate(prefabProjectile);
        tempProjectile.GetComponent<ProjectileDamageTrigger>().InitializeValues(directionToPlayer, spawnPosition, (short) commonBehaviour.enemyListIndex);
    }


    //Shows the radius
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Draw attack range
        Handles.color = new Color(1.0f, 0, 0, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, attackRange);
    }
    #endif
}
