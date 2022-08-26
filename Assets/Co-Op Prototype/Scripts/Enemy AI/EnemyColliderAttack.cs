using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unlike the good, but ultimately flawed system that doesn't take rotation in mind, this uses a collider for rotation. In the future to be expanded in attackDetection and attackDamage hitboxes, via pre-existing colliders.
public class EnemyColliderAttack : BaseEnemyAttack
{
    [Header("Trigger Colliders")]
    //public Collider2D detectionCollider;//For now, unused.
    public Collider2D damageCollider;

    private ContactFilter2D filteringPlayer = new ContactFilter2D();



    //Original just calculated the hitboxes. But we don't need that here.
    public override void Start()
    {
        filteringPlayer = new ContactFilter2D();
        filteringPlayer.SetLayerMask(WhatIsPlayer);
    }

    public override void Update()
    {
        UpdateTimers();
    }

    public override void StartAttack()
    {
        base.StartAttack();

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.MELEE_ATTACK_CHARGE;
        commonBehaviour.NewStateTransition();
    }


    //Basic Melee Attack
    public override void CompleteAttack()
    {
        if (interrupted)
        {
            ResetAttackTimers();
            return;
        }

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.MELEE_ATTACK;
        commonBehaviour.NewStateTransition();

        damageCollider.OverlapCollider(filteringPlayer, colliders);

        if (colliders != null)
            //Checking every collider.
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)//Found player's collider.
                    Damage(colliders[i].gameObject);
            }

        Debug.Log("Completed the attack");

        //So it starts FinishAttack timer. (see UpdateTimers())
        finishAttackActive = true;
        //Invoke("FinishAttack", recoveryTime);
    }
}
