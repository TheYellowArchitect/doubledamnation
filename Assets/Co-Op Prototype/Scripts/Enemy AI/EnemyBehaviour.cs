using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

//Physics for enemies be like: Velocity for X axis, Forces for Y axis.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyPathfinder))]
public class EnemyBehaviour : MonoBehaviour, IDamageable
{
    //Instead of const int, you should use enum, stupid! Not to mention you can choose an enumeration from the inspector instead of putting a memorized number smh!
    const int Hollow = 0, Centaur = 1, Satyr = 2, Minotaur = 3, Harpy = 4, Cyclops = 5, Unknown = 6, Hollow2 = 7, Centaur2 = 8, Satyr2 = 9, Minotaur2 = 10, Harpy2 = 11, Cyclops2 = 12, Unknown2 = 13;
    [Tooltip("So monster-exclusive behaviours are defined.")]//Used super rarely lmao.
    public int monsterType;

    [Header("Actions")]
    [ReorderableList]
    [Tooltip("The possible attacks this monster can perform")]
    public List<BaseEnemyAttack> possibleAttacks = new List<BaseEnemyAttack>();



    [Header("Float values")]
    [Tooltip("How fast this enemy moves")]
    public float movementSpeed;

    public float currentMovementSpeed;

    [MinValue(0f)]
    [Tooltip("In what range the enemy can detect you")]
    public float SightRange;

    [Range(0, 100)]
    [Tooltip("How much %chance, for this enemy to chase/move towards obstructed player, instead of standing")]
    public int ChaseObstructedPlayerChance;

    [Header("OnTakenDamage Properties")]
    [MinValue(0f)]
    [Tooltip("For how long after an hit, this monster/enemy cannot act")]
    public float hitRecoveryTime;

    [Tooltip("The movement multiplier while hit")]
    public float hitMovementMultiplier;

    [Tooltip("When player jumps right atop this, for how long should hitstun play?")]
    public float hitstunPlayerStompsDuration;

    [Tooltip("When enemy lands right above player, for how long should hitstun play on the player?")]
    public float hitstunStompPlayerDuration;

    [Tooltip("When enemy lands right above player, how much power should he apply?")]
    public int stompKnockbackPower;

    [Tooltip("When player attacks this enemy, how much mana should he take?\n0 is default.")]
    public int giveManaOnHit = 0;

    [Tooltip("When player kills this enemy, how much mana should he take?\n1 is default.")]
    public int giveManaOnDeath = 1;

