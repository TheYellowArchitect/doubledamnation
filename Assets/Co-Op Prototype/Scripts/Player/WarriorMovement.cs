using System;
using System.Collections;
using System.Collections.Generic;
using EZCameraShake;
using NaughtyAttributes;
using UnityEngine;

//Behold, when you get lazy or in rush, "God Object" happens. pfff. Single responsibility principle ruined. This class is more like a Warrior(Controller), instead of just movement. it has pretty much everything you can think of.
//TODO: visual notification when Xaxis sidestep happens(e.g.by getting moved left, and also going left)
[DisallowMultipleComponent]
public class WarriorMovement : MonoBehaviour
{
    public static WarriorMovement globalInstance;

    [InfoBox("This is the most horrible code in this entire project, so do tread carefully. You better use the (public Inspector) variables below, and not delve into the code unless you know what you are doing :(")]

    [InfoBox("Vertical Speed: 13(8)\nMidairGravity: 18.5(13)")]


    public bool VerticalMovementSpeed = true;
    [Header("Movement Speed (Vertical)")]

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("Speed to go down/up[0,1] while midair, via input.\n(Recommended: 8)")]
    public float VerticalControlSpeed;//8f

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("Determines the magnitude of the jump, aka the scale, since all the VerticalJumpVariables are multipled with this, to determine Yspeed :P")]
    public float VerticalSpeed;

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("In the total speedY, this is divided to it, and the final wallslide speed is calculated.\nThe bigger the number, the smaller the speed\n(Recommended: 1.15f)")]
    public float WallslideSpeed;//1.15f

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("Added into influenceY upon Jumping.\nThe bigger the number, the higher the jump.\n(Recommended: 20f)")]
    public float FirstJumpPower;//18f

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("Added into influenceY upon Jumping a 2nd time.\nThe bigger the number, the higher the jump.\n(Recommended: 8f)")]
    public float SecondJumpPower;//8f

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("Added into influenceY upon WallJumping.\nThe bigger the number, the higher the jump.\n(Recommended: 17f)")]
    public float WalljumpPower;//17f

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("When walljumping with a lot of influenceX, it doesnt add the influenceX onto influenceY as is, but divides by this very number first!")]
    public float WalljumpPowerDivider = 2.5f;

    [ShowIf("VerticalMovementSpeed")]
    [Tooltip("The bigger the number, the faster the fastfall.\nIt scales with negativeY input\n(Recommended: 25f)")]
    public float FastfallControlSpeed;//25f. 
    //30 is for when u implement actual fastfall visually, so it matches "insta-ground" (animation and proper wind vfx ftw)
    //Currently, having 30, it feels unnatural/weird.



    public bool HorizontalMovementSpeed = true;
    [Header("Movement Speed (Horizontal)")]
    //Following 3 are for X axis(HORIZONTAL), on different warrior conditions.(grounded,midair,crouching)
    [ShowIf("HorizontalMovementSpeed")]
    [Tooltip("Speed when not jumping or crouching. aka most of the game; grounded.\n(Recommended:7)")]
    public float movementSpeed;//7f

    [ShowIf("HorizontalMovementSpeed")]
    [Tooltip("Speed when jumping (midair==true) Shouldn't exceed movementSpeed(?).\n(Recommended:9)")]
    public float airSpeed;//9f

    [ShowIf("HorizontalMovementSpeed")]
    [Tooltip("Added into influenceX upon WallJumping.\nThe bigger the number, the strongest the horizontal speed.\n(Recommended: 5f)")]
    public float WalljumpBouncePower;//5f

    [ShowIf("HorizontalMovementSpeed")]
    [Tooltip("Upon going onto Level 3 (The winds of Oblivion), what should the speed increase be? Flat increase to both movementSpeed and airSpeed.")]
    public int FinalLevelMovementBoost = 10;


    public bool Gravity = true;
    [Header("Gravity")]

    [ShowIf("Gravity")]
    [Tooltip("Gravity when IsAtopGround()\n(Recommended: 6)")]
    public float GroundedGravity;//6f

    [ShowIf("Gravity")]
    [Tooltip("Gravity when midair\n(Recommended: 13)")]
    public float MidairGravity;//13f





    public bool DodgerollWavedash = true;
    [Header("Dodgeroll/Wavedash")]


    [ShowIf("DodgerollWavedash")]
    [Tooltip("Added into influenceX upon dodgerolling(ground).\nThe bigger the number, the fastest the speed.\n(Recommended: ?f)")]
    public float DodgerollPower;//?f

    [ShowIf("DodgerollWavedash")]
    [Tooltip("Added into influenceX upon wavedashing(air).\nThe bigger the number, the fastest the speed.\n(Recommended: ?f)")]
    public float WavedashPower;//?f

    [ShowIf("DodgerollWavedash")]
    [MinValue(0f)]
    [Tooltip("When again can dodgeroll be used?")]
    public float DodgerollCooldown;

    [ShowIf("DodgerollWavedash")]
    [MinValue(0f)]
    [Tooltip("How much does dodgeroll last?\nPlease match this with the animation time.")]
    public float DodgerollDuration;

    [ShowIf("DodgerollWavedash")]
    [Range(0f, 3f)]
    [Tooltip("When dodgeroll is opposing player1's movement direction, how more powerful should it be?")]
    public float DodgerollReverseMultiplier;

    [ShowIf("DodgerollWavedash")]
    [Range(0, 1)]
    [Tooltip("When player1 inputs down on movement joystick, how much should the multiplier on reduction be?")]
    public float DodgerollErosionPower;

    [ShowIf("DodgerollWavedash")]
    [MinValue(0f)]
    [Tooltip("After how many seconds from the start of a dodgeroll, does the active frame window for phantom dodge active/ready to be enabled?")]
    public float PhantomDodgerollStart;

    [ShowIf("DodgerollWavedash")]
    [MinValue(0f)]
    [Tooltip("After how many seconds from the start of phantom dodge active frames, does the active frame window close? aka get disabled?\nPlease less than DodgerollDuration or it bugs horribly.")]
    public float PhantomDodgerollEnd;


    public bool Attack = true;
    //Attack stuff
    [Header("Attack")]

    [ShowIf("Attack")]
    [MinValue(0f)]
    [Tooltip("How long is enemy \"stunned\" after an attack. A multiplier, 1 is the normal for monsters, since time knockbacked is:\nMonsterRecoveryTime * hitstun :P")]
    public float hitstun;

    [ShowIf("Attack")]
    [Tooltip("What layer contains stuff that can be damaged/killed by the spear?")]
    public LayerMask WhatIsDamageable;

    [ShowIf("Attack")]
    [Tooltip("When using the dash attack, how far away should player be pushed?")]
    public float DashAttackPower;


    public bool FrameData = true;
    [Header("Frame Data")]

    [ShowIf("FrameData")]
    [Tooltip("How much time till midair attacks start")]
    public float MidairAttackCastingTime;

    [ShowIf("FrameData")]
    [Tooltip("How much time till ground attacks start")]
    public float GroundAttackCastingTime;

    [ShowIf("FrameData")]
    [Tooltip("How much time till dash attack start")]
    public float DashAttackCastingTime;

    [ShowIf("FrameData")]
    [Tooltip("How much time till midair attacks ends")]
    public float MidairAttackRecoveryTime;

    [ShowIf("FrameData")]
    [Tooltip("How much time till ground attacks ends")]
    public float GroundAttackRecoveryTime;

    [ShowIf("FrameData")]
    [Tooltip("How much time till dash attack ends")]
    public float DashAttackRecoveryTime;


    public bool KnockbackPower = true;
    [Header("Knockback Power")]

    [ShowIf("KnockbackPower")]
    [Tooltip("How far does it launch an enemy into the X axis, with a Ground Attack")]
    public int GroundX;

    [ShowIf("KnockbackPower")]
    [Tooltip("How far does it launch an enemy into the X axis, with a Dash Attack")]
    public int DashX;

    [ShowIf("KnockbackPower")]
    [Tooltip("How far does it launch an enemy into the X axis, with a Midair Attack")]
    public int MidairX;

    [ShowIf("KnockbackPower")]
    [Tooltip("How far does it launch an enemy into the Y axis")]
    public int knockbackPowerY;



    public bool SpearColliders = true;
    [Header("Spear Colliders")]

    [ShowIf("SpearColliders")]
    //The spear attack colliders :P
    public Collider2D horizontalAttackCollider;
    [ShowIf("SpearColliders")]
    public Collider2D diagonalUpAttackCollider;
    [ShowIf("SpearColliders")]
    public Collider2D diagonalDownAttackCollider;
    [ShowIf("SpearColliders")]
    public Collider2D verticalUpAttackCollider;
    [ShowIf("SpearColliders")]
    public Collider2D verticalDownAttackCollider;



    public bool Death = true;
    [Header("Death")]

    [ShowIf("Death")]
    [Tooltip("In how much time does the first death phase happen? Like, when does the slow-mo death stop?")]
    public float DeathStopDelay;

    [ShowIf("Death")]
    [Tooltip("In how much time does the second death phase happen? IT MUST ALWAYS BE BIGGER THAN DeathStopDelay, otherwise bugs.")]
    public float ReviveDelay;

    [ShowIf("Death")]
    [Tooltip("How much the animation lasts, so the next level transition happens right after")]
    public float deathTeleportDuration;



    public bool Darkwind = true;
    [Header("Darkwind")]

    [ShowIf("Darkwind")]
    [Tooltip("When exactly, when player starts running, should the runningDarkwind particles, start emission?")]
    public float TimeToStartRunDarkwind;

    [ShowIf("Darkwind")]
    [Tooltip("The particleSystem of runningDarkwind(the wind-trail)")]
    public ParticleSystem currentDarkwindTrail;

    [ShowIf("Darkwind")]
    [Tooltip("The particleSystem for when you are wavedashing")]
    public ParticleSystem darkwindWavedash;
    [ShowIf("Darkwind")]
    [Tooltip("The transform (for rotating) for (darkwind) Wavedashing")]
    public Transform darkwindWavedashDirection;

    [ShowIf("Darkwind")]
    [Tooltip("The particleSystem for when you are dogerolling")]
    public ParticleSystem darkwindDodgeroll;
    [ShowIf("Darkwind")]
    [Tooltip("The transform (for rotating) for (darkwind) dodgerolling")]
    public Transform darkwindDodgerollDirection;

    [ShowIf("Darkwind")]
    [Tooltip("The particleSystem for when you are gliding")]
    public ParticleSystem darkwindGliding;

    [ShowIf("Darkwind")]
    [Tooltip("How many particles should be spawning over time? Multiplying with joystick's 0~1 Y value, you get the ultimate emission rate.")]
    public int glidingEmissionOverTime;

    [ShowIf("Darkwind")]
    [Tooltip("Material to be used on Dash(dodgeroll/wavedash) for level 2")]
    public Material Dash2;

    [ShowIf("Darkwind")]
    [Tooltip("Material to be used on Dash(dodgeroll/wavedash) for level 3")]
    public Material Dash3;

    [ShowIf("Darkwind")]
    [Tooltip("Material to be used on Glide for level 2")]
    public Material Glide2;

    [ShowIf("Darkwind")]
    [Tooltip("Material to be used on Glide for level 3")]
    public Material Glide3;

    [ShowIf("Darkwind")]
    [Tooltip("Material to be used on Damaged for level 2")]
    public Material Damaged2;

    [ShowIf("Darkwind")]
    [Tooltip("Material to be used on Damaged for level 2")]
    public Material Damaged3;

    [ShowIf("Darkwind")]
    [Tooltip("Particle System to be used on Level2")]
    public GameObject darkwindTrail2;

    [ShowIf("Darkwind")]
    [Tooltip("Particle System to be used on Level3")]
    public GameObject darkwindTrail3;


    public bool Audio = true;
    [ShowIf("Audio")]
    [Tooltip("The 3 SFX heard on dash attacks. Randomized ofc")]
    public AudioClip[] spearDashAttacksSFX;
    [ShowIf("Audio")]
    [Tooltip("The 3 SFX heard on ground and midair attacks. Randomized ofc")]
    public AudioClip[] spearGroundMidairAttacksSFX;
    [ShowIf("Audio")]
    [Tooltip("The 4 SFX heard when player hits an enemy.")]
    public AudioClip[] spearHitAttacksSFX;
    [ShowIf("Audio")]
    [Tooltip("The SFX heard when player lands on the ground")]
    public AudioClip landingGroundSFX;
    [ShowIf("Audio")]
    [Tooltip("The SFX heard when player lands on a platform")]
    public AudioClip landingPlatformSFX;
    [ShowIf("Audio")]
    [Tooltip("The SFX heard when player lands on a platform")]
    public AudioClip playerRunningSFX;
    [ShowIf("Audio")]
    [Tooltip("When warrior jumps from ground")]
    public AudioClip warriorJumpGroundSFX;
    [ShowIf("Audio")]
    [Tooltip("When warrior jumps in midair")]
    public AudioClip warriorJumpMidairSFX;
    [ShowIf("Audio")]
    [Tooltip("When mage jumps from ground")]
    public AudioClip mageJumpGroundSFX;
    [ShowIf("Audio")]
    [Tooltip("When mage jumps in midair")]
    public AudioClip mageJumpMidairSFX;
    [ShowIf("Audio")]
    [Tooltip("When dodgeroll")]
    public AudioClip dodgerollSFX;
    [ShowIf("Audio")]
    [Tooltip("When wavedash")]
    public AudioClip wavedashSFX;
    [ShowIf("Audio")]
    [Tooltip("When gliding")]
    public AudioClip glideSFX;



    public bool Misc = true;
    [Header("Miscellaneous")]

    [ShowIf("Misc")]
    [MinValue(0.01f)]
    [Tooltip("How often the XY axis external forces are reduced towards 0.\nHeavily recommended for 0.02f")]
    public float InfluenceDelay;//0.02f

    [ShowIf("Misc")]
    [MinValue(0.1f)]
    [Tooltip("How much the XY axis external forces are reduced towards 0, each InfluenceDelay cycle.\n Recommended 0.9f")]
    public float InfluenceDiminisher;//0.9f

    [ShowIf("Misc")]
    [MinValue(0f)]
    [MaxValue(1f)]
    [Tooltip("When warrior jumps, how powerful should the volume be?")]
    public float warriorGroundJumpingSFXVolume = 0.05f;

    [ShowIf("Misc")]
    [MinValue(0f)]
    [MaxValue(1f)]
    [Tooltip("When warrior jumps, how powerful should the volume be?")]
    public float warriorMidairJumpingSFXVolume = 0.15f;


    public bool ReadOnly = true;
    [Header("ReadOnly")]

    private float totalInfluenceY;   //These, hold the "acceleration" I put manually.
    private float controllableInfluenceX;
    private float UncontrollableInfluenceX;

    private float speedX, speedY;           //Current speed in X,Y axis.
    private bool midair = false;
    private bool warriorJump = false;
    private bool warriorHasJumped = false;
    private bool mageJump = false;
    private bool mageHasJumped = false;
    private bool lastJumpWasWarrior = false;//Remember/Cache who jumped last, warrior or mage.
    private bool canDodgeroll = true;       //First time dodgerolling, when false it is on cooldown. Reset to false when dodgeroll is exitted
    private bool startFloating = true;       //First time floating
    private bool dodgerollRight = false;
    private bool dodgerollInput = false;         //If true -> dodgeroll
    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool isDodgerolling = false;
    private bool canPhantomDodgeroll = false;       //Decided by 2 timers, which set it to true and false
    private bool hasPressedDodgeTwice = false;
    private int phantomDodgerollCount = 0;
    private bool facingLeft = false;        //Facing right the sprite, so it will Flip properly
    private bool doWavedashEffect = false;   //When true, player "Wavedashed" with InfluenceX thingy.
    private bool oppositeForces = true;      //When influenceX and input is the same direction
    private bool ignorePlatforms = false;   //To ignore platforms/collisions with them, when speedY > 0
    private bool doFlipPlayer = false;

    //Walljump variables
    private float lastWalljumpXPosition = -1;
    private Collider2D lastWalljumpCollider = null;
    private bool isWallSliding = false;

    /// <summary>
    /// Made to detect if sidewalled, so momentum will stop there.
    /// Has nothing to do with wallsliding mechanics
    /// </summary>
    private bool isSidewalled = false;

    /// <summary>
    /// Made for usage of the mini-state-machine for wallsliding's animations.
    /// Like the state machine in this class, it works the same more or less.
    /// current, and target.
    /// </summary>
    private WallSlideCheck.wallSlideAnimationState currentWallSlideAnimation = WallSlideCheck.wallSlideAnimationState.All;//Starting with one so it is not null/empty

    private bool justEnteredCrouch = false;

    #region Future Walljump Notes
    /*
    In the future, where every level will have united colliders, if you walljump off a wall 1, then fall, and walljump wall 2 that is on the same X **and same collider** it won't work. Hence, for floating walls on same X there is no worries, but if sth like this happens:
    ===
    =====(wall 1)
    ===
    =====(wall 2)

    ^You cannot walljump off wall 1 to walljumping wall 2 in a row, since they are in the same collider and have same X ;)
    Emphasis that this problem is NON-EXISTENT CURRENTLY. Hence the "in the future".

    And the solution to that is simply some raycasting "magic".
    */
    #endregion

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool isAtopGround = false;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool isAtopPlatform = false;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool isInsidePlatform = false;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool canWalljump = true;         //First time walljumping

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    //Used in Wallslide
    //to determine if should walljump or not.
    //By checking the wall position and player input.
    private bool shouldWalljump = false;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    //Used for DarkwindDistortionManager.WallJumpingLimit.Twice
    //to determine if should walljump or not, if ^ is activated
    private bool shouldSecondWalljump = false;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool canWallslide = false;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private bool isWallslidingLeft;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    [SerializeField]
    private Animator animatorController;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    public WarriorRendererBehaviour rendererBehaviour;

    private bool currentDarkwindTrailOn = true;

    //private GameObject gamemanager;
    private new Rigidbody2D rigidbody;
    private Transform FeetPosition;           // A position marking where to check if the player is grounded. aka where player's feet are.
    //private Transform CeilingCheck;        // A position marking where to check for ceilings. aka where player's head is.
    private Collider2D[] colliders;          // Initialized here, so they wont be created upon every ground/jump/anything/collision check.
    private Dictionary<int, GameObject> wallSlideColliderDictionary = new Dictionary<int, GameObject>(); //Exclusively for wallslide and walljump.
    private TimerManager timerManager;
    private WarriorHealth warriorHealthScript;
    //private CameraShaker mainCameraShaker;
    //private IEnumerator InfluenceReducer;  // Will be created in the future to stop InfluenceReducer() lel.
    private bool hasStartedAttack = false;         //First time attack, aka upon Entry
    private IDamageable EnemyToHit;
    private PlayerManaManager playerManaManager;
    [ShowIf("ReadOnly")]
    [ReadOnly]
    public MageBehaviour mageBehaviour;
    //private bool activateAttack = false;           //Once casting time is done, it signals HIT IT!
    private PlatformBehaviour sidePlatform;
    private KillMeterManager killMeter;
    [HideInInspector]
    public bool permadieDebugger = false;

    //Used for darkwind's VFX
    private int activeUpstreamDarkwind = 0;
    private int previousUpstreamDarkwind;//Caching ftw.
    private bool isGlidingActive = false;

    //Currently used in SFX, for deciding which clip should play.
    private int tempIndex;

    //Is SFX Running Coroutine active?
    private bool RunningSFXActive = false;
    private short activeRunningSFX = 0;

    //For the spear when player attacks with right tilt
    private enum AttackState { DiagonalUp, DiagonalDown, Horizontal, VerticalUp, VerticalDown };
    private AttackState currentAttackState;
    private Collider2D[] spearAttackCollisions = new Collider2D[10];//Stores the colliders hit by spears

    //This variable exists so holding down the joystick wont fuck P2's dash.
    //And while holding down should "erode"/decrease the dash for micro-spacing and refreshing the dash,
    //this makes sure that it must be input'd down AFTER dodgerolling!
    private bool holdingDownInputOnDodgerollActivation = false;

    [HideInInspector]
    public bool justTookDamage = false;


    [ShowIf("ReadOnly")]
    [ReadOnly]
    public bool isHitstunned = false;//WarriorHealth via Timer sets this to false after getting hit.

    //A Dictionary of the (touching) GroundColliders
    private Dictionary<int, GameObject> currentGroundCollisions = new Dictionary<int, GameObject>();

    //Constants (not in enum)
    public const int IDLE = 0, RUN = 1, JUMP = 2, WALLSLIDE = 3, FLOATING = 4, WALLJUMP = 5, DODGEROLL = 6, WAVEDASH = 7, HITGROUND = 8, HITMIDAIR = 9, ATTACKGROUND = 10, ATTACKMIDAIR = 11, ATTACKDASH = 12, DEATH = 13, REVIVE = 14, TAUNT = 15, CROUCH = 16, DIALOGUE = 17;

    //State-Machine
    [ShowIf("ReadOnly")]
    [ReadOnly]
    public int currentState;
    [ShowIf("ReadOnly")]
    [ReadOnly]
    public int targetState;
    [ShowIf("ReadOnly")]
    [ReadOnly]
    public int previousState;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    public bool dying;
    [ShowIf("ReadOnly")]
    [ReadOnly]
    public bool startDying;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    public bool InLevelCutscene;//Exists just in case I need in the future to find out if the current cutscene is levelCutscene.

    [ShowIf("ReadOnly")]
    [ReadOnly]
    public Vector3 checkpointPosition;

    [ShowIf("ReadOnly")]
    [ReadOnly]
    public float fastfallPower;

    //To notify FadeUIManager to start fading in
    public event Action startedDying;

    public event Action startedRevive;//Notify a lot of stuff, including DialogueSystem that player just died. Another is fade-out.
    public event Action finishedRevive;//Notify dialogueManager to speak!

    //A Dictionary of the platforms disabled/interacted(for proper re-activation, since after deactivation, without this it wont work)
    private Dictionary<int, PlatformBehaviour> PlatformsInteracted = new Dictionary<int, PlatformBehaviour>();
    //A Dictionary updated exclusively from SetAtopPlatformCollisionDictionary() of its child, AtopPlatformCheck.cs
    private Dictionary<int, GameObject> atopPlatformCollisionDictionary = new Dictionary<int, GameObject>();
    private List<int> tempIntList = new List<int>();

    //PlatformBehaviour latestPlatformTouched;

    WarriorAnimationManager commonAnimationManager;
    AnimationContainer currentlyActiveAnimationContainer;

    //Notifies (enemies for now) when player is midair, and when not
    public event Action isMidair;
    public event Action isGrounded;

    public event Action skippedDialogue;
    public event Action skippedRevive;

    public Action<ushort> justKilledEnemy;

    public bool touchedGround = false;

    //Dialogue cutscene hacks
    public enum DialogueAnimation {Idle, PoseEnd, PoseStartTaunt};
    public DialogueAnimation targetDialogueAnimation;

    //Temp/Dummy variables
    float tempFloat;
    Vector3 tempPoint1;
    Vector3 tempPoint2;
    GameObject tempGameObject;

    //Timers
    Timer dodgerollCooldownTimer;
    Timer dodgerollDurationTimer;
    //Starts phantomDodgerollStart seconds after dodgeroll started, and when it ends, it makes canPhantomDodgeroll = true; Check DetermineDodgeroll()
    Timer phantomDodgerollStartTimer;
    //Starts phantomDodgerollEnd seconds after phantom timer ends (above timer), and when it ends, it makes canPhantonDodgeroll = false;
    Timer phantomDodgerollEndTimer;

    Timer castingAttackMidairTimer;
    Timer castingAttackGroundTimer;
    Timer castingAttackDashTimer;

    Timer recoveryAttackMidairTimer;
    Timer recoveryAttackGroundTimer;
    Timer recoveryAttackDashTimer;

    Timer darkwindRunningTimer;

    //Cooldowns
    private static bool DodgerollInCooldown = false;

    private void Awake()
    {
        //Timers
        timerManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TimerManager>();
        //Dodgeroll Timers
        dodgerollCooldownTimer = timerManager.CreateTimer(dodgerollCooldownTimer, 0, DodgerollCooldown, false, true);
        dodgerollCooldownTimer.TriggerOnEnd += CooldownDodgeRollOver;
        dodgerollDurationTimer = timerManager.CreateTimer(dodgerollDurationTimer, 10, DodgerollDuration, false, true);
        dodgerollDurationTimer.TriggerOnEnd += DodgerollFinished;

        phantomDodgerollStartTimer = timerManager.CreateTimer(phantomDodgerollStartTimer, 11, PhantomDodgerollStart, false, true);
        phantomDodgerollStartTimer.TriggerOnEnd += StartPhantomDodgerollWindow;
        phantomDodgerollEndTimer = timerManager.CreateTimer(phantomDodgerollEndTimer, 12, PhantomDodgerollEnd, false, true);
        phantomDodgerollEndTimer.TriggerOnEnd += EndPhantomDodgerollWindow;

        //Checks the phantom dodgeroll values are valid
        if (PhantomDodgerollStart + PhantomDodgerollEnd > DodgerollDuration)
            Debug.LogError("Dodgeroll is fucked up, phantom dodgeroll can be reactivated even after dodge is finished, like wtf.");

        //Attack Timers
        castingAttackMidairTimer = timerManager.CreateTimer(castingAttackMidairTimer, 4, MidairAttackCastingTime, false, true);
        castingAttackGroundTimer = timerManager.CreateTimer(castingAttackGroundTimer, 5, GroundAttackCastingTime, false, true);
        castingAttackDashTimer = timerManager.CreateTimer(castingAttackDashTimer, 6, DashAttackCastingTime, false, true);

        castingAttackMidairTimer.TriggerOnEnd += ActivateAttack;
        castingAttackGroundTimer.TriggerOnEnd += ActivateAttack;
        castingAttackDashTimer.TriggerOnEnd += ActivateAttack;

        recoveryAttackMidairTimer = timerManager.CreateTimer(recoveryAttackMidairTimer, 7, MidairAttackRecoveryTime, false, true);
        recoveryAttackGroundTimer = timerManager.CreateTimer(recoveryAttackGroundTimer, 8, GroundAttackRecoveryTime, false, true);
        recoveryAttackDashTimer = timerManager.CreateTimer(recoveryAttackDashTimer, 9, DashAttackRecoveryTime, false, true);

        recoveryAttackMidairTimer.TriggerOnEnd += FinishAttack;
        recoveryAttackGroundTimer.TriggerOnEnd += FinishAttack;
        recoveryAttackDashTimer.TriggerOnEnd += FinishAttack;

        //Misc timers?
        darkwindRunningTimer = timerManager.CreateTimer(darkwindRunningTimer, 13, TimeToStartRunDarkwind, false, true);
        darkwindRunningTimer.TriggerOnEnd += DarkwindRunningStarts;

        //Subscribes to death action.
        GetComponent<WarriorHealth>().DiedEvent += StartDying;
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedDeathDialogue += FinishedReviveDialogue;
        dying = false;
        startDying = false;

        commonAnimationManager = GetComponent<WarriorAnimationManager>();
        //gamemanager = GameObject.FindGameObjectWithTag("GameManager");
        rigidbody = GetComponent<Rigidbody2D>();
        //currentDarkwindTrail = GetComponentInChildren<ParticleSystem>();
        playerManaManager = GameObject.Find("ManaManager").GetComponent<PlayerManaManager>();//lazy. Coulda do it better.
        mageBehaviour = GameObject.FindGameObjectWithTag("Mage").GetComponent<MageBehaviour>();// ^
        killMeter = GameObject.FindGameObjectWithTag("GameManager").GetComponent<KillMeterManager>();

        //mainCameraShaker = GameObject.FindGameObjectWithTag("mainCamera").GetComponent<CameraShaker>();

        //Used to be GroundCheck. An empty gameobject showing the location of the feet.
        //Now long deleted BUT 'AtopPlatformCheck' has the exact same position as GroundCheck!
        FeetPosition = transform.Find("AtopPlatformCheck");

        ////////////////////////////////////////////////////////////////////

        warriorHealthScript = GetComponent<WarriorHealth>();

        animatorController = GetComponentInChildren<Animator>();

        rendererBehaviour = GetComponentInChildren<WarriorRendererBehaviour>();
        //starting state
        currentState = IDLE;

        controllableInfluenceX = 0;
        UncontrollableInfluenceX = 0;

        totalInfluenceY = 0;

        speedX = 0;
        speedY = 0;
    }

    void Start()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedLevelCutscene += FinishedLevelCutscene;
        InLevelCutscene = false;

        currentDarkwindTrail.Stop();

        StartCoroutine(InfluenceReducer());

        globalInstance = this;
    }

    //This acts as a netcoding wrapper on the wrapper
    //Purely for convenience, to pass merely the inputSnapshot
    public void Movement(InputTrackingData inputSnapshotToFeed)
    {
        Movement(inputSnapshotToFeed.warriorInputData, inputSnapshotToFeed.mageInputData);
    }

    //This acts as a wrapper, so InputManager need not worry, and just dump its preferred input format, and this function "cleans it up" and calls Movement() properly.
    public void Movement(WarriorInputData warriorInput, MageInputData mageInput)
    {
        Movement(warriorInput.movementInputDirection, warriorInput.combatInputDirection, mageInput.aboutToJump, mageInput.aboutToDodgeroll, mageInput.dodgerollRight);
    }

    public void Movement(Vector2 inputDirection, Vector2 combatInputDirection, bool _mageJump = false, bool _dodgeroll = false, bool _dodgerollRight = false)//axisX and influence are optional arguments.
    {
        //DO NOT CHANGE "InfluenceXY" float, without CalculateForces()!!!! Otherwise, bugs bugs bugs! (because of how it works!)

        /////////////Debugging ///////////////////////////////////
        //Shows velocity rays
        DisplayVelocity(inputDirection);

        //Shows Platform&GroundHitboxes
        //Now happens in the gameobjects holding those colliders (GroundCollision, AtopPlatform, InsidePlatform)

        //Shows WallJumpHitbox
        //Now happens in the gameobjects holding those colliders, in this case, WallSlideNotify.cs gameobjects :P

        //Shows platformHitbox
        //Now happens in the gameobjects holding those colliders, in this case, InsidePlatformCheck.cs gameobject :P

        /////////////////Deterministic architecture *INTENSIFIES*///////////////////////
        //Did I mention it also helps on debugging? :)

        //Determine dodgeroll, so next frame dodgeroll state happens, and the direction is stored.
        DetermineDodgeroll(_dodgeroll, _dodgerollRight);

        //Determine the boolean holdingDownInput;
        DetermineDodgerollDownInput(inputDirection.y, false);

        //Determine Jumps (if warrior or mage has jumped) so as to determine if a Jump should happen
        DetermineJump(inputDirection, _mageJump);

        //Finds target state
        DetermineInputState(inputDirection, combatInputDirection);

        //Sets IsAtopPlatform depending if above platform or not
        DetermineIsAtopPlatform(inputDirection.y);

        //If input is downwards when grounded, dash is a lot less effective.
        DetermineCrouchErosion(inputDirection.y);

        //Sets IsWallsliding to true or false.
        DetermineWallSlide();

        //The most important of the Determine, it essentially runs the "Did warrior get hit?"
        DetermineHit();
        ////////////////////////////////////////////////////////////////////////////////



        //To detect the exact time the ground is touched, via jump/wallslide/whatever
        touchedGround = false;


        //State-Machine pattern to not fuck up
        switch (currentState)
        {
            case IDLE:
                //Run or Jump or Floating

                //Downwards Input detected
                if (targetState == FLOATING)
                {
                    //Descend a platform
                    if (isAtopPlatform)
                        DescendPlatform();//Disables the collider of platform for X seconds
                    //If player presses down, while he is atop ground, so it won't go to floating animation for 1 frame and come back... 
                    //Edit: i am retarded, and shouldnt get down input = FLOATING. simple as that.
                    else if (isAtopGround)
                        targetState = IDLE;

                }
                //No need to put attacc, since targetState from here will always be AttackGround and NewStateTransition() is a thing ;)

                //Fell off a ledge
                if (!isAtopGround && !isAtopPlatform)
                    targetState = FLOATING;

                //Mainly made for when u have influenceX from floating and get into idle, since u get into forward pose, but could be useful for future possible cases.
                if (targetState != FLOATING && GetTotalInfluenceX() == 0)
                {

                    #region LazyCrouchCancel Bug
                    //If you read this, this is a reminder of yet another stupid hack you did.
                    //And this one doesn't even work properly, as in NewStateTransition() it plays Idle for 1 frame.
                    //This is noticeable in 60 FPS. But whatever, not even 1% will play with Lazy Crouch Cancel
                    //Hopefully the few that do, do not notice the 1 frame annoyance in their eyes (which subconsciously they notice)
                    //and go back to the intended and default way of crouch cancel.
                    #endregion

                    //Check darkwind distortion manager for different crouch, so there is crouch animation on idle
                    if (previousState == CROUCH && DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Lazy)
                        animatorController.Play(StateToString(CROUCH));
                    else
                        animatorController.Play(StateToString(IDLE));

                    //Tbh this is such a fine example of why you should split physics/input state machine from animations.
                    //Some states can have more than 1 animation, e.g. idle, so these shitty hacks happen.
                }


                //Changes state to the targetState
                NewStateTransition();
                break;

            case CROUCH:

                //Reminder that down-input here, doesnt descend from platform.
                //So when u slide on a platform, u press down, and when it gets influenceX = 0, it auto-goes to idle and descends.

                
                //If bigger than first dash influence, always gets it to first dash. Ofc only if canphantomdodgeroll.
                /*if (controllableInfluenceX > DodgerollPower && canPhantomDodgeroll)
                        controllableInfluenceX = DodgerollPower;
                else if (controllableInfluenceX < DodgerollPower * -1 && canPhantomDodgeroll)
                        controllableInfluenceX = DodgerollPower * -1;
                */
                if (controllableInfluenceX == 0)//If 0, time to stop.
                {
                    //So it wont get to DashErosion()
                    holdingDownInputOnDodgerollActivation = true;

                    if (inputDirection.x < -0.1f || inputDirection.x > 0.1f)
                        targetState = RUN;
                    else
                        targetState = IDLE;

                    if (inputDirection.y < -0.1f && isAtopPlatform)
                    {
                        targetState = FLOATING;

                        DescendPlatform();
                    }
                        
                }
                else if (justEnteredCrouch)
                {
                    justEnteredCrouch = false;//With this, it runs this codeblock only once.

                    //Not default
                    if (DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Lazy)
                    {
                        //If it doesnt reset to 0, it will be broken aka infinite chaindashes while preserving influenceX...
                        controllableInfluenceX = 0;

                        //Restore Dash, whenever player presses crouch lel
                        OnKillResetDodgeroll();
                    }
                    else//if (DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Perfect)
                    {
                        if (canPhantomDodgeroll)//If crouching on the proper dash timer instead of instantly, gain dash (yes, you can crouch at any point of dodgeroll but only at ideal time do you gain free dash)
                        {
                            //Reward for Perfect Crouch is preserve some influenceX and have dash, which means more dashes possible in a smaller amount of time, which is situational af.
                            //controllableInfluenceX = 0;
                            //Still, less situational than "If you stop at perfect crouch, you insta-stop while getting dat chaindash", since u now at least have some influenceX.
                            //So, crouch optimally is used for 1 frame LUL
                            //So much depth with this... especially since it turns out u cant dash while in crouch :)                            

                            Debug.Log("RESET!");

                            //Reset dash
                            if (canPhantomDodgeroll)
                                OnKillResetDodgeroll();
                        }
                        //else if atop platform, insta-gtfo and fall off platform, instead of crouching.
                        /*else if (isAtopPlatform)
                        {//Commented away because with this, crouch on platforms is only possible at canphantomdodgeroll
                            targetState = FLOATING;

                            DescendPlatform();

                            //Only 1 frame is lost

                        }*/
                        
                    }
                    
                }


                

                //===
                //Copy-paste more or less from IDLE
                //===

                //Fell off a ledge
                if (!isAtopGround && !isAtopPlatform)
                    targetState = FLOATING;

                //Changes state to the targetState
                NewStateTransition();
                break;

            case RUN:

                //SFX for running//editted af lel
                if (RunningSFXActive == false && activeRunningSFX == 0 && LevelManager.currentLevel < 3)
                    StartCoroutine(RunningSFX());

                //Downwards Input detected//spagghetti logic below btw, but it works.
                if (targetState == FLOATING)
                {
                    //Descend a platform
                    if (isAtopPlatform)
                    {
                        Debug.Log("Drop Platform");
                        DescendPlatform();
                    }
                    //If player presses down, while he is atop ground, so it won't go to floating animation for 1 frame and come back...
                    else if (isAtopGround && (inputDirection.x > 0.12f || inputDirection.x < -0.12f))
                    {
                        targetState = RUN;
                    }
                }

                //ATTACC
                else if (targetState == ATTACKGROUND)
                {
                    targetState = ATTACKDASH;
                }

                //Fell of a ledge
                if (!isAtopGround && !isAtopPlatform)
                {
                    targetState = FLOATING;

                    //If Warrior shouldn't be able to jump (maybe upgrade after earth level?)
                    //warriorHasJumped = true;
                }

                //If about to change state
                if (targetState != RUN)
                    RunningSFXActive = false;
                else
                {
                    DetermineAnimatorSpeed(inputDirection.x);

                    //Mainly made for when u have influenceX and get into running, since u get into forward pose, and its bad if it reaches 0 and still run lul
                    if (GetTotalInfluenceX() == 0)
                        animatorController.Play(StateToString(RUN));
                }
                    


                //Changes state to the targetState
                NewStateTransition();
                break;

            case JUMP://2 Jumps can be made fyi

                //If input.y < 0f -> influenceY = 0f; DO IT

                #region //timerJumpCheck, to see when it should float
                //A frame has passed, jump increases
                //timerTable[(int)TimerNames.Jump]++;
                /*
                //If jump animation finished (replace this with animation check next time.)
                if (timerTable[(int)TimerNames.Jump] == timerMaxTable[(int)TimerNames.Jump])
                {
                    timerTable[(int)TimerNames.Jump] = 0f;

                    targetState = FLOATING;
                    NewStateTransition();
                }
                */
                #endregion

                //First JUMP
                //About to jump right now
                if (mageJump || warriorJump)//Put an isAtopGround, if you want to NOT jump midair/Floating...
                {
                    //Spagghetti bugfix. This, with the timer/IEnumerator in ground collision, fix the platforming jump.
                    isAtopGround = false;


                    //Right before the "jump" it makes vertical non-player-input speed, 0 so it won't bug, and jump with over 20+ speeds.
                    //Because Jump() sometimes happen with influenceY being not 0, and bizzare physics (superjumps!) happen. Plz dont delete this line.

                    //Reactivates platform (from downwards input/Ignoring), just in case
                    //It still ignores the platforms upwards, cuz of velocityY > 0 -> Ignore layer, But it now resets the collisions
                    ReactivateInteractedPlatforms();
                    //if (latestPlatformTouched != null)
                    //latestPlatformTouched.ReactivatePlatformForPlayer();

                    //This is first jump
                    if ((mageJump && warriorHasJumped == false) || (warriorJump && mageHasJumped == false))
                    {
                        //Debug.Log("First Jump");

                        if (mageJump)
                        {
                            if (midair == false)
                            {
                                //Rune-Circle VFX
                                VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MagicCircleGround, FeetPosition.position);

                                //SFX
                                PlayerSoundManager.globalInstance.PlayClip(mageJumpGroundSFX, PlayerSoundManager.AudioSourceName.MageJump1, 1.2f, 0.25f);
                            }
                            else
                            {
                                //Rune-Circle VFX
                                VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MagicCircleMidair, FeetPosition.position);

                                //SFX
                                PlayerSoundManager.globalInstance.PlayClip(mageJumpMidairSFX, PlayerSoundManager.AudioSourceName.MageJump1, 1.2f, 0.25f);
                            }


                            totalInfluenceY = 0;
                            //speedY = 0;

                            //tl;dr: Vertical speed = Vertical Speed + FirstJumpPower(20)
                            CalculateForces(false, FirstJumpPower);
                        }
                        else//warriorjump
                        {
                            if (midair == false)
                            {
                                //Wind VFX
                                VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.JumpSmokeGround, FeetPosition.position + new Vector3(0, 1.5f, 0));

                                //SFX
                                PlayerSoundManager.globalInstance.PlayClip(warriorJumpGroundSFX, PlayerSoundManager.AudioSourceName.WarriorJump1, 1f, warriorGroundJumpingSFXVolume);
                            }
                            else
                            {
                                //Wind VFX
                                VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.JumpSmokeMidair, FeetPosition.position - new Vector3(0, -1.5f, 0));

                                //SFX
                                PlayerSoundManager.globalInstance.PlayClip(warriorJumpMidairSFX, PlayerSoundManager.AudioSourceName.WarriorJump1, 1f, warriorMidairJumpingSFXVolume);
                            }


                            totalInfluenceY = 0;
                            //speedY = 0;

                            //tl;dr: Vertical speed = Vertical Speed + FirstJumpPower(20)
                            CalculateForces(false, FirstJumpPower);
                        }



                    }
                    //This is a double Jump
                    else
                    {
                        //Debug.Log("Double Jump");

                        if (mageJump)
                        {
                            //Rune-Circle VFX
                            VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MagicCircleMidair, FeetPosition.position);

                            //SFX
                            PlayerSoundManager.globalInstance.PlayClip(mageJumpMidairSFX, PlayerSoundManager.AudioSourceName.MageJump1, 1.2f, 0.25f);
                        }
                        else
                        {
                            //Wind VFX
                            VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.JumpSmokeMidair, FeetPosition.position - new Vector3(0, -1.5f, 0));

                            //SFX
                            PlayerSoundManager.globalInstance.PlayClip(warriorJumpMidairSFX, PlayerSoundManager.AudioSourceName.WarriorJump1, 1f, warriorMidairJumpingSFXVolume);
                        }


                        //When SpeedY is 0, with current formula

                        /* Commented because it made the only viable/fun way of moving, P1 jumping, then P2. bleh! ROCKETJUMPGOOOOOOO!
                        float tempInfluenceY = MidairGravity - VerticalControlSpeed;//approximately close when speedY is 0
                        if (influenceY < tempInfluenceY)
                            CalculateForces(false, SecondJumpPower + tempInfluenceY);
                        //So the difference won't feel dramatic
                        else if (influenceY < tempInfluenceY * 1.5f)
                            CalculateForces(false, SecondJumpPower + tempInfluenceY / 2);
                        else
                        */

                        //Using some of the above (sorry for the spagghetti but it "feels" good in-game right now, so no more fucking around):
                        float tempInfluenceY = MidairGravity - VerticalControlSpeed;//approximately close when speedY is 0
                        if (totalInfluenceY < tempInfluenceY)
                            CalculateForces(false, SecondJumpPower + tempInfluenceY);
                        else
                            //here ends the bad copypasta of the above
                            CalculateForces(false, SecondJumpPower);//Limits it via CalculateForces


                    }

                    //Updates the jump limits
                    if (mageJump)
                    {
                        mageJump = false;
                        mageHasJumped = true;
                        lastJumpWasWarrior = false;
                    }
                    else
                    {
                        warriorJump = false;
                        warriorHasJumped = true;
                        lastJumpWasWarrior = true;
                    }


                }
                //JUMP -> WAVEDASH
                //If player has pressed the "DodgeRoll button"
                else if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false)
                {
                    targetState = WAVEDASH;
                    NewStateTransition();
                }
                //JUMP -> FLOATING
                //Detects either the down input (which also puts floating into a special state) and influenceY aka the "jumpSpeed" that is still left
                else if (inputDirection.y < -0.1f || (totalInfluenceY > 0 && totalInfluenceY < 11f))
                {
                    targetState = FLOATING;
                    NewStateTransition();
                }
                //Check when finally leaves ground
                else if (midair == false && !isAtopGround && !isAtopPlatform)
                {
                    midair = true;

                    //Updates the subscribers
                    if (isMidair != null)
                        isMidair.Invoke();
                }
                //JUMP -> ATTACK
                else if (targetState == ATTACKMIDAIR)
                {
                    NewStateTransition();
                }
                //JUMP -> WALLSLIDE
                //Wall slides
                else if (midair && isWallSliding)
                {
                    targetState = WALLSLIDE;
                    NewStateTransition();
                }
                //JUMP -> IDLE
                //Jump is finished
                else if (midair && (isAtopGround || isAtopPlatform))//By returning true, it sets booleans: midair and hasJumped to -> False
                {
                    touchedGround = true;
                }


                break;//JumpBreak

            case FLOATING:

                //First time FLOATING
                if (startFloating)
                {
                    startFloating = false;

                    midair = true;

                    //Updates the subscribers
                    if (isMidair != null)
                        isMidair.Invoke();
                }
                //FLOATING -> WAVEDASH
                //If player has pressed the "DodgeRoll button"
                else if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false)
                {
                    targetState = WAVEDASH;
                    NewStateTransition();
                }
                //FLOATING -> JUMP
                else if ((mageJump && mageHasJumped == false) || (warriorJump && warriorHasJumped == false))
                {
                    startFloating = true;

                    //Goes to Jump and detects the mage/warrior Jump ;)
                    targetState = JUMP;
                    NewStateTransition();
                }
                //FLOATING -> ATTACK
                else if (targetState == ATTACKMIDAIR)
                {
                    NewStateTransition();
                }
                //FLOATING -> WALLSLIDE
                //Wall slides
                else if (midair && isWallSliding)
                {
                    startFloating = true;

                    targetState = WALLSLIDE;
                    NewStateTransition();
                }
                //FLOATING -> IDLE
                //Floating is finished
                else if (midair && (isAtopGround || isAtopPlatform))//&& midair)? deleted it, cuz there was a scenario that player was on ground(FLOATING), but midair == true...
                //By returning true, it sets booleans: midair and hasJumped to -> False
                {
                    touchedGround = true;
                }
                //FLOATING -> FASTFALL 
                //  Since hardcoded and last-minute addition to this state machine, this is not its own state with its own badass animation and fast-falling VFX :(
                //  Though, this should be part of the FLOATING state, and make the current one the default floating state. Aka 2 sub-states of FLOATING, where there are currently 0.
                //  After all, only the "velocity" changes, animation and VFX for it, nothing else, the transitions should be the same after all. 
                //  That said, this code is so spagghetti that making FASTFALL its own state, by copy-pasting FLOATING would make more sense imo
                else if (midair && inputDirection.y < -0.1f)
                {
                    //Instantly nullify influenceY, if its positive/upwards
                    if (totalInfluenceY > 0)
                        totalInfluenceY = 0;

                    //See the variable FastfallControlSpeed!
                }

                break;

            case WALLSLIDE:

                //WALLSLIDE -> IDLE
                //Wallsliding is finished
                if (isAtopGround || isAtopPlatform)
                {
                    touchedGround = true;
                }
                //WALLSLIDE -> FLOATING
                else if (midair && isWallSliding == false)
                {
                    //So it will flip when it comes back to a wall

                    targetState = FLOATING;
                    NewStateTransition();
                }
                //WALLSLIDE -> WALLJUMP
                //Player 2 Input detected. time to bounce off that wall
                else if (_mageJump)
                {
                    shouldWalljump = false;

                    //btw, the below could get into an "DetermineShouldWallJump()"

                    //If DarkwindDistortion options, ignore/bypass the walljump validity calculations
                    if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Infinite)
                        shouldWalljump = true;
                    else if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Never)
                        shouldWalljump = false;
                    else//Determine if should walljump
                    {
                        //Iterate through all walljump colliders.
                        foreach (var pickedDictionaryKey in wallSlideColliderDictionary.Keys)
                        {

                            //If Wallsliding is on the right side of the tile
                            if (facingLeft)
                                //tempFloat equals the position of the right side of the tile
                                tempFloat = wallSlideColliderDictionary[pickedDictionaryKey].GetComponent<BoxCollider2D>().bounds.max.x;
                            else
                                //tempFloat equals the position of the left side of the tile
                                tempFloat = wallSlideColliderDictionary[pickedDictionaryKey].GetComponent<BoxCollider2D>().bounds.min.x;

                            //If the walljump you want to make, is approximately the same with the last walljump's X, then NO walljump, because it's the same wall more or less.
                            //tl;dr: if (differentWall)
                            if (tempFloat < lastWalljumpXPosition - 0.3f || tempFloat > lastWalljumpXPosition + 0.3f)
                            {
                                lastWalljumpXPosition = tempFloat;

                                shouldWalljump = true;

                                //Reset the double walljump as well since new wall is detected
                                shouldSecondWalljump = true;
                            }
                            //else if (sameWall)
                            else if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Twice)
                            {
                                if (shouldSecondWalljump == true)
                                {
                                    shouldSecondWalljump = false;

                                    shouldWalljump = true;
                                }
                            }


                            //Useless. But in the future it may be useful.
                            //If it's not the same jumpCollider
                            if (lastWalljumpCollider != wallSlideColliderDictionary[pickedDictionaryKey].GetComponent<BoxCollider2D>())
                                lastWalljumpCollider = wallSlideColliderDictionary[pickedDictionaryKey].GetComponent<BoxCollider2D>();
                        }
                    }


                    if (shouldWalljump)
                    {
                        canWalljump = true;
                        targetState = WALLJUMP;
                    }
                    else if (mageHasJumped == false)
                        targetState = JUMP;
                    else//without this else, it somehow goes to stateRun/Floating, and it glitches VISUALLY for 2 frames.
                        targetState = currentState;

                    NewStateTransition();//Changes state to the targetState
                }
                //WALLSLIDE -> WAVEDASH
                //If player has pressed the "DodgeRoll button"
                else if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false && ( (_dodgerollRight && isWallslidingLeft) || (_dodgerollRight == false && isWallslidingLeft == false) ) )
                {
                    targetState = WAVEDASH;
                    NewStateTransition();
                }
                //WALLSLIDE -> ATTACK
                else if (targetState == ATTACKMIDAIR)
                {
                    //Without the below ifelse, the attack always happens to the side of the wall!
                    //I guess you could call this a boilerplate check, so it doesn't happen inside NewStateTransition
                    //which would be if (currentState == WALLSLIDE && targetState == ATTACKMIDAIR) do the below;

                        //FacingLeft == true, means the player is on the right side of a wall
                        if (facingLeft == true && combatInputDirection.x > 0)
                            FlipPlayer();
                            //FacingLeft == true, means the player is on the left side of a wall
                        else if (facingLeft == false && combatInputDirection.x < 0)
                            FlipPlayer();

                    NewStateTransition();
                }
                break;

            case WALLJUMP:

                //WALLJUMP -> IDLE
                if (isAtopGround || isAtopPlatform)
                    //By returning true, it sets booleans: midair and hasJumped to -> False
                    touchedGround = true;

                //About to walljump right now, first time
                else if (canWalljump)
                {
                    //Debug.Log("Walljumped: " + inputDirection);

                    //Wallslide -> Walljump uses _mageJump, to detect the input, not if it can jump.
                    //mageJump = false, because it will jump next frame if not.
                    mageJump = false;
                    //mageHasJumped = true;
                    //mageHasJumped = false;

                    //So player won't double jump or w/e
                    canWalljump = false;

                    //DumpValues(inputDirection);

                    //Right before the "jump" it makes vertical non-player-input speed, 0 so it won't bug, and jump with over 20+ speeds.
                    //Because Jump() sometimes happen with influenceY being not 0, and bizzare physics (superjumps!) happen. Plz dont delete this line.
                    totalInfluenceY = 0;

                    //If towards the wall (Y)
                    if ( (isWallslidingLeft && inputDirection.x <= 0) || (isWallslidingLeft == false && inputDirection.x >= 0) )
                    {
                        //Y axis
                            //Vertical speed = Vertical Speed + walljumpPower
                            CalculateForces(false, WalljumpPower, false);



                        //X axis

                            ResetTotalInfluenceX();

                            if (isWallslidingLeft)
                                CalculateForces(true, WalljumpBouncePower);
                            else
                                CalculateForces(true, WalljumpBouncePower * -1);
                    }
                    else//Opposite to the wall, to pinball the momentum. Momentum redirection POG
                    {
                        //Y axis
                            //Adds a part of influenceX, into influenceY
                            CalculateForces(false, WalljumpPower + Mathf.Abs(controllableInfluenceX) / WalljumpPowerDivider * inputDirection.y , false);

                        //X axis
                            if (isWallslidingLeft)
                                CalculateForces(true, WalljumpBouncePower * Mathf.Abs(inputDirection.x), false, true, true);
                            else
                                CalculateForces(true, WalljumpBouncePower * Mathf.Abs(inputDirection.x) * -1, false, true, true);
                    }
                    


                    //Rune-Circle(Horizontal) VFX //TODO: Fix the origin position of that magic circle. Dependant on facingLeft/Right btw.
                    VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MagicCircleWalljump, transform.position);
                    /*
                    if (isWallslidingLeft)
                        VFXManager.globalInstance.GetLastCreatedVFX().transform.eulerAngles = new Vector3(0, 0, 90);
                    else
                        VFXManager.globalInstance.GetLastCreatedVFX().transform.eulerAngles = new Vector3(0, 0, 270);
                    */

                    //SFX
                    PlayerSoundManager.globalInstance.PlayClip(mageJumpGroundSFX, PlayerSoundManager.AudioSourceName.MageJump1, 1.2f, 0.25f);
                }
                //WALLJUMP -> WAVEDASH
                //If player has pressed the "DodgeRoll button"
                else if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false)
                {
                    targetState = WAVEDASH;
                    NewStateTransition();
                }
                //WALLJUMP -> ATTACK
                else if (targetState == ATTACKMIDAIR)
                {
                    NewStateTransition();
                }
                //Jumped off the wall
                else if (canWalljump == false && isWallSliding == false)
                {
                    //WALLJUMP -> FLOATING
                    //Detects either the down input (which also puts floating into a special state) and influenceY aka the "jumpSpeed" that is still left
                    if (inputDirection.y < -0.1f || (totalInfluenceY > 0 && totalInfluenceY < 11f))
                    {
                        targetState = FLOATING;
                        NewStateTransition();
                    }

                    canWallslide = true;//So it will NOT re-wall slide right away. (look below)
                }
                //WALLJUMP -> WALLSLIDE
                else if (canWallslide && isWallSliding)
                {
                    canWallslide = false;

                    targetState = WALLSLIDE;
                    FlipPlayer();
                    NewStateTransition();
                }

                break;

            case DODGEROLL:

                if (canDodgeroll)
                {
                    canDodgeroll = false;

                    isDodgerolling = true;

                    //So proper phantom dodgerolling happens
                    hasPressedDodgeTwice = false;
                    canPhantomDodgeroll = false;

                    ////Timer///
                    dodgerollCooldownTimer.elapsed = 0f;
                    dodgerollCooldownTimer.Activate();
                    dodgerollDurationTimer.elapsed = 0f;
                    dodgerollDurationTimer.Activate();

                    phantomDodgerollEndTimer.Reset();

                    phantomDodgerollStartTimer.elapsed = 0f;
                    phantomDodgerollStartTimer.Activate();
                    //Invoke("CooldownDodgeRollOver", 3f);

                    DodgerollInCooldown = true; //Checked via WarriorInput
                    ////////////

                    phantomDodgerollCount++;

                    //SFX
                    PlayerSoundManager.globalInstance.PlayClip(dodgerollSFX, PlayerSoundManager.AudioSourceName.PlayerDash1, 1.2f + (phantomDodgerollCount * 0.1f));

                    //VFX!
                    if (dodgerollRight)
                        darkwindDodgerollDirection.eulerAngles = new Vector3(0, 0, 90);
                    else
                        darkwindDodgerollDirection.eulerAngles = new Vector3(0, 0, 270);


                    //Speedwave
                    VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.Dodgeroll, transform.position + new Vector3(0, 3, 0));
                    if (dodgerollRight == false)
                        VFXManager.globalInstance.GetLastCreatedVFX().transform.localScale = new Vector2(VFXManager.globalInstance.GetLastCreatedVFX().transform.localScale.x * -1, VFXManager.globalInstance.GetLastCreatedVFX().transform.localScale.y);

                    //Play the VFX
                    darkwindDodgeroll.Play();

                    //DashErosion so player1 wont insta-stop the dodgeroll
                    DetermineDodgerollDownInput(inputDirection.y, true);

                    //tl;dr: InfluenceX = InfluenceX + DodgerollPower
                    //<<<<<BALANCE IT HERE!
                    if (dodgerollRight)
                        CalculateForces(true, DodgerollPower, false, true, true);
                    else
                        CalculateForces(true, DodgerollPower * -1, false, true, true);

                    dodgerollInput = false;
                }
                //DODGEROLL -> JUMP
                else if (mageJump || warriorJump)
                {
                    NewStateTransition();
                }
                //DODGEROLL -> ATTACK
                else if (targetState == ATTACKGROUND || targetState == ATTACKDASH)
                {
                    NewStateTransition();
                }
                //DODGEROLL -> CROUCH
                else if (targetState == CROUCH)
                {
                    NewStateTransition();
                }
                //DODGEROLL -> FLOATING
                //Fell of a ledge
                else if (!isAtopGround && !isAtopPlatform)
                {
                    targetState = FLOATING;
                    NewStateTransition();
                }
                //DODGEROLL -> FLOATING
                //Drop-Cancelled Dodgeroll
                else if (isAtopPlatform && targetState == FLOATING)
                {
                    DescendPlatform();
                    NewStateTransition();
                }
                //DODGEROLL -> WALLSLIDE (No? Make an "touches wall while in ground" animation so it looks natural)
                else if (midair && isWallSliding)
                {
                    targetState = WALLSLIDE;
                    NewStateTransition();
                }
                //Dodgeroll -> IDLE
                //Animation finished, move on.
                else if (!isDodgerolling)//only with timer
                {
                    if (controllableInfluenceX < 15 && controllableInfluenceX > -15)
                    {
                        if (inputDirection == Vector2.zero)
                            targetState = IDLE;
                        else
                            targetState = RUN;

                        NewStateTransition();
                    }
                    else if (inputDirection.x != 0)
                    {
                        //If the joystick input and dodgeroll is on same side
                        if ( (controllableInfluenceX > 0 && inputDirection.x > 0) || (controllableInfluenceX < 0 && inputDirection.x < 0) )
                        {
                            //TODO: Some VFX I guess?
                        }
                        else//Opposite side.
                        {
                            targetState = RUN;
                            NewStateTransition();

                            if (DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull == false)
                                controllableInfluenceX = 0;
                        }
                    }
                        

                }

                break;

            case WAVEDASH:
                if (canDodgeroll)
                {
                    canDodgeroll = false;

                    isDodgerolling = true;

                    //So proper phantom dodgerolling happens
                    hasPressedDodgeTwice = false;
                    canPhantomDodgeroll = false;

                    ////Timer///
                    dodgerollCooldownTimer.elapsed = 0f;
                    dodgerollCooldownTimer.Activate();
                    dodgerollDurationTimer.elapsed = 0f;
                    dodgerollDurationTimer.Activate();

                    phantomDodgerollEndTimer.Reset();
                    phantomDodgerollStartTimer.elapsed = 0f;
                    phantomDodgerollStartTimer.Activate();
                    //Invoke("CooldownDodgeRollOver", 3f);

                    DodgerollInCooldown = true; //Checked via WarriorInput
                    ////////////

                    phantomDodgerollCount++;

                    //SFX
                    //PlayerSoundManager.globalInstance.PlayClip(wavedashSFX, PlayerSoundManager.AudioSourceName.PlayerDash1, 0.85f + (phantomDodgerollCount * 0.1f), 1);
                    PlayerSoundManager.globalInstance.PlayClip(dodgerollSFX, PlayerSoundManager.AudioSourceName.PlayerDash1, 1.2f + (phantomDodgerollCount * 0.1f));

                    //VFX!
                    //Particle Effects
                    if (dodgerollRight)
                        darkwindWavedashDirection.eulerAngles = new Vector3(0, 0, 90);
                    else
                        darkwindWavedashDirection.eulerAngles = new Vector3(0, 0, 270);

                    //Speedwave
                    VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.Wavedash, transform.position);
                    if (dodgerollRight == false)
                        VFXManager.globalInstance.GetLastCreatedVFX().transform.localScale = new Vector2(VFXManager.globalInstance.GetLastCreatedVFX().transform.localScale.x * -1, VFXManager.globalInstance.GetLastCreatedVFX().transform.localScale.y);

                    //VFXManager.globalInstance.GetLastCreatedVFX().transform.eulerAngles = new Vector3(0, 0, 0);
                    //else
                    //VFXManager.globalInstance.GetLastCreatedVFX().transform.eulerAngles = new Vector3(0, 0, 180);



                    //After setting the rotation above, DEW IT!
                    darkwindWavedash.Play();



                    //tl;dr: InfluenceX = InfluenceX + DodgerollPower
                    if (dodgerollRight)
                        CalculateForces(true, WavedashPower, false, true, true);
                    else
                        CalculateForces(true, WavedashPower * -1, false, true, true);
                        

                    dodgerollInput = false;
                }
                //WAVEDASH -> JUMP
                else if (mageJump || warriorJump)
                {
                    NewStateTransition();
                }
                //WAVEDASH -> IDLE
                else if (isAtopGround || isAtopPlatform)
                {
                    touchedGround = true;
                }
                //WAVEDASH -> ATTACK
                else if (targetState == ATTACKMIDAIR)
                {
                    NewStateTransition();
                }
                //WAVEDASH -> WALLSLIDE
                else if (midair && isWallSliding)
                {
                    targetState = WALLSLIDE;
                    NewStateTransition();
                }
                //WAVEDASH -> FLOATING
                //Animation finished, move on.
                else if (!isDodgerolling)
                {
                    targetState = FLOATING;
                    NewStateTransition();
                }

                break;

            //If hit, from DetermineHit(), it changes currentState = hit Midair/Ground, so it ignores the above.
            //However! Check the wavedash/dodgeroll variable changes upon exit. (since what im doing here is hacky.)
            case HITMIDAIR:

                //Time to stop dis. Timer from WarriorHealth
                if (isHitstunned == false)
                {
                    targetState = FLOATING;
                    NewStateTransition();
                }
                //Hit -> Jump //SMASH BROS, DOUBLE JUMP, THAT AIN'T FALCO!
                else if (mageJump || warriorJump)
                {
                    warriorHealthScript.ResetHitstun();

                    NewStateTransition();
                }
                else if (isAtopGround || isAtopPlatform)
                {
                    targetState = HITGROUND;
                    NewStateTransition();//Starting from start of animation tho?
                }

                break;

            case HITGROUND:

                //Time to stop dis. Timer from WarriorHealth
                if (isHitstunned == false)
                {
                    //I think it bugs without this. HitMidair leads here for example, but touching the ground, it's not reset.
                    touchedGround = true;

                    if (inputDirection == Vector2.zero)
                        targetState = IDLE;
                    else
                        targetState = RUN;

                    NewStateTransition();
                }
                //Hit -> Jump
                else if (mageJump || warriorJump)
                {
                    warriorHealthScript.ResetHitstun();

                    NewStateTransition();
                }
                else if (!isAtopGround && !isAtopPlatform)
                {
                    targetState = HITMIDAIR;
                    NewStateTransition();//Starting from start of animation tho?
                }

                break;

            case ATTACKMIDAIR:

                //Mandatory stuff to transition to other states

                /////////////////////////////////
                if (!hasStartedAttack)
                {
                    hasStartedAttack = true;

                    //ATTACC
                    castingAttackMidairTimer.Activate();

                    PlaySpearThrustSFX();
                }
                //////////////////////////////////////////

                //ATTACKMIDAIR -> WAVEDASH
                //If player has pressed the "DodgeRoll button"
                if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false)
                {
                    targetState = WAVEDASH;
                    NewStateTransition();
                }
                //ATTACKMIDAIR -> JUMP
                else if (mageJump)
                {
                    //Goes to Jump and detects the mage/warrior Jump ;)
                    targetState = JUMP;
                    NewStateTransition();
                }
                /*Nope cuz it seems janky af, and it auto-cancels the attackmidair.
                //
                //See SetSideWalled(), where it checks if just entered a wall, so it cancels only on that moment. Hence the transition happens, without looking like trash.
                //
                //ATTACKMIDAIR -> WALLSLIDE
                //Wall slides
                else if (midair && isWallSliding)
                {
                    targetState = WALLSLIDE;
                    NewStateTransition();
                }
                */
                //ATTACKMIDAIR -> IDLE
                //Floating is finished
                else if (midair && (isAtopGround || isAtopPlatform))//&& midair)? deleted it, cuz there was a scenario that player was on ground(FLOATING), but midair == true...
                //By returning true, it sets booleans: midair and hasJumped to -> False
                {
                    touchedGround = true;
                }

                //Mandatory stuff /end

                break;

            case ATTACKGROUND:

                /////////////////////////////////
                if (!hasStartedAttack)
                {
                    hasStartedAttack = true;

                    //Debug.Log("Entered here.");

                    //ATTACC
                    castingAttackGroundTimer.Activate();

                    PlaySpearThrustSFX();
                }
                //////////////////////////////////////////

                //Fell of a ledge
                else if (!isAtopGround && !isAtopPlatform)
                {
                    targetState = FLOATING;

                    //If Warrior shouldn't be able to jump (maybe upgrade after earth level?)
                    //warriorHasJumped = true;

                    //If I want to get the "falling from ledge" gravity better, here, put a boolean, and at determineYspeed
                    //if (cameFromIdle/Run) -> blabla - GroundedGravity
                    //else -> blabla - MidairGravity

                    NewStateTransition();
                }

                //Downwards Input detected, Descend a platform
                //ATTACKGROUND -> FLOATING
                else if (targetState == FLOATING && isAtopPlatform && !isAtopGround)
                {
                    Debug.Log("Downwards Detected");
                    DescendPlatform();//Disables the collider of platform for X seconds
                    NewStateTransition();
                }
                //ATTACKGROUND -> WAVEDASH
                //If player has pressed the "DodgeRoll button"
                else if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false)
                {
                    targetState = DODGEROLL;
                    NewStateTransition();
                }
                //ATTACKGROUND -> JUMP
                else if (mageJump)
                {
                    //Goes to Jump and detects the mage/warrior Jump ;)
                    targetState = JUMP;
                    NewStateTransition();
                }



                break;

            case ATTACKDASH:

                /////////////////////////////////
                if (!hasStartedAttack)
                {
                    hasStartedAttack = true;

                    //Dash attack, also pushes player forward
                    //tl;dr: InfluenceX = InfluenceX + DodgerollPower

                    //if facing right>
                    if (facingLeft == false)
                        CalculateForces(true, DashAttackPower, false, true, true);
                    //else facing left<
                    else
                        CalculateForces(true, DashAttackPower * -1, false, true, true);
                        
                        

                    //ATTACC
                    castingAttackDashTimer.Activate();

                    PlaySpearThrustSFX();
                }
                //////////////////////////////////////////

                //Fell of a ledge
                else if (!isAtopGround && !isAtopPlatform)
                {
                    targetState = FLOATING;

                    //If Warrior shouldn't be able to jump (maybe upgrade after earth level?)
                    //warriorHasJumped = true;

                    //If I want to get the "falling from ledge" gravity better, here, put a boolean, and at determineYspeed
                    //if (cameFromIdle/Run) -> blabla - GroundedGravity
                    //else -> blabla - MidairGravity

                    NewStateTransition();
                }

                //Downwards Input detected, Descend a platform
                //ATTACKDASH -> FLOATING
                else if (targetState == FLOATING && isAtopPlatform && !isAtopGround)
                {
                    Debug.Log("Downwards Detected");
                    DescendPlatform();//Disables the collider of platform for X seconds
                    NewStateTransition();
                }
                //ATTACKDASH -> DODGEROLL
                //If player has pressed the "DodgeRoll button"
                else if (dodgerollInput && canDodgeroll && DodgerollInCooldown == false)
                {
                    targetState = DODGEROLL;
                    NewStateTransition();
                }
                //ATTACKDASH -> JUMP
                else if (mageJump)
                {
                    //Goes to Jump and detects the mage/warrior Jump ;)
                    targetState = JUMP;
                    NewStateTransition();
                }

                break;

            //The pose right when a level begins.
            case TAUNT:
                //Just checks if targetState happens.
                if (targetState == IDLE)
                    targetState = TAUNT;

                //Changes state to the targetState
                NewStateTransition();
                break;

            case DEATH:
                if (startDying)
                {
                    //Things to do while dying
                    //========================

                    //Manual (and hacky) way to keep forces.
                    //speedX = influenceX;
                    DetermineXSpeed(Vector2.zero);

                    speedY = (totalInfluenceY - GroundedGravity) * VerticalSpeed;
                    //speedY = influenceY * VerticalSpeed;

                    //Now that speedX and speedY are calculated and final, change the velocity.
                    rigidbody.velocity = new Vector2(speedX, speedY);


                    //Slow-mo
                    Time.timeScale += 0.005f;
                }

                return;

            case REVIVE:
                if (DoBothPlayersHaveActiveInput(inputDirection, combatInputDirection, _mageJump, _dodgeroll) == true && PlayerStatsManager.globalInstance.GetTotalDeaths() < 75)//Insanity
                {
                    //Unzoom
                    GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().ResetZoomToDefault();

                    //P2 to start fading once skip happens, otherwise looks awkward af.
                    mageBehaviour.StartFadingTransparency();

                    //TODO: Have some VFX explosion to show the hype instead of spawning out of thin air.

                    //Notify DialogueManager + Enable Spells + Enable PauseMenu
                    if (skippedRevive != null)
                        skippedRevive.Invoke();

                    //Move to a new state.
                    NewStateTransition();//No need getting current/targetState, since the above inputs must have done it by now ;)
                }

                break;

            case DIALOGUE:
                if (DoBothPlayersHaveActiveInput(inputDirection, combatInputDirection, _mageJump, _dodgeroll) == true && PlayerStatsManager.globalInstance.GetTotalDeaths() < 75)//Insanity
                {

                    Debug.Log("Skipped with " + PlayerStatsManager.globalInstance.GetTotalDeaths() + " deaths");
                    //Don't allow skip if level exit cutscene, or if first time. (if speedrunning, 0 fucks given, and if not first time, you can skip enter level cutscenes)
                    if (InLevelCutscene)
                    {
                        return;
                        /* Fuck it. So many bugs eg it disables pause menu from opening, and have to manually call its event invocation.
                        So, I straight up disabled skipping levels. Except if you are on speedrun mode, which ofc matters there, but for the rest of the players, not so much - or at least enough for me to spend a full day or 2 debugging spagghetti

                        //LevelCutscenes are unskippable if playing first time.
                        if (PlayerStatsManager.globalInstance.GetSpeedrunMode() == false && PlayerStatsManager.globalInstance.GetPlayerClear() == false)
                            return;//aka dont skip

                        //Band-Aid but whatever, if it works. 
                        //The amount of ppl that will want to skip levelcutscene AND not be in speedrun mode, is too low and this feature is unimportant anw, cuz speedrun mode.
                        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().skipLevelCutsceneDialogue = true;

                        return;
                        //With this^, current (voice)line ends, and cutscene is over (end level to load, or can move in new level)
                        //But ofc, skipLevelCutsceneDialogue REMAINS TRUE!!!! -_-
                        //Anyway, its functional and not a serious bug.
                        */

                        /* Still moves, so commented out until proper dialogue skip is implemented while not being able to move/act here.
                        if (LevelManager.levelCutsceneDialogueIndex % 2 == 1)
                        {
                            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayersInterruptedDialogue();
                                //^LMAO, kinda the same thing as //Notify DialogueManager + Enable Spells
                                    if (skippedDialogue != null)
                                        skippedDialogue.Invoke();
                            //targetState = IDLE;
                            //NewStateTransition();
                            //If the above 2 lines are uncommented, you interrupt 1 time for each time you press spacebar!!!
                            return;
                        }
                        else
                            Debug.Log("Don't skip!");
                        */
                    }

                    //Unzoom
                    GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().ResetZoomToDefault();

                    //P2 to start fading once skip happens, otherwise looks awkward af.
                    mageBehaviour.StartFadingTransparency();

                    //Notify DialogueManager + Enable Spells
                    if (skippedDialogue != null)
                        skippedDialogue.Invoke();

                    //Move to a new state
                    NewStateTransition();//No need getting current/targetState, since the above inputs must have done it by now ;)
                }

                //Debug.Log("Midair: " + midair + " isatop something: " + (isAtopGround || isAtopPlatform));
                if (midair && (isAtopGround || isAtopPlatform))
                {
                    ResetStateConditions();

                    //Update all subscribers, on the status that player touched ground
                    if (isGrounded != null)
                        isGrounded.Invoke();

                    animatorController.Play(StateToString(DIALOGUE));
                }
                    

                return;
        }

        //Touched ground, and resets jump and stuff.
        if (touchedGround)
        {
            ResetStateConditions();

            //Reached the ground
            if (inputDirection.x < 0.1f && inputDirection.x > -0.1f)
                targetState = IDLE;
            else
                targetState = RUN;

            NewStateTransition();

            //Update all subscribers, on the status that player touched ground
            if (isGrounded != null)
                isGrounded.Invoke();

            //Debug.Log("Landed");
            //Debug.Break();
        }

        //Checks input + forceInfluencer + midaircheck
        DetermineXSpeed(inputDirection);

        //Checks input + forceInfluencer + midaircheck
        DetermineYSpeed(inputDirection, _mageJump);

        //Checks input + state machine
        DetermineUpstreamDarkwind(inputDirection.y);

        DeterminePlatformLayerCollision();

        //Now that speedX and speedY are calculated and final, change the velocity.
        //If mage + Online + Non-Desynchronized
        if (NetworkInputSnapshotManager.globalInstance.rejectMageLocalVelocity && NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y);//speedX/Y conflicts position assigned by NetworkPositionController, and so it is "default"
        else//if offline play, or host online
            rigidbody.velocity = new Vector2(speedX, speedY);

        //With timer, starts emitting only when player is moving for 2+ seconds, and stops when player isn't moving.
        DetermineDarkwindParticleEmission(rigidbody.velocity, inputDirection);

        //FlipsPlayer depending on state//Please check the state, some shouldnt be able to flip!!!!
        DetermineFlipPlayer(inputDirection);
    }

    /// <summary>
    /// Resets all flags needed to walljump again anywhere.
    /// </summary>
    public void ResetWalljump()
    {
        canWalljump = true;
        lastWalljumpXPosition = -1;
        lastWalljumpCollider = null;

        //For DarkwindDistortionManager.WallJumpingLimit.Twice
        shouldSecondWalljump = true;
    }

    public void ResetStateConditions(bool overrideMidair = true)
    {
        //No longer midair
        if (overrideMidair)
            midair = false;

        //so it won't bounce off the ground
        totalInfluenceY = 0f;

        //So it can re-dodgeroll
        canDodgeroll = true;

        //So it can re-float
        startFloating = true;

        //so it can slide from walljump
        canWallslide = false;

        //Resets side platform
        if (sidePlatform != null)
        {
            sidePlatform.ReactivatePlatformForPlayer();
            sidePlatform = null;
        }

        ResetJumps();
    }

    public void ResetJumps()
    {
        //So it can re-walljump
        ResetWalljump();

        //Resets Jumps
        warriorHasJumped = false;
        mageHasJumped = false;

        //Making sure
        mageJump = false;
        warriorJump = false;
        dodgerollInput = false;
    }

    //Called by DetermineInputState to check if a combat state should happen or not
    //However, it sets the variable: currentAttackState. It returns false if no valid input was found btw :P
    //movementInputDirection is used for dodgeroll, to determine whether transition from dodgeroll goes to attackGround or attackDash
    //Also, sorry for being spagghetti, but it all just works.... (end me...)
    public bool DetermineAttackState(Vector2 combatInputDirection, float movementInputDirectionX)
    {

        //Grounded
        //Fully Vertical^
        //X axis:-0.2~0.2  
        //Y axis:0.9~1.0
        //Fully Horizontal<->
        //X axis: -1.00~-0.25|0.25~1.00
        //Y axis:-0.25~+0.25
        //Diagonal Horizontal Downwards//Maybe make this 0.2 instead of 0.25?
        //X axis: -1.00~-0.25|0.25~1.00
        //Y axis: -0.25~-1
        //Diagonal Horizontal Upwards
        //X axis: -1.00~-0.25|0.25~1.00
        //Y axis: +0.25~1

        //Midair
        //Fully Vertical ^Upwards
        //X axis:-0.2~0.2
        //Y axis:0.9~1.0
        //Fully Vertical Downwards
        //X axis:-0.2~0.2
        //Y axis:-0.9~1.0
        //Fully Horizontal<->
        //X axis: -1.00~-0.25|0.25~1.00
        //Y axis:-0.25~+0.25
        //Diagonal Horizontal Downwards//Maybe make this 0.2 instead of 0.25?
        //X axis: -1.00~-0.25|0.25~1.00
        //Y axis: -0.25~-1
        //Diagonal Horizontal Upwards
        //X axis: -1.00~-0.25|0.25~1.00
        //Y axis: +0.25~1

        //Reminder that the above are false, and that WarriorInputManager.cs
        //has by default tiltstick mechanics (aka only on edges, and doesnt accept same input) so the above axis data is outdated/useless.

        bool attacking = false;        

        //Diagonal Horizontal Upwards
        if (combatInputDirection.y > 0.25f && !(combatInputDirection.x < 0.25f && combatInputDirection.x > -0.25f))
        {
            attacking = true;
            currentAttackState = AttackState.DiagonalUp;
        }
        //Diagonal Horizontal Downwards
        else if (combatInputDirection.y < -0.25f && !(combatInputDirection.x < 0.05f && combatInputDirection.x > -0.05f))//was 0.25, but 0.05 shouldnt matter since WarriorInput filters dead zone.
        {
            attacking = true;
            currentAttackState = AttackState.DiagonalDown;
        }
        //Fully Horizontal
        else if (combatInputDirection.y < 0.25f && combatInputDirection.y > -0.25f && !(combatInputDirection.x < 0.25f && combatInputDirection.x > -0.25f))
        {
            attacking = true;
            currentAttackState = AttackState.Horizontal;
        }
        //Fully Vertical Upwards
        else if (combatInputDirection.y > 0.9f && combatInputDirection.x < 0.25f && combatInputDirection.x > -0.25f)
        {
            attacking = true;
            currentAttackState = AttackState.VerticalUp;
        }
        //Fully Vertical Downwards for midair, DiagonalDown for Grounded.
        else if (combatInputDirection.y < -0.9f && combatInputDirection.x < 0.2f && combatInputDirection.x > -0.2f)
        {
            attacking = true;
            if (!isAtopGround && !isAtopPlatform && midair)
                currentAttackState = AttackState.VerticalDown;
            else
                currentAttackState = AttackState.DiagonalDown;
        }




        if (attacking)
        {
            //Flip appropriately
            DetermineFlipPlayer(combatInputDirection);


            //Set the state for the state machine
            if (!isAtopGround && !isAtopPlatform && midair)
                targetState = ATTACKMIDAIR;
            else if (currentState == IDLE)
                targetState = ATTACKGROUND;
            else if (currentState == RUN)
                targetState = ATTACKDASH;
            else if (currentState == DODGEROLL)
            {
                if (combatInputDirection.x < 0f && facingLeft == false)
                    FlipPlayer();
                else if (combatInputDirection.x > 0f && facingLeft == true)
                    FlipPlayer();


                if (movementInputDirectionX != 0)
                    targetState = ATTACKDASH;
                else
                    targetState = ATTACKGROUND;
            }

        }

        return attacking;
    }

    /// //////////State Machine Functions/////

    //In other words, it makes CERTAIN(except Jump), the transitions will be legit, so later it simply transitions without checking.
    public void DetermineInputState(Vector2 movementInputDirection, Vector2 combatInputDirection)
    {

        //If no input
        if (!midair && !mageJump && !warriorJump && !dodgerollInput && movementInputDirection == Vector2.zero && combatInputDirection == Vector2.zero)
        {
            //If its possible, do if checks for each state here.
            targetState = IDLE;
        }
        //DodgeRoll
        else if (dodgerollInput && canDodgeroll)
        {
            if (midair)
                targetState = WAVEDASH;
            else
                targetState = DODGEROLL;
        }
        //JumpInput
        else if (mageJump || warriorJump)
        {
            targetState = JUMP;
        }
        //ATTACC
        else if (DetermineAttackState(combatInputDirection, movementInputDirection.x))//If this returns false, it simply validated that there was no valid attack input.
        {
            //DetermineAttackState does: If inputs are proper, SETS the target state (attackMidair/Ground) and the currentState
        }

        //Run
        else if (movementInputDirection.x > 0.12f || movementInputDirection.x < -0.12f)//Input isn't zero
        {
            targetState = RUN;
        }
        //Floating
        else if (movementInputDirection.y < -0.1f && (currentState != CROUCH || movementInputDirection.y < -0.8f))
        {
            targetState = FLOATING;
        }

    }

    //Below function (which has singlehandedly saved the state machine as otherwise it would be unreadable)
    //should also have a flag of sorts, to keep the animation/sprites playing in previous state.

    //Changes state
    public void NewStateTransition()
    {
        //If they are different, so it won't replay the same animation from the start every frame.
        if (currentState != targetState)
        {
            /* Trying to fix FlipPlayer() is meaningless.
            //Test
            if (currentState == WALLSLIDE)
                FlipPlayer();
            */

            //if (currentState == DODGEROLL)
            //Debug.Log("Target state is: " + targetState);

            if (currentState == RUN)
            {
                animatorController.speed = 1;

                //Made because when there is an influenceX, and ur input is opposite to it, from midair to ground u go to run
                //but when u stop the input from run to idle while influenceX, it doesnt go to 0 ;)
                if (targetState == IDLE)
                {
                    if (DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull == false)
                        controllableInfluenceX = 0;
                }
            }

            if (targetState == CROUCH)
                justEnteredCrouch = true;

            //For attacks, so they won't bug
            ResetAttackTimers();

            //If leaving attacking state, stops the playing clip.
            ResetSpearThrustSFX();

            OnDodgerollExit();

            //Remember previous state
            previousState = currentState;

            //Updates state
            currentState = targetState;

            //Transitions to state.
            animatorController.Play(StateToString(targetState));

            //When you go idle/run with influenceX, have better animation.
            DetermineIdleRunInfluencePose();

            //Hack made exclusively for darkwind distortion crouchcancel
            if (previousState == CROUCH && currentState == IDLE && DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Lazy)
                animatorController.Play("Crouch");
        }
    }

    //Returns string from every state.
    public string StateToString(int state)
    {
        if (state == IDLE)
            return "Idle";
        else if (state == RUN)
        {
            if (LevelManager.currentLevel < 3)
                return "Run";
            else
                return "Hover";
        }
        else if (state == JUMP)
            return "Jump";
        else if (state == WALLSLIDE)
            return WallSlideAnimationStateToString(currentWallSlideAnimation);
        else if (state == FLOATING)
            return "Floating";
        else if (state == WALLJUMP)
            return "Walljump";
        else if (state == DODGEROLL || state == WAVEDASH)
        {
            //Debug.Log("facing left is: " + facingLeft + " and dodgerollRight is: " + dodgerollRight);

            //Backwards
            if (facingLeft && dodgerollRight || facingLeft == false && dodgerollRight == false)
            {
                if (state == DODGEROLL)
                    return "DodgerollBackwards";
                else
                    return "WavedashBackwards";
            }

            //Forward
            else
            {
                if (state == DODGEROLL)
                    return "DodgerollForward";
                else
                    return "WavedashForward";
            }
        }
        else if (state == HITGROUND)
            return "HitGround";
        else if (state == HITMIDAIR)
            return "HitMidair";

        //Attacks, but here it gets the animation right, the actual stuff happens in the switch case :P
        //currentAttackState is determined at methods: DetermineInput->DetermineAttackState
        else if (state == ATTACKMIDAIR)
        {
            if (currentAttackState == AttackState.DiagonalDown)
                return "ADiagonalDownMidair";
            else if (currentAttackState == AttackState.DiagonalUp)
                return "ADiagonalUpMidair";
            else if (currentAttackState == AttackState.Horizontal)
                return "AHorizontalMidair";
            else if (currentAttackState == AttackState.VerticalUp)
                return "AVerticalUpMidair";
            else if (currentAttackState == AttackState.VerticalDown)
                return "AVerticalDownMidair";
        }
        //Has 2: Dash Attack and Normal Attack
        else if (state == ATTACKGROUND)
        {
            if (currentAttackState == AttackState.DiagonalDown)
                return "ADiagonalDownGround";
            else if (currentAttackState == AttackState.DiagonalUp)
                return "ADiagonalUpGround";
            else if (currentAttackState == AttackState.Horizontal)
                return "AHorizontalGround";
            else if (currentAttackState == AttackState.VerticalUp)
                return "AVerticalUpGround";
        }
        else if (state == ATTACKDASH)
        {
            if (currentAttackState == AttackState.DiagonalDown)
                return "ADiagonalDownDash";
            else if (currentAttackState == AttackState.DiagonalUp)
                return "ADiagonalUpDash";
            else if (currentAttackState == AttackState.Horizontal)
                return "AHorizontalDash";
            else if (currentAttackState == AttackState.VerticalUp)
                return "AVerticalUpDash";
        }

        else if (state == DEATH)
            return "Death";
        else if (state == REVIVE)
            return "Revive";
        else if (state == TAUNT)
        {
            if (PlayerStatsManager.globalInstance.IsFirstRun())
                return "TauntPose3";
        }
        else if (state == CROUCH)
            return "Crouch";

        if (state == DIALOGUE)
        {
            /* Argue Animations looked CRINGE
            //int animationToPlay = UnityEngine.Random.Range(1, 6);//max is exclusive, hence 4 instead of 3.

            //if (animationToPlay < 3)
            //1
            //else if (animationToPlay < 5)
            //2
            //else
            //3

            //return "Argue" + animationToPlay.ToString();
            */
            if (midair)
                return "Floating";
            else
            {
                if (targetDialogueAnimation == DialogueAnimation.Idle)
                    return "Idle";
                else if (targetDialogueAnimation == DialogueAnimation.PoseEnd)
                {
                    if (PlayerStatsManager.globalInstance.GetTotalDeaths() == 0)
                        return "TauntPose2";
                    else
                        return "TauntPose1";
                }
            }
        }

        Debug.Log("Bug at StateToString!!, the state is: " + state);

        return "Null";
    }

    ///////////////////////////////////////////////////////////////////////

    //Used for dialogue.
    public void PlayAnimation(string animationToPlay)
    {
        if (animationToPlay == "None")
            return;
        else
            animatorController.Play(animationToPlay);
    }

    ///////////////////////////////////////////////////////////////////////

    public void DetermineInfluenceXIdleRunAnimation()
    {
        //Facing same direction as influenceX
        if ((GetTotalInfluenceX() > 0 && facingLeft == false) || (GetTotalInfluenceX() < 0 && facingLeft == true))
            animatorController.Play("GroundDashFrameForward");
        else//Facing opposite direction from influenceX
            animatorController.Play("GroundDashFrameBackward");
    }

    /// <summary>
    /// If you go idle/run with influenceX, it should not be idle animation otherwise it will look janky.
    /// </summary>
    public void DetermineIdleRunInfluencePose()
    {
        if (GetTotalInfluenceX() == 0)
            return;

        //If you dont return it here for those who have this active, animation-wise it will look horrible.
        //if (DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull == true)
        //return;
        //^Fuck it, animation-wise it will look bad, but its better than fucking idle animation. (should be run animation, literally all there is to fix this to look good)
        //And truly making it proper animation-wise, would take a full day, and would make the code even more spagghetti...

        if (currentState == IDLE || currentState == RUN)
            DetermineInfluenceXIdleRunAnimation();
    }

    public void DetermineDodgerollDownInput(float inputY, bool onDodgerollActivation)
    {
        if (onDodgerollActivation)
        {
            if (inputY < -0.1f)
                holdingDownInputOnDodgerollActivation = true;
            else
                holdingDownInputOnDodgerollActivation = false;
        }
        else//Testing every Update() on deterministic logic block
        {
            if (holdingDownInputOnDodgerollActivation == true && inputY > -0.1f)
                holdingDownInputOnDodgerollActivation = false;
        }
    }

    public void DetermineDodgeroll(bool _dodgeroll, bool _dodgerollRight)
    {
        //If player2 has inputted dodgeroll
        if (_dodgeroll)
        {
            //For transitioning from any state to dodgeroll
            if (DodgerollInCooldown == false)
                dodgerollInput = _dodgeroll;
            dodgerollRight = _dodgerollRight;

            //Debug.Log("has he spammed? " + hasPressedDodgeTwice + " and phantom dodgeroll is possible? " + canPhantomDodgeroll);

            //The below makes sure dodgerolling works PERFECTLY, with phantom dodgerolling, anti-mashing n stuff.

            //Phantom (Chain)Dash codeblock
            if (true)//used to be if(isDodgerolling), and should eventually be a variable from darkwindDistortionManager
            {
                //No chaindash allowed, for whatever reason. (speedrun category confirmedu?!)
                if (DarkwindDistortionManager.globalInstance.playerChaindash == DarkwindDistortionManager.Chaindash.Disabled)
                    return;

                if (canPhantomDodgeroll || DarkwindDistortionManager.globalInstance.playerChaindash == DarkwindDistortionManager.Chaindash.Infinite)
                {
                    //If mashing, and by chance 2nd mash on active frame window, next dodgeroll aint happenin.
                    if (!hasPressedDodgeTwice || DarkwindDistortionManager.globalInstance.playerChaindash == DarkwindDistortionManager.Chaindash.Infinite)
                    {
                        //"Resets" dodge, so it insta-dodges right now
                        //Resets the dodge action
                        canDodgeroll = true;

                        //Resets cooldown
                        //CooldownDodgeRollOver();//Commented out cuz it fucked VFX/SFX
                        DodgerollInCooldown = false;

                        //Manually here, since my retarded state machine (NewStateTransition()) doesn't play the same animation again.
                        if (currentState == DODGEROLL)
                            animatorController.Play(StateToString(DODGEROLL), -1, 0f);
                        else if (currentState == WAVEDASH)//dont use simple else, but else if wavedash, cuz current state could be attacking ;)
                            animatorController.Play(StateToString(WAVEDASH), -1, 0f);
                        else if (currentState != DEATH && currentState != REVIVE && currentState != DIALOGUE)//Coming here from different state, e.g. attack or dash etc etc.
                        {
                            if (isAtopGround || isAtopPlatform)//Coming here from different state.
                                targetState = DODGEROLL;
                            else
                                targetState = WAVEDASH;

                            NewStateTransition();
                        }
                        
                    }
                }

                hasPressedDodgeTwice = true;
            }
        }
    }

    //Determine if warrior or mage has jumped
    public void DetermineJump(Vector2 inputDirection, bool _mageJump)
    {
        if (mageHasJumped == false && _mageJump)
            mageJump = _mageJump;

        if (warriorHasJumped == false && inputDirection.y > 0.5)
            warriorJump = true;
        
    }

    public void DetermineUpstreamDarkwind(float _inputDirectionY)
    {
        //Isn't midair check useless since floating? hmmm
        if (midair && _inputDirectionY > 0.3f && (currentState == FLOATING || currentState == ATTACKMIDAIR) && speedY < 0)
        {
            //First time entering
            if (isGlidingActive == false)//Sets rotation, so it seems like "resistant"/surfing :P
            {
                isGlidingActive = true;

                if (darkwindGliding.isPlaying == false)
                    darkwindGliding.Play();

                darkwindGliding.startRotation = UnityEngine.Random.Range(0, 360);

                //SFX
                //If not playing before, play it
                //if (PlayerSoundManager.globalInstance.IsGlidePlaying() == false)
                PlayerSoundManager.globalInstance.PlayClip(glideSFX, PlayerSoundManager.AudioSourceName.Glide, 1f, Mathf.Lerp(-0.1f, 0.4f, _inputDirectionY));
                //PlayerSoundManager.globalInstance.ChangeVolume(Mathf.Lerp(0,0.5f, _inputDirectionY), PlayerSoundManager.AudioSourceName.Glide);
            }

            
            previousUpstreamDarkwind = activeUpstreamDarkwind;
            //Exponentially/ek8etika, not linearly, to make up for the "usual" gliding of 1.0 and to fuck over the lower ones.
            activeUpstreamDarkwind = (int)(_inputDirectionY * glidingEmissionOverTime * 2f);
            if (activeUpstreamDarkwind != previousUpstreamDarkwind) //So it wont change every frame.
                darkwindGliding.emissionRate = activeUpstreamDarkwind;//Fuck this shit. Look below, both are "suggested" choices, but they dont fucking work and docs/internet has no explanation. ggwp. or im retarded.
                                                                      //darkwindGliding.emission.rate.constant = 0;
                                                                      //darkwindGliding.emission.rateOverTime.constant = 0;


            //SFX changing volume depending on input, kinda like above.
            if (PlayerSoundManager.globalInstance.GetGlideVolume() != Mathf.Lerp(-0.1f, 0.4f, _inputDirectionY))
                PlayerSoundManager.globalInstance.ChangeVolume(Mathf.Lerp(-0.1f, 0.4f, _inputDirectionY), PlayerSoundManager.AudioSourceName.Glide);
            //PlayerSoundManager.globalInstance.PlayClip(glideSFX, PlayerSoundManager.AudioSourceName.Glide, 1f, Mathf.Lerp(0.1f, 0.5f, _inputDirectionY));
        }
        else if (isGlidingActive) //Active but not to glide, so shut it down!
            StopGlidingFX();

    }

    public void StopGlidingFX()
    {
        activeUpstreamDarkwind = 0;
        isGlidingActive = false;
        darkwindGliding.emissionRate = 0;

        //SFX
        PlayerSoundManager.globalInstance.Stop(PlayerSoundManager.AudioSourceName.Glide);
    }

    //Depending on movementY input, it slows down or even insta-cancels the dash.
    public void DetermineCrouchErosion(float inputY)
    {
        //If midair, erosion/crouch cannot happen so just gtfo from this logic
        if (midair)
            return;

        //if (inputY < 0)
            //Debug.Log("Negative InputY: " + inputY);

        //This and the above ifs, confirm it can also be done on a platform, just not on -0.8 to -1 values
        if (isAtopPlatform && inputY < -0.8f)//If -1, dont do crouch
            return;

        //If influence is of a SINGLE dash, dont bother with crouch
        //(as there is a visual bug atm where trying to fall a platform while momentum will show a crouch frame in between which needs timers and input caching, this is a practical way to solve this)
        //This is not a visual^ bug, but delays all platform falls because to go Y from 0 to -0.8f takes 3 frames.
        if (isAtopPlatform && GetTotalInfluenceX() > -25f && GetTotalInfluenceX() < 25f)
            return;

        //If no erosion is to happen, then obviously it shouldnt happen, aka dont continue this logic
        if (inputY >= 0 || controllableInfluenceX == 0)
            return;

        if (holdingDownInputOnDodgerollActivation == false)
        {
            //Would use CalculateForces, but when opposite powers, it instantly gets influenceX = 0
            //Reduce InfluenceX depending on input (so micro-spacing is possible)
            if (controllableInfluenceX > 0)
                controllableInfluenceX = controllableInfluenceX + DodgerollPower * inputY * DodgerollErosionPower;//inputY is negative ;)
            else
                controllableInfluenceX = controllableInfluenceX + DodgerollPower * inputY * DodgerollErosionPower * -1;
            //perhaps change the above if else chain, to include if Y < -0.9 -> controllableInfluenceX = 0

            targetState = CROUCH;

            if (controllableInfluenceX < 5 && controllableInfluenceX > -5)
                controllableInfluenceX = 0;
        }
        //else, ignore. Aka, player must re-flick the controller.

    }


    public void DetermineXSpeed(Vector2 inputDirection)
    {
        //When reviving, dont move obviously.
        if (currentState == REVIVE || currentState == DIALOGUE)
        {
            speedX = 0;
            return;
        }

        //Opposing forces.
        if ((inputDirection.x > 0 && GetTotalInfluenceX() < 0) || (inputDirection.x < 0 && GetTotalInfluenceX() > 0))
        {

            if (currentState == DODGEROLL || currentState == WAVEDASH)
            {
                speedX = GetTotalInfluenceX() * DodgerollReverseMultiplier;

                return;
            }
            //If walljump, but u hold towards the side of the wall.
            else if (currentState == WALLJUMP)
            {
                //speedX = inputDirection.x * airSpeed * -1 + GetTotalInfluenceX();
                speedX = GetTotalInfluenceX();
                //speedX = inputDirection.x * airSpeed + GetTotalInfluenceX();
                //above fucking breaks the walljump, when no influence and simple walljump of opposite side, cuz it literally gets speedX to be opposite of the walljump power!
                //so yeah, fucking keep the GetTotalInfluence() AND DO NOT CHANGE IT! I have done ALL possible changes to it, so dont think u can do better when u revisit it.

                return;
            }
            else if (currentState == WALLSLIDE)
            {
                //Without this, its impossible to wallslide at high speeds, cuz of the influence.
                speedX = inputDirection.x * airSpeed + GetTotalInfluenceX();

                //Do note that this is on opposite forces, but on the below if (midair) it does the same indirectly for wallslide if its on the same forces ;)
                //Spagghetti, but hey, if it works.

                return;
            }
            else if (UncontrollableInfluenceX != 0)
            {
                speedX = UncontrollableInfluenceX;

                return;
            }
            //The return; above can NOT be split from branches to unify them, since there is no ELSE, otherwise it will bug.

        }

        if (midair)
        {
            //If input and player influence is same side
            if ((inputDirection.x >= 0 && controllableInfluenceX > 0) || (inputDirection.x <= 0 && controllableInfluenceX < 0))
                //speedX = (inputDirection.x * airSpeed + influenceX) * Time.deltaTime * 100;
                speedX = inputDirection.x * airSpeed + GetTotalInfluenceX();


            else//if opposite forces with player Influence.
                speedX = inputDirection.x * airSpeed + UncontrollableInfluenceX;
                
        }
        else if (currentState == ATTACKGROUND || currentState == ATTACKDASH)//Stand Still
            speedX = GetTotalInfluenceX();
        /*else if (currentState == CROUCH)
        {   
            speedX = (inputDirection.x * movementSpeed + GetTotalInfluenceX()) * -1 * inputDirection.y * DodgerollErosionPower;

            /*
            if (GetTotalInfluenceX() > 0)
                speedX = (inputDirection.x * movementSpeed + GetTotalInfluenceX()) * inputDirection.y * DodgerollErosionPower * -1;//The last part is negative since inputY is negative.
            else
                speedX = (inputDirection.x * movementSpeed + GetTotalInfluenceX()) * inputDirection.y * DodgerollErosionPower * -1;//The last part is positive since 2 negatives
            //
        }*/
        else
        {
            //If input and player influence is same side
            if ((inputDirection.x >= 0 && controllableInfluenceX > 0) || (inputDirection.x <= 0 && controllableInfluenceX < 0))
                //speedX = (inputDirection.x * movementSpeed + influenceX) * Time.deltaTime * 100;//dont make movementspeed *=100; cuz influenceX gets cucked.
                speedX = inputDirection.x * movementSpeed + GetTotalInfluenceX();//dont make movementspeed *=100; cuz influenceX gets cucked.


            else//if opposite forces with player Influence.
                speedX = inputDirection.x * movementSpeed + UncontrollableInfluenceX;
        }



    }

    public void DetermineYSpeed(Vector2 inputDirection, bool _mageJump)
    {
        if (midair && isWallSliding)
        {
            //If not "Fastfall", aka 99% of your wallslides
            if (inputDirection.y > -0.1f)
                //speedY = (inputDirection.y * VerticalControlSpeed - MidairGravity + influenceY) * Time.deltaTime * 100 / WallslideSpeed;
                speedY = (inputDirection.y * VerticalControlSpeed - MidairGravity + totalInfluenceY) * VerticalSpeed / WallslideSpeed;
            else//Fastfall, literally the above but with a tweak ;)
            {
                //Using influenceY to go down faster.
                if (totalInfluenceY > 0)
                    speedY = (inputDirection.y * VerticalControlSpeed - MidairGravity - totalInfluenceY) * VerticalSpeed / WallslideSpeed;
                else
                    speedY = (inputDirection.y * VerticalControlSpeed - MidairGravity + totalInfluenceY) * VerticalSpeed / WallslideSpeed;
            }
                
        }
        else if (midair && currentState == WAVEDASH)
        {
            //So player won't go up/down while wavedashing
            speedY = 0f;
        }
        else if (midair)//Jump or Floating
        {
            //Ensures that mage's first jump is high. (cuz without player1, inputdirection.y = 0 and bunnyhop is real)
            //ffs! These 2 lines ruined double jump! And atop of that, magejump as well since no freedom!
            //if (mageHasJumped && currentState == JUMP)
                //inputDirection.y = 1f;

            //Got to ensure that mage's first jump is high (because without player 1, inputDirection.y =0
            //and if he makes it > 0, he jumps himself!
            //((For mage's 2nd jump, perhaps have mixed control, but whatever, I think its good rn))
            if (currentState == JUMP && lastJumpWasWarrior == false && _mageJump == true)//_mageJump means holding down spacebar
                inputDirection.y = 1f;




            if (inputDirection.y > -0.1f)
                //speedY = (inputDirection.y * VerticalControlSpeed - MidairGravity + influenceY) * Time.deltaTime * 100;
                speedY = (inputDirection.y * VerticalControlSpeed - MidairGravity + totalInfluenceY) * VerticalSpeed;//Made to make up for the above, to be similar.
                                                                                                                //Debug.Log("SpeedY = " + speedY + ": " + "(VerticalControlSpeed)" + VerticalControlSpeed + "-(midairGravity)" + MidairGravity + "+ (influenceY)" + influenceY + ") * (Time.deltaTime)" + Time.deltaTime + "* 100");

            else //Fastfall!
                speedY = (inputDirection.y * FastfallControlSpeed - MidairGravity + totalInfluenceY) * VerticalSpeed;//The same as above but with the addition of "-fastfallPower"
        }

        else //Grounded
            //speedY = (influenceY - GroundedGravity) * Time.deltaTime * 100;
            speedY = (totalInfluenceY - GroundedGravity) * VerticalSpeed;
    }

    public void DeterminePlatformLayerCollision()
    {
        if (speedY > 0 && ignorePlatforms == false)
        {
            //Debug.Log("Ignoring");
            ignorePlatforms = true;
            Physics2D.IgnoreLayerCollision(8, 12, true);
        }
        else if (speedY < 0 && ignorePlatforms == true && !isInsidePlatform)//Delete the inside platform?
        {
            ignorePlatforms = false;
            Physics2D.IgnoreLayerCollision(8, 12, false);
            //Debug.Log("Going back");
        }

        ///if (Input.GetKey(KeyCode.DownArrow))
            ///Debug.Log(Physics2D.GetIgnoreLayerCollision(8, 12));
    }

    ///////////////////////////////////////////////////////////////////////

    public void DetermineIsAtopPlatform(float inputDirectionY)
    {
        if (IsAtopPlatform(inputDirectionY))
        {
            //First time landed
            if (isAtopPlatform == false)
            {
                //SFX
                PlayerSoundManager.globalInstance.PlayClip(landingPlatformSFX, PlayerSoundManager.AudioSourceName.PlayerLanding);
            }
            isAtopPlatform = true;
        }
        else
            isAtopPlatform = false;
    }

    //Useless inputDirectionY.
    public bool IsAtopPlatform(float inputDirectionY)
    {
        if (tempIntList.Count > 0)
            tempIntList = new List<int>();

        //Player is targetting down.
        //if (inputDirectionY < -0.1f)
        //return false;

        if (speedY > 0)
            return false;

        //This means that the warrior is INSIDE a platform, not atop.
        if (isInsidePlatform)
            return false;

        //Iterate all platforms registered collision-wise to be valid for "AtopPlatform"
        foreach(var pickedKey in atopPlatformCollisionDictionary.Keys)
        {
            //If the platform isn't registered before, add it in the dictionary
            if (PlatformsInteracted.ContainsKey(pickedKey) == false)
            {
                if (atopPlatformCollisionDictionary[pickedKey].gameObject != null)
                    PlatformsInteracted.Add(pickedKey, atopPlatformCollisionDictionary[pickedKey].gameObject.GetComponent<PlatformBehaviour>());
                else
                    tempIntList.Add(pickedKey);
            }
                
        }

        //Cannot remove keys of a collection while iterating it
        if (tempIntList.Count > 0)
        {
            for (int i = 0; i < tempIntList.Count; i++)
                atopPlatformCollisionDictionary.Remove(tempIntList[i]);
        }

        if (atopPlatformCollisionDictionary.Count > 0)
            return true;
        else
            return false;

    }

    //Disable platform for player so he descends
    public void DescendPlatform()
    {
        //Iterate all platforms registered collision-wise to be valid for "AtopPlatform"
        foreach(var pickedPlatformGameObject in atopPlatformCollisionDictionary.Values)
        {
            if (pickedPlatformGameObject.GetComponent<PlatformBehaviour>())
                pickedPlatformGameObject.GetComponent<PlatformBehaviour>().DisablePlatformForPlayer();
        }

    }

    public void ReactivateInteractedPlatforms()
    {
        if (PlatformsInteracted.Count > 0)
        {
            if (tempIntList.Count > 0)
                tempIntList = new List<int>();

            //Invoke the script to stop ignoring the collider
            foreach (KeyValuePair<int, PlatformBehaviour> entry in PlatformsInteracted)
            {
                if (entry.Value != null)//yes, it can give null, which is admittedly a bug but at least now it won't crash
                    entry.Value.ReactivatePlatformForPlayer();
                else
                    tempIntList.Add(entry.Key);
                
            }

            //tempIntList are used, because you cannot add or remove an element of a collection while iterating it without having InvalidOperationExceptionError
            if (tempIntList.Count > 0)
            {
                for (int i = 0; i < tempIntList.Count; i++)
                    PlatformsInteracted.Remove(tempIntList[i]);
            }

            //Clear the dictionary to start anew.
            PlatformsInteracted.Clear();

        }
    }

    //Checks if warrior is touching any wall to the sides
    public void DetermineWallSlide()//Would be more optimal to simply have 2 (or even 1) colliders, that simply follow player's position and inform him via action.
    {
        if (currentState == DEATH | currentState == REVIVE)
            return;

        //Debug.Log ("Key count: " + wallSlideColliderDictionary.Count);

        if (tempIntList.Count > 0)
            tempIntList = new List<int>();

        //Checking every collider, by iterating the dictionary keys
        //No forloop i=0++, because keys are unique and are far away from 0 ;)
        foreach (var pickedDictionaryKey in wallSlideColliderDictionary.Keys)
        {
            if (wallSlideColliderDictionary[pickedDictionaryKey] != null)
            {
                if (transform.position.x > wallSlideColliderDictionary[pickedDictionaryKey].transform.position.x)
                    isWallslidingLeft = true;
                else
                    isWallslidingLeft = false;

            }
            else
            {
                tempIntList.Add(pickedDictionaryKey);
                continue;
            }
            
            //Correcting the wallslide.
            if (currentState == WALLSLIDE)
            {
                if (facingLeft == false && isWallslidingLeft)
                    FlipPlayer();
                else if (facingLeft && isWallslidingLeft == false)
                    FlipPlayer();
            }


            //Making sure its in the air, so it doesnt walljump out of standing next to a wall.
            if (midair)
            {
                //WallJump Possible
                isWallSliding = true;

                return;
            }

        }

        //Cannot remove/add keys in a foreach collection iteration
        if (tempIntList.Count > 0)
        {
            for (int i = 0; i < tempIntList.Count; i++)
                wallSlideColliderDictionary.Remove(tempIntList[i]);
        }

        isWallSliding = false;
    }

    //Spagghetti
    public void DetermineHit()
    {
        if (justTookDamage)
        {
            //Resets this flag
            justTookDamage = false;

            isHitstunned = true;

            //So when it goes to the switch(currentState){} -> It goes to the below, checks ofc, and does stuff as intended.
            if (isAtopGround || isAtopPlatform)
                currentState = HITGROUND;
            else
                currentState = HITMIDAIR;

            //Changes animator controller depending on health.
            //animatorController.runtimeAnimatorController = currentlyActiveAnimationContainer.GetAnimator(warriorHealthScript.CurrentHealth);

            animatorController.Play(StateToString(currentState));

            //Stop running SFX
            RunningSFXActive = false;

            //Stop glide SFX
            PlayerSoundManager.globalInstance.Stop(PlayerSoundManager.AudioSourceName.Glide);
        }

    }

    //Used to skip revive/dialogue
    //The parameters are from Movement()
    public bool DoBothPlayersHaveActiveInput(Vector2 inputDirection, Vector2 combatInputDirection, bool _mageJump = false, bool _dodgeroll = false)
    {
        //If Player1 has any input active
        if (inputDirection != Vector2.zero || combatInputDirection != Vector2.zero)
            if (_mageJump == true || _dodgeroll == true)
                return true;

        //else
        return false;
    }

    public void DetermineDarkwindParticleEmission(Vector2 velocity, Vector2 inputDirection)
    {
        //If not moving
        //Debug.Log("Playing: " + currentDarkwindTrail.isPlaying);

        //Debug.Log("velocityX is: " + velocity.x);

        if (velocity.x == 0f && inputDirection == Vector2.zero && midair == false)
        {
            //Debug.Log("Stahped");
            DarkwindRunningEnds();
        }

        else if (darkwindRunningTimer.isActive == false)
        {
            //Debug.Log("Hmm");
            darkwindRunningTimer.Activate();
        }


        /*
        if (currentState != IDLE || inputDirection.x != 0f || inputDirection.y != 0f || midair || velocity != Vector2.zero)
        {
            //Debug.Log("isemitting" + currentDarkwindTrail.isEmitting);
            //Debug.Log("Timer" + darkwindRunningTimer.elapsed);
            //If no timer is running
            if (darkwindRunningTimer.isActive == false && currentDarkwindTrail.isEmitting == false)
            {
                darkwindRunningTimer.Activate();
            }
                
        }
        //Idle, stahp!
        else
            DarkwindRunningEnds();
       */

    }

    public void DarkwindRunningStarts()
    {
        currentDarkwindTrailOn = true;
        currentDarkwindTrail.Play();
    }

    //Called via Timer, when there is no running/moving
    public void DarkwindRunningEnds()
    {
        //Shuts down the particle emission and resets the timer
        if (currentDarkwindTrailOn)
        {
            //Stop particle emission
            currentDarkwindTrail.Stop();

            //Announce the stopped particle emission
            currentDarkwindTrailOn = false;

            //IsActive -> false && elapsed = 0f;
            darkwindRunningTimer.Reset();
        }

    }

    public void DarkwindRunningClear()
    {
        //Debug.Log("DarkwindTrailOn? " + currentDarkwindTrailOn);
        //Shuts down the particle emission and resets the timer
        if (currentDarkwindTrailOn)
        {
            //Stop particle emission
            currentDarkwindTrail.Clear();
            currentDarkwindTrail.Play();
            currentDarkwindTrail.Stop();

            //Announce the stopped particle emission
            currentDarkwindTrailOn = false;

            //IsActive -> false && elapsed = 0f;
            darkwindRunningTimer.Reset();
        }
    }

    IEnumerator RunningSFX()
    {
        activeRunningSFX++;
        RunningSFXActive = true;
        while (RunningSFXActive)
        {
            //TODO: Change the clip to be a little more... "aerial".
            PlayerSoundManager.globalInstance.PlayClip(playerRunningSFX, PlayerSoundManager.AudioSourceName.PlayerMovement);

            yield return new WaitForSeconds(0.35f);
        }

        activeRunningSFX--;
        //It reaches here when it is "about to close", aka runningSFXActive == false.
        yield break;
    }

    //Triggered by timer
    public void ActivateAttack()
    {
        int numberOfCollidersDetected = 0;

        //Please check if attack got cancelled somehow so no bugs.

        ContactFilter2D filteringEnemy = new ContactFilter2D();
        filteringEnemy.SetLayerMask(WhatIsDamageable);

        //Detects if attack got "interrupted"
        if (currentState != ATTACKMIDAIR && currentState != ATTACKGROUND && currentState != ATTACKDASH)
        {
            ResetAttackTimers();

            return;
        }

        //Clear collisions beforehand, so it won't attack dead enemies that are registered here...
        for (int i = 0; i < spearAttackCollisions.Length; i++)
            spearAttackCollisions[i] = null;


        //Collider Check
        //spearAttackCollisions is the Collider2D[] "exported" from the below function, which means it contains the collisions detected inside the spear colliders!
        if (currentAttackState == AttackState.DiagonalUp)
            numberOfCollidersDetected = diagonalUpAttackCollider.OverlapCollider(filteringEnemy, spearAttackCollisions);
        else if (currentAttackState == AttackState.DiagonalDown)
            numberOfCollidersDetected = diagonalDownAttackCollider.OverlapCollider(filteringEnemy, spearAttackCollisions);
        else if (currentAttackState == AttackState.Horizontal)
            numberOfCollidersDetected = horizontalAttackCollider.OverlapCollider(filteringEnemy, spearAttackCollisions);
        else if (currentAttackState == AttackState.VerticalUp)
            numberOfCollidersDetected = verticalUpAttackCollider.OverlapCollider(filteringEnemy, spearAttackCollisions);
        else if (currentAttackState == AttackState.VerticalDown)//AttackState is set by DetermineInput, so no worries checking if midair or not.
            numberOfCollidersDetected = verticalDownAttackCollider.OverlapCollider(filteringEnemy, spearAttackCollisions);


        Debug.Log("Activated attack");

        //Debug.Log(currentAttackState);
        //Debug.Log(currentState);

        //An enemy got hit! -> Damage Enemy(ies)
        if (numberOfCollidersDetected > 0)
        {
            Debug.Log("Hit: " + spearAttackCollisions[0].gameObject.name);
            //Debug.Break();

            //APPLYING DAMAGE RIGHT BELOW

            //Iterating through all enemies hit
            for (int i = 0; i < spearAttackCollisions.Length; i++)
            {
                //Reached the end
                if (spearAttackCollisions[i] == null)
                    break;

                EnemyToHit = spearAttackCollisions[i].gameObject.GetComponent<IDamageable>();
                //If the enemy has an IDamageable (all should have but anw)
                if (EnemyToHit != null)
                {
                    //He should die now, you killed him! SFX+ SetMana++ ResetDash++
                    //Hit Enemy SFX
                    tempIndex = UnityEngine.Random.Range(0, 4);//What if I told you, this variable could be outright deleted from this class, by simply making the code a bit harder to read. (aka replace tempIndex with UnityEngine.Random.Range(blabla);)
                    if (EnemyToHit.CurrentHealth == 1)
                    {
                        //Killed him, so raise the pitch
                        PlayerSoundManager.globalInstance.PlayClip(spearHitAttacksSFX[tempIndex], PlayerSoundManager.AudioSourceName.SpearHit, 0.75f);

                        //Reward player with 1 mana (except minibosses)
                        playerManaManager.AddPlayerMana(spearAttackCollisions[i].gameObject.GetComponent<EnemyBehaviour>().giveManaOnDeath);

                        //Fire level for blood/kill meter
                        if (LevelManager.currentLevel == 2)
                            killMeter.ResetBloodMeter();

                        //ResetDash
                        OnKillResetDodgeroll();

                        //You can now walljump again after getting hit.
                        if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.KillAndHit || DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.Kill)
                            ResetWalljump();

                        //If netcoding, notify the kill and which monster was killed
                        if (justKilledEnemy != null)
                            justKilledEnemy(spearAttackCollisions[i].gameObject.GetComponent<EnemyBehaviour>().enemyListIndex);
                    }
                    else
                    {
                        //Normalize the pitch
                        PlayerSoundManager.globalInstance.PlayClip(spearHitAttacksSFX[tempIndex], PlayerSoundManager.AudioSourceName.SpearHit, 1);

                        //Minibosses reward player with 1 mana -> Denied. But for future content hmm.... Dat level editor, players will make use of this for sure.
                        playerManaManager.AddPlayerMana(spearAttackCollisions[i].gameObject.GetComponent<EnemyBehaviour>().giveManaOnHit);
                    }
                        



                    if (currentState == ATTACKMIDAIR)
                        EnemyToHit.TakeDamage(1, transform.position, MidairX, knockbackPowerY, hitstun);
                    else if (currentState == ATTACKGROUND)
                        EnemyToHit.TakeDamage(1, transform.position, GroundX, knockbackPowerY, hitstun);
                    else if (currentState == ATTACKDASH)
                        EnemyToHit.TakeDamage(1, transform.position, DashX, knockbackPowerY, hitstun);


                    //Minor Screen shake(The player musn't notice :P )
                    //Checking if insanity cuz it really fucks the camera there.
                    if (GameObject.FindGameObjectWithTag("BackgroundManager").GetComponent<InsanityToggle>().GetInsanityActive() == false)
                        CameraShaker.Instance.ShakeOnce(4f, 4f, 0f, .1f);



                    //Update Playerstats
                    PlayerStatsManager.globalInstance.IncreaseDamageDealtCount();

                    EnemyToHit = null;//while i clear the spearAttackCollisions array, this is a confirm, haven't had time to debug with this removed, but it should work without it as well.
                }
                /* Linking by warrior is removed because it removes the "Link" capability of P2, and also because P1 can link via powerup anyway
                else if (spearAttackCollisions[i].gameObject.CompareTag("DesyncSphere"))//if netcoding bs + desynced sphere detected
                {
                    //Debug.Log("Moshi moshi");

                    //If max mana, link! (smh mana cost hard-coding. Fix this, by making all spells' mana costs, public static -> Cannot convert to Spell, this way, though.)
                    if (playerManaManager.GetCurrentMana() == 4 && NetworkCommunicationController.globalInstance.IsServer())//2nd parameter confuses me, see what is inside the scope below
                    {
                        if (NetworkCommunicationController.globalInstance.IsServer())//IsWarrior/Host
                        {
                            NetworkCommunicationController.globalInstance.SendDesyncedSphereLink();
                            playerManaManager.AddPlayerMana(-4);
                        }
                        else//IsMage/Client
                            FindObjectOfType<SpellLink>().MageCastLink();
                    }
                    continue;//Go to next colliders instead of exiting
                }
                */
                /*
                else if (spearAttackCollisions[i].gameObject.CompareTag("EnemyProjectile"))
                {
                    Debug.Break();
                    Debug.Log("Something detected! Name is: " + colliders[i].gameObject.name);

                    spearAttackCollisions[i].gameObject.GetComponent<ProjectileDamageTrigger>().Death();
                }
                */

            }

        }

        //Check netcoding bs -_-






        //Start Recovery
        if (currentState == ATTACKMIDAIR)
            recoveryAttackMidairTimer.Activate();
        else if (currentState == ATTACKGROUND)
            recoveryAttackGroundTimer.Activate();
        else if (currentState == ATTACKDASH)
            recoveryAttackDashTimer.Activate();

    }

    //Triggered when the attack finishes (recovery) and manually gtfos the switch-case
    public void FinishAttack()
    {
        //resets this, so player can attack again.
        //hasStartedAttack = false;//Not needed, since exitting from any attack state, hasStartedAttack = false ;)


        //Detects if attack got "interrupted"
        if (currentState != ATTACKMIDAIR && currentState != ATTACKGROUND && currentState != ATTACKDASH)
            return;


        if (currentState == ATTACKMIDAIR)
            targetState = FLOATING;
        else if (currentState == ATTACKGROUND || currentState == ATTACKDASH)
            targetState = IDLE;
        /* Commenting the below since inputDirection is out of scope, and belongs to Movement :(
        else if (currentState == ATTACKGROUND)
        {
            if (inputDirection == Vector2.zero)
                targetState = IDLE;
            else
                targetState = RUN;
        }*/

        //Resets influenceX so after dash, it won't glide or fuck up the movement, aka feel natural :P
        if (currentState == ATTACKDASH)
        {
            //ResetTotalInfluenceX();


            /* 
            //The reason I remove this, is mostly because after a single dash attack, moving in the opposite way of the dash attack
            //makes influence exist and **resist** your movement, which fucking sucks. Like, you legit cannot move in the opposite way for 2 seconds...
            //aside of that, if the below really worked, I think the sliding would be fun for doing dash+dash-attack combos.

            Debug.Log("Old Influence is: " + influenceX);
            if (facingLeft)
                CalculateForces(true, 10);
            else
                CalculateForces(true, -10);
            Debug.Log("New Influence is: " + influenceX);
            */

            //aylmao dat above. tl;dr: Sliding is fun, even if it seems janky to some, cuz it opens up movement so much more!
            //And while you may think that your average player would dislike this "sliding" 
            //and would rather stop in place when it happens (e.g. dash to dash attack and no further input, slides while idle)
            //what is REALLY needed, is some animations for those circumstances ;)
            //One for sliding forward while not doing anything (e.g. running) and one for sliding backwards while not doing anything.
        }
            

        NewStateTransition();

        Debug.Log("Completely finished the attack");
    }

    public void ResetAttackTimers()
    {
        //For attacks, so they won't bug
        if (currentState == ATTACKMIDAIR || targetState == ATTACKMIDAIR)
        {
            castingAttackMidairTimer.Reset();
            recoveryAttackMidairTimer.Reset();
            hasStartedAttack = false;
        }
        else if (currentState == ATTACKGROUND || targetState == ATTACKGROUND)
        {
            castingAttackGroundTimer.Reset();
            recoveryAttackGroundTimer.Reset();
            hasStartedAttack = false;
        }
        else if (currentState == ATTACKDASH || targetState == ATTACKDASH)
        {
            castingAttackDashTimer.Reset();
            recoveryAttackDashTimer.Reset();
            hasStartedAttack = false;
        }
    }

    //Get if player is doing an attack, but the active hitbox (ActivateAttack()) hasn't happened yet.
    public bool GetChargingAttack()
    {
        if (currentAttackState == AttackState.DiagonalDown && (currentState == ATTACKMIDAIR || currentState == ATTACKGROUND || currentState == ATTACKDASH))
        {
            if (castingAttackDashTimer.elapsed != 0 || castingAttackGroundTimer.elapsed != 0 || castingAttackMidairTimer.elapsed != 0)
                return true;
        }
        return false;
    }

    public void ResetSpearThrustSFX()
    {
        if (currentState == ATTACKDASH || currentState == ATTACKGROUND || currentState == ATTACKMIDAIR)
            PlayerSoundManager.globalInstance.Stop(PlayerSoundManager.AudioSourceName.SpearThrust);
    }

    public void PlaySpearThrustSFX()
    {
        //Spear thrust SFX
        PlayerSoundManager.globalInstance.ChangePitch(1, PlayerSoundManager.AudioSourceName.SpearThrust);
        tempIndex = UnityEngine.Random.Range(0, 3);
        if (currentState == ATTACKDASH)
            PlayerSoundManager.globalInstance.PlayClip(spearDashAttacksSFX[tempIndex], PlayerSoundManager.AudioSourceName.SpearHit);
        else//Ground/Dash
            PlayerSoundManager.globalInstance.PlayClip(spearGroundMidairAttacksSFX[tempIndex], PlayerSoundManager.AudioSourceName.SpearHit);
    }

    public void OnDodgerollExit()
    {
        canDodgeroll = true;
    }

    //Mage behaviour invokes this via fireball (or reflecting enemy projectiles ofc)
    public void RangedKillReward(int manaToReward)
    {
        //Reward player with manaToReward mana
        playerManaManager.AddPlayerMana(manaToReward);

        //Fire level for blood/kill meter
        if (LevelManager.currentLevel == 2)
            killMeter.ResetBloodMeter();

        //ResetDash
        OnKillResetDodgeroll();

        //You can now walljump again after getting hit.
        if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.KillAndHit || DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.Kill)
            ResetWalljump();
    }

    //TODO: Check if game has paused.
    //Runs every InfluenceDelay seconds, and it slowly gets InfluenceXY towards 0, by InfluenceDiminisher every cycle.
    IEnumerator InfluenceReducer()
    {
        while (true)
        {
            //Axis X
            if (GetTotalInfluenceX() != 0)
            {
                //Positive influenceX
                if (GetTotalInfluenceX() > 0)
                {
                    //If its really small, get it to 0, so it won't bounce between [-0.5,0.5]
                    if (controllableInfluenceX < InfluenceDiminisher)
                        controllableInfluenceX = 0;

                    if (UncontrollableInfluenceX < InfluenceDiminisher)
                        UncontrollableInfluenceX = 0;
                       

                    //Reduces influenceX, since the point is to slowly go towards 0.
                    if (UncontrollableInfluenceX > InfluenceDiminisher)
                        UncontrollableInfluenceX -= InfluenceDiminisher;
                    else if (controllableInfluenceX > InfluenceDiminisher)
                        controllableInfluenceX -= InfluenceDiminisher;
                }
                //Negative influenceX
                else if (GetTotalInfluenceX() < 0)
                {
                    //If its really small, get it to 0, so it won't bounce between [-0.5,0.5]
                    if (controllableInfluenceX > InfluenceDiminisher * -1)
                        controllableInfluenceX = 0;

                    if (UncontrollableInfluenceX > InfluenceDiminisher * -1)
                        UncontrollableInfluenceX = 0;

                    //"Reduces" influenceX, since the point is to slowly go towards 0.
                    if (UncontrollableInfluenceX < InfluenceDiminisher * -1)
                        UncontrollableInfluenceX += InfluenceDiminisher;
                    else if (controllableInfluenceX < InfluenceDiminisher * -1)
                        controllableInfluenceX += InfluenceDiminisher;

                }
            }
            //Axis Y
            if (totalInfluenceY != 0)
            {
                //Positive influenceY
                if (totalInfluenceY > 0)
                {
                    //If its really small, get it to 0, so it won't bounce between [-0.5,0.5]
                    if (totalInfluenceY < InfluenceDiminisher)
                        totalInfluenceY = 0;

                    //Reduces influenceY, since the point is to slowly go towards 0.
                    totalInfluenceY -= InfluenceDiminisher;
                }
                //Negative influenceY
                else if (totalInfluenceY < 0)
                {
                    //If its really small, get it to 0, so it won't bounce between [-0.5,0.5]
                    if (totalInfluenceY > InfluenceDiminisher)
                        totalInfluenceY = 0;

                    //"Reduces" influenceY, since the point is to slowly go towards 0.
                    totalInfluenceY += InfluenceDiminisher;
                }
            }


            //Wait for next second.
            yield return new WaitForSeconds(InfluenceDelay);
        }
    }

    /// <summary>
    /// Please use it sparingly. This function deletes all forces and accelerations of this body.
    /// </summary>
    public void ResetInfluences()
    {
        ResetTotalInfluenceX();
        totalInfluenceY = 0;
    }

    //Called when mana is about to change, and it does any checks/limits here ;)
    public void SetMana(int value)
    {
        if (value == 0 && GameManager.testing == false)
            Debug.LogError("Lost/Gained 0 mana? Really? plz fix.");

        //Lose mana
        if (value < 0)
            for (int i = 0; i < value * -1; i++)
                playerManaManager.RemoveMana();

        //Gain Mana
        else
            for (int i = 0; i < value; i++)
                playerManaManager.AddMana();

    }

    //Calculates external forces, related to movement.
    public void CalculateForces(bool axisX, float influence, bool restricted = true, bool controllable = true, bool exclusivelyAdditive = false)
    {
        if (axisX)
        {
            if (controllable)
            {
                //If opposite forces, reset to 0 or rotate influence!
                if ( (influence > 0 && controllableInfluenceX < 0) || (influence < 0 && controllableInfluenceX > 0) )
                {
                    //If its not meant to be added on top of current influenceX
                    if (exclusivelyAdditive == false)
                        controllableInfluenceX = 0;

                    //ExclusivelyAdditive, means that the influence is added towards current direction whatever that is
                    //In simpler words, not simply preserve the momentum, but enhance it!!
                    else
                        controllableInfluenceX = controllableInfluenceX * -1;

                }

                //if is in contact with a ground wall, reset influenceX, for no cheese by stacking momentum on the wall (regardless if additive or not)
                if (isSidewalled && isAtopGround && DarkwindDistortionManager.globalInstance.allowGroundWallSpeedCheese == false)
                    controllableInfluenceX = 0;

                controllableInfluenceX += influence;
            }
            else
                UncontrollableInfluenceX += influence;

            //Limit InfluenceX to 200.
            if (DarkwindDistortionManager.globalInstance.allowInfiniteSpeedMomentum == false)
            {
                //If should limit, limit it (in a hacky, but fun in-game way!)
                if (GetTotalInfluenceX() > 200)
                {
                    UncontrollableInfluenceX = 0;
                    controllableInfluenceX = 200;
                }
                else if (GetTotalInfluenceX() < - 200)
                {
                    UncontrollableInfluenceX = 0;
                    controllableInfluenceX = -200;
                }
            }
        }
        else//axisY
        {
            totalInfluenceY += influence;

            //So jump doesn't get out of control
            if (totalInfluenceY > 32 && restricted)//Make 32 a public variable, ffs.//Also, figure out how to make both types of jumps reach same velocity?(both players jumping from ground, and the other being the optimal jump difference)
                totalInfluenceY = 32;
        }
    }

    public float GetTotalInfluenceX()
    {
        return controllableInfluenceX + UncontrollableInfluenceX;
    }

    public float GetControllableInfluenceX()
    {
        return controllableInfluenceX;
    }

    public float GetUncontrollableInfluenceX()
    {
        return UncontrollableInfluenceX;
    }

    //To be used from hazards (so dashing to them doesn't get u to stay there)
    public void ResetTotalInfluenceX()
    {
        controllableInfluenceX = 0;
        UncontrollableInfluenceX = 0;
    }

    public void ResetTotalInfluenceY()
    {
        totalInfluenceY = 0;
    }


    //Miscellaneous Functions//

    //Please check the state, some shouldnt be able to flip!!!!
    public void DetermineFlipPlayer(Vector2 inputDirection)
    {
        //Current states that it doesn't flip and it insta-returns:
        //Wallslide
        //Walljump
        //Attacking
        //Hit
        //Dodgerolling

        //Shouldn't flip at wallJump
        if (currentState == WALLSLIDE || currentState == WALLJUMP)
            return;

        //Shouldn't flip on attacking
        if (currentState == ATTACKMIDAIR || currentState == ATTACKGROUND || currentState == ATTACKDASH)
            return;

        //Shouldn't flip while damaged lel.
        if (currentState == HITGROUND || currentState == HITMIDAIR)
            return;

        //Shouldn't flip while dodgerolling/wavedashing lel
        if (currentState == DODGEROLL || currentState == WAVEDASH)
            return;

        //Shouldn't flip while reviving/dialoguing lel
        if (currentState == REVIVE || currentState == DIALOGUE)
            return;

        //Flip input detected, poll/arrange for a flip! (via the boolean)
        if (inputDirection.x < 0f && facingLeft == false)
            doFlipPlayer = true;
        else if (inputDirection.x > 0f && facingLeft == true)
            doFlipPlayer = true;

        //Flip confirmed, and in run state
        if (doFlipPlayer && currentState == RUN && controllableInfluenceX != 0)
        {
            if (DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull == false)
                controllableInfluenceX = 0;//HALT!

            //Replaying the animation, so the dodgeroll animation will definitely not be playing on the other side.
            animatorController.Play("Run");

        }

        //Time to flip player
        if (doFlipPlayer)
        {
            FlipPlayer();
            doFlipPlayer = false;
        }
            
    }

    public void WallFlipPlayer()
    {
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void FlipPlayer()
    {
        facingLeft = !facingLeft;
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    //InvokeOnNextFrame should be a global coroutine.

    /// <summary>
    /// Facing the wall on revive is weird. Depending on level, it flips the player towards the exit/level
    /// </summary>
    public void DetermineLevelFlipPlayer()
    {
        //Level0~2
        if (LevelManager.currentLevel < 3 && facingLeft)
            FlipPlayer();
        //Level3~4
        else if (LevelManager.currentLevel > 2 && facingLeft == false)
            FlipPlayer();
    }

    public void StartDying()
    {
        //Useful flags for WarriorHealth script
        dying = true;
        startDying = true;

        //Update the Death Counter.
        PlayerStatsManager.globalInstance.IncreaseDeathCount();

        //Update the deathPositions
        PlayerStatsManager.globalInstance.UpdateDeathPosition(transform.position);

        //Slow motion HOYPE
        Time.timeScale = 0.3f;

        //Natural transition there
        targetState = DEATH;
        NewStateTransition();

        //Play DeathSFX
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //Fade out the music currently playing.
        MusicSoundManager.globalInstance.MusicFadeOut();

        //Reset all state conditions so 0 bugs when revived, cuz i dont want to check from what state i came here smh.
        ResetStateConditions(false);

        //Ignore all platforms
        //Physics2D.IgnoreLayerCollision(8, 12, true);

        //Changes layer to 16(Uninteractive), so it will ignore all ground/enemies/platforms.
        gameObject.layer = 16;

        //TODO: Reset all timers running.

        //TODO: On getting attacked, u shouldn't be able to change facing!

        //Stop running SFX (in case it isnt stopped already?)
        RunningSFXActive = false;

        if (isGlidingActive) //Active but not to glide, so shut it down!
            StopGlidingFX();

        //Notifies FadeUIManager, and brazier
        if (startedDying != null)
            startedDying.Invoke();

        //If level editor + pause mode
        if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
        {

        }
        else// Zoom in the camera for that revive/dialogue.
            GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().CutsceneZoomIn(0.1f);

        //Starts the timer(because timers are dependent on timescale, using Coroutine which is independent
        StartCoroutine(InvokeRealtimeCoroutine(DeathStop, DeathStopDelay));

        //Revives the player after this ends
        StartCoroutine(InvokeRealtimeCoroutine(Revive, ReviveDelay));
    }

    private IEnumerator InvokeRealtimeCoroutine(Action action, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (action != null)
            action();
    }

    public void DeathStop()
    {
        //stops the dying stuff, like knockback + slow-mo
        startDying = false;

        //Reset the time.
        Time.timeScale = 1f;

        //Pause the particle effects
        DarkwindRunningEnds();
        //If you remove this ^, add frames 16~24 back to death.

        //Putting this in Revive() bugs it horribly for some reason. So it is done beforehand here, players won't notice. After all, only P2 is noticed but he should be faded by then.
        DetermineLevelFlipPlayer();
    }

    public void Revive()
    {
        //Does more or less what is necessary to spawn at levelstart.
        ResetAtLevelStart();

        //Play revival animation
        targetState = REVIVE;
        NewStateTransition();

        //There is a bug where isHitstunned retains up to Revival and onwards which means ur stuck there. I have no idea how, but this is an fix to the symptom not the disease :P
        isHitstunned = false;//I think it doesn't work.

        //Play SFX
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Revive);

        //Changes controller, since max health now.
        //animatorController.runtimeAnimatorController = currentlyActiveAnimationContainer.GetAnimator(warriorHealthScript.MaxHealth);

        //Changes layer to 8(Player), so it won't ignore all ground/enemies/platforms.
        gameObject.layer = 8;

        //startDying should be useless, and dying isn't used but whatever, this is a failsafe just in case.
        dying = false;
        startDying = false;

        //Play that circle animation
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MagicCircleRevive, checkpointPosition + new Vector3(0.5f, -3, 6));

        //Play the darkwind VFX
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.ReviveWind, checkpointPosition + new Vector3(0.6f, 2.73f));

        //Update playerstats that the previous run ended
        PlayerStatsManager.globalInstance.ResetCurrentRunStats();

        //Transitions to dialogueState when revive animation ends (which takes approx 2.45 seconds)
        StartCoroutine(InvokeRealtimeCoroutine(StartReviveDialogue, 2.45f));

        //Notifies every1 that player is dead. Unknown rn how many subscirbers, but FadeUIManager is definitely subscribed.
        if (startedRevive != null)
            startedRevive.Invoke();
    }

    public void ResetAtLevelStart()
    {
        //Move to checkpoint.
        //transform.position = checkpointPosition;//Just saying, rigidbody.position may be better. idk full differences, but if i want to trigger something in revive area, it won't happen.
        rigidbody.position = checkpointPosition;//otherwise, it bugs with hazard's collisionExit.

        //TODO: no sprite should be visible when this is a thing.
        //Resets the rotation, because on death, it rotates towards the damage dealt
        rendererBehaviour.ResetRotation();

        //Reset velocity
        rigidbody.velocity = Vector2.zero;

        //Makes both InfluenceX&Y -> 0
        ResetInfluences();

        //Clear the trail
        DarkwindRunningClear();

        //Stop running SFX
        RunningSFXActive = false;

        //Set camera instantly at player's location (while screen is fully blacc) so it won't be moving from death->revive locations, when revive is happening!
        if (LevelManager.currentLevel != 7)
            GameObject.FindGameObjectWithTag("CameraHolder").transform.position = rigidbody.position;

        //Play Music
        MusicSoundManager.globalInstance.PlayMusic(MusicSoundManager.AudioSourceType.checkpointStart);
    }

    public void StartReviveDialogue()
    {
        if (currentState == REVIVE)
        {
            //Play dialogue animations
            targetState = DIALOGUE;
            NewStateTransition();

            if (finishedRevive != null)
                finishedRevive.Invoke();
        }
        
    }

    //Triggered by DialogueManager when dialogue ends.
    public void FinishedReviveDialogue()
    {
        //TODO: Get sprite renderer to show the dialogueIdle sprite/animation. -> So... dialogueIdle1 which is already made? ;)
        currentState = IDLE;

        //Unzoom (if no level editor + pause mode)
        //If level editor + pause mode
        if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
        {

        }
        else
            GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().ResetZoomToDefault();

        //Debugger to die nonstop to see the dialogues
        if (permadieDebugger)
            warriorHealthScript.Die();
    }

    public void StartLevelCutscene()
    {
        //Flag used to know if in levelcutscene
        InLevelCutscene = true;

        //So in level 2, P2 and his mana won't dissapear mid-dialogue
        killMeter.SetCutsceneStatus(true);

        //Play dialogue animations.
        targetState = DIALOGUE;
        NewStateTransition();

        //Zoom in the camera for that revive/dialogue.
        if (LevelEditorMenu.globalInstance == null)//If not level editor
            GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().StartCutsceneZoomIn();

        //Reset velocity/accelerations.
        ResetInfluences();
        rigidbody.velocity = new Vector2(0f, -15f);

        //Stop running SFX
        RunningSFXActive = false;

        if (isGlidingActive) //Active but not to glide, so shut it down!
            StopGlidingFX();

        //Reset all state conditions so 0 bugs when no cutscene, cuz i dont want to check from what state i came here smh.
        ResetStateConditions(false);

        //De-activate the VFX of DarkwindRunning
        DarkwindRunningEnds();

        ResetTimers();
    }

    public void FinishedLevelCutscene()
    {
        //Useless flag that may be used in the future
        InLevelCutscene = false;

        //So in level 2, P2 and his mana will start dissapearing after mid-dialogue
        killMeter.SetCutsceneStatus(false);

        //TODO: This camera shit shouldnt be on warrior!!!!
        //Unzoom so camera is normal
        if (LevelManager.levelCutsceneDialogueIndex == 10)
            GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<Animator>().Play("ZoomOutMax");
        else if (LevelEditorMenu.globalInstance == null)//If not level editor, Zoom in the camera for that revive/dialogue.
        //if (LevelManager.levelCutsceneDialogueIndex < 10)//9 is the creditsbosses end, 10 is finalboss, so if less than 10 do normal camera
            GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().EndCutsceneZoom();

        //P2 to start fading once skip happens, otherwise looks awkward af.
        //However, not when end cutscene, otherwise he will fade on the start cutscene of next level -_-
        if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)
            mageBehaviour.StartFadingTransparency();
        else//End cutscene.
        {
            //Unsubscribe from this level's brazier
            if (GameObject.FindGameObjectWithTag("Brazier") != null)
                GameObject.FindGameObjectWithTag("Brazier").GetComponent<BrazierBehaviour>().UnsubscribeFromPlayer();
        }
            

        //Set to idle, so no bugs wont happen
        currentState = IDLE;

        //Play Animation depending on variable -> wut?
        //First Run -> TauntPose3
        if (PlayerStatsManager.globalInstance.IsFirstRun())
            animatorController.Play("TauntPose3", -1, 0f);
    }

    //Called by Level Editor, finishing a level, because there is no proper level ending -_-
    public void PlayVictoryPose()
    {
        if (PlayerStatsManager.globalInstance.GetTotalDeaths() == 0)
            animatorController.Play("TauntPose2");
        else
            animatorController.Play("TauntPose1");
    }

    //Resets every possible timer.
    public void ResetTimers()
    {
        //Reset Cooldown
        //CooldownDodgeRollOver();//Commented out cuz it fucked VFX/SFX
        DodgerollInCooldown = false;

        //Resets phantomdodgeroll cd, and isDodgerolling -> false.
        DodgerollFinished();

        //Resets all attack timers, so 0 bugs.
        ResetAttackTimers();

        //Hitstun not needed, since it will eventually stop from death or cutscene.
    }

    /// <summary>
    /// Since I don't have a walking animation, at least reduce the animation speed.
    /// </summary>
    /// <param name="inputX"></param>
    public void DetermineAnimatorSpeed(float inputX)
    {
        if (inputX < 0)
            inputX = inputX * -1;

        //Normal Playback
        if (inputX > 0.8f)
        {
            if (animatorController.speed != 1)
                animatorController.speed = 1;
        }
        else if (inputX > 0.4f)
        {
            if (animatorController.speed != 0.6f)
                animatorController.speed = 0.6f;
        }
        else
        {
            if (animatorController.speed != 0.4f)
                animatorController.speed = 0.4f;
        }

    }

    void DisplayVelocity(Vector2 inputDirection)
    {
        if (GameManager.testing)
        {
            //Blue = Velocity
            //Debug.Log(rigidbody.velocity + " and " + transform.localPosition);
            Debug.DrawRay(transform.localPosition, rigidbody.velocity / 3, Color.blue);// dividing by 3 cuz the ray is too big.

            //Green = Input
            Debug.DrawRay(transform.localPosition, inputDirection, Color.green);
        }
    }




    //When player lands right atop a monster.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Collision with player is: " + collision.contacts[0].normal);

        if (collision.gameObject.CompareTag("Enemy") && collision.contacts[0].normal.y > 0.95f)
        {
            //Debug.Log("Detected");
            //Debug.Break();
            Debug.Log("Upwards collision");

            //Hacky but whatever.
            if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
                return;

            //Push Player to the left
            warriorHealthScript.StompPushed(false);

            //Push enemy to the right
            collision.gameObject.GetComponent<EnemyBehaviour>().StompPushed(true);
        }
        //So midair, he wont get stuck forever like that. The side platform variable is "refreshed" upon touching a ground or platform.
        //When player lands on the sides/edge of platform. In a normal game, you get a stumble/getup animation. Not this one, not today. Gotta keep the flow!
        else if (collision.gameObject.CompareTag("Platform") && collision.contacts[0].normal.x != 0 && collision.contacts[0].normal.y == 0)
        {
            sidePlatform = collision.gameObject.GetComponent<PlatformBehaviour>();
            sidePlatform.DisablePlatformForPlayer();

            //if it bugs, check the currentstate to be FLOATING and also reset it on jump ;)
        }

        #region OG Ground Check
        /*//To check for ground
        //If contacted ground
        if ((WhatIsGround & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
        {
            Vector2 collisionPoint = collision.contacts[0].normal;

            //Debug.Log("Collided with the ground at: " + collisionPoint);

            /*
            Code Text on how to check the collisionPoint details:
            normal.y is numerically equal to the sine of the normal angle relative to the horizontal plane
            thus you can refine your code by accepting floor or roof collisions only when the normal.y is higher than the sine of the angle( < -45 or > 45)
            for instance, would become normal.y < -0.707 or normal.y > 0.707).

            //If standing atop the ground
            if (collisionPoint.y > 0)//actually, it should be 1 if flat
            {
                isAtopGround = true;

                //Add the collided ground gameobject to the currentCollisions dictionary, so when it exits, the identification will be ez
                //If the collision isn't registered before, add it in the dictionary
                if (currentGroundCollisions.ContainsKey(collision.gameObject.GetInstanceID()) == false)
                    currentGroundCollisions.Add(collision.gameObject.GetInstanceID(), collision.gameObject);
            }
        }
        */
        #endregion
    }



    //Called via LevelManager
    public void OnLevelLoad()
    {
        //so it won't bug, and player is always grounded.//one of them should be deleted i think. One of them is an artifact probably.
        GetComponentInChildren<GroundDetectionPlayer>().ClearCollisions();
        currentGroundCollisions.Clear();

        //same reason as above. Especially in loading new levels, this bugs if you were wallsliding.
        GetComponentInChildren<WallSlideCheck>().ClearCollisions();
        wallSlideColliderDictionary.Clear();

        //Reset position. All levels have 0,0 as the start, except level editor
        //Should just refactor (See checkpointposition) to always spawn at Start entrance, raycasting downwards from middle of sprite to get perfect position.
        if (LevelManager.currentLevel != 7)
        {
            rigidbody.position = Vector3.zero;
            transform.position = Vector3.zero;
        }
        else
        {
            rigidbody.position = checkpointPosition;
            transform.position = checkpointPosition;
        }
        //You may think that rigidbody.position is enough, but without this, the new darkwindTrail (spawned below on upgradedarkwind) glitches out for 1 frame and goes all over the place.

        //Gets current level container./pallete.//kappa.
        currentlyActiveAnimationContainer = commonAnimationManager.LevelUpdateAnimations();

        //Changes controller depending on health.//Not anymore! Clean this animator Container shit up when u have time.
        animatorController.runtimeAnimatorController = currentlyActiveAnimationContainer.GetAnimator();


        //Notifies manaManager, so it updates the orb color palette, and same for manaRing
        playerManaManager.OnLevelLoad();

        UpgradeDarkwindMaterials();

        mageBehaviour.OnLevelLoad();

        warriorHealthScript.MaxHealth = warriorHealthScript.NormalMaxHealth;

        //Reset HP (not temp hp!)
        warriorHealthScript.SetStartingHealth();

        GetComponent<HealthIndicationManager>().Reset();

        if (LevelManager.currentLevel == 3)
        {
            movementSpeed += FinalLevelMovementBoost;
            airSpeed += FinalLevelMovementBoost;
        }

        //Flip based on level
        if (LevelManager.currentLevel < 3 && facingLeft == true)
            FlipPlayer();
        else if (LevelManager.currentLevel == 7)
        {
            if (facingLeft)
                FlipPlayer();
        }
        else if (LevelManager.currentLevel > 2 && facingLeft == false)
            FlipPlayer();


        //===Enviromental VFX===
        //Destroys previous EnviromentVFX
        Destroy(GameObject.FindGameObjectWithTag("EnviromentVFX"));

        //Attaches the enviroment VFX to the player so they spawn on him.
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.Enviroment, transform.position + new Vector3(0, -10, 0), this.gameObject);

        //So on 1 HP, background VFX gets intense. Passes the above freshly created VFX so no need to find for tag "EnviromentVFX"
        warriorHealthScript.CacheEnviromentalVFX( VFXManager.globalInstance.GetLastCreatedVFX() );
    }

    /// <summary>
    /// Changes the material of the following particle systems: Dodgeroll,Wavedash,Glide,Damaged -> Depending on level (coloring). Also, changes the running trail.
    /// </summary>
    public void UpgradeDarkwindMaterials()
    {

        if (LevelManager.currentLevel == 2)
        {
            //Sets all the darkwind slices (half-moons) particles, for dodgeroll/wavedashing, gliding and damaged.
            darkwindDodgeroll.gameObject.GetComponent<ParticleSystemRenderer>().material = Dash2;
            darkwindWavedash.gameObject.GetComponent<ParticleSystemRenderer>().material = Dash2;
            darkwindGliding.gameObject.GetComponent<ParticleSystemRenderer>().material = Glide2;
            warriorHealthScript.SetDarkwindDamagedMaterial(Damaged2);

            //Sets the (darkwind) trail
            tempGameObject = Instantiate(darkwindTrail2, transform.GetChild(0));
            tempGameObject.transform.localPosition += new Vector3(0, 0);//aka player's feet//old. Sad, but it had some visual glitches. Was (-0.4f|-1) and then (-0.2f|-1.4f), both problematic, but if fixed, both cooler.(the -1.4f especially)
            Destroy(currentDarkwindTrail.gameObject);
            currentDarkwindTrail = tempGameObject.GetComponent<ParticleSystem>();
        }
        else if (LevelManager.currentLevel == 3)
        {
            //Sets all the darkwind slices (half-moons) particles, for dodgeroll/wavedashing, gliding and damaged.
            darkwindDodgeroll.gameObject.GetComponent<ParticleSystemRenderer>().material = Dash3;
            darkwindWavedash.gameObject.GetComponent<ParticleSystemRenderer>().material = Dash3;
            darkwindGliding.gameObject.GetComponent<ParticleSystemRenderer>().material = Glide3;
            warriorHealthScript.SetDarkwindDamagedMaterial(Damaged3);

            //Sets the (darkwind) trail
            tempGameObject = Instantiate(darkwindTrail3, transform.GetChild(0));
            tempGameObject.transform.localPosition += new Vector3(0, 0);
            Destroy(currentDarkwindTrail.gameObject);
            currentDarkwindTrail = tempGameObject.GetComponent<ParticleSystem>();
        }
    }

    public void UpdateCheckpointPosition(GameObject checkpointGameObject)
    {
        UpdateCheckpointPosition(checkpointGameObject.transform.position);
    }

    public void UpdateCheckpointPosition(Vector3 checkpointNewPosition)
    {
        checkpointPosition = checkpointNewPosition;
    }

    /// <summary>
    /// Used by Debugger.cs, when gameManager.testing == true
    /// If == false, then debugger should never get here, because these buttons are reserved for WarriorInputManager jumps.
    /// </summary>
    public void TestForces()
    {
        if (Input.GetButtonDown("ButtonX"))
            CalculateForces(true, -20f);
        if (Input.GetButtonDown("ButtonA"))
            CalculateForces(true, 20f);
        if (Input.GetButtonDown("ButtonB"))
            CalculateForces(false, -20f, false);
        if (Input.GetButtonDown("ButtonY"))
            CalculateForces(false, 20f, false);
    }

    public void DumpValues(Vector2 inputDirection)
    {
        //Pauses
        Debug.Break();

        Debug.Log("SpeedX: " + speedX + "\nSpeedY: " + speedY + "\nInfluenceX: " + GetTotalInfluenceX() + "\nInfluenceY: " + totalInfluenceY + "\ninputDirectionX: " + inputDirection.x + "\ninputDirectionY: " + inputDirection.y + "\nmidair: " + midair + "\ndeltaTime " + Time.deltaTime);
        Debug.Log("WarriorJump: " + warriorJump + "\nWarriorHasJumped: " + warriorHasJumped + "\nMageJump: " + mageJump + "\nMageHasJumped: " + mageHasJumped + "\nIgnoringBetweenPlayer/Platform: " + Physics2D.GetIgnoreLayerCollision(8, 12));
    }

    //Timer Events
    public void CooldownDodgeRollOver()
    {
        DodgerollInCooldown = false;

        DodgerollPlayerFeedback();

    }

    public void DodgerollFinished()
    {
        isDodgerolling = false;
    }

    //Window == Active frame window for the input of phantom dodgeroll, this and the below are triggered via timer :P
    public void StartPhantomDodgerollWindow()
    {
        canPhantomDodgeroll = true;

        phantomDodgerollEndTimer.Activate();
    }

    public void EndPhantomDodgerollWindow()
    {
        canPhantomDodgeroll = false;

        phantomDodgerollCount = 0;
    }

    //VFX + SFX
    public void DodgerollPlayerFeedback()
    {
        //VFX to signal to the player he can dodgeroll
        playerManaManager.AnimateOrbs();//Used to color the orbs instead lul

        //SFX to make it more impactful
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.DashCooldown);
    }

    public void OnKillResetDodgeroll()
    {
        canDodgeroll = true;

        canPhantomDodgeroll = true;

        dodgerollCooldownTimer.Reset();

        DodgerollInCooldown = false;

        DodgerollPlayerFeedback();
    }

    /// <summary>
    /// FinalLevel somehow activates dodgeroll by pressing shift lmao.
    /// Would disable accessing this class at finalLevel but w/e, the SFX are good lmao
    /// (No, it's definitely not because I can fuck something up, why did you think that?)
    /// </summary>
    public void UnsubscribeDodgeroll()
    {
        dodgerollCooldownTimer.TriggerOnEnd -= CooldownDodgeRollOver;
    }

    //Triggered by timer, every 0.01 seconds after activated.
    public void AlightPlayerManaOnKill()
    {
        playerManaManager.FadeOrbColor();
    }


    //This is how ground detection is set/updated. Invoked by GroundDetectionPlayer.cs
    public void SetAtopGround(bool value)
    {
        //Just landed
        if (value == true && isAtopGround == false)
        {
            //SFX
            PlayerSoundManager.globalInstance.PlayClip(landingGroundSFX, PlayerSoundManager.AudioSourceName.PlayerLanding);
        }

        isAtopGround = value;
    }

    /// <summary>
    /// Called by InsidePlatformCheck.cs
    /// Instead of checking every frame if inside platform like a retard
    /// You are notified event-wise, without bothering with the behaviour of walljump
    /// You just get isInsidePlatform and ggwp.
    /// </summary>
    /// <param name="value"></param>
    public void SetInsidePlatform(bool value)
    {
        isInsidePlatform = value;
    }

    /// <summary>
    /// This is the only isFlag which isn't processed directly.
    /// Hence the [Collision] in its naming.
    /// It is the notification of AtopPlatformCheck.cs
    /// So everything is cleaner instead of drawing a collider each frame -_-
    /// </summary>
    /// <param name="value"></param>
    public void SetAtopPlatformCollisionDictionary(Dictionary<int, GameObject> childDictionary)
    {
        atopPlatformCollisionDictionary = childDictionary;
    }

    //Detects the side wall/ground collisions of player collider. Invoked by WallSlideCheck.cs
    //Has little to nothing to do with wallsliding state machine.
    public void SetSideWalled(bool value)
    {
        isSidewalled = value;

        //If grounded and just hit a wall, instantly stop all momentum.
        if (isSidewalled == true && isAtopGround)
            ResetTotalInfluenceX();

        //And to prevent cheese momentum, aka dashing towards a wall to stack influenceX to infinite and then reverse it
        //on the function CalculateForces, no influenceX can be added on top of current influenceX ;)
        //Hence, problem solved! Groundwalls nullify influenceX :D



        //Attack Midair state cannot normally transition to wallslide, cuz it looks  B A D and cancerously clunky.
        //However, this should work since it does change it only once, instead of checking every frame of the update.
        //May be disliked by players since it cuts attack midair instead of waiting for it, and then sliding... 
        //I guess, darkwind distortion choice? Still looks bad though, and it also takes away depth.
        //
        //Btw, you could simply make one more variable justSidewalled, and it checks the below condition on case: ATTACKMIDAIR instead of writing it here.
        if (isSidewalled == true && midair && currentState == ATTACKMIDAIR)
        {
            targetState = WALLSLIDE;
            NewStateTransition();
        }

    }

    public void SetWallSlideCollisionDictionary(Dictionary<int, GameObject> wallSlideCollideCheckDictionary)
    {
        //Clone the dictionary of this class when a trigger happens
        //Instead of asking every frame its dictionary.
        wallSlideColliderDictionary = wallSlideCollideCheckDictionary;
    }

    /// <summary>
    /// Converts the wallSlideAnimationState, to a string which exists in the animation machine, so as it can be played
    /// </summary>
    /// <param name="targetWallSlideAnimation"></param>
    /// <returns></returns>
    public string WallSlideAnimationStateToString(WallSlideCheck.wallSlideAnimationState targetWallSlideAnimation)
    {
        if (targetWallSlideAnimation == WallSlideCheck.wallSlideAnimationState.NinjaFeet)
            return "WallSlideNinjaFeet";
        else if (targetWallSlideAnimation == WallSlideCheck.wallSlideAnimationState.StableFeet)
            return "WallSlideStableFeet";
        else if(targetWallSlideAnimation == WallSlideCheck.wallSlideAnimationState.Head)
            return "WallSlideHead";
        else //if (targetWallSlideAnimation == WallSlideCheck.wallSlideAnimationState.All)
            return "WallSlideAll";
    }

    /// <summary>
    /// Called by WallSlideCheck.cs
    /// </summary>
    /// <param name="targetWallSlideAnimation"></param>
    public void UpdateWallSlideAnimationState( WallSlideCheck.wallSlideAnimationState targetWallSlideAnimation)
    {
        if (currentState == WALLSLIDE && targetWallSlideAnimation != currentWallSlideAnimation)
        {
            //Play the equivalent animation/sprite, given the parameter state
            animatorController.Play(WallSlideAnimationStateToString(targetWallSlideAnimation));
        }

        //Don't change the order, codelines are called, as they will all break.
        //The queue is crystal clear and bugless currently.
        //Putting this inside if codeblock above breaks it completely (or exit works but entering doesnt?)
        //Putting this above codeblock makes entering work, exit broken
        currentWallSlideAnimation = targetWallSlideAnimation;

    }

    //Get private values
    public bool GetMidair()
    {
        return midair;
    }

    public int GetCurrentState()
    {
        return currentState;
    }

    public bool AreSpellsDisabledState()
    {
        if (currentState == DEATH || currentState == REVIVE || currentState == DIALOGUE)
            return true;

        return false;
    }

    public int GetTargetState()
    {
        return targetState;
    }

    public bool GetFacingLeft()
    {
        return facingLeft;
    }

    public float GetSpeedY()
    {
        return speedY;
    }

    public int GetPhantomDodgerollCount()
    {
        return phantomDodgerollCount;
    }

    public Vector2 GetInfluence()
    {
        return new Vector2(GetTotalInfluenceX(), totalInfluenceY);
    }

    public float GetInfluenceX()
    {
        return GetTotalInfluenceX();
    }

    public float GetInfluenceY()
    {
        return totalInfluenceY;
    }



    public bool GetAtopGround()
    {
        return isAtopGround;
    }

    public bool GetAtopPlatform()
    {
        return isAtopPlatform;
    }

    public bool GetIsWallsliding()
    {
        return isWallSliding;
    }

    public bool GetHasMageJumped()
    {
        return mageHasJumped;
    }

    //Cooldowns

    public bool GetDodgerollCooldown()
    {
        return DodgerollInCooldown;
    }

    //Used to polish push spell VFX when on wall
    public bool GetSidewalled()
    {
        return isSidewalled;
    }

    //Used to polish push spell VFX when on wall
    public bool GetIsWallslidingLeft()
    {
        return isWallslidingLeft;
    }
}