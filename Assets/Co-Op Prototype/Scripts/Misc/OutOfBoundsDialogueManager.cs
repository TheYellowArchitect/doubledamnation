using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Should be very lightweight
/// Its job is to store which outofbounds to say (literally only 2 exist kek)
/// When OutOfBounds has its own container, then this should be cleaner!
/// Fucking spagghetti again!
/// </summary>
public class OutOfBoundsDialogueManager : MonoBehaviour
{
    public static OutOfBoundsDialogueManager globalInstance;

    //0 is none
    //1 is having told 1, and so on and so on
    public int dialogueFlag = 0;

	void Start ()
    {
        globalInstance = this;
    }
	
	public void StartOutOfBoundsDialogue()
    {
        //If beyond max dialogues, gtfo
        if (dialogueFlag > 1)
            return;

        //Invoke dialogue
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(16 + dialogueFlag);

        //Increase dialogue by one
        dialogueFlag++;
            
    }

    //Triggered by DialogueManager, so as to store flag properly and unsubscribe if needed.
    public void OutOfBoundsDialogueFinished()
    {
        dialogueFlag++;
    }
}
