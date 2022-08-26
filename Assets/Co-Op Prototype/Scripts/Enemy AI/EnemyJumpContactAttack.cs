using UnityEngine;
using NaughtyAttributes;

//This isn't made as a subclass to BaseEnemyAttack, cuz it plugs in to EnemyJump. And it has its own parameters (no father) so it won't get confusing/buggy :P
//Plugged in: EnemyJump.MidairContactAttack
[RequireComponent(typeof(EnemyBehaviour))]
[RequireComponent(typeof(EnemyJump))]
public class EnemyJumpContactAttack : MonoBehaviour
{
    //Variables
    [Tooltip("Should it do damage when jumping upwards (upwards/rising phase)")]
    public bool damageRising = false;

    [Tooltip("Should it damage horizontally?")]
    public bool damageHorizontally = false;

    [Tooltip("Should it do damage when falling (downwards/falling phase)")]
    public bool damageFalling = false;

    [Range(0,4)]
    [Tooltip("Damage from 0~3")]
    public int damagePower;

    [Tooltip("How far it knockbacks back the damaged target upon hit, on the X axis")]
    public int knockbackPowerX;

    [Tooltip("How far it knockbacks back the damaged target upon hit, on the Y axis")]
    public int knockbackPowerY;

    [MinValue(0f)]
    [Tooltip("After player is hit, for how long he cannot act, aka for how long is he inside the Hit Animation?")]
    public float hitstun;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("What layer player/target is.")]
    public LayerMask WhatIsPlayer;

    [ReadOnly]
    public bool isJumping = false;


    //Caching
        private Rigidbody2D commonRigidbody;

        private Collider2D[] colliders;

        private IDamageable damageableObject;

        //Notified from EnemyJump
        private bool isRising;

    private void Awake()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start ()
    {
        //Destroy this if nothing is set to happen on touch
        if (!damageRising && !damageFalling)
            Debug.LogError("Useless JumpContactAttack, please fix. Doesn't have neither up or down contactDmg");
    }

    //Called from EnemyJump.CompletedJump event
    public void FirstFrameCollision()
    {
        if (damageRising == false)
            return;

        //Note: commonCollider.bounds.extents is top right, while -1 is bottom left.
        //Gotta check if there is anyone inside in frame1.
        Collider2D commonCollider = GetComponent<Collider2D>();
        
        //Merging: Adjust the 1.2f thingy
        Debug.DrawLine(transform.position + commonCollider.bounds.extents * -1.2f, transform.position + commonCollider.bounds.extents * 1.2f, Color.blue, 3f);
        Collider2D[] colliders = Physics2D.OverlapAreaAll(transform.position + commonCollider.bounds.extents * -1.2f, transform.position + commonCollider.bounds.extents * 1.2f, WhatIsPlayer);

        //Debug.Break();
        //Debug.Log(colliders.Length);

        //Checking every collider.
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)//Found player's collider.
            {
                Damage(colliders[i].gameObject);
            }
        }
        //////////////////////////////////////////////////////////////
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log("Hit something, also isJumping -> " + isJumping);
        //Debug.Log("isRising-> " + isRising + " damageRising-> " + damageRising);
        //Debug.Log("Hit something");

        if (commonRigidbody.velocity.y > 0)
            isRising = true;
        else
            isRising = false;

        //set via EnemyJump with .isJumping = value;P
        if (isJumping)
        {
            bool DoDamage = false;
            //Damage happens when going upwards, and jump is upwards rn
            if (isRising && damageRising)
                DoDamage = true;
            //Damage happens when going downwards, and jump is downwards rn
            else if (!isRising && damageFalling)
                DoDamage = true;

            //The damage part
            if (DoDamage)
            {
                //Debug.Log("Damage hype?");

                //if other.gameobject != this.gameobject if the enemy has more than 1 collider? Polygonal though hmmm

                Damage(other.gameObject);
            }


        }//End of isJumping check

        
    }

    //Returns true if it damaged, false if not
    public void Damage(GameObject damagedObject)
    {
        //damageableObject is IDamageable. GetComponent should work.
        damageableObject = damagedObject.GetComponent<IDamageable>();//as Idamageable rip

        if (damageableObject != null)
        {
            damageableObject.TakeDamage(damagePower, transform.position, knockbackPowerX, knockbackPowerY, hitstun);
            damageableObject = null;
        }
    }

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}
