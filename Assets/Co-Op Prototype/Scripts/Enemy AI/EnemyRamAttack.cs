using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyRamAttack : BaseEnemyAttack
{
    [Header("Ramming Properties")]
    //[Required]
    //[Tooltip("The collider used by this gameobject, may it be BoxCollider2D, or polygonal or whatever else")]
    [HideInInspector]
    public Collider2D commonCollider;

    [Tooltip("The speed to charge forward")]
    public float chargeSpeed;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("So it detects when it hits a wall")]
    public LayerMask WhatIsWall;

    private Rigidbody2D commonRigidbody;

    private bool rammingActive = false;//To enable physics stuff (movement + dmg)
    private bool rammingLeft = false;  //Via facing, gets what direction to ram towards

    public override void Start()
    {
        //No hitboxes needed at ramming. (hmmm)
        commonRigidbody = GetComponent<Rigidbody2D>();

        commonCollider = GetComponent<Collider2D>();
    }

    public override void StartAttack()
    {
        base.StartAttack();

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.RAM_ATTACK_CHARGE;
        commonBehaviour.NewStateTransition();
    }

    //Basic Melee Attack
    public override void CompleteAttack()
    {
        if (interrupted)
            return;

        //Play animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.RAM_ATTACK;
        commonBehaviour.NewStateTransition();

        Debug.Log("Completed ram");

        rammingActive = true;

        if (commonBehaviour.GetFacingRight())
            rammingLeft = false;
        else
            rammingLeft = true;

        //Check if player is already in range in first frame
        FirstFrameCollision();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (rammingActive && other.gameObject != this.gameObject)
        {
            // Debug.Log(collisionPoint);
            Vector2 collisionPoint = other.contacts[0].normal;

            //If hit wall horizontally
            if ((WhatIsWall & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
            {
                if (collisionPoint.x < -0.95f || collisionPoint.x > 0.95f)
                {
                    //Hit wall
                    Debug.Log("Hit wall.");

                    //End attack (should also activate if wall/ground)
                    FinishAttack();
                }
            }
            //If hit player
            else if ((WhatIsPlayer & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
            {
                if ( (commonBehaviour.GetFacingRight() && collisionPoint.x < -0.95f) || (commonBehaviour.GetFacingRight() == false && collisionPoint.x > 0.95f) )
                {
                    //Checks if damageable inside, no worries.
                    Damage(other.gameObject);

                    //Debug.Log("name is: " + other.gameObject.name);

                    //End attack (should also activate if wall/ground)
                    FinishAttack();
                }
                
            } 
        }   
    }

    //Called at the very start of ramming, cuz it doesn't detect collisions that are already happening :P
    public void FirstFrameCollision()
    {
        //Note: commonCollider.bounds.extents is top right, while -1 is bottom left.
        //Gotta check if there is anyone inside in frame1.
        Collider2D commonCollider = GetComponent<Collider2D>();

        Debug.DrawLine(transform.position + commonCollider.bounds.extents * -1.2f, transform.position + commonCollider.bounds.extents * 1.2f, Color.blue, 3f);
        Collider2D[] colliders = Physics2D.OverlapAreaAll(transform.position + commonCollider.bounds.extents * -1.2f, transform.position + commonCollider.bounds.extents * 1.2f, WhatIsPlayer);

        //Checking every collider.
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)//Found player's collider.
            {
                //Applies damage
                if (Damage(colliders[i].gameObject))
                {
                    Debug.Log("Nani?");
                    //If it did damage:
                    FinishAttack();
                }
            }
        }
        //////////////////////////////////////////////////////////////
    }

    public void FixedUpdate()
    {
        // Commented out because InterruptAttack() is called from EnemyBehaviour
        //if (interrupted)//Interrupted pulled via EnemyBehaviour(mastah), so it will stop this immediately.
            //InterruptAttack();

        if (rammingActive)
        {
            if (rammingLeft)
                commonRigidbody.velocity = new Vector2(-1 * chargeSpeed, commonRigidbody.velocity.y);
            else
                commonRigidbody.velocity = new Vector2(1 * chargeSpeed, commonRigidbody.velocity.y);
        }

    }

    public override void FinishAttack()
    {
        if (interrupted)
            return;

        Debug.Log("Finished");
        rammingActive = false;
        onCooldown = true;

        //Play animation
        commonBehaviour.targetAnimationState = 0;//0 -> Idle.
        commonBehaviour.NewStateTransition();

        commonBehaviour.isAttacking = false;
        commonBehaviour.SetIsJumping(false);
    }

    public override void InterruptAttack()
    {
        interrupted = true;
        rammingActive = false;
        onCooldown = true;

        //Play animation
        commonBehaviour.isAttacking = false;
        commonBehaviour.SetIsJumping(false);
    }

}
