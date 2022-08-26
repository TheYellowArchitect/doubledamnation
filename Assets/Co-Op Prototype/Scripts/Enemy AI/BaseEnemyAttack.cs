using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

//Plug in (the children) of this class onto EnemyBehaviour
[RequireComponent(typeof(EnemyBehaviour))]
public class BaseEnemyAttack : MonoBehaviour
{
    //Variables
    [Header("Base Attack Properties")]
        [Range(0, 4)]
        [Tooltip("Damage from 0~4")]
        public int damagePower;

        [OnValueChanged("CalculateHitboxes")]
        [MinValue(0f)]
        [Tooltip("How far away to trigger the attack, in a direction.\nlower range attacks are prioritized btw")]
        public float attackRange;

        [MinValue(0f)]
        [Tooltip("After player is hit, for how long he cannot act, aka for how long is he inside the Hit Animation?\nOver 1 second is retarded, don't do it.")]
        public float hitstun;

        [MinValue(0f)]
        [Tooltip("How quickly till attack starts")]
        public float castingTime;

        [MinValue(0f)]
        [Tooltip("How long till it finishes, once started")]
        public float recoveryTime;

        [Tooltip("How far it knockbacks back the damaged target upon hit, on the X axis")]
        public int knockbackPowerX;

        [Tooltip("How far it knockbacks back the damaged target upon hit, on the Y axis")]
        public int knockbackPowerY;

        [MinValue(0f)]
        [Tooltip("Once this attack is done, when again can this attack happen?\n0 means, no cooldown ;)")]
        public float attackCooldown;//Cooldown time so it wont spam

        //Currentl used for harpy's ranged attack
        [Tooltip("Should for each attack, the interval/cooldown be made longer? If no, leave it at 0.\nIf yes, do put the number. (flat stacking only btw)")]
        public float addedCooldownInterval;

        [OnValueChanged("CalculateHitboxes")]
        [Tooltip("On the Yhitbox, where is the origin point on Y axis")]
        public float offsetFromOriginY;

        [OnValueChanged("CalculateHitboxes")]
        [Tooltip("On the X hitbox, where is the origin point on X axis")]
        public float offsetFromOriginX = 0;

        [OnValueChanged("CalculateHitboxes")]
        [Tooltip("On the Yhitbox, what is the length")]
        public float attackRangeY;

        [Tooltip("The SFX when attacking")]
        public AudioClip enemyAttackSFX;

        [Tooltip("The SFX if attack hits player")]
        public AudioClip enemyHitSFX;

        [Tooltip("Not just the pitch of the HitSFX, but it is added onto the default pitch. usually between 0.8 and 1 btw.")]
        public float enemyHitSFXpitchAdditive;

        [ValidateInput("IsLayerMaskEmpty")]
        [Tooltip("What layer player/target is.")]
        public LayerMask WhatIsPlayer;

        [ReadOnly]
        public bool onCooldown;


    protected Vector3 attackHitboxTopLeft;
    protected Vector3 attackHitboxBottomRight;
    protected EnemyBehaviour commonBehaviour;

    //for cooldown
    protected float cooldownTimeElapsed = 0;

    //Caching
    protected Vector3 tempPoint1;

    protected IDamageable damageableObject;

    protected Collider2D[] colliders;

    [ReadOnly]
    [SerializeField]
    protected bool interrupted = false;

    //So it can reset cooldown.
    [ReadOnly]
    [SerializeField]
    protected float currentCooldown;

    //Timer for no bugs. Sadly, Invoke doesn't seem trustworthy. Using Timer from TimerManager makes it complicated, since I haven't implemented an purge function to delete useless/used timers.
    protected bool completeAttackActive = false;
    protected float completeAttackTimeElapsed = 0f;
    
    protected bool finishAttackActive = false;
    protected float finishAttackTimeElapsed = 0f;

