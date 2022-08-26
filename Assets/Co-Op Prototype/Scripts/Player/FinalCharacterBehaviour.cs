using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

//TODO: StartFinalCutscene, SFX for jump tweak
public class FinalCharacterBehaviour : MonoBehaviour
{
    //Public
    //======
    [Header("Movement")]
    public int movementSpeedX = 10;
    public int jumpPower = 10;
    public int fastfallPower = 10;

    [Header("Timers")]
    public float timeToFinishAttack;
    [Tooltip("Once attack finishes, how much time until the player can act again?")]
    public float timeToRecoverAttack = 0;
    [Tooltip("Once you get hit, how much time until the hitstun ends and u can act again?")]
    public float timeToRecoverFromHit;
    public float timeToDie;

    [Header("Attack Hitbox")]
    [Tooltip("Used to for example, elevate the attack hitbox.")]
    public float attackOffsetY;
    public float attackX;
    public float attackY;
    [Tooltip("When player attacks, the hitbox is in the center, and it moves by the offset for the direction!")]
    public float attackDirectionOffset;

    [Header("Attack")]
    public int knockbackPowerX = 230;
    public int knockbackPowerY;
    [Tooltip("Should be 1.")]
    public int damagePower;
    //[Tooltip("For each hit, how much should knockbackPowerX rise?")]
    //public int knockbackPowerXIncreaser;

    [Header("JumpRotation")]
    [Tooltip("When you jump, the sprite should rotate from [-this,+this].")]
    public int degreesToRotateOnJump = 30;
    [Tooltip("Time to do the full rotation while jumping. Should match the jumping power/gravity btw.")]
    public float timeToRotateFully = 0.95f;
    [Tooltip("This value is removed every frame, from the end-part of the lerp.")]
    public float rotationDegreeLessener = 0.65f;

    [Header("SFX")]
    [Tooltip("When you hit the opponent")]
    public AudioClip attackDamageSFX;
    [Tooltip("When you attack the opponent")]
    public AudioClip attackSFX;

    public AudioClip mageJumpSFX;
    public AudioClip warriorJumpSFX;

    [Header("Misc")]
    [Tooltip("Made to differentiate the input, for player1 and player2 obviously :P")]
    public bool usesKeyboard;

    

    [Tooltip("Since only one ground, it is stored right here.")]
    public GameObject finalDestination;

    [Tooltip("Since only one enemy, he is stored right here for convenience, instead of searching lel")]
    public GameObject finalOpponent;

    [Tooltip("To be decided at start of fight :P")]
    public bool facingRight = true;

    //State-Machine
    //plz dont lecture me on variable security (public on self-sustained state machine), I am **Certain** these variables will never be violated.
    [Header("ReadOnly")]
    public State currentState;
    public State targetState;
    public enum State { Idle, Run, Jump, Hit, AttackSetup, AttackRecovery };

    public bool isHitstunned = false;
    public bool isAttacking = false;
    public bool isAttackRecovering = false;
    public bool isDying = false;
    public bool isGrounded = false;
    public bool hasJumped = false;
    public bool inCutscene = true;

    public Vector3 directionTowardsOpponent;

    public float movementInputDirectionX;//LeftJoystickX
    public float movementInputDirectionY;//LeftJoystickY
    public int attackInputDirectionX;//tfw u assign an int here, apologies, memory.


    //Private
    //=======
    private Rigidbody2D commonRigidbody;
    private Animator commonAnimator;

    private bool attackedRight = false;
    private Collider2D[] attackColliders;
    private Vector3 attackTopLeft;
    private Vector3 attackBottomRight;

    private Vector3 directionHit;
    private Vector3 directionOffset;
    private int currentKnockbackPowerX;

