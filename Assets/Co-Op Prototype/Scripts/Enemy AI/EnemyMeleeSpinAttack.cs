using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyMeleeSpinAttack : BaseEnemyAttack
{

    public override void StartAttack()
    {
        base.StartAttack();

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.MELEE_ATTACK2_CHARGE;
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
        commonBehaviour.targetAnimationState = EnemyBehaviour.MELEE_ATTACK2;
        commonBehaviour.NewStateTransition();

        colliders = Physics2D.OverlapAreaAll(transform.position + attackHitboxTopLeft, transform.position + attackHitboxBottomRight, WhatIsPlayer);//lmao, it type-casts Vector3->Vector2 automatically, dis unity! feelsgoodman

        Debug.DrawLine(transform.position + attackHitboxTopLeft, transform.position + attackHitboxBottomRight, Color.white, 2f);

        //Debug.Log("Collider length is: " + colliders.Length);

        //Checking every collider.
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)//Found player's collider.
                Damage(colliders[i].gameObject);
        }

        Debug.Log("Completed the attack");

        finishAttackActive = true;
        //Invoke("FinishAttack", recoveryTime);
    }
}
