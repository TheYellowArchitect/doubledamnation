using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class got pivoted af in game design.
// It was supposed to stomp the ground, and hit everyone around the stomped area, which it did for most of the development and some of the Alpha.
// But it is bullshit tbh. No VFX, and at least, it should push everyone on the sides, aka 0 dmg and apply force, not fucking hit them.
// Hence, I just reduced the range...
// Original Satyr hitboxes: 1.47 instead of 3.5
// Original Minotaur hitboxes: 2.85 instead of 5, with offsetX of 0.75 instead of 0
// Hopefully in the future I will have the courage to touch the enemy spagghetti and make stomp as it should optimally be.

[RequireComponent(typeof(EnemyBehaviour))]
[RequireComponent(typeof(EnemyJump))]
public class EnemyJumpStompAttack : BaseEnemyAttack
{
    #region dlt dis
    /*
    //Calculate the hitboxes
    attackHitboxTopLeft = new Vector2(attackRange * -1, attackRangeY + offsetFromOriginY);
    attackHitboxBottomRight = new Vector2(attackRange, -1 * attackRangeY + offsetFromOriginY);

    commonBehaviour = GetComponent<EnemyBehaviour>();
    if (commonBehaviour == null)
        Debug.LogError("No behaviour detected on this enemy");

    whatIsPlayer = commonBehaviour.WhatIsPlayer;

}
*/

    /*
    public override void StartAttack()
    {
        //TODO: Play "Attack" Animation

        Debug.Log("Started jump-attacking...");

        Invoke("CompleteAttack", castingTime);

        //Jump part

            //Play StartJumpAnimation?

            applyJumpPower = false;
            currentJumpPower = 0;
            currentJumpPowerDiminisher = JumpPowerDiminisher;
            rising = true;

            commonBehaviour.SetIsJumping(true);

            Invoke("CompleteAttack", castingTime);
    }

    public override void CompleteAttack()
    {
        //Play Jump Animation?
        //VFX ty
        applyJumpPower = true;
    }

    private void FixedUpdate()
    {
        if (applyJumpPower)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * currentJumpPower);

            if (rising)
            {
                currentJumpPower += currentJumpPowerDiminisher;

                //Fallin time
                if (currentJumpPower > jumpPower)
                {
                    rising = false;
                }
            }
            else
            {
                currentJumpPower -= currentJumpPowerDiminisher;
            }

            //currentJumpPowerDiminisher *= 1.2f;

            if (currentJumpPower < 0)
                applyJumpPower = false;
        }
    }
    */
    #endregion

    //Nerfed af, to work only on the feet of the user.
    //However, make a boolean of secondary hitboxes in the future, that work as they should.
    //Aka, these hitboxes around the feet, are 0 dmg hitboxes, that push enemies outwards!

    //This is an attack, but it doesn't work normally (Start(isAttacking=true)->Complete->Finish->isAttacking=false)
    //It has cooldown from parent, but no need since EnemyJump has it as well ;) and this is attached there so!

    public override void Start()
    {
        base.Start();

        //Subscribes to jump's touch Ground and Player, so they will be notified.
        EnemyJump commonJump = GetComponent<EnemyJump>();
        commonJump.touchedGround.AddListener(CompleteAttack);
        //commonJump.touchedPlayer.AddListener(CompleteAttack);
    }

    //No worries about hitting him right above the ground + at the groundtouch, since 2 second i-frames after he is hit.
    public override void CompleteAttack()
    {
        if (onCooldown)//Do nothing.
            return;

        onCooldown = true;

        //VFX ty

        if (GameManager.testing)
            Debug.Log("Stomp Time!");

        colliders = Physics2D.OverlapAreaAll(transform.position + attackHitboxTopLeft, transform.position + attackHitboxBottomRight, WhatIsPlayer);

        if (GameManager.testing)
            Debug.DrawLine(transform.position + attackHitboxTopLeft, transform.position + attackHitboxBottomRight, Color.white, 2f);

        //Checking every collider.
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].gameObject != gameObject)//Found player's collider.
                Damage(colliders[i].gameObject);

        //Debug.Log("Completed the attack");

        //It bugs with this, it makes all the next attacks be out of sequence.
        //Invoke("FinishAttack", recoveryTime);//sets commonbehaviour.isAttacking -> false; and OnCooldown = true;
    }
}
