using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMidCutscene : MonoBehaviour
{
    [Tooltip("Go at the MidCutscenesDialoguesContainer, and check the dialogues. Then find the dialogue you want from these, and put the number of it here.")]
    public int dialogueIndex;

    public bool triggeredOnce = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggeredOnce == false)
        {
            triggeredOnce = true;

            //Notify Dialogue manager, aka where the actual stuff happens and dialogues n stuff happen as well :P
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(dialogueIndex);
        }
        //else nothing;
    }
}