    [Header("Misc")]
    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("Upon jumping, what should the raycast detect to say \"Hey, what i detected is ground! Now, jump is over.\"?")]
    public LayerMask WhatIsGround;

    [Tooltip("When midair, what force is applied downwards? Literally addForce(vector2.down * midairGravity) xD")]
    public float midairGravity;

    [Tooltip("Can it in any case, rotate? Like, act like a spinning ball/sphere?")]
    public bool canRotate = false;

    [Tooltip("Is this a flying enemy/monster?")]
    public bool canFly = false;

    [ShowIf("CanFly")]
    public float movementSpeedY = 0f;

    [ShowIf("CanFly")]
    public float currentMovementSpeedY;

    [MinValue(1)]
    [Tooltip("How often per second, the A.I. updates, when in sight range.")]
    public int activeRefreshRate;

    [MinValue(1)]
    [Tooltip("How often per second, the A.I. updates, when NOT in sight range.\nShould be bigger than active.")]
    public int passiveRefreshRate;

    [MinValue(0f)]
    [Tooltip("When is \"almost\" touching the player, so it stops?")]
    public float playerTouchRange;

    //Properties, mandatory, since interface IDamageable demands them, and Unity doesn't play well with properties.
    [Header("Health")]
    [SerializeField]
    private int maxHealth;
    public int MaxHealth
    {
        get { return maxHealth; }
        set
        {
            if (value < 1) Debug.Log("Cannot be negative or 0, watcha doin?");
            else maxHealth = value;
        }
    }

    [SerializeField]
    private int currentHealth;
    public int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }


    //"common" adjective/prefix means that the gameobject that "hosts" this component, also "hosts" other components.
    //All these components, are common, like Rigidbody2D.

    //Common Caching
    private Rigidbody2D commonRigidbody;
    //[Tooltip("If it should jump, this shouldn't be empty. It literally contains the jump and all its configurations.")]
    [HideInInspector]
    public EnemyJump jumpAction;//Would rename it to commonJump, but dependencies are established.
    //private WarriorMovement warriorBehaviour;
    //NO! use commonColider, then collider.bounds.size, you fool!//I think .extents is actually half so it saves 1 calculation.
    //private SpriteRenderer commonRenderer;


    //Where enemy/this spawned
    private Vector3 originPosition;

    //Caching

    //Self-explanatory
    //private Vector3 warriorPosition;

    [Header("Read-Only/Debugging")]
    //From pathfinding/waypoint distance
    [ReadOnly]
    [SerializeField]
    private Vector2 directionToPlayer;    

    [ReadOnly]
    [SerializeField]
    //to check which actions (attack/jump has range)
    private float distanceFromPlayer;

    [ReadOnly]
    [SerializeField]
    //IsPlayer behind "Ground"
    private bool isPlayerObstructed;

    [ReadOnly]
    [SerializeField]
    //IsPlayer behind hazard?
    private bool isPathfindingHazardous;

    [ReadOnly]
    [SerializeField]
    //IsPlayer atop a platform?
    private bool isPlayerOnPlatform;

    [ReadOnly]
    [SerializeField]
    //Is origin position blocked/obstructed?
    private bool isOriginPositionObstructed = true;

    //If the enemy/this that has this script attached, has an <EnemyAttack>
    private bool hasAttackAction;


    [ReadOnly]
    //Is doing an attack, so do not interrupt :D
    public bool isAttacking = false;

    [ReadOnly]
    //Is moving, interrupt freely btw, made for FixedUpdate()
    public bool isMoving;

    [ReadOnly]
    //So as to detect ground without raycasts, and to help with the artificial "gravity"(not Unity's) and also the EnemyJump(so as it won't jump in midair)
    public bool isGrounded;

    [ReadOnly]
    //So it won't go to origin position, if it is already there ;)
    public bool isOnOriginPosition;

    [ReadOnly]
    //Is jumping, so do not interrupt :D
    public bool isJumping = false;

    [ReadOnly]
    [SerializeField]
    //So FixedUpdate wont fuck shit up!
    public bool isDying;

    //30% RNG
    [ReadOnly]
    public bool chasesObstructions;

    [ReadOnly]
    [SerializeField]
    //If taken damage, so nothing happens for a while
    private bool isHit;

    [ReadOnly]
    //Stores the new hp
    public int playerKillerHP = 0;

    [ReadOnly]
    public GameObject PlayerKillerVFX = null;

    [ReadOnly]
    public float timeToDeathFreeze = 0.4f;

    [ReadOnly]
    public float currentTimeToDeathFreeze;

    [ReadOnly]
    public float currentHitMovementMultiplier;

    [ReadOnly]
    [Tooltip("Gotta cache the player but not on start otherwise CPU burst. Cache should happen when monster dies")]
    public bool playerCollidersCached = false;

    [ReadOnly]
    [Tooltip("When dead, ignore player.")]
    public bool playerCollidersIgnored = false;

    [ReadOnly]
    [Tooltip("In LevelManager.enemyList")]
    public ushort enemyListIndex;

    private BoxCollider2D boxPlayerCollider;       //Body(Top&Mid)
    private CircleCollider2D circlePlayerCollider; //Feet(Bottom)

    [ReadOnly]
    [Tooltip("Made to excuse my shitty physics detection, so the wall is detected, registered, and then no X velocity will be applied, WHILE MIDAIR!\nLiterally a flag oncollision enter/exit.")]//?
    public bool sideWallDetected = false;
    [ReadOnly]
    public GameObject sideWall;

    [ReadOnly]
    public int satyrBugfixJumpRight = 0;

    private GameObject sidePlatformIgnored;
    private Collider2D commonCollider;

    //For lerp
    private float currentHitTime;

    //Used in OnColliderExit for raycasting
    private float colliderExtent;

    [ReadOnly]
    [SerializeField]
    //Direction enemy/this is facing
    private bool facingRight = true;

    //Current attack being used
    private BaseEnemyAttack currentAttack;

    //Made to find origin of player hit, so this/enemy will be applied force from it.
    [ReadOnly]
    [SerializeField]
    private Vector3 directionHit;

    //Calculates the direction to originPos
    Vector3 directionToOriginPos;

    [ReadOnly]
    [SerializeField]
    //Boolean that stores when player is midair, subscribed event changes this.
    private bool playerIsMidair = false;

    [ReadOnly]
    [SerializeField]
    //Spagghetti but to explain: Each time a dying enemy is hit, it resets the Invoke() function, so I won't have to keep timer for every monster. If hit once, and it reaches DeathFreeze/died.Invoke(), it checks deathreset, if 0, it works, otherwise it decreases by 1 and waits for next Invoke("DeathFreeze").
    private int deathResets = 0;

    [ReadOnly]
    [SerializeField]
    //The simple and most elegant way of checking if a coroutine is running/active.
    private bool coroutineRunning;

    //Indicates current refresh rate, is either active or passive btw.
    private int currentRefreshRate;

    //On FixedUpdate, stores the last known position.
    //private Vector3 lastPosition;

    //State-Machine (for animations lel)//Setup animations should be at 4+. Hardcoded even when enum'd hellodarknessmyoldfriend~
    public const int IDLE = 0, RUN = 1, MELEE_ATTACK = 2, RANGED_ATTACK = 3, RAM_ATTACK = 4, MELEE_ATTACK2 = 5, MELEE_ATTACK_CHARGE = 6, RANGED_ATTACK_CHARGE = 7, RAM_ATTACK_CHARGE = 8, MELEE_ATTACK2_CHARGE = 9, HITGROUND = 10, HITMIDAIR = 11, JUMP_CHARGE = 12, JUMP = 13, FALLING = 14, DEATH = 15;
    [ReadOnly]
    public int currentAnimationState;
    [ReadOnly]
    public int targetAnimationState;
    [HideInInspector]
    public Animator animatorController;

    //This action, is subscribed by EnemyPathfinder, so I can simply invoke this, and DeathFreeze works.
    public Action died;

    public static Action<ushort> playerKillerEvent;

    //FLYING-ONLY (also bad design but yolo at this point)
    //////////////////////
    [ReadOnly]
    [SerializeField]
    private int flyingObstructionType;

    enum flyingObstructionDirection { NONE, TOPRIGHT, TOPLEFT, BOTTOMRIGHT, BOTTOMLEFT };

    private HarpyAuraAttack commonAura;

    //RaycastHit2D hit;//To check if hazard when player obstructed when the shitty pseudo-pathfinding algorithm happens
    ///////////////////
    

    //Initialization for itself.
    private void Awake()
    {
        //commonPathfinder = GetComponent<EnemyPathfinder>();
        commonRigidbody = GetComponent<Rigidbody2D>();
        animatorController = GetComponentInChildren<Animator>();
        //animatorController = transform.FindChild("SpriteVisible").GetComponent<Animator>();//mfw im using string to access children. 2 fucking children and using string instead of an int. smh.
        //commonRenderer = GetComponent<SpriteRenderer>();
        //Attach the jump action manually ;)
        if (GetComponent<EnemyJump>() != null)
            jumpAction = GetComponent<EnemyJump>();

        //aka, has it collided with any Ground gameobject? :P
        isGrounded = false;

        //Set origin position, so it can go there when player is away, and to revive there
        originPosition = transform.position;

        currentTimeToDeathFreeze = timeToDeathFreeze;
        currentHitMovementMultiplier = hitMovementMultiplier;
        currentMovementSpeed = movementSpeed;
        currentMovementSpeedY = movementSpeedY;

        commonCollider = GetComponent<Collider2D>();

        commonAura = GetComponentInChildren<HarpyAuraAttack>();

        //ChaseObstructedPlayerChance% RNG to chase players when obstructed
        if (UnityEngine.Random.Range(0, 100) < ChaseObstructedPlayerChance)
            chasesObstructions = true;
        else
            chasesObstructions = false;

        //Freeze rotation, so no rotation change on landing, messing up the grounded+animations, or on attack!
        if (!canRotate)
            commonRigidbody.freezeRotation = true;

        //Sets Current Health = MaxHealth
        SetStartingHealth();

        //Sets variables like bool HasAttackAction, if enemy has any attack, so it won't recalculate a lot
        InitializeActionVariables();

        coroutineRunning = false;

        isOnOriginPosition = true;

        //State-Machine(no need to NewStateTransition(), since Idle is default animation.)
        currentAnimationState = IDLE;

        targetAnimationState = IDLE;
    }

    //Initialization for others.
    void Start()
    {
        //Subscribe to player's midair status, to detect when player is midair, and when grounded.
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().isMidair += PlayerIsMidair;
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().isGrounded += PlayerIsGrounded;

        //warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement<().GetMidair();

        //warriorPosition = GameObject.FindGameObjectWithTag("Warrior").transform.position;

        //Subscribes the TouchedGround function to EnemyJump.TouchedGround
        if (jumpAction != null)
            jumpAction.touchedGround.AddListener(TouchedGround);

        if (!coroutineRunning)
            StartCoroutine(UpdateBehaviour());
    }

    /*
    private void Update()
    {
        //Debug.Log(commonRigidbody.velocity);
        //Debug.Log("Distance from player is: " + distanceFromPlayer);
        //DisplayVisionHitbox();

        //Debug.Log("IsGrounded? " + isGrounded);
        /*
        if (isGrounded)
        {
            Debug.Break();

            //Prints the dictionary
            foreach (KeyValuePair<int, GameObject> kvp in currentGroundCollisions)
            {
                Debug.Log("Unique ID is: " + kvp.Key + " and name is: " + kvp.Value.name);
            }
        }
        * /
        

        //Debug.Log("Is on origin position? " + isOnOriginPosition);

        //Debug.Log("Is attacking? " + isAttacking + " Is jumping? " + isJumping);

        //Debug.Log("The force in Y applied is: ");// + rigidbody2D.speed or whatever.<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
    }
    */

    public IEnumerator UpdateBehaviour()
    {
        coroutineRunning = true;
        //Loops so it restarts after 1/RefreshRate seconds :D
        while (coroutineRunning)
        {
            //If online and client, dont do any logic
            if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            {//Hack, which exits the coroutine entirely.
                coroutineRunning = false;
                continue;
            }

            //Determines the refresh rate.
            if (distanceFromPlayer < SightRange)
                currentRefreshRate = activeRefreshRate;
            else
                currentRefreshRate = passiveRefreshRate;

            //Monster/Enemy is attacking, so no further A.I. calculations needed (or is hit rn, so it doesnt matter)
            while (isAttacking || isHit)
                yield return new WaitForSeconds(1 / (currentRefreshRate + 1));


            //Debug.Log("Distance from player is: " + distanceFromPlayer);

            //If enemy "detects" player
            if (distanceFromPlayer < SightRange)//warrior health, used so the monsters go back to originPosition when player dead.
            {

                if (WarriorHealth.dying)
                {
                    //VICTORY TAUNT PLZ!//Cut feature, rip. After release....
                    StandStill();
                    yield return new WaitForSeconds(4);
                }

                if (isPathfindingHazardous)
                {
                    //Maybe a midair check?
                    isMoving = true;
                    //and with isPathfindingHazardous at movement, it will go back to originPos ;)
                }
                else if (isPlayerObstructed)//Ground
                {
                    if (canFly)//Obstruction A.I. is adjusted in FixedUpdate movement.
                        isMoving = true;
                    else if (chasesObstructions)//&& warriorIsMidair)//this is usually 30% (from Start)
                    {
                        //Debug.Log(directionToPlayer);

                        //If upwards, jump (everything except the direction.y condition, is copy-pasta'd)
                        if (directionToPlayer.y > 0.05f && directionToPlayer.y < 0.8f && !isJumping && isGrounded && jumpAction != null && !jumpAction.onCooldown && jumpAction.jumpRange > distanceFromPlayer)
                        {
                            TriggerJumpAction();
                            yield return new WaitForSeconds(jumpAction.castingTime);//So as not to move while charging jump, but I should just have another boolean here ah well
                        }
                        //If downwards, can probably run over there.
                        else if (directionToPlayer.y < -0.1f)
                            //Will run towards the player
                            isMoving = true;
                        else
                            StandStill();
                    }

                    else if (!isJumping)
                        StandStill();
                }
                else//Normal = No obstructions/hazards
                {
                    if (canFly)
                    {
                        if (hasAttackAction)
                            DetermineAttackStart();//True means it checks for ranged attacks only  

                        if (!isAttacking)
                        {
                            if (playerTouchRange > distanceFromPlayer)//So he won't push the player.
                                StandStill();
                            else
                                isMoving = true;
                        }
                    }
                    else if (playerIsMidair || isPlayerOnPlatform)//(false -> warriorIsMidair)
                    {
                        //Debug.Log("Is player midair: " + playerIsMidair + " is player on platform? " + isPlayerOnPlatform);

                        //TODO: I think it should check if jumpAction has rising/falling, so it can be considered an attack.
                        //If can do any attack
                        if (hasAttackAction && isGrounded && !isJumping)//maybe remove isJumping? idk
                            DetermineAttackStart();

                        if (!isAttacking)//&& !isjumping, instead of being put below.<--- old one
                        {
                            //To jump or not to jump?
                            if (!isJumping && isGrounded && jumpAction != null && !jumpAction.onCooldown && jumpAction.jumpRange > distanceFromPlayer)
                            {
                                TriggerJumpAction();
                                yield return new WaitForSeconds(jumpAction.castingTime);//So as not to move while charging jump, but I should just have another boolean here ah well
                            }
                            else if (!isJumping)
                            {
                                isMoving = false;//StandStill() here bugs it, so isMoving = false -> FixedUpdate it does the state transition there.
                                //StandStill();

                                //So monster won't look same direction even if player passes right above... it looks like monster is retarded. (And player notices the braindead A.I. xd)
                                if (directionToPlayer.x < 0 && facingRight)
                                    FlipSprite();
                                else if (directionToPlayer.x > 0 && facingRight == false)
                                    FlipSprite();
                            }


                        }
                    }
                    else//Player on the ground or same terrain
                    {
                        //If can do any attack
                        if (hasAttackAction && isGrounded && !isJumping)
                            DetermineAttackStart();

                        //Jump
                        if (!isAttacking && !isJumping && isGrounded && jumpAction != null && !jumpAction.onCooldown && jumpAction.jumpRange > distanceFromPlayer)
                        {
                            TriggerJumpAction();
                            yield return new WaitForSeconds(jumpAction.castingTime);//So as not to move while charging jump, but I should just have another boolean here ah well
                        }

                        //If not attack -> Move (except if super small range so it wont push player or w/e)
                        if (!isAttacking)
                        {
                            if (playerTouchRange > distanceFromPlayer)//So he won't push the player.
                                StandStill();
                            else
                                isMoving = true;
                        }

                    }

                }

            }
            else//Go back to origin position (if not, stand still)
            {
                //Debug.Log("Player out of sight, origin position obstructed? " + isOriginPositionObstructed);
                //Debug.Log("is on origin? " + isOnOriginPosition + "is obstructed? " + isOriginPositionObstructed);

                if (!isOnOriginPosition && !isOriginPositionObstructed)
                {
                    //isMoving = true, in FixedUpdate moves enemy to either target or originPosition
                    //It goes to originPosition when out of sight, so with this it automatically goes to originPos ;)
                    isMoving = true;
                }
                else
                    StandStill();//Original is line below. No bugs, this just seems better.
                                 //isMoving = false;

            }


            yield return new WaitForSeconds(1 / currentRefreshRate);
        }
    }


    //Flawed like so many things in this project. It uses only RangeX, but what if the condition is something else? rangeY is the simplest example.
    public void DetermineAttackStart(bool exclusivelyRanged = false, bool exclusivelyRam = false, bool exclusivelyMelee = false)//Instead of exclusive, should work with inclusive/possible types. That way, its far easier/better, and you can use "exclusive" attacks, by including only one type.
    {
        bool attackSelected = false;

        //Debug.Log("Attack range is: " + possibleAttacks[0].attackRange + " and distance from player is: " + distanceFromPlayer + "ranged attack: " + ranged);

        //Loop through all possible attacks, to see which to attack depending on range
        foreach (BaseEnemyAttack pickedAttack in possibleAttacks)
        {
            //Debug.Log("Picked attack[0]'s range is: " + possibleAttacks[0].attackRange);
            //Debug.Log("Picked attack[1]'s range is: " + possibleAttacks[1].attackRange);

            //if (gameObject.name == "Hollow (24)")
                //Debug.Log("Attackboi is: " + pickedAttack.IsPlayerInAttackDistance(distanceFromPlayer));

            //Is inside range, ATTACK!
            if (pickedAttack.IsPlayerInAttackDistance(distanceFromPlayer) && pickedAttack.onCooldown == false)
            {
                //Melee Attack
                if (exclusivelyRanged == false && exclusivelyRam == false && exclusivelyMelee == false)
                {
                    attackSelected = true;
                }
                else if (exclusivelyRanged)//Ranged Attack
                {
                    EnemyRangedAttack tempRangedAttack = pickedAttack as EnemyRangedAttack;

                    if (tempRangedAttack != null && isPlayerObstructed == false)
                    {
                        Debug.Log("Ranged attack selected");
                        attackSelected = true;
                    }
                }
                else if (exclusivelyRam)
                {
                    EnemyRamAttack tempRamAttack = pickedAttack as EnemyRamAttack;

                    if (tempRamAttack != null)
                    {
                        Debug.Log("Ram attack selected");
                        attackSelected = true;
                    }
                }
                else if (exclusivelyMelee)
                {
                    EnemyMeleeAttack tempMeleeAttack = pickedAttack as EnemyMeleeAttack;

                    if (tempMeleeAttack != null)
                    {
                        Debug.Log("Melee attack selected");
                        attackSelected = true;
                    }
                }
            }

            //Attack is pickedAttack!
            if (attackSelected)
            {
                TriggerBaseEnemyAttack(pickedAttack);
                break;//Returning otherwise it does starts all attacks that are possible within range aylmao
            }
                
        }
    }

    public void TriggerBaseEnemyAttack(BaseEnemyAttack attackToUse)
    {
        //Debug.Log("Picked attack that got chosen has " + attackToUse.attackRange + " range, and distance from player is: " + distanceFromPlayer);
        isAttacking = true;
        isMoving = false;
        
        if (canFly == false)
            DetermineFlipSprite(directionToPlayer.x);

        if (!canFly)
            commonRigidbody.velocity = new Vector2(0, commonRigidbody.velocity.y);
        else
            commonRigidbody.velocity = new Vector2(commonRigidbody.velocity.x, commonRigidbody.velocity.y);//used to be 0,0

        currentAttack = attackToUse;

        attackToUse.StartAttack();
    }

    public void TriggerJumpAction()
    {
        //Woulda make a JumpFunction, but it must be IEnumerator aylmao.
        jumpAction.StartJump();//Also sets isJumping -> true

        //Otherwise, it "glides", could work for flame level tbh.
        isMoving = false;
        commonRigidbody.velocity = new Vector2(0f, commonRigidbody.velocity.y);//"Resets" velocity.
    }



    public void FixedUpdate()
    {
        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            return;

        //Debugging spagghetti, apologies. To explain: Monster is still in Falling animation "sliding in ground" while he is actually in an other state like run/idle.
        if (animatorController.GetCurrentAnimatorStateInfo(0).IsName("Falling") && isGrounded)// && isJumping)
        {
            //jumpAction.JustTouchedGround();

            animatorController.Play("Idle");
            currentAnimationState = IDLE;
        }


        //So if dead, it won't try to use physics or whatever.
        if (isDying)
        {
            if (!canFly)
                commonRigidbody.velocity = new Vector2(-200 * directionToPlayer.x * currentMovementSpeed / 100, commonRigidbody.velocity.y);//-1 is for opposite of player, 100 is for death to have more impact. 100 shouldn't be hardcoded tho but anyway GOTTA CODE FAST!
            else
                commonRigidbody.velocity = new Vector2(-200 * directionToPlayer.x * currentMovementSpeed / 100, -200 * directionToPlayer.y * currentMovementSpeedY / 100);

            return;
        }


        //So gravity applies normally
        if (!isGrounded && !canFly)
            commonRigidbody.AddForce(Vector2.down * midairGravity);

        //Rotation for flying: Ignored for now, it can work if you uncomment the below, the rotation in NewStateTransition, and in movement(isMoving&velocity), NEW&Final, everytime the facing changes, adjust rotation as well.
        /*
            if (canRotate && canFly)
            {
                if (transform.localEulerAngles.z > 45f || transform.localEulerAngles.z < -45f)
                {
                    if (isMoving)
                    {
                        if (transform.localEulerAngles.z > 45f)
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 45f);
                        else
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -45f);

                    }
                    else
                        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);

                    commonRigidbody.freezeRotation = true;
                    commonRigidbody.freezeRotation = false;

                }
                    
            }
        */

        //So it never "rotates"
        /* Fixed on Awake via freezeRotation
        if (!canRotate && (int)transform.localEulerAngles.z != 0)
            //transform.Rotate(0, 0, transform.localEulerAngles.z * -1);
            transform.localEulerAngles = Vector3.zero;
        //Debug.Log ((int)transform.localEulerAngles.z);
        */

        //Apply forces depending on state
        if (isHit)
        {
            currentHitTime += Time.fixedDeltaTime;
            if (currentHitTime > hitRecoveryTime)
                currentHitTime = hitRecoveryTime;

            //commonRigidbody.velocity = new Vector2(-1 * directionHit.x * hitMovementMultiplier * Time.deltaTime, commonRigidbody.velocity.y);

            //if (!canFly)
                commonRigidbody.velocity = new Vector2(-1 * directionHit.x * currentHitMovementMultiplier * Mathf.Lerp(1, 0, currentHitTime / hitRecoveryTime), -1 * directionHit.y * currentHitMovementMultiplier * Mathf.Lerp(1, 0, currentHitTime / hitRecoveryTime));
            //else
                //commonRigidbody.velocity = new Vector2(-1 * directionHit.x * hitMovementMultiplier * Mathf.Lerp(1, 0, currentHitTime / hitRecoveryTime), -1 * directionHit.y * hitMovementMultiplier * Mathf.Lerp(1, 0, currentHitTime / hitRecoveryTime));

            //Debug.Log("The math result is: " + Mathf.Lerp(1, 0, currentHitTime / hitRecoveryTime));
            //Debug.Log("Current HitTime is: " + currentHitTime + " and hitRecoveryTime is: " + hitRecoveryTime);

            isOnOriginPosition = false;
        }
        else if (isMoving)//TODO: Cache the direction below, so it checks if its the same, and doesnt change the velocity at all ;)
        {
            targetAnimationState = RUN;


            //Debug.Log((originPosition - transform.position).magnitude);

            //Moving towards the player
            if (distanceFromPlayer < SightRange && !isPathfindingHazardous)//the hazardous pathfinding, ensures it will go below.
            {

                if (canFly == false)
                {
                    DetermineFlipSprite(directionToPlayer.x);

                    #region useless complicated spagghetti
                    //Unknown spagghetti.
                    /*
                    bool switchCondition = true;//so it instantly moves on jump, right after leaving the ground.
                    
                    bool switchCondition2 = false;//demands is Grounded or isRising.
                    if (jumpAction != null)
                    {
                        if (jumpAction.rising)
                        {
                            switchCondition = false;

                            switchCondition2 = true;
                        }
                        else
                        {
                            switchCondition = true;

                            switchCondition2 = false;
                        }
                            
                    }*/

                    /*
                    bool switchCondition = false;
                    if (sideWallDetected)
                    {
                        switchCondition = true;
                        if (isGrounded)
                            switchCondition = true;
                        else if (jumpAction != null)
                        {
                            if (jumpAction.GetIsRising())
                                switchCondition = true;
                        }
   
                            
                    }
                    //
                    if (switchCondition == false)
                        commonRigidbody.velocity = new Vector2(directionToPlayer.x * currentMovementSpeed, commonRigidbody.velocity.y);
                    else
                        commonRigidbody.velocity = new Vector2(0, commonRigidbody.velocity.y);
                    */


                    /*
                    if (isPlayerObstructed == false)
                       commonRigidbody.velocity = new Vector2(directionToPlayer.x * currentMovementSpeed, commonRigidbody.velocity.y);
                    else//if player is obstructed, and downwards.
                    {

                    }
                    */
                    //
                    #endregion

                    //This satyr if-block, is for a bug, where player can just chill forever above his head.
                    if (distanceFromPlayer < 4.1f && (monsterType == Satyr || monsterType == Satyr2) && directionToPlayer.y > 0.97f)
                    {
                        //Set the direction for the first time
                        if (satyrBugfixJumpRight == 0)
                        {
                            if (UnityEngine.Random.value < 0.5f)
                                satyrBugfixJumpRight = 1;
                            else
                                satyrBugfixJumpRight = -1;
                        }

                        commonRigidbody.velocity = new Vector2(satyrBugfixJumpRight * currentMovementSpeed, commonRigidbody.velocity.y);

                    }
                    else
                    {
                        if (sideWallDetected)//Shouldn't it be sideWallDetected && player on same direction?
                            commonRigidbody.velocity = new Vector2(0, commonRigidbody.velocity.y);
                        else
                            commonRigidbody.velocity = new Vector2(directionToPlayer.x * currentMovementSpeed, commonRigidbody.velocity.y);
                    }

                    

                    
                }
                else
                {
                    //If player obstructed, made this block, cuz when player was behind a sharp corner, harpy would hug the edge. Breaks immersion cuz user realizes this A.I. is actually braindead. Shhh.. . its a secret.
                    if (isPlayerObstructed)
                    {
                        //If First-Time
                        if (flyingObstructionType == (int)flyingObstructionDirection.NONE)
                        {
                            //DetermineFlyingObstructionDirection();
                            //Top-Right
                            if (directionToPlayer.y > 0.01f && directionToPlayer.x > 0.01f)
                            {
                                flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;

                                DetermineFlipSprite(1);

                                //commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, 1 * currentMovementSpeedY);
                            }
                            //Top-Left
                            else if (directionToPlayer.y > 0.01f && directionToPlayer.x < -0.01f)
                            {
                                flyingObstructionType = (int)flyingObstructionDirection.TOPLEFT;

                                DetermineFlipSprite(-1);

                                //commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, 1 * currentMovementSpeedY);
                            }
                            //Bottom-Right
                            else if (directionToPlayer.y < -0.01f && directionToPlayer.x > 0.01f)
                            {
                                flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;

                                DetermineFlipSprite(1);

                                //commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, -1 * currentMovementSpeedY);
                            }
                            //Bottom-Left
                            else if (directionToPlayer.y < -0.01f && directionToPlayer.x < -0.01f)
                            {
                                flyingObstructionType = (int)flyingObstructionDirection.BOTTOMLEFT;

                                DetermineFlipSprite(-1);

                                //commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, -1 * currentMovementSpeedY);
                            }

                            //Edge-cases: direction.x or direction.y are 0.
                            else if (directionToPlayer.x < -0.01f && directionToPlayer.x > 0.01f)
                                //Topright or left, doesnt matter, as long it goes top->UPwards
                                flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;
                            else if (directionToPlayer.y < -0.01f && directionToPlayer.y > 0.01f)
                                flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;

                        }

                        //Check if hazard on that way.
                        //if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
                            //hit = Physics2D.Raycast(transform.position, new Vector3(0.5f,0.5f), 50, whatisha);


                        //Retarded to change the velocity every damn frame, but whatever. I could at least check if the vector matches with the cached velocity vector. Oh wait, possible bugs + I dont cache velocity vector. Fuck.
                        //DetermineFlyingObstructionVelocity();
                        //Top-Right
                        if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
                            commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, 1 * currentMovementSpeedY);

                        //Top-Left
                        else if (flyingObstructionType == (int)flyingObstructionDirection.TOPLEFT)
                            commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, 1 * currentMovementSpeedY);

                        //Bottom-Right
                        else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMRIGHT)
                            commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, -1 * currentMovementSpeedY);

                        //Bottom-Left
                        else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMLEFT)
                            commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, -1 * currentMovementSpeedY);

                        /*
                        //DetermineFlyingObstructionWallBump();
                        if (lastPosition == transform.position)
                        {
                            Debug.Log("Same Position");
                            //ReverseFlyingDirection();
                            if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
                            {
                                //Your A.I. is crippingly flawed, but you HAVE to finish it, and not make it 100% braindead? No algorithm can save you now? (and no raycasts to check where blocked, because it would make this into superspagghetti?) R N G
                                if (UnityEngine.Random.Range(0, 2) == 0)
                                    flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;
                                else
                                    flyingObstructionType = (int)flyingObstructionDirection.TOPLEFT;
                            }

                            else if (flyingObstructionType == (int)flyingObstructionDirection.TOPLEFT)
                            {
                                //Fucking RNG, I resort to this, because it is the END of the A.I. No more!
                                if (UnityEngine.Random.Range(0, 2) == 0)
                                    flyingObstructionType = (int)flyingObstructionDirection.BOTTOMLEFT;
                                else
                                    flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;
                            }

                            else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMRIGHT)
                            {
                                //50% RNG
                                if (UnityEngine.Random.Range(0, 2) == 0)
                                    flyingObstructionType = (int)flyingObstructionDirection.BOTTOMLEFT;
                                else
                                    flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;
                            }

                            else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMLEFT)
                            {
                                //Fucking RNG, I resort to this, because it is the END of the A.I. No more!
                                if (UnityEngine.Random.Range(0, 2) == 0)
                                    flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;
                                else
                                    flyingObstructionType = (int)flyingObstructionDirection.TOPLEFT;
                            }

                            //The more I push on, the sadder it gets. Isn't there something that exceeds code spagghetti?

                            //Retarded to change the velocity every damn frame, but whatever. I could at least check if the vector matches with the cached velocity vector. Oh wait, possible bugs + I dont cache velocity vector. Fuck.
                            //DetermineFlyingObstructionVelocity();
                            //Top-Right
                            if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
                                commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, 1 * currentMovementSpeedY);

                            //Top-Left
                            else if (flyingObstructionType == (int)flyingObstructionDirection.TOPLEFT)
                                commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, 1 * currentMovementSpeedY);

                            //Bottom-Right
                            else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMRIGHT)
                                commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, -1 * currentMovementSpeedY);

                            //Bottom-Left
                            else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMLEFT)
                                commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, -1 * currentMovementSpeedY);
                             
                        }
                        */
                        #region //Old Simple "avoid corners" A.I.
                        /*
                        //Player is upwards
                        if (directionToPlayer.y > 0.01f)
                            //commonRigidbody.velocity = new Vector2(directionToPlayer.x * movementSpeed, 1 * movementSpeedY);
                            commonRigidbody.velocity = new Vector2(0, 1 * movementSpeedY);

                        //Player is downwards
                        else if (directionToPlayer.y < -0.01f)
                            //commonRigidbody.velocity = new Vector2(directionToPlayer.x * movementSpeed, -1 * movementSpeedY);
                            commonRigidbody.velocity = new Vector2(0, -1 * movementSpeedY);

                        //Player to the right
                        if (directionToPlayer.x > 0.01f)
                            commonRigidbody.velocity = new Vector2(1 * movementSpeed, commonRigidbody.velocity.y);

                        //Player to the left
                        else if (directionToPlayer.x < -0.01f)
                            commonRigidbody.velocity = new Vector2(-1 * movementSpeed, commonRigidbody.velocity.y);
                        */

                        /*
                        //Player to the right
                        else if (directionToPlayer.x > 0.01f)
                            commonRigidbody.velocity = new Vector2(1 * movementSpeed, directionToPlayer.y * movementSpeedY);

                        //Player to the left
                        else if (directionToPlayer.x < -0.01f)
                            commonRigidbody.velocity = new Vector2(-1 * movementSpeed, directionToPlayer.y * movementSpeedY);
                        */
                        #endregion



                    }
                    else//Player is not obstructed, Chase!
                    {

                        DetermineFlipSprite(directionToPlayer.x);

                        commonRigidbody.velocity = new Vector2(directionToPlayer.x * currentMovementSpeed, directionToPlayer.y * currentMovementSpeedY);
                    }
                    //Debug.Log("directionToPlayer: " + directionToPlayer);

                }

                isOnOriginPosition = false;
            }
            //Else, moving towards original location(spawn point)
            else if (isOnOriginPosition == false)
            {
                //Debug message
                if (isOnOriginPosition)
                    Debug.LogError("Moving to origin position but also at origin position? wtf.");

                directionToOriginPos = originPosition - transform.position;

                //Debug.Log("Magnitude:distancecollider" + directionToOriginPos.magnitude + "   " + colliderExtent * 1.1f);

                if (directionToOriginPos.magnitude < colliderExtent) //* 1.1f)//it used to be 2, but if u think about it, horizontal/vertical so 1.2 is enough.
                {
                    isMoving = false;
                    isOnOriginPosition = true;
                    if (!canFly)
                        commonRigidbody.velocity = new Vector2(0f, commonRigidbody.velocity.y);
                    else
                        commonRigidbody.velocity = Vector2.zero;

                    targetAnimationState = IDLE;

                }
                else
                {
                    //Check if right above originPos
                    //Debug.Log("Distance is: " + (originPosition - transform.position));

                    directionToOriginPos = directionToOriginPos.normalized;


                    if (!canFly)
                    {
                        DetermineFlipSprite(directionToOriginPos.x);

                        if (sideWallDetected)
                            commonRigidbody.velocity = new Vector2(0, commonRigidbody.velocity.y);
                        else
                            commonRigidbody.velocity = new Vector2(directionToOriginPos.x * currentMovementSpeed, commonRigidbody.velocity.y);

                        /*
                        bool switchCondition = true;
                        if (jumpAction != null)
                        {
                            if (jumpAction.rising)
                                switchCondition = false;
                            else
                                switchCondition = true;
                        }

                        if (isGrounded == false && sideWallDetected == true && switchCondition)
                            commonRigidbody.velocity = new Vector2(0, commonRigidbody.velocity.y);
                        else
                            commonRigidbody.velocity = new Vector2(directionToOriginPos.x * currentMovementSpeed, commonRigidbody.velocity.y);
                        */
                    }
                    else
                    {
                        if (isOriginPositionObstructed)
                        {
                            //Coulda go with topleftright, bottomleftright. One more velocity apply per frame pfff...
                            //Origin is upwards
                            if (directionToOriginPos.y > 0.01f)
                                commonRigidbody.velocity = new Vector2(0, 1 * currentMovementSpeedY);

                            //Origin is downwards
                            else if (directionToOriginPos.y < -0.01f)
                                commonRigidbody.velocity = new Vector2(0, -1 * currentMovementSpeedY);

                            //Origin to the right
                            if (directionToOriginPos.x > 0.01f)
                                commonRigidbody.velocity = new Vector2(1 * currentMovementSpeed, commonRigidbody.velocity.y);

                            //Origin to the left
                            else if (directionToOriginPos.x < -0.01f)
                                commonRigidbody.velocity = new Vector2(-1 * currentMovementSpeed, commonRigidbody.velocity.y);
                        }
                        else
                            commonRigidbody.velocity = new Vector2(directionToOriginPos.x * currentMovementSpeed, directionToOriginPos.y * currentMovementSpeedY);

                        DetermineFlipSprite(commonRigidbody.velocity.x);
                    }

                }
            }
            else if (isOnOriginPosition == true)
                StandStill();

            //Debug.Log("target state is: " + targetAnimationState + "and current is: " + currentAnimationState);
            NewStateTransition();

        }
        else if (isOnOriginPosition && !isAttacking && !isJumping)
        {
            //So it won't set velocity every fixedFrame, it just accesses it; usually ;)
            if (commonRigidbody.velocity != Vector2.zero)
            {
                if (!canFly)
                    commonRigidbody.velocity = new Vector2(0f, commonRigidbody.velocity.y);
                else
                    commonRigidbody.velocity = Vector2.zero;
            }


            StandStill();
        }
        else//Standing
        {

            if (!isAttacking && !isJumping && isGrounded)
            {
                //So it won't set velocity every fixedFrame, it just accesses it; usually ;)
                if (commonRigidbody.velocity != Vector2.zero)
                {
                    if (!canFly)
                        commonRigidbody.velocity = new Vector2(0f, commonRigidbody.velocity.y);
                    else
                        commonRigidbody.velocity = Vector2.zero;
                }

                StandStill();
            }

        }

        //Debug.Log("Velocity is: " + commonRigidbody.velocity);


        //lastPosition = transform.position;
    }

    

    public void StandStill()
    {
        //Spagghetti, don't ask. (somewhere, it goes to state 0/Idle, but it doesn't play the idle animation, like wth. Hence, checking every frame feelsbadman
        if (animatorController.GetCurrentAnimatorStateInfo(0).IsName("Idle") == false && isGrounded && isHit == false)
            animatorController.Play("Idle");

        //Debug.Log("Standing Still.");
        //Play Stand Animation
        targetAnimationState = IDLE;
        NewStateTransition();

        isMoving = false;

        //commonRigidbody.velocity = Vector2.zero;
    }

    public void PlayHitAnimation()
    {
        if (isGrounded)
            animatorController.Play("Hit Ground");
        else
            animatorController.Play("Hit Midair");
    }

    public void SetIsGrounded(bool value)
    {
        //if (canFly)
        //return;

        isGrounded = value;

        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            return;

        if (value == true)
        {
            //Fell from midair/jumping/falling, so, go back to idle.
            if (currentAnimationState == FALLING || currentAnimationState == JUMP)
            {
                //Debug.Log("Idle now");

                //Play Idle animation
                targetAnimationState = IDLE;
                NewStateTransition();
            }

            //Initialization status. While it will run many useless times, it is here to ensure the "Activated" Idle, the first one when on the floor.
            if (currentAnimationState == IDLE)
                animatorController.Play(StateToString(targetAnimationState));
        }
        else
        {
            //Checks if fell off a ledge
            if (!isJumping)
            {
                EnemyRamAttack tempRamAttack = currentAttack as EnemyRamAttack;
                if (tempRamAttack != null)
                    return;

                targetAnimationState = FALLING;
                NewStateTransition();
            }
        }
    }

    public void TouchedGround()
    {
        if (canFly)
            return;

        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            return;

        commonRigidbody.velocity = new Vector2(0f, 0f);

        isJumping = false;

        //TODO: Check this out below, does it work as intended?
        isMoving = false;
        //If out of range, it will midair go towards origin point.

    }

    public void DetermineFlipSprite(float moveX)
    {

        if (moveX < 0f && facingRight)
            FlipSprite();
        else if (moveX > 0f && facingRight == false)
            FlipSprite();
    }

    public void FlipSprite()
    {
        facingRight = !facingRight;
        Vector2 localScale = transform.localScale;
        localScale.x = localScale.x * -1;
        transform.localScale = localScale;

        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() && NetworkDamageShare.globalInstance.IsSynchronized())
            NetworkCommunicationController.globalInstance.SendMonsterFacing(facingRight, enemyListIndex);
    }

    public void NewStateTransition()
    {
        if (currentAnimationState != targetAnimationState)
        {

            //FlipSprite bug...
            //if (canFly && currentAnimationState == MELEE_ATTACK)
                //DetermineFlipSprite(directionToPlayer.x);

            if (isHit && (currentAnimationState == HITMIDAIR || currentAnimationState == HITGROUND))
                return;

            //Experimental spagghetti! (running almost every frame instead of event-based because i fucked up the state machine...)
            if ((isGrounded == false && isJumping == false) && targetAnimationState != HITMIDAIR && canFly == false)
            {
                if (animatorController.GetCurrentAnimatorStateInfo(0).IsName("Falling") == false)
                    animatorController.Play("Falling");

                //Update state
                currentAnimationState = targetAnimationState;

                return;
            }

            //Since isMoving goes with jumping, moving animation denies jump's
            if (targetAnimationState == RUN)
            {
                if ((currentAnimationState == JUMP_CHARGE || currentAnimationState == JUMP))
                    return;
                else if (currentAnimationState == FALLING && isGrounded == false)//No fallSliding lel. Not fixed?!
                    return;
            }
            


            //Transitions to state.
            animatorController.Play(StateToString(targetAnimationState));

            //Update state
            currentAnimationState = targetAnimationState;
        }
    }

    //Returns string from every state.
    public string StateToString(int state)
    {
        if (state == IDLE)
            return "Idle";
        else if (state == RUN)
            return "Run";
        else if (state == MELEE_ATTACK)
            return "Melee Attack";
        else if (state == MELEE_ATTACK2)
            return "Melee Attack2";
        else if (state == RANGED_ATTACK)
            return "Ranged Attack";
        else if (state == RAM_ATTACK)
            return "Ram Attack";
        else if (state == MELEE_ATTACK_CHARGE)
            return "Melee Attack Charge";
        else if (state == MELEE_ATTACK2_CHARGE)
            return "Melee Attack2 Charge";
        else if (state == RANGED_ATTACK_CHARGE)
            return "Ranged Attack Charge";
        else if (state == RAM_ATTACK_CHARGE)
            return "Ram Attack Charge";
        else if (state == HITGROUND)
            return "Hit Ground";
        else if (state == HITMIDAIR)
            return "Hit Midair";
        else if (state == JUMP_CHARGE)
            return "Jump Charge";
        else if (state == JUMP)
            return "Jump";
        else if (state == FALLING)
            return "Falling";
        else if (state == DEATH)
            return "Death";


        //If none of the above.
        Debug.LogError("Animation invalid.");
        return null;
    }

    [Button("Take 1 Damage")]
    public void ETakeDamage()
    {
        TakeDamage(1, Vector2.zero);
    }

    public void TakeDamage(int damageTaken, Vector3 damageOrigin, int knockbackPowerX = 0, int knockbackPowerY = 0, float hitstunMultiplier = 1f, bool hitByStrongPush = false, bool hitByHazard = false)
    {
        //Should be borderline impossible to get here, now that i hacked the corpse-combo hack.
        if (isDying)
        {
            if (hitByHazard == false)
            {
                DetermineVFXHit(damageTaken, damageOrigin, false);

                DetermineDirectionHit(damageOrigin, false);

                if (directionHit.y > 0.95f)
                    return;

                //This may be complicated, do read the variable "deathResets" details.
                deathResets++;
                Invoke("DeathFreeze", currentTimeToDeathFreeze);

                
            }//else return;

            return;
        }

        CurrentHealth = CurrentHealth - damageTaken;

        //So at fixedUpdate, the force is applied, and monster cannot act till it finishes
        isHit = true;
        currentHitTime = 0f;

        DetermineInterruptCurrentAttack();

        Invoke("HitStopped", hitRecoveryTime * hitstunMultiplier);

        //VFX of damage here
        //VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedEnemy, Vector3.Lerp(transform.position, damageOrigin, 0.2f), this.gameObject);
        DetermineVFXHit(damageTaken, damageOrigin, hitByHazard);
        DetermineDirectionHit(damageOrigin, hitByHazard);
        if (hitByHazard)
            currentHitMovementMultiplier = hitMovementMultiplier * 4;//so it looks more obvious/dynamic to the player.
        else if (hitByStrongPush)
            currentHitMovementMultiplier = hitMovementMultiplier * 3;//so it looks more obvious/dynamic to the player.

        Debug.DrawRay(transform.position, directionHit, Color.black, 1);

        //Dead
        if (CurrentHealth < 1)
        {
            //Play the Hit animation
            if (isGrounded)
                animatorController.Play("Hit Ground");
            else
                animatorController.Play("Hit Midair");

            Die();
        }
        else
        {
            //Play the Hit animation
            if (isGrounded)
                targetAnimationState = HITGROUND;
            else
                targetAnimationState = HITMIDAIR;

            NewStateTransition();
        }
    }

    public void DetermineVFXHit(int damageTaken, Vector3 damageOrigin, bool hitByHazard = false)
    {
        if (hitByHazard == false)
        {
            if (damageTaken != 0)//Player's Push.
            {
                if (monsterType != Harpy)//anything except harpy
                    VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedEnemy, transform.position);
                else
                    VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedEnemy, Vector3.Lerp(transform.position, damageOrigin, .5f));//So it will "hit" harpy's body and not the air...
            }
        }
    }

    public void DetermineDirectionHit(Vector3 damageOrigin, bool hitByHazard = false)
    {
        if (hitByHazard)
            directionHit = (damageOrigin - transform.position).normalized;
            //So it resets if hit by player.
            //currentHitMovementMultiplier = hitMovementMultiplier;
        else
            directionHit = directionToPlayer;

    }

    public void StompPushed(bool OriginIsRightDirection, bool stompedByPlayer = true)
    {
        //So at fixedUpdate, the force is applied, and monster cannot act till it finishes
        isHit = true;
        currentHitTime = 0f;

        //Interrupt
        DetermineInterruptCurrentAttack();

        if (stompedByPlayer)
            Invoke("HitStopped", hitstunPlayerStompsDuration);
        else
            Invoke("HitStopped", hitstunStompPlayerDuration);

        //Wind VFX?
        if (OriginIsRightDirection)
            directionHit = Vector3.left;
        else
            directionHit = Vector3.right;

        //Debug.Break();
        //Debug.DrawRay(transform.position, directionHit, Color.magenta, 1f);

        //Play the Hit animation
        if (isGrounded)
            targetAnimationState = HITGROUND;
        else
            targetAnimationState = HITMIDAIR;

        NewStateTransition();
    }

    public void DetermineInterruptCurrentAttack()
    {
        if (currentAttack != null && isAttacking)
        {
            if (GameManager.testing)
                Debug.Log("Interruption time!");
            currentAttack.InterruptAttack();
        }
    }

    /// <summary>
    /// Put all the functions together, when lava hits.
    /// </summary>
    public void LavaHit(float deathFreezeValue)
    {
        DetermineInterruptCurrentAttack();
        SetTimeToDeathFreeze(deathFreezeValue);
        Die();
    }

    /// <summary>
    /// Made because IDamageable is hard-coded("Unity supports interfaces" quote here) and adding an difference on "hitByLava" or any new argument fucks everything up. At this point, I should pass the entire gameobject to avoid the arguments.
    /// Aside of the rant, this function is used for Die(), so it invokes deathfreeze on X seconds. (it resets it, but is used by lava currently)
    /// </summary>
    /// <param name="_value"></param>
    public void SetTimeToDeathFreeze(float _value)
    {
        currentTimeToDeathFreeze = _value;
    }

    //tfw no Die function with parameters, like no killcount increase.
    [Button("Omae wa mo... Shindeiru.")]
    public void Die()
    {
        //if (GameManager.testing)
            //Debug.Log("You killed an enemy. Y-You monster!");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
        if (isDying)
        {
            //This probably happens when multi-hit, from the playtesting. Assumed, because hitting an enemy, then hitting hazard while dying causes this.
            Debug.Log("OI! BREAKTIME! Die() called twice!!!! WTF!?!!");
            Debug.Break();
        }
#endif

        PlayerStatsManager.globalInstance.IncreaseKillCount();

        //Double the killcount if desynced (hence having a reason to desync)
        if (NetworkCommunicationController.globalInstance != null && NetworkDamageShare.globalInstance.IsSynchronized() == false)
            PlayerStatsManager.globalInstance.IncreaseKillCount();
        
        
        //Making sure the killer won't get more buffs.
        if (playerKillerHP != 0)
        {
            currentMovementSpeed = movementSpeed;
            currentMovementSpeedY = movementSpeedY;
            Destroy(PlayerKillerVFX);
        }
            

        playerKillerHP = 0;


        /*
        if (isGrounded)
            //Ground animation
        else
            //Midair animation
        */

        //Hacky, but hey, if it works.(fml!)
        if (canFly)
            commonAura.playerInRange = false;

        //stronger damage VFX cuz kill
        if (maxHealth > 1)//This limits it so fodder trash wont have epic vfx :P
            VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DyingEnemy, transform.position);//Right now, this is placeholder asset. Sad...

        //if (gameObject.activeInHierarchy)
        //"Freeze" next 2 frames(considering 60 fps). Aka, Freeze-hit technique.
        //If insane, no deathfreeze cuz it fucks with insanity!
        if (GameObject.FindGameObjectWithTag("BackgroundManager").GetComponent<InsanityToggle>().GetInsanityActive() == false)
            StartCoroutine(FreezeGame(0.03f));//Right now, it is 3 frames.


        //Stop the coroutine(check if it has not been disabled from others)
        if (coroutineRunning)
        {
            coroutineRunning = false;
            StopCoroutine(UpdateBehaviour());//so it doesnt check if(isDying) inside coroutine.
        }

        isDying = true;

        //Play Idle + isMoving = false;
        StandStill();
        PlayHitAnimation();//a little hack, but it works. (**spagghetti LEVEL UP!**)

            //tfw u do a hack to put deathfreeze&corpsecombos, but then do another hack to remove it... thisisfine.
            //So, the right below 2 lines, bypass the hack, so there wont be any corpse combos. 
            //For the following game design reason:
            //  Dead enemies, hinder/block movement, so u cannot dash when dead, because they are alive for at least 0.4 seconds, so as you can attack them.
            //  and movement > random easter egg mechanic.
            deathResets = 0;

            //Disable polygonal collider2D, so it wont block the player while dying! (it also means no attack/corpsecombos while dying!)
            //For example, killing then dashing will slightly block player!
            IgnorePlayerCollision(true);

            //Hopefully in the future u will make a corpse combo, properly implemented by default.
            //aka the more u hit, the more the knockback, so its more like juggling and chasing the corpse b4 it touches the ground POG
            Invoke("DeathFreeze", currentTimeToDeathFreeze);

        currentTimeToDeathFreeze = timeToDeathFreeze;

        //If online, then un-register sending position
        if (NetworkCommunicationController.globalInstance != null)
        {
            //Register which enemy to no longer update
            NetworkEnemyPositionInterpolationController.globalInstance.RemoveEnemyIndex(enemyListIndex);
        }
    }

    //This method is called when dead, instead of pathfinder's disable, it does pathfinder's disable (components&coroutines) on top of disabling the gameobject and pathfinder.
    public void DeathFreeze()
    {

        if (deathResets == 0)//check the variable details.
        {
            if (monsterType == 0)//hollow
                VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.EnemyMagicalBurst, transform.position);

            //Calls it via EnemyPathfinder.
            if (died != null)
                died.Invoke();
        }
        else
        {
            deathResets--;
            Debug.Log("GOT IT BOIII: " + deathResets);
        }

            

        
    }

    public IEnumerator FreezeGame(float duration)
    {
        //Comments are to count the frames.
        
        //float frames = Time.frameCount;
        //float frames2 = Time.renderedFrameCount;

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;

        /*
        Debug.Log("FRAMES BEFORE:: " + frames + " " + frames2);

        frames = Time.frameCount;
        frames2 = Time.renderedFrameCount;

        Debug.Log("FRAMES BEFORE:: " + frames + " " + frames2);
        */
    }

    public void SetStartingHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public void HitStopped()
    {
        isHit = false;

        currentHitMovementMultiplier = hitMovementMultiplier;
    }

    private void OnDisable()
    {
        StandStill();

        /*
         isMoving = false;
        if (isGrounded)
        {
            StandStill();
        }
        else
        {
            //Making sure of the above, so it won't bug.
            currentAnimationState = FALLING;
            targetAnimationState = FALLING;
        }
        */

        //If online and host, then un-register sending position
        if (NetworkCommunicationController.globalInstance != null)
        {
            //Register which enemy to no longer update
            NetworkEnemyPositionInterpolationController.globalInstance.RemoveEnemyIndex(enemyListIndex);
        }



        //Stop the coroutine(check if it has not been disabled from others)
        if (coroutineRunning)
        {
            coroutineRunning = false;
            StopCoroutine(UpdateBehaviour());
        }

    }

    //Reactivated from pathfinder, aka player is near.
    private void OnEnable()
    {
        //pfff, my CPU is crying for doing the below calculation here.
        colliderExtent = GetComponent<Collider2D>().bounds.extents.magnitude;

        //Reset dis (useful only when flying, aka harpy)
        flyingObstructionType = (int) flyingObstructionDirection.NONE;

        StandStill();

        //If online, then register sending position
        if (NetworkCommunicationController.globalInstance != null)
        {
            //Register which enemy to update
            NetworkEnemyPositionInterpolationController.globalInstance.AddEnemyIndex(enemyListIndex, commonRigidbody);            

            if (NetworkCommunicationController.globalInstance.IsServer() && NetworkDamageShare.globalInstance.IsSynchronized())
                NetworkCommunicationController.globalInstance.SendMonsterFacing(facingRight, enemyListIndex);
        }

        if (!coroutineRunning)
            StartCoroutine(UpdateBehaviour());
    }

    //Called from LevelManager->Pathfinder->this, so it goes back to origin position, and gets full hp :P AND! pathfinder when re-enabled. So, after every restart basically.
    public void Revive()
    {
        //So it won't think it's dead. (rip fixedUpdate bugs)
        isDying = false;

        //Goes back to spawn position
        transform.position = originPosition;//Perhaps rigidbody.position? Probably not.

        //Resets hp.
        if (playerKillerHP == 0)
            currentHealth = maxHealth;

        //Reset cooldowns (Minotaur is pleased.)
        for (int i = 0; i < possibleAttacks.Count; i++)
            possibleAttacks[i].ResetAttackCooldown();

        //Reset the direction when player lands atop of satyr
        if (monsterType == Satyr || monsterType == Satyr2)
            satyrBugfixJumpRight = 0;

        //No idea if it works, but there is a bug of enemies attacking on spawn, from the before-restarted level. This should fix it//If not, put it OnEnabled().
        DetermineInterruptCurrentAttack();

        //If ignored on death previously, re-activate here.
        //DeterminePlayerCollisionActivation();//Literally 2 lines so not worth it.
        if (playerCollidersCached == true && playerCollidersIgnored == true)
            IgnorePlayerCollision(false);

        if (canFly)//Before flying.... all the A.I. code was so beautiful and organized. Not perfect but not even close to this...
            GetComponentInChildren<HarpyAuraAttack>().SetStartingMana(3);
    }

    //Called by Pathfinding, very frequently :P
    public void RefreshBehaviour(Vector3 _directionToPlayer, float _distanceFromPlayer, bool _isPlayerObstructed = false, bool _isPathfindingHazardous = false, bool _isPlayerOnPlatform = false, bool _isOriginPositionObstructed = false)
    {
        directionToPlayer = _directionToPlayer;
        distanceFromPlayer = _distanceFromPlayer;
        isPlayerObstructed = _isPlayerObstructed;
        isPathfindingHazardous = _isPathfindingHazardous;
        isPlayerOnPlatform = _isPlayerOnPlatform;
        isOriginPositionObstructed = _isOriginPositionObstructed;

        //"Unlock" A.I. direction.
        if (canFly && isPlayerObstructed == false)
            flyingObstructionType = (int)flyingObstructionDirection.NONE;
    }

    public void InitializeActionVariables()
    {
        if (possibleAttacks.Count == 0)
        {
            hasAttackAction = false;
            return;
        }
        hasAttackAction = true;

        //Check if there is a null attack slot, so it doesn't error.
        for (int j = 0; j < possibleAttacks.Count; j++)//This iteration is to make sure it won't fuck up with many attacks/ranges
        {
            bool deleteDis = false;
            if (possibleAttacks[j] == null)
                deleteDis = true;

            if (deleteDis)
            {
                possibleAttacks.RemoveAt(j);
                j = -1;//so it goes back to the beginning, cuz the list auto-sorts its indexes.
            }

            //Debug.LogError("An attack in EnemyBehaviour list, is null/empty/undefined. -> NullReferenceException");
        }

        //Sorting the list, lower slots are lower range ;)
        BaseEnemyAttack tempAttack;
        for (int j = 0; j < possibleAttacks.Count - 1; j++)//This iteration is to make sure it won't fuck up with many attacks/ranges
        {
            //Literally swapping, depending on lowrange
            for (int i = 1; i < possibleAttacks.Count; i++)
            {

                if (possibleAttacks[i].attackRange < possibleAttacks[i - 1].attackRange)
                {
                    tempAttack = possibleAttacks[i];
                    possibleAttacks[i] = possibleAttacks[i - 1];
                    possibleAttacks[i - 1] = tempAttack;
                }
            }
        }
    }

    //When enemy is right above player, via ledgedrop or jump, he shouldnt just bounce off or walk on him xD
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.contacts[0].normal.y > 0.95f)
            {
                //If satyr or minotaur.
                if ( (isGrounded == false) && (monsterType == Satyr || monsterType == Minotaur || monsterType == Satyr2 || monsterType == Minotaur2) )
                {
                    //Without isGrounded==false condition, so many bs deaths happen.

                    //Stomp attack!
                    GetComponent<EnemyJumpStompAttack>().CompleteAttack();
                }
                else
                {
                    //Debug.Log("Detected");
                    //Debug.Break();
                    Debug.Log("Enemy Upwards collision");

                    //Push Player to the left
                    collision.gameObject.GetComponent<WarriorHealth>().StompPushed(false, stompKnockbackPower, hitstunStompPlayerDuration);

                    //TODO: Check if stompPush should always be towards the left? 
                    //I mean game-design-wise, it works for every direction, but I think the best solution is to push towards proper direction (left or right depending on vector2 difference) instead of 100% left and 100% right

                    //Push this to the right a bit
                    StompPushed(true, false);
                }
            }
            else if ((monsterType == Minotaur || monsterType == Minotaur2) && collision.contacts[0].normal.y < -0.95f)//MinotaurImpale
            {
                collision.gameObject.GetComponent<WarriorHealth>().TakeDamage(1, transform.position, 15, 18, 0.2f);
            }


        }
        //Harpy Wallbump. See FixedUpdate -> isMoving -> canFly && playerObstructed.
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            Vector2 collisionPoint = collision.contacts[0].normal;

            if (canFly && isPlayerObstructed)
            {
                Debug.Log("Change collision direction! " + collisionPoint);

                //The below are ifs instead of elseifs... butwhy.

                //Collided with something at the left side
                if (collisionPoint.x < -0.9f)
                {
                    if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
                        flyingObstructionType = (int)flyingObstructionDirection.TOPLEFT;
                    else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMRIGHT)
                        flyingObstructionType = (int)flyingObstructionDirection.BOTTOMLEFT;

                    DetermineFlipSprite(-1);

                    //Debug.Log("right: new direction " + flyingObstructionType);
                }

                //Collided with something at the right side
                if (collisionPoint.x > 0.9f)
                {
                    if (flyingObstructionType == (int)flyingObstructionDirection.TOPLEFT)
                        flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;
                    else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMLEFT)
                        flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;

                    DetermineFlipSprite(1);

                    //Debug.Log("left: new direction " + flyingObstructionType);
                }

                //Collided with something at the DOWNwards side
                if (collisionPoint.y < -0.9f)
                {
                    if (flyingObstructionType == (int)flyingObstructionDirection.TOPLEFT)
                        flyingObstructionType = (int)flyingObstructionDirection.BOTTOMLEFT;
                    else if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
                        flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;

                    //Debug.Log("up: new direction " + flyingObstructionType);
                }

                //Collided with something at the UPwards side
                if (collisionPoint.y > 0.9f)
                {
                    if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMLEFT)
                        flyingObstructionType = (int)flyingObstructionDirection.TOPLEFT;
                    else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMRIGHT)
                        flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;

                    //Debug.Log("down: new direction " + flyingObstructionType);
                }
            }
            else//Not harpy/flying
            {
                if (collisionPoint.x != 0)
                {
                    if (collision.gameObject.CompareTag("Platform"))
                    {
                        //This is for those moments a monster is running, but a platform acts as a wall. REEEEEEEEEEEEEEEEEEEE
                        sidePlatformIgnored = collision.gameObject;
                        //If not ignoring
                        if (Physics2D.GetIgnoreCollision(commonCollider, sidePlatformIgnored.GetComponent<PlatformBehaviour>().platformCollider) == false)
                            //Ignore collision with the platform.
                            Physics2D.IgnoreCollision(commonCollider, sidePlatformIgnored.GetComponent<PlatformBehaviour>().platformCollider, true);
                    }
                    else//Ground
                    {
                        sideWall = collision.gameObject;
                        sideWallDetected = true;

                        //Disgusting hack. If level editor and not satyr, then sidewalled = false...
                        if (LevelManager.currentLevel == 7 && monsterType != Satyr && monsterType != Satyr2)
                            sideWallDetected = false;

                        Debug.Log("SidewallDetected Collision Point X,Y is: " + collisionPoint.x + ", " + collisionPoint.y);
                    }
                    
                }
            }
        }
    }

    //For when hazard is ahead, and change!
    public void RotateFlyingDirection()
    {
        if (flyingObstructionType == (int)flyingObstructionDirection.TOPRIGHT)
            flyingObstructionType = (int)flyingObstructionDirection.BOTTOMRIGHT;
        else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMRIGHT)
            flyingObstructionType = (int)flyingObstructionDirection.BOTTOMLEFT;
        else if (flyingObstructionType == (int)flyingObstructionDirection.BOTTOMLEFT)
            flyingObstructionType = (int)flyingObstructionDirection.TOPLEFT;
        else if (flyingObstructionType == (int)flyingObstructionDirection.TOPLEFT)
            flyingObstructionType = (int)flyingObstructionDirection.TOPRIGHT;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //Debug.Log("collision.contacts.Length is: " + collision.contacts.Length+ " and its name is: " + collision.gameObject.name);//why 0 length?

        if (collision.gameObject == sideWall)
            sideWallDetected = false;

        /*
        //Stop ignoring the platform that was side-ignored
        if (sidePlatformIgnored != null && sidePlatformIgnored == collision.gameObject)
        {
            //If ignoring
            if (Physics2D.GetIgnoreCollision(commonCollider, sidePlatformIgnored.GetComponent<PlatformBehaviour>().platformCollider) == true)
                //De-Ignore collision with the platform.
                Physics2D.IgnoreCollision(commonCollider, sidePlatformIgnored.GetComponent<PlatformBehaviour>().platformCollider, false);

            sidePlatformIgnored = null;
        }
        */
            
        /*
        //I really wanted to avoid using raycasting. Even if I "mastered" it at the previous project (which was actually small, and was completed in 2 days) there are so many pitfalls... Hopefully it is used only here, and it won't expand.

        //Raycasts both left and right, to detect the collision
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 4);

        Debug.DrawLine(transform.position, transform.position + new Vector3(4,0,0), Color.magenta, 2f);

        if (hit.collider != null && hit.collider.gameObject == collision.gameObject)
            sideWallDetected = false;
        
        hit = Physics2D.Raycast(transform.position, Vector2.left, 4);

        if (hit.collider != null && hit.collider.gameObject == collision.gameObject)
            sideWallDetected = false;
            */

        /*//Why are the contacts.length always 0?! no idea if bug, but no time fml1@#!
        if (collision.contacts.Length != 0 && collision.contacts != null)//Dissapearing in thin air to help CPU sucks sometimes.
        {
            Debug.Log("Exit point: " + collision.contacts[0].normal);

            if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform")) && collision.contacts[0].normal.x != 0)
                sideWallDetected = false;//Used in velocity, so monster won't "stick" to wall while jumping
        }
          */
    }

    /// <summary>
    /// Called from BaseEnemyAttack. By putting a value, it increases this monster's hp + movement speed, and adds it the playerkillerVFX if not added.
    /// </summary>
    /// <param name="value"></param>
    public void AddPlayerKillerHP(int value, bool notifyOtherPlayer = true)//usual is 2. Boolean is for netcoding, otherwise infinite loop happens
    {
        Debug.Log("Upgrading Monster!");
        //Bonuses are reset when enemy/monster Dies();

        //Level Editor
        if (LevelManager.currentLevel == 7)
            return;

        playerKillerHP += value;

        //Increase health
        currentHealth += playerKillerHP;

        //Increase Movement Speed by 50%
        currentMovementSpeed += movementSpeed * 0.5f;
        currentMovementSpeedY += movementSpeedY * 0.5f;

        //SFX
        PlayerSoundManager.globalInstance.PlayTempHPGainSFX();

        //TempHPGain VFX
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.TempHPGain, transform.position + new Vector3(-6.5f, 8, 0), gameObject);

        if (PlayerKillerVFX == null)
        {
            //Playerkiller VFX
            VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.PlayerKiller, transform.position, gameObject);
            PlayerKillerVFX = VFXManager.globalInstance.GetLastCreatedVFX();//Coulda change the void return type to gameobject instead of this hacky way, but bugs may arise.
        }

        //Netcoding
        if (playerKillerEvent != null && notifyOtherPlayer)
            playerKillerEvent(enemyListIndex);
    }

    /// <summary>
    /// Called true on death, false on revive.
    /// Using code from PlatformBehaviour. 
    /// \nDepending on boolean parameter, ignores player's colliders, or de-ignores them. Doing caching too if not done before (always on Die since cant revive if not ded)
    /// </summary>
    /// <param name="toIgnore"></param>
    public void IgnorePlayerCollision (bool toIgnore)
    {
        if (playerCollidersCached == false)
        {
            playerCollidersCached = true;

            //Store player's colliders here
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            boxPlayerCollider = player.GetComponent<BoxCollider2D>();
            circlePlayerCollider = player.GetComponent<CircleCollider2D>();
        }

        playerCollidersIgnored = toIgnore;

        Physics2D.IgnoreCollision(commonCollider, boxPlayerCollider, toIgnore);
        Physics2D.IgnoreCollision(commonCollider, circlePlayerCollider, toIgnore);
    }



    //Fetched from pathfinder on flying enemies, so it won't store/cache rigidbody.
    public Vector2 GetVelocity()
    {
        return commonRigidbody.velocity;
    }

    //Shows the radius
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Draw attack range
        Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, SightRange);
    }
