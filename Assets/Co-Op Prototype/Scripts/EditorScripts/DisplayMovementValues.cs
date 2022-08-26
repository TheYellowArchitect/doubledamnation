using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(WarriorMovement))]
public class DisplayMovementValues : MonoBehaviour
{
    public WarriorMovement moveScript;

    private Text UItext;

	// Use this for initialization
	void Start ()
    {
        UItext = GetComponent<Text>();
        DisplayUIValues();
	}
	
	// Update is called once per frame
	void Update ()
    {
        DisplayUIValues();
	}

    void DisplayUIValues()
    {
        UItext.text = "int currentState: " + moveScript.StateToString(moveScript.GetCurrentState()) + "\nint targetState: " + moveScript.StateToString(moveScript.GetTargetState()) + "\nbool midair: " + moveScript.GetMidair() + "\nbool IsAtopGround: " + moveScript.GetAtopGround() + "\nbool IsAtopPlatform: " + moveScript.GetAtopPlatform() + "\nbool facingLeft: " + moveScript.GetFacingLeft() + "\nint DodgerollChain: " + moveScript.GetPhantomDodgerollCount();
    }
}