    public virtual void DisplayAttackHitbox()
    {
        //Top-Left to Bottom-left
        tempPoint1 = new Vector3(attackHitboxTopLeft.x, attackHitboxBottomRight.y);
        Debug.DrawLine(transform.position + attackHitboxTopLeft, transform.position + tempPoint1, Color.red);

        //Top-Right to Bottom-Right
        tempPoint1 = new Vector3(attackHitboxBottomRight.x, attackHitboxTopLeft.y);
        Debug.DrawLine(transform.position + tempPoint1, transform.position + attackHitboxBottomRight, Color.red);

        //Bottom-Left to Bottom-Right
        tempPoint1 = new Vector3(attackHitboxTopLeft.x, attackHitboxBottomRight.y);
        Debug.DrawLine(transform.position + tempPoint1, transform.position + attackHitboxBottomRight, Color.red);

        //Top-Right to Top-Left
        tempPoint1 = new Vector3(attackHitboxBottomRight.x, attackHitboxTopLeft.y);
        Debug.DrawLine(transform.position + tempPoint1, transform.position + attackHitboxTopLeft, Color.red);
    }

    private void Awake()
    {
        commonBehaviour = GetComponent<EnemyBehaviour>();

        currentCooldown = attackCooldown;
    }

    public virtual void Start()
    {
        //Calculate the hitboxes
        CalculateHitboxes();
    }

    public void CalculateHitboxes()
    {
        attackHitboxTopLeft = new Vector2(attackRange * -1 + offsetFromOriginX, attackRangeY + offsetFromOriginY);
        attackHitboxBottomRight = new Vector2(attackRange + offsetFromOriginX, -1 * attackRangeY + offsetFromOriginY);
    }

    public virtual void Update()
    {
        DisplayAttackHitbox();

        //Sucks that finishing TimerManager to work smoothly (to-be-used-on-next-projects) will take much time, and using it now with a small tweak will not be optimal.
        UpdateTimers();
    }

    public virtual void UpdateTimers()
    {
        //Cooldown timer
        if (onCooldown)
        {
            cooldownTimeElapsed += Time.deltaTime;
            if (cooldownTimeElapsed > currentCooldown)
            {
                cooldownTimeElapsed = 0;
                onCooldown = false;

                //Increases cooldown per attack, almost always, increased by 0 ;)
                currentCooldown += addedCooldownInterval;
            }
        }

        //CompleteAttack Timer
        if (completeAttackActive)
        {
            completeAttackTimeElapsed += Time.deltaTime;
            if (completeAttackTimeElapsed > castingTime)
            {
                completeAttackTimeElapsed = 0f;
                completeAttackActive = false;

                CompleteAttack();
            }
        }

        //FinishAttack Timer
        if (finishAttackActive)
        {
            finishAttackTimeElapsed += Time.deltaTime;
            if (finishAttackTimeElapsed > recoveryTime)
            {
                finishAttackTimeElapsed = 0f;
                finishAttackActive = false;

                FinishAttack();
            }
        }

        //Debug.Log(cooldownTimeElapsed);
    }

    //"Triggered" by enemybehaviour, so it starts here
    public virtual void StartAttack()
    {
        //Play "Attack" Animation, from overriden children :P

        Debug.Log("Started attacking...");

        interrupted = false;

        //Not needed, since the script that calls the attacks, declares this as .isMoving = false :P
        //commonBehaviour.isMoving = false;

        //So it starts CompleteAttack timer. (see UpdateTimers())
        completeAttackActive = true;
        //Invoke("CompleteAttack", castingTime);
    }

    public virtual void CompleteAttack()
    {
        Debug.Log("Not implemented CompleteAttack!");


        //So it starts FinishAttack timer. (see UpdateTimers())
        finishAttackActive = true;
        //Invoke("FinishAttack", recoveryTime);
    }

    //Coulda pass it by reference, or event/action BUT too lazy. I doubt it will backfire...
    public virtual void FinishAttack()
    {
        if (interrupted)
        {
            ResetAttackTimers();
            return;
        }
            

        //Play animation
        commonBehaviour.targetAnimationState = EnemyBehaviour.IDLE;
        commonBehaviour.NewStateTransition();

        onCooldown = true;

        commonBehaviour.isAttacking = false;

        if (GameManager.testing)
            Debug.Log("Finished attack");
    }

