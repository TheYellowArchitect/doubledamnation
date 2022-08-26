using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Should be spherical
//Always on top of a trigger gameobject
public class SunbeamDialogueDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SunbeamDialogueManager.globalInstance.WitnessedSunbeamTop();

        //Debug.Log("TRIGGERED REEEEE!");
    }
}