    private Transform childSpriteObject;
    private bool [] isRotating = new bool [2] { false, false };//For the 2 coroutines, to track and shut them down, I know its spagghetti but it works.
    private float rotationTimeLeft = 0;
    private float currentTimeToRotateFully = 0;//Existing so as to change the timeToRotate fully variable on the check, tho probably un-needed
    private float currentDegreesToRotateOnJump = 0;//Changing in run-time, so the end scale of the lerp will be smaller if needed. Essentially replacing the above variable.

    //Timers
    private float attackFinishTimeLeft = 0;
    private float attackRecoveryTimeLeft = 0;
    private float hitTimeLeft = 0;
    private float DeathTimeLeft = 0;

    //Properties, mandatory, since interface IDamageable demands them, and Unity doesn't play well with properties.
    [Header("Health")]
    public int maxHealth;
    public int currentHealth;


    // Use this for initialization
    void Start ()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();
        commonAnimator = GetComponentInChildren<Animator>();

        childSpriteObject = transform.GetChild(0);
        currentTimeToRotateFully = timeToRotateFully;

        attackTopLeft = new Vector3(attackX * -1, attackY + attackOffsetY);
        attackBottomRight = new Vector3(attackX, (attackY * -1) + attackOffsetY);

        currentHealth = maxHealth;
    }
	
	// This is called every frame, by FinalCharacterInput.Update()
	public void ProcessInput(float inputX, float inputY, int attackDirection)
    {
        UpdateTimers();

        //=============
        //Receives Input
        //=============

        //Debugging TODO: Delete dis
        if (GameManager.testing && Input.GetKeyDown(KeyCode.PageUp))
            inCutscene = !inCutscene;

        if (inCutscene)
            return;

        movementInputDirectionX = inputX;//LeftJoystickX
        movementInputDirectionY = inputY;//LeftJoystickY
        if (attackInputDirectionX == 0)
            attackInputDirectionX = attackDirection;//tfw u assign an int here, apologies, memory.

        DetermineTargetState();//sets the variable targetState according to input.

        //Display stuff
        //=======
        DisplayVelocity(movementInputDirectionX, movementInputDirectionY);

        DisplayAttackHitbox();
        //=======
        //=======

        

        directionTowardsOpponent = (finalOpponent.transform.position - transform.position).normalized;

        if (isDying)
            return;//so it wont go to state machine below obv.

        //=============
        //State Machine 
        //=============
        switch(currentState)//since it is small, could be using ifs but whatever. the more similar it is to what i have made, the less prone to error :)
        {
            case State.Idle:

                //TargetState is set from DetermineTargetState, and Idle&Run can go anywhere, so...
                NewStateTransition();
                break;

            case State.Run:

                NewStateTransition();
                break;

            case State.Jump:

                //first time jumping
                if (hasJumped == false && isGrounded && movementInputDirectionY == 1)
                {
                    Debug.Log("Jumping!");

                    hasJumped = true;

                    if (usesKeyboard)
                    {
                        //Rune-Circle VFX
                        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MagicCircleGround, transform.position - new Vector3(0, 2.78f, 0) );

                        //SFX
                        PlayerSoundManager.globalInstance.PlayClip(mageJumpSFX, PlayerSoundManager.AudioSourceName.MageJump1, 1.2f, 0.25f);
                    }
                    else
                    {
                        //Wind VFX
                        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.JumpSmokeGround, transform.position - new Vector3(0, 1.4f, 0));

                        //Wind SFX
                        PlayerSoundManager.globalInstance.PlayClip(warriorJumpSFX, PlayerSoundManager.AudioSourceName.WarriorJump1, 1f, 0.15f);
                    }
                    

                    //And with gravity it goes down.
                    commonRigidbody.AddForce(Vector2.up * jumpPower);

                    //Plays jump animation.
                    commonAnimator.Play(StateToString(State.Jump));

                    //Mad spagghetti, rotation as a whole lmao. Could be simplified af but w/e, if it works...
                    if (isRotating[0] == true)
                    {
                        isRotating[0] = false;
                        isRotating[1] = true;
                    }
                    else if (isRotating[1] == true)
                    {
                        isRotating[1] = false;
                        isRotating[0] = true;
                    }
                    else//No rotation
                    {
                        isRotating[0] = true;
                    }

                    StartCoroutine(RotationCoroutine());
                }
                else if (targetState == State.AttackSetup)
                    NewStateTransition();

                break;

            case State.AttackSetup:
                
                //Spagghetti indeed, but anw. From determinetargetstate -> newstatetransition -> it gets to this case.
                if (isAttacking == false)
                {
                    //Uses timer to activate attack automatically.
                    isAttacking = true;

                    //AttackInputDirectionX is 0!!! Fix!!!
                    if (attackInputDirectionX == 1)
                        attackedRight = true;
                    else
                        attackedRight = false;

                    //SFX
                    PlayerSoundManager.globalInstance.PlayClip(attackSFX, PlayerSoundManager.AudioSourceName.SpearHit, 0.3f * currentHealth);

                    //Flips player if attacks backwards.
                    DetermineFlipAttackingPlayer();

                    if (attackedRight)
                        directionOffset = new Vector3(attackDirectionOffset, 0, 0);
                    else
                        directionOffset = new Vector3(attackDirectionOffset * -1, 0, 0);

                    DisplayActiveAttackHitbox(true);
                }

                if (targetState == State.Jump && isGrounded == true)
                {
                    ResetAttackFlags();

                    NewStateTransition();
                }

                break;


            case State.AttackRecovery:
                break;

            case State.Hit:
                
                //Hit "expired", timer makes this false.
                if (isHitstunned == false)
                {
                    if (isGrounded)
                        targetState = State.Idle;
                    else
                        targetState = State.Jump;

                    NewStateTransition();
                }
                //Jump shouldn't "Break" the hitstun here! Just knockback really far away :P
                break;

        }

        DetermineFlipPlayer();

        //=============
        //Resets values
        //=============
        /* Useless since next frame catches them.
        movementInputDirectionX = 0;
        movementInputDirectionY = 0;
        attackInputDirectionX = 0;
        */

	}

    /// <summary>
    /// Sets the speed/velocity.
    /// </summary>
    private void FixedUpdate()
    {
        if (isDying)
            commonRigidbody.velocity = new Vector2(-1 * directionHit.x * currentKnockbackPowerX * Mathf.Lerp(1, 0, hitTimeLeft / timeToRecoverFromHit), commonRigidbody.velocity.y);

        //If gravity fails me: if (isGrounded == false) addForce down!
        else if(isHitstunned)
            commonRigidbody.velocity = new Vector2(-1 * directionHit.x * currentKnockbackPowerX * Mathf.Lerp(1, 0, hitTimeLeft / timeToRecoverFromHit), -1 * directionHit.y * knockbackPowerY * Mathf.Lerp(1, 0, hitTimeLeft / timeToRecoverFromHit));
        else if (currentState != State.AttackSetup && currentState != State.AttackRecovery)
        {
            if (isGrounded == false && movementInputDirectionY == -1)
                commonRigidbody.velocity = new Vector2(movementInputDirectionX * movementSpeedX, commonRigidbody.velocity.y - fastfallPower);
            else//Normal movement
                commonRigidbody.velocity = new Vector2(movementInputDirectionX * movementSpeedX, commonRigidbody.velocity.y);
        }
        else if (currentState == State.AttackSetup || currentState == State.AttackRecovery)
        {
            if (isGrounded)
                commonRigidbody.velocity = Vector2.zero;
            else if (isGrounded == false && movementInputDirectionY == -1)//Fast-fall viable, new meta to drop-attack ftw.
                commonRigidbody.velocity = new Vector2(movementInputDirectionX * movementSpeedX, commonRigidbody.velocity.y - fastfallPower);
            else
                commonRigidbody.velocity = new Vector2(movementInputDirectionX * movementSpeedX, commonRigidbody.velocity.y);
        }
        //Used to slide in the ground, not much tho but it was cool but too confusing for first time, dont introduce so many new mechanics at the fucking final boss!!!
        //else if (currentState == State.AttackSetup)
            //commonRigidbody.velocity = new Vector2(commonRigidbody.velocity.x * Mathf.Lerp(1, 0, attackFinishTimeLeft / timeToFinishAttack), commonRigidbody.velocity.y * Mathf.Lerp(1, 0, attackFinishTimeLeft / timeToFinishAttack));

    }

    /// <summary>
    /// Takes the gameobject childSpriteObject and rotates it degreesToRotateOnJump (z on rotation) over time (lerp) using the variable timeToRotateFully, aka linear lerp
    /// Also wanted to add that you are retarded and u could make this an animation.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotationCoroutine()
    {
        currentDegreesToRotateOnJump = degreesToRotateOnJump;

        //Resets the rotation if it has any
        if (childSpriteObject.transform.rotation != Quaternion.Euler(Vector3.zero))
            childSpriteObject.transform.rotation = Quaternion.Euler(Vector3.zero);

        if (isRotating[0])
        {
            while (isRotating[0])
            {

                if (facingRight)
                    childSpriteObject.rotation = Quaternion.Euler(childSpriteObject.rotation.eulerAngles.x, childSpriteObject.rotation.eulerAngles.y, Mathf.Lerp(degreesToRotateOnJump, -currentDegreesToRotateOnJump, rotationTimeLeft / currentTimeToRotateFully));
                else
                    childSpriteObject.rotation = Quaternion.Euler(childSpriteObject.rotation.eulerAngles.x, childSpriteObject.rotation.eulerAngles.y, Mathf.Lerp(-degreesToRotateOnJump, currentDegreesToRotateOnJump, rotationTimeLeft / currentTimeToRotateFully));

                //If no X velocity, decrease the degrees, so the end part of the above lerp will look more natural in-game.
                if (movementInputDirectionX == 0)//why not rigidbody.x hmmm
                    currentDegreesToRotateOnJump = Mathf.Abs(currentDegreesToRotateOnJump) - rotationDegreeLessener;

                //Upgrade the rotation.
                rotationTimeLeft += Time.deltaTime;

                //Check if the rotation is over.
                //RotationTimeLeft gets += deltaTime from Update()
                if (rotationTimeLeft > currentTimeToRotateFully || movementInputDirectionY == -1)//-1 means fastfall, rotation shouldnt happen while on the ground ;)
                    isRotating[0] = false;//Hence exits the while loop

                yield return new WaitForEndOfFrame();
            }
        }
        else if (isRotating[1])
        {
            while (isRotating[1])
            {

                if (facingRight)
                    childSpriteObject.rotation = Quaternion.Euler(childSpriteObject.rotation.eulerAngles.x, childSpriteObject.rotation.eulerAngles.y, Mathf.Lerp(degreesToRotateOnJump, -currentDegreesToRotateOnJump, rotationTimeLeft / currentTimeToRotateFully));
                else
                    childSpriteObject.rotation = Quaternion.Euler(childSpriteObject.rotation.eulerAngles.x, childSpriteObject.rotation.eulerAngles.y, Mathf.Lerp(-degreesToRotateOnJump, currentDegreesToRotateOnJump, rotationTimeLeft / currentTimeToRotateFully));

                //If no X velocity, decrease the degrees, so the end part of the above lerp will look more natural in-game.
                if (movementInputDirectionX == 0)//why not rigidbody.x hmmm
                    currentDegreesToRotateOnJump = Mathf.Abs(currentDegreesToRotateOnJump) - rotationDegreeLessener;

                //Upgrade the rotation.
                rotationTimeLeft += Time.deltaTime;

                //Check if the rotation is over.
                //RotationTimeLeft gets += deltaTime from Update()
                if (rotationTimeLeft > currentTimeToRotateFully || movementInputDirectionY == -1)
                    isRotating[1] = false;//Hence exits the while loop

                yield return new WaitForEndOfFrame();
            }
        }

        ResetRotation();

        //Debug.Log("End booleans: hasJumped " + hasJumped + " and isGrounded " + isGrounded);//Returns true and true, no fucking idea why it doesnt make sense...
    }

    private void ResetRotation()
    {
        rotationTimeLeft = 0;

        //Resets it, tho check out the coroutine so as not to clash?
        childSpriteObject.transform.rotation = Quaternion.Euler(childSpriteObject.rotation.eulerAngles.x, childSpriteObject.rotation.eulerAngles.y, 0);
    }
    
    /// <summary>
    /// Changes state to the TargetState. As long as targetstate != currentstate, the transition happens.
    /// </summary>
    public void NewStateTransition()
    {
        //If they are different, so it won't replay the same animation from the start every frame.
        if (currentState != targetState)
        {
            //Updates state
            currentState = targetState;

            //Transitions to state.
            commonAnimator.Play(StateToString(targetState));
        }
    }

    /// <summary>
    /// Returns string from every state.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public string StateToString(State state)
    {
        if (state == State.Idle)
            return "Idle";
        else if (state == State.Run)
            return "Run";
        else if (state == State.Jump)
            return "Jump";
        else if (state == State.Hit)
            return "Hit";
        else if (state == State.AttackSetup)
            return "AttackSetup";
        else if (state == State.AttackRecovery)
            return "AttackRecovery";

        Debug.Log("Bug at StateToString");
        Debug.Break();
        return "Null";
    }

    /// <summary>
    /// Depending on Inputs (left&right joystick) it determines the TargetState!
    /// </summary>
    public void DetermineTargetState()
    {
        //If no input
        if (hasJumped == false && movementInputDirectionY == 1)
            targetState = State.Jump;
        else if (attackInputDirectionX != 0)
            targetState = State.AttackSetup;
        else if (isGrounded && movementInputDirectionX != 0)
            targetState = State.Run;
        else if (isGrounded)
            targetState = State.Idle;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == finalDestination)
        {
            if (isGrounded == false)//&& hasJumped
                JustTouchedGround();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {

        if (collision.gameObject == finalDestination)
            isGrounded = false;
        
    }

    /// <summary>
    /// Transitions to Idle state, and hasJumped = false; and sets velocity to 0 for that frame
    /// </summary>
    public void JustTouchedGround()
    {
        //Debug.Break();

        isGrounded = true;
        hasJumped = false;

        commonRigidbody.velocity = Vector2.zero;

        targetState = State.Idle;
        NewStateTransition();
        
    }



    public void ActivateAttack()
    {

        //Current attack is interrupted, don't actually attack!
        if (currentState != State.AttackSetup)
            return;

        //Hitbox HOYPE
        attackColliders = Physics2D.OverlapAreaAll(transform.position + attackTopLeft + directionOffset, transform.position + attackBottomRight + directionOffset);

        //Display the above
        DisplayActiveAttackHitbox(false);

        //Checking every collider.
        for (int i = 0; i < attackColliders.Length; i++)
            if (attackColliders[i].gameObject == finalOpponent)
            {
                Debug.Log("hitboi");

                finalOpponent.GetComponent<FinalCharacterBehaviour>().GetHit(damagePower);
            }

        targetState = State.AttackRecovery;
        NewStateTransition();

        isAttackRecovering = true;
        //^Goes to the timer ftw.
    }

    public void RecoverAttack()
    {
        isAttackRecovering = false;
        isAttacking = false;
        attackInputDirectionX = 0;

        targetState = State.Idle;
        NewStateTransition();
    }

    public void GetHit(int damageTaken)
    {
        //Play the SFX
        PlayerSoundManager.globalInstance.PlayClip(attackDamageSFX, PlayerSoundManager.AudioSourceName.SpearHit, 0.3f * currentHealth);

        

        currentHealth = currentHealth - damageTaken;

        if (currentHealth > 0)
            currentKnockbackPowerX = knockbackPowerX / currentHealth;//Power of knockback depends on how much HP you have. Not much, but should be a lot.
        else
            currentKnockbackPowerX = (int) (knockbackPowerX * 2.5f);

        //So at fixedUpdate, the force is applied, and monster cannot act till it finishes
        isHitstunned = true;

        //Interrupts attack properly.
        if (isAttacking)
            ResetAttackFlags();

        directionHit = directionTowardsOpponent;

        


        //VFX of damage here
        //VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedPlayer, Vector3.Lerp(transform.position, finalOpponent.transform.position, 0.2f), this.gameObject);
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedPlayer, transform.position + new Vector3(3.9f,2.1f,0));

        //Shake Camera
        CameraShaker.Instance.ShakeOnce(25f, 15f, .1f, 1.1f);
            
        //Play the Hit animation
        currentState = State.Hit;
        commonAnimator.Play(StateToString(State.Hit));
    }

    public void ResetAttackFlags()
    {
        isAttacking = false;
        isAttackRecovering = false;
        attackFinishTimeLeft = 0;
        attackRecoveryTimeLeft = 0;
    }


    /// <summary>
    /// Obsolete.
    /// </summary>
    public void StartDying()
    {
        isDying = true;//Starts the countdown to when the cutscene begins.

        inCutscene = true;
        finalOpponent.GetComponent<FinalCharacterBehaviour>().inCutscene = true;//so he won't move.

        //Notify FinalBattleManager.cs
        FinalBattleManager.globalInstance.InitiateCombatEnding(this.gameObject);

        //Update PlayerStats
        PlayerStatsManager.globalInstance.IncreaseKillCount();//the saddest kill...
    }

    public void StartFinalCutscene()
    {
        Time.timeScale = 1;

        Debug.Break();

        //TODO: Final Cutscene
        //Look into DialogueSystem.cs:
        //Cutscene 11 is for player1 victory, 12 is for player2 victory. They speak a little more, and then boom, falling forever.
    }

    /// <summary>
    /// Stores timer/cooldown for attack&dying&hit. Runs every frame via Update()
    /// </summary>
    public void UpdateTimers()
    {
        if (isAttacking)
        {
            if (isAttackRecovering == false)
            {
                attackFinishTimeLeft += Time.deltaTime;
                if (attackFinishTimeLeft > timeToFinishAttack)
                {
                    attackFinishTimeLeft = 0;

                    ActivateAttack();
                }
            }
            else
            {
                attackRecoveryTimeLeft += Time.deltaTime;
                if (attackRecoveryTimeLeft > timeToRecoverAttack)
                {
                    attackRecoveryTimeLeft = 0;

                    RecoverAttack();
                }
            }
            
        }
        else if (isHitstunned)
        {
            hitTimeLeft += Time.deltaTime;
            if (hitTimeLeft > timeToRecoverFromHit)
            {
                hitTimeLeft = 0;
                isHitstunned = false;
            }
        }
        else if (isDying)
        {
            DeathTimeLeft += Time.deltaTime;
            if (DeathTimeLeft > timeToDie)
            {
                DeathTimeLeft = 0;
                isDying = false;

                StartFinalCutscene();
            }

        }

    }

    public void DetermineFlipPlayer()
    {
        if (currentState == State.Hit || currentState == State.AttackSetup || currentState == State.AttackRecovery)
            return;

        if (movementInputDirectionX < 0f && facingRight == true)
            FlipPlayer();
        else if (movementInputDirectionX > 0f && facingRight == false)
            FlipPlayer();
    }

    public void DetermineFlipAttackingPlayer()
    {
        if (attackInputDirectionX == -1 && facingRight)
            FlipPlayer();
        else if (attackInputDirectionX == 1 && facingRight == false)
            FlipPlayer();
    }

    public void FlipPlayer()
    {       
        facingRight = !facingRight;
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        
    }

    /// <summary>
    /// Ripped straight out of WarriorMovement.
    /// </summary>
    /// <param name="inputDirection"></param>
    void DisplayVelocity(float inputDirectionX, float inputDirectionY)
    {
        if (GameManager.testing)
        {
            Vector2 inputDirection = new Vector2(inputDirectionX, inputDirectionY);

            //Blue = Velocity
            //Debug.Log(rigidbody.velocity + " and " + transform.localPosition);
            Debug.DrawRay(transform.localPosition, commonRigidbody.velocity / 3, Color.blue);// dividing by 3 cuz the ray is too big.

            //Green = Input
            Debug.DrawRay(transform.localPosition, inputDirection, Color.green);
        }
    }

    /// <summary>
    /// Originally DisplayGroundHitbox from WarriorMovement.
    /// </summary>
    void DisplayAttackHitbox()
    {
        if (GameManager.testing == true)
        {
            Vector3 tempPoint1;
            //Top-Left to Bottom-left
            tempPoint1 = new Vector3(attackTopLeft.x, attackBottomRight.y);
            Debug.DrawLine(transform.position + attackTopLeft, transform.position + tempPoint1, Color.yellow);

            //Top-Right to Bottom-Right
            tempPoint1 = new Vector3(attackBottomRight.x, attackTopLeft.y);
            Debug.DrawLine(transform.position + tempPoint1, transform.position + attackBottomRight, Color.yellow);

            //Bottom-Left to Bottom-Right
            tempPoint1 = new Vector3(attackTopLeft.x, attackBottomRight.y);
            Debug.DrawLine(transform.position + tempPoint1, transform.position + attackBottomRight, Color.yellow);

            //Top-Right to Top-Left
            tempPoint1 = new Vector3(attackBottomRight.x, attackTopLeft.y);
            Debug.DrawLine(transform.position + tempPoint1, transform.position + attackTopLeft, Color.yellow);

            //Offset thingy that goes unaccounted
            Debug.DrawLine(transform.position, transform.position + new Vector3(attackDirectionOffset, 0));
        }
    }

    /// <summary>
    /// A reminder that this is 100% accurate if u stay still. It remains on the point of starting to attack.
    /// </summary>
    void DisplayActiveAttackHitbox(bool startingToAttack)
    {
        Color hitboxColor;
        if (startingToAttack)
            hitboxColor = Color.blue;
        else
            hitboxColor = Color.red;

        if (GameManager.testing == true)
        {
            Vector3 tempPoint1;
            //Top-Left to Bottom-left
            tempPoint1 = new Vector3(attackTopLeft.x, attackBottomRight.y);
            Debug.DrawLine(transform.position + attackTopLeft + directionOffset, transform.position + tempPoint1 + directionOffset, hitboxColor, 1);

            //Top-Right to Bottom-Right
            tempPoint1 = new Vector3(attackBottomRight.x, attackTopLeft.y);
            Debug.DrawLine(transform.position + tempPoint1 + directionOffset, transform.position + attackBottomRight + directionOffset, hitboxColor, 1);

            //Bottom-Left to Bottom-Right
            tempPoint1 = new Vector3(attackTopLeft.x, attackBottomRight.y);
            Debug.DrawLine(transform.position + tempPoint1 + directionOffset, transform.position + attackBottomRight + directionOffset, hitboxColor, 1);

            //Top-Right to Top-Left
            tempPoint1 = new Vector3(attackBottomRight.x, attackTopLeft.y);
            Debug.DrawLine(transform.position + tempPoint1 + directionOffset, transform.position + attackTopLeft + directionOffset, hitboxColor, 1);
        }
    }

}