    public virtual void InterruptAttack()
    {
        onCooldown = true;
        commonBehaviour.isAttacking = false;//It was on SetIsAttacking(false) but stack overflow, so much for double-checking.(old vers lel)
        interrupted = true;

        if (GameManager.testing)
            Debug.Log("Interrupted");
    }

    //Returns true if it damaged, false if not
    public virtual bool Damage(GameObject damagedObject)
    {
        //damageableObject is IDamageable. GetComponent should work.
        damageableObject = damagedObject.GetComponent<IDamageable>();//as Idamageable rip, shoulda been WarriorHealth but anyway.

        if (damageableObject != null)
        {
            //If he kills player
            if (WarriorHealth.isInvulnerable == false && WarriorHealth.dying == false && damagePower >= damagedObject.GetComponent<WarriorHealth>().GetTotalCurrentHP() )
                commonBehaviour.AddPlayerKillerHP(2);

            Debug.Log("Current health | Temp HP | damagePower = " + damageableObject.CurrentHealth + " | " + damagedObject.GetComponent<WarriorHealth>().TempHP + " | " + damagePower);

            if (WarriorHealth.isInvulnerable == false && enemyHitSFX != null)
            {
                //player has will have 3+ hp after damage
                if (damageableObject.CurrentHealth + damagedObject.GetComponent<WarriorHealth>().TempHP - damagePower >= 3)
                {
                    PlayerSoundManager.globalInstance.PlayClip(enemyHitSFX, PlayerSoundManager.AudioSourceName.EnemyHit, 1.3f , 0.6f);
                }
                //player will have 2 hp after damage
                else if (damageableObject.CurrentHealth + damagedObject.GetComponent<WarriorHealth>().TempHP - damagePower == 2)
                {
                    PlayerSoundManager.globalInstance.PlayClip(enemyHitSFX, PlayerSoundManager.AudioSourceName.EnemyHit, 1 + enemyHitSFXpitchAdditive, 0.6f);
                }
                //player will have 1 hp after damage
                else if (damageableObject.CurrentHealth + damagedObject.GetComponent<WarriorHealth>().TempHP - damagePower == 1)
                {
                    PlayerSoundManager.globalInstance.PlayClip(enemyHitSFX, PlayerSoundManager.AudioSourceName.EnemyHit, 0.8f + enemyHitSFXpitchAdditive, 0.6f);
                }
                //Dead
                else if (damageableObject.CurrentHealth + damagedObject.GetComponent<WarriorHealth>().TempHP - damagePower == 0)
                {
                    PlayerSoundManager.globalInstance.PlayClip(enemyHitSFX, PlayerSoundManager.AudioSourceName.EnemyHit, 0.5f + enemyHitSFXpitchAdditive, 0.8f);
                }
            }
            

            damageableObject.TakeDamage(damagePower, transform.position, knockbackPowerX, knockbackPowerY, hitstun);//Invulnerability is decided inside here.
            damageableObject = null;

            return true;
        }

        return false;
    }

    public bool IsPlayerInAttackDistance(float distanceFromPlayer)
    {
        //while the offsets are not always correct (cuz of direction/facing), it makes the distance a little more than actual range which is a good thing. (then again, im retarded for not making detection and damage range different.)
        if (distanceFromPlayer > attackRange + offsetFromOriginX && distanceFromPlayer > attackRangeY + offsetFromOriginY)
            return false;
        else
            return true;
    }

    public virtual void ResetAttackTimers()
    {
        ResetCompleteAttackTimer();

        ResetFinishAttackTimer();
    }

    public virtual void ResetCompleteAttackTimer()
    {
        completeAttackActive = false;
        completeAttackTimeElapsed = 0f;
    }

    public virtual void ResetFinishAttackTimer()
    {
        finishAttackActive = false;
        finishAttackTimeElapsed = 0f;
    }

    public void ResetAttackCooldown()
    {
        onCooldown = false;
        cooldownTimeElapsed = 0f;

        currentCooldown = attackCooldown;
    }

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}
