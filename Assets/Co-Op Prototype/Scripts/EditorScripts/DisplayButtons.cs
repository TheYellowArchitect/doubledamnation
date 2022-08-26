using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Change EVERY Image(buttons) to -> Raw Image
public class DisplayButtons : MonoBehaviour
{
    public GameManager gameManager;
    //Stores joysticks, to change location for UI
    [Tooltip("The joystick game objects, which are simply pictures of a joystick xD")]
    public GameObject[] Joysticks = new GameObject[2];
    private Vector3[] startJoystickPosition = new Vector3[2];

    [Tooltip("Holds all the \"images\" of buttons. In other words, the buttons top right.")]
    public CanvasRenderer[] UIbuttons;

    [Tooltip("Time before an pressed button vanishes.\nRecommended=0.5f")]
    public float defaultButtonTimer;

    //Holds the time-values of above buttons, so they are disabled.
    private float[] timeLeft = new float[16];
    private bool[] startCountdown = new bool[16];

    private Vector2 leftInputDirection;
    private Vector2 rightInputDirection;

    private void Start()
    {
        //Initialize button array(each one expires after 0.5f seconds, aka half a second.)
        for (int i = 0; i < 16; i++)
        {
            timeLeft[i] = defaultButtonTimer;
            startCountdown[i] = false;
        }

        //Store the original joystick UI locations
        startJoystickPosition[0] = Joysticks[0].transform.position;
        startJoystickPosition[1] = Joysticks[1].transform.position;

        //Make left joystick "visible"
        UIbuttons[14].SetAlpha(255f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        leftInputDirection = GetLeftJoystickInput();
        rightInputDirection = GetRightJoystickInput();
        if (GameManager.testing)//==true
        {
            ShowUIbuttons();
            HideUIbuttons();
        }
    }

    Vector2 GetLeftJoystickInput()//WarriorInput.cs also has it. GlobalFunctions material? :P
    {
        //Get Input from joysticks
        leftInputDirection.x = Input.GetAxis("LeftJoystickHorizontal");
        leftInputDirection.y = Input.GetAxis("LeftJoystickVertical");
        //thisTransform.position = startPos + leftInputDirection;

        //Dealing with dead zones, without even getting to movement :D
        if (leftInputDirection.x < 0.1f && leftInputDirection.x > -0.1f)
            leftInputDirection.x = 0;
        if (leftInputDirection.y < 0.1f && leftInputDirection.y > -0.1f)
            leftInputDirection.y = 0;

        return leftInputDirection;
    }

    Vector2 GetRightJoystickInput()//WarriorInput.cs also has it. GlobalFunctions material? :P
    {
        //Get Input from joysticks
        rightInputDirection.x = Input.GetAxis("RightJoystickHorizontal");
        rightInputDirection.y = Input.GetAxis("RightJoystickVertical");
        //thisTransform.position = startPos + leftInputDirection;

        //Dealing with dead zones, without even getting to movement :D
        if (rightInputDirection.x < 0.1f && rightInputDirection.x > -0.1f)
            rightInputDirection.x = 0;
        if (rightInputDirection.y < 0.1f && rightInputDirection.y > -0.1f)
            rightInputDirection.y = 0;

        return rightInputDirection;
    }

    void ShowUIbuttons()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetButtonDown("ButtonA"))
            {
                //Make visible
                UIbuttons[0].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[0] = true;
            }
            if (Input.GetButtonDown("ButtonB"))
            {
                //Make visible
                UIbuttons[1].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[1] = true;
            }
            if (Input.GetButtonDown("ButtonX"))
            {
                //Make visible
                UIbuttons[2].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[2] = true;
            }
            if (Input.GetButtonDown("ButtonY"))
            {
                //Make visible
                UIbuttons[3].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[3] = true;
            }
            if (Input.GetButtonDown("ButtonSelect"))
            {
                //Make visible
                UIbuttons[4].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[4] = true;
            }
            if (Input.GetButtonDown("ButtonStart"))
            {
                //Make visible
                UIbuttons[5].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[5] = true;
            }
            if (Input.GetButtonDown("ButtonR1"))
            {
                //Make visible
                UIbuttons[6].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[6] = true;
            }
            if (Input.GetButtonDown("ButtonL1"))
            {
                //Make visible
                UIbuttons[7].SetAlpha(255f);

                //Starts countdown for timeLeft[] so it goes alpha(0f) again, in ? Function.
                startCountdown[7] = true;
            }
        }

        if (leftInputDirection != Vector2.zero)
        {
            UIbuttons[14].SetAlpha(255f);
            timeLeft[14] = defaultButtonTimer;
        }

        if (rightInputDirection != Vector2.zero)
        {
            UIbuttons[15].SetAlpha(255f);
            timeLeft[15] = defaultButtonTimer;
        }
        //Convert fully to Vector2 or sth, one day~
        Joysticks[0].transform.position = startJoystickPosition[0] + new Vector3(leftInputDirection.x, leftInputDirection.y, 0f) * 12;
        Joysticks[1].transform.position = startJoystickPosition[1] + new Vector3(rightInputDirection.x, rightInputDirection.y, 0f) * 12;

        //If no input direction -> hide the joystick.
        if (leftInputDirection == Vector2.zero)
            startCountdown[14] = true;
        if (rightInputDirection == Vector2.zero)
            startCountdown[15] = true;
    }

    void HideUIbuttons()
    {

        //Iteration through every "button"
        for (int i = 0; i < 16; i++)
        {
            //Checks if it has been active.
            if (startCountdown[i] == true)
            {
                //Reduces time by the amount that passed.
                timeLeft[i] = timeLeft[i] - Time.deltaTime;

                //Timer finished
                if (timeLeft[i] < 0)
                {
                    //Makes invisible by maxing transparency(alpha channels)
                    UIbuttons[i].SetAlpha(0f);

                    //False so it denies countdown.
                    startCountdown[i] = false;

                    //Reput the float time value back to default(0.5f) value
                    timeLeft[i] = 0.5f;
                }
            }
        }
    }
}
