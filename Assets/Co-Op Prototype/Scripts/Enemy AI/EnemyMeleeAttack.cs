using UnityEngine;

[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyMeleeAttack : BaseEnemyAttack
{

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

        if (commonBehaviour.GetFacingRight())
        {
            tempPoint1 = new Vector2(0f, attackHitboxTopLeft.y);
            colliders = Physics2D.OverlapAreaAll(transform.position + tempPoint1, transform.position + attackHitboxBottomRight, WhatIsPlayer);//lmao, it type-casts Vector3->Vector2 automatically, dis unity! feelsgoodman

            Debug.DrawLine(transform.position + tempPoint1, transform.position + attackHitboxBottomRight, Color.white, 2f);
        }
        else//facingLeft
        {
            tempPoint1 = new Vector2(0f, attackHitboxBottomRight.y);
            colliders = Physics2D.OverlapAreaAll(transform.position + attackHitboxTopLeft, transform.position + tempPoint1, WhatIsPlayer);//lmao, it type-casts Vector3->Vector2 automatically, dis unity! feelsgoodman

            Debug.DrawLine(transform.position + attackHitboxTopLeft, transform.position + tempPoint1, Color.white, 2f);
        }

        //Debug.Log("Collider length is: " + colliders.Length);

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