#endif

    //TODO: Cache the below, ffs.
    //TODO: Make this IEnumerator, and make the lines last as much the yield does.
#if UNITY_EDITOR
    //Called every Update()
    public void DisplayVisionHitbox()//Useless now, replaced by the above method
    {


        //Calculate the hitboxes
        Vector3 tempPoint;
        Vector3 visionTopLeft = new Vector2(SightRange * -1, SightRange);
        Vector3 visionBottomRight = new Vector2(SightRange, -1 * SightRange);

        //Top-Left to Bottom-left
        tempPoint = new Vector3(visionBottomRight.x * -1, visionBottomRight.y);
        Debug.DrawLine(transform.position + visionTopLeft, transform.position + tempPoint, Color.white);

        //Top-Right to Bottom-Right
        tempPoint = new Vector3(visionTopLeft.x * -1, visionTopLeft.y);
        Debug.DrawLine(transform.position + tempPoint, transform.position + visionBottomRight, Color.white);

        //Bottom-Left to Bottom-Right
        tempPoint = new Vector3(visionBottomRight.x * -1, visionBottomRight.y);
        Debug.DrawLine(transform.position + tempPoint, transform.position + visionBottomRight, Color.white);

        //Top-Right to Top-Left
        tempPoint = new Vector3(visionTopLeft.x * -1, visionTopLeft.y);
        Debug.DrawLine(transform.position + tempPoint, transform.position + visionTopLeft, Color.white);
    }
#endif

    public void SetIsAttacking(bool value)
    {
        isAttacking = value;
    }

    public void SetIsJumping(bool value)
    {
        isJumping = value;
    }

    public bool GetFacingRight()
    {
        return facingRight;
    }

    public Vector2 GetDirectionToPlayer()
    {
        return directionToPlayer;
    }

    public bool IsCoroutineRunning()
    {
        return coroutineRunning;
    }

    //By player/warrior events/subscriptions, aka observer-pattern
    public void PlayerIsMidair()
    {
        playerIsMidair = true;
    }

    public void PlayerIsGrounded()
    {
        playerIsMidair = false;
    }

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}
