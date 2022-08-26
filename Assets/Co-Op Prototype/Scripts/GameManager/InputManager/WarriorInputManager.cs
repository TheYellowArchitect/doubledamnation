using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorInputManager : MonoBehaviour
{
    /// ////////////////////////////////
    //TODO: Probably put the below in a proper place?
    //TODO: Change X into a proper pressure? Idk, 0.51? 0.75?
    public float buttonAPressure = 1f;
    public float buttonBPressure = -1f;
    public float buttonXPressure = 0.7f;
    //Note that Y is the very definition of a short-hop, and is only meant for a short press.
    /////////////////////////////////

    /// <summary>
    /// When input is disabled, this is returned instead of normal input.
    /// </summary>
    private WarriorInputData defaultInputData;

    private WarriorInputData currentFrameInput;

    //These 2 are used as temps/dummies, to calculate the JoystickInputs
    //and being readable. They are obviously seperate from currentFrameInput.movement/combatInputDirection
    private Vector2 movementInputDirection;
    private Vector2 combatInputDirection;

    /// <summary>
    /// This variable exists, so one attack happens for one tilt, aka it auto-centers it after one attack is confirmed.
    /// Otherwise, if P1 dash-attacks and P2 dashes and cancels the dash-attack, in the very next frame
    /// Dash will be cancelled, for another dash-attack, whereas P1 didnt input another attack input.
    /// </summary>
    private Vector2 previousRightJoystickInput;
    private Vector2 currentRightJoystickInput;


    /// <summary>
    /// This variable exists, so it tracks the trigger and uses logic with it
    /// instead of using logic many times in a row with Input.GetAxis("Triggers")
    /// Triggers means L2 and R2 btw.
    /// </summary>
    private float backTriggersPressure;

    //This madness... smh. Should be removed or tweaked near release.
    private bool simulateKeyboard = false;

    private void Start()
    {
        previousRightJoystickInput = Vector2.zero;

        SetDefaultWarriorInput();
    }

    //This is called by InputManager
    public WarriorInputData CalculateInput()
    {
        //Create a new inputTimestamp for this frame
        currentFrameInput = new WarriorInputData();

        //Set the inputData, to be this.inputData
        currentFrameInput.movementInputDirection = GetLeftJoystickInput();
        currentRightJoystickInput = GetRightJoystickInput();

        currentFrameInput.combatInputDirection = currentRightJoystickInput;

        //ABXY Buttons and L2/R2 for jumping, and if detected, tweaks currentFrameInput.movementInputDirection.y
        //Only if **not** gameManager.Testing, because otherwise, these buttons are used by Debugger.cs (with that boolean)
        if (GameManager.testing == false)
            ProcessAlternativeJumpButtons();

        //Does any needed changes on AttackInput, depending on DarkwindDistortion.
        ProcessAttackInput();


        return currentFrameInput;
    }



    //TODO: Fix the Project Settings -> Input. Why? Because u fkin retard have duplicates, and i legit dont know how Unity didnt hit those warnings, since they have been there for so many versions!
    //Movement
    Vector2 GetLeftJoystickInput()//DisplayButtons.cs also has it. GlobalFunctions material? :P
    {
        //Get Input from joysticks
        movementInputDirection.x = Input.GetAxis("LeftJoystickHorizontal");
        movementInputDirection.y = Input.GetAxis("LeftJoystickVertical");

        if (simulateKeyboard)//smh.
        {
            //Get Input from keyboard
            movementInputDirection.x = Input.GetAxis("KeyboardHorizontal");
            movementInputDirection.y = Input.GetAxis("KeyboardVertical");
        }

            ///Debug.Log("|MOVEMENT: " + movementInputDirection.x);//Debug msg


        //Dealing with dead zones, without even getting to movement :D
        if (movementInputDirection.x < 0.12f && movementInputDirection.x > -0.12f)
            movementInputDirection.x = 0;
        //Changed below cuz deadzone of .y, means when mage jumps, the following flicker is weird.
        //if (movementInputDirection.y < 0.1f && movementInputDirection.y > -0.1f)
        if (movementInputDirection.y < 0.4f && movementInputDirection.y > -0.1f)
            movementInputDirection.y = 0;

            //Debug.Log("||" + movementInputDirection.x);//Debug msg

        return movementInputDirection;
    }

    //Combat(Attack/Shield via _X_ button and right joystick)
    //tfw u realise that cutting so many features, one was cut that actually helped the game. The shield doesnt belong in this game, game-design ftw!
    Vector2 GetRightJoystickInput()
    {
        //Get Input from joysticks
        combatInputDirection.x = Input.GetAxis("RightJoystickHorizontal");
        combatInputDirection.y = Input.GetAxis("RightJoystickVertical");

        if (simulateKeyboard)
        {
            //Get Input from keyboard
            combatInputDirection.x = Input.GetAxis("KeyboardHorizontalAttack");
            combatInputDirection.y = Input.GetAxis("KeyboardVerticalAttack");
        }


        //Dealing with dead zones, without even getting to movement :D
        if (combatInputDirection.x < 0.1f && combatInputDirection.x > -0.1f)
            combatInputDirection.x = 0;
        //Changed below cuz deadzone of .y, means when mage jumps, the following flicker is weird.
        //if (combatInputDirection.y < 0.1f && combatInputDirection.y > -0.1f)
        if (combatInputDirection.y < 0.1f && combatInputDirection.y > -0.1f)
            combatInputDirection.y = 0;

        //Debug.Log("X: " + combatInputDirection.x + "  Y: " + combatInputDirection.y);

        return combatInputDirection;
    }

    /// <summary>
    /// This function does Input.GetButton() for Buttons ABXY, and then Input.GetAxis() for L2 and R2
    /// and then uses that for jumping. Player Choice!!
    /// 
    /// Reminder that this happens AFTER the joystick input is gotten, so it will always override the joystick's Y.
    /// </summary>
    public void ProcessAlternativeJumpButtons()
    {
        //L2/R2 Triggers
        backTriggersPressure = Input.GetAxis("Triggers");
        if (backTriggersPressure > 0.1f || backTriggersPressure < -0.1f)
        {
            backTriggersPressure = Mathf.Abs(backTriggersPressure);

            //0.5 on Y axis, is the requirement to jump on "WarriorMovement.cs", though that should be a slider imo.
            if (backTriggersPressure < 0.5f)
                backTriggersPressure = 0.51f;

            currentFrameInput.movementInputDirection.y = backTriggersPressure;

            Debug.Log("Triggers is: " + Input.GetAxis("Triggers") + " and backTriggersPressure is: " + backTriggersPressure);
        }

        //"ABXY" Buttons
        //Mind you, all the below buttons give different results.
        if (Input.GetButton("ButtonA"))
            currentFrameInput.movementInputDirection.y = buttonAPressure;

        if (Input.GetButton("ButtonX"))
            currentFrameInput.movementInputDirection.y = buttonXPressure;

        //This seems like a fullhop, but its actually a shorthop since it happens only once.
        if (Input.GetButtonDown("ButtonY"))
            currentFrameInput.movementInputDirection.y = 1;

        //Even if from the others you jump high, this forces a fastfall.
        if (Input.GetButton("ButtonB"))
            currentFrameInput.movementInputDirection.y = buttonBPressure;





        //Easter egg, from a very fun and happy day :D
        if (Input.GetKey(KeyCode.Mouse2) && Input.GetKey(KeyCode.Mouse4) && Input.GetKey(KeyCode.BackQuote) && Input.GetKey(KeyCode.RightBracket))
            currentFrameInput.movementInputDirection.y = 1;
    }

    /// <summary>
    /// Does any changes needed to combatInput (currently from same input, and if not at edges)
    /// </summary>
    public void ProcessAttackInput()
    {
        //Read this function's summary instead of a useless comment here
        DetermineIgnoreSameAttackInput();

        //Read this function's summary instead of a useless comment here
        DetermineAttackInputOnEdges();
    }

    /// <summary>
    /// If the player choice is to ignore attack input when its the same as previous, it does: currentFrameInput.combatInputDirection = Vector2.zero;
    /// </summary>
    public void DetermineIgnoreSameAttackInput()
    {
        //If the player choice is to ignore same attack joystick input,
        //Checks if current attack input, is the same one with previous one
        //so it resets/snaps back to center/vector2.zero instead of attacking.
        if (DarkwindDistortionManager.globalInstance.ignoreSameAttackJoystickInput)
        {
            //If same input, Reset/Snap input back to Center/Vector2.zero
            if (currentRightJoystickInput == previousRightJoystickInput)
                currentFrameInput.combatInputDirection = Vector2.zero;

            //Store/Remember previous joystick input, so as to compare with the next input ;)
            previousRightJoystickInput = currentRightJoystickInput;
        }
    }

    /// <summary>
    /// If the player choice is to accept input only on edges, it does: currentFrameInput.combatInputDirection = Vector2.zero;
    /// </summary>
    public void DetermineAttackInputOnEdges()
    {
        //If the player choice is to accept input only on edges,
        //Checks if the input is on the edges, and if so,
        //it resets/snaps back to center/vector2.zero instead of giving proper attack input.
        if (DarkwindDistortionManager.globalInstance.acceptAttackInputOnlyOnJoystickEdges && Vector2.SqrMagnitude(currentRightJoystickInput) < DarkwindDistortionManager.globalInstance.RightJoystickRadius)
            currentFrameInput.combatInputDirection = Vector2.zero;

        //if (GameManager.testing && Vector2.SqrMagnitude(currentRightJoystickInput) > 0.1f)
            //Debug.Log("Square Magnitude is: " + Vector2.SqrMagnitude(currentRightJoystickInput));
    }

    //Used when input is disabled.
    public void SetDefaultWarriorInput()
    {
        //Create a new inputTimestamp for this frame
        defaultInputData = new WarriorInputData();

        //Set the inputData, to be this.inputData
        defaultInputData.movementInputDirection = Vector2.zero;
        defaultInputData.combatInputDirection = Vector2.zero;
    }

    public WarriorInputData GetDefaultWarriorInput()
    {
        return defaultInputData;
    }

    public void SetSimulateKeyboard(bool value)
    {
        simulateKeyboard = value;
    }
}
