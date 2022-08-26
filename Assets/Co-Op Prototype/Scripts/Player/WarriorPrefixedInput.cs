using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Finds the nearest start/gate object, and puts the Input every Update() to go the left/right side until it is reached.
/// Insantiated onto warrior when insanity ending is a thing.
/// </summary>
public class WarriorPrefixedInput : MonoBehaviour
{
    [Tooltip("If true, once this script \"ends\" it calls DialogueManager.TriggerInsanityEnding()")]
    public bool triggerInsanityEndingOnEnd = false;



    private WarriorMovement warriorBehaviour;

    //Useful to disable the input, when it reached the gate.
    //So it disables the warrior Input, then starting this, so there won't be a desynchronization until it is disabled. just making sure, without this there should be no problems tbh.
    public bool activateSimulatedInput = false;

    public Vector3 archwayPosition;
    public float distanceToArchway;
    private Vector2 prefixedMovementInput;

    // Use this for initialization
    void Start ()
    {
        warriorBehaviour = GetComponent<WarriorMovement>();
        
        //Do note, that we make .enabled = false, instead of .disableInput.
        //Because, we REPLACE warriorBehaviour.Movement(), and if we disabled input, it would still send to Movement() default inputs! (e.g. for warrior joysticks, vector2.zero)
        GameObject.FindGameObjectWithTag("GameManager").transform.GetChild(2).gameObject.GetComponent<MasterInputManager>().enabled = false;

        archwayPosition = GameObject.Find("Start").transform.position;
        //^Soz for using name rip performance, but does it really matter for this point in the game, since it is used just once?
        //If you have to "fix" this, then simply make the sprite a child object of "Start" and give it the tag StartingArchway or sth and use this instead to find it.

        Vector3 directionToArchway = (archwayPosition - transform.position).normalized;
        if (directionToArchway.x < 0)
            prefixedMovementInput = Vector2.left;
        else
            prefixedMovementInput = Vector2.right;
        //^Implying he went back, not confirming.

        activateSimulatedInput = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (activateSimulatedInput == false)
            return;

        //Update distance every frame.
        distanceToArchway = Vector3.Distance(transform.position, archwayPosition);//Vector3.distance bad.

        Debug.Log("Distance to archway is: " + distanceToArchway);

        //If distance is less than X, stop moving.
        if (distanceToArchway < 5)
        {
            prefixedMovementInput = Vector2.zero;
            activateSimulatedInput = false;//Could just delet this entire class lmao.

            if (triggerInsanityEndingOnEnd)
                GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().TriggerInsanityEnding();
        }
        

        warriorBehaviour.Movement(prefixedMovementInput, Vector2.zero);
	}

    //Shows the radius
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Draw attack range
        Handles.color = new Color(0.6f, 0.9f, 0f, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, distanceToArchway);
    }
    #endif
}
