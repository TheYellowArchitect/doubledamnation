using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsDialogueDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OutOfBoundsDialogueManager.globalInstance.StartOutOfBoundsDialogue();

        //Debug.Log("TRIGGERED REEEEE!");

        Destroy(this);
    }

}
