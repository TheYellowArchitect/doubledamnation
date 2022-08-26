using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using NaughtyAttributes;

//Merging: When an monster/enemy stands above each other, its really weird, fix dat.
//Must be decided: Pass Through? Collide and damage? Collide and nothing?(current one) If landing atop someone "stuns" them, and pushes them, shit should work.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyJump : MonoBehaviour
{
    [MinValue(0f)]
    [Tooltip("Once a jump is done, when again can this jump?")]
    public float jumpCooldown;//Cooldown time so it wont spam

    [MinValue(0f)]
    [Tooltip("At what range from \"Enemy\" is it allowed to jump?")]
    public float jumpRange;//When to start jumpin

    [MinValue(0f)]
    [Tooltip("How quickly till jump starts (preparation time b4 actual jumping)")]
    public float castingTime;

    [MinValue(0f)]
    [Tooltip("How far upwards it jumps, preferably it should be 10 times higher than Diminisher for smooth jump")]
    public float jumpPower;

    [MinValue(0f)]
    [Tooltip("The curve of the jump, it increases JumpPower from 0 -> JumpPower, aka how fast it reaches there.")]
    public float JumpPowerAmplifier;

    [MinValue(0f)]
    [Tooltip("The curve of the jump, it decreases JumpPower from JumpPower -> to 0, aka how fast it reaches 0.")]
    public float JumpPowerDiminisher;

    [Tooltip("When falling downwards, this is the Force limit where it cannot exceed. (usually it is negative jumpPower)\nAlso, midairGravity is added onto this")]
    public float JumpPowerDeaccelerationLimit;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("Upon jumping, what layer to raycast to detect if player below")]
    public LayerMask WhatIsPlayer;

    /*old satyr values (after stomp got fixed) are:
        jump cooldown: 0
        jump range: 35
        casting time: 0.5
        jump power: 400
        jump power amplifier: 30
        jump power diminisher: 50
        jump power deacceleration limit: -300

    new (not final) satyr values are:
        jump cooldown: 0
        jump range: 35
        casting time: 0.5
        jump power: 450
        jump power amplifier: 50
        jump power diminisher: 60
        jump power deacceleration limit: -300

    */


    private EnemyBehaviour commonBehaviour;//Sucks that i am using this, but its to update and say: "Yo, im jumping now k?" so behaviour won't go wack and do weird shit
    private LayerMask WhatIsGround;

    [ReadOnly]
    public bool applyJumpPower = false;//Bool to start jumping (forces n shit)
    [ReadOnly]
    public float currentJumpPower;//Current force used in the Y axis.
    //private float currentJumpPowerDiminisher;//This was supposed to be tweaked for smoother movement, but its literally jumpPower_Copy LUL

    //Indicates if rising/upwards phase, or falling/downwards phase. Used with applyJumpPower, but alone, it affects raycasting in FixedUpdate, so default is true
    [ReadOnly]
    public bool rising;

    //Caching this so it won't re-calculate every fixedUpdate on raycasting-> Now, deprecated.
    //private float colliderBoundsExtents;

    //Timer variables (for cooldown)
    private float timeElapsed = 0;

    [ReadOnly]
    public bool onCooldown = false;

    //Caching
    private Rigidbody2D commonRigidbody;

    //[Tooltip("Should this jump damage anyone on rising and/or falling? Like, rising spike effect, or falling block ;)\nIf yes, drag the EnemyJumpContact script here, and configure it")]
    //public EnemyJumpContactAttack commonJumpContact;//It is like this, only to notify isJumping. Could be done via 2 event subscriptions but yeah, anw. :P
    [HideInInspector]
    public EnemyJumpContactAttack commonJumpContact;

    //To notify when touched ground/jump finished
    public UnityEvent touchedGround;

    public UnityEvent touchedPlayer;

    public UnityEvent startedJump;

    public UnityEvent completedJump;


	void Start ()
    {
        rising = true;//True, cuz it doesnt matter BUT it matters in first frame, otherwise it "Touches the ground" which is unwanted.
        commonBehaviour = GetComponent<EnemyBehaviour>();
        commonRigidbody = GetComponent<Rigidbody2D>();

        //If has EnemyJumpContactAttack
        if (GetComponent<EnemyJumpContactAttack>() != null)
        {
            commonJumpContact = GetComponent<EnemyJumpContactAttack>();
            //Subscribe the FirstFrameCollision event, IF a contactAttack is attached.
            completedJump.AddListener(commonJumpContact.FirstFrameCollision);
        }

        //colliderBoundsExtents = GetComponent<Collider2D>().bounds.extents.magnitude;
        WhatIsGround = commonBehaviour.WhatIsGround;

        JumpPowerDeaccelerationLimit = JumpPowerDeaccelerationLimit + commonBehaviour.midairGravity;
            
    }

    private void Update()
    {
        //Cooldown timer
        if (onCooldown)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > jumpCooldown)
            {
                timeElapsed = 0;
                onCooldown = false;
            }
        }
    }

    public void StartJump()
    {
        //Too fast player movement aka skipping levels, can trigger this as it runs same frame with Start -_-
        if (commonBehaviour == null)
            Start();

        //Play JumpCharge animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.JUMP_CHARGE;
        commonBehaviour.NewStateTransition();


        if (GameManager.testing)
            Debug.Log("Started Jumping");

        applyJumpPower = false;
        currentJumpPower = 0;
        //currentJumpPowerDiminisher = JumpPowerDiminisher;

        commonBehaviour.SetIsJumping(true);

        //Triggers any subscribed action to this
        if (startedJump != null)
            startedJump.Invoke();

        Invoke("CompleteJump", castingTime);
    }

    public void CompleteJump()
    {
        //Play Jump animation.
        commonBehaviour.targetAnimationState = EnemyBehaviour.JUMP;
        commonBehaviour.NewStateTransition();

        //Start moving towards player, if not in range -> origin point.
        commonBehaviour.isMoving = true;

        //VFX ty

        applyJumpPower = true;
        rising = true;

        if (completedJump != null)
            completedJump.Invoke();

        //If any "on touch -> damage" behaviour is plugged in here, start it :P
        if (commonJumpContact != null)
            commonJumpContact.isJumping = true;
    }

    private void FixedUpdate()
    {
        //Debug.Log("Jump on cooldown: " + onCooldown + " , timelapsed: " + timeElapsed);
        //Debug.Log("Current Jump Power is: " + currentJumpPower);

        //If online and client, dont change velocity/rigidbody, because the host does that!
        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false)
            return;

        if (applyJumpPower)
        {
            commonRigidbody.AddForce(Vector2.up * currentJumpPower);

            if (rising)
            {
                //currentJumpPower += currentJumpPowerDiminisher;
                currentJumpPower += JumpPowerAmplifier;

                //Fallin time
                if (currentJumpPower > jumpPower)
                {
                    rising = false;
                }
            }
            else
            {
                if (currentJumpPower > JumpPowerDeaccelerationLimit)
                {
                    //Debug.Log("Hmm: " + currentJumpPower);
                    //currentJumpPower -= currentJumpPowerDiminisher;

                    currentJumpPower -= JumpPowerDiminisher;
                }
                //else, no speed increase = no downwards X-TRM falling

                //Play falling animation
                commonBehaviour.targetAnimationState = EnemyBehaviour.FALLING;
                commonBehaviour.NewStateTransition();
            }

            //currentJumpPowerDiminisher *= 1.2f;//An early relic of this code, tweaking the diminisher itself was a thing back then... in the old ages... kappa
        }
    }

    //To check for player
    public void OnCollisionEnter2D(Collision2D collision)
    {
        //Check if contacted player or ground
        if (!rising && applyJumpPower)//timeElapsed check, so it won't raycast at the first second...
        {
            //Don't ask, layermasks are weird. But yeah, the below checks if the collision is Ground.
            if ((WhatIsGround & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer && collision.contacts[0].normal.y  > 0)
            {
                if (GameManager.testing)
                    Debug.Log("Ground from Jump, Detected");

                JustTouchedGround();
            }
            else if ((WhatIsPlayer & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
            {
                Debug.Log("Player from Jump Detected");
                //Debug.Break();

                //Pause Y speed
                commonRigidbody.velocity = new Vector2(commonRigidbody.velocity.x, 0f);

                if (touchedPlayer != null)
                    touchedPlayer.Invoke();

                //On touch, it pushes the player away, and pushes this enemy down (so physics wont fuck shit up)

                //Merging: Adjust the number 1000
                //So it touches the ground
                //commonRigidbody.AddForce(Vector2.down * 1000);

                //Merging: CalculateForces for the warrior. Also accurate direction, ty.
                //collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 1000);
            }

            
        }
    }

    public void JustTouchedGround()
    {
        applyJumpPower = false;
        rising = true;//true is default, so it won't raycast ;) check above, if (!rising)->this raycasting hehe.

        onCooldown = true;

        //Activates the subscribed method of EnemyBehaviour.cs :P
        if (touchedGround != null)
        {
            touchedGround.Invoke();
        }

        if (commonJumpContact != null)
        {
            commonJumpContact.isJumping = false;
        }
    }

    public bool GetIsRising()
    {
        return rising;
    }

    //Shows the radius
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Draw attack range
        Handles.color = new Color(0f, 1.0f, 0f, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, jumpRange);
    }
    #endif

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}
