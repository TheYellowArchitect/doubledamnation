using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerGrassMovementInteraction : MonoBehaviour
{
    private Animator commonAnimator;

    /// <summary>
    /// Created because player enters the trigger2D() 2 times instead of once, because he has 2 colliders.
    /// </summary>
    private bool recognized = false;

	// Use this for initialization
	void Start ()
    {
        commonAnimator = GetComponent<Animator>();
	}

    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        //==============================================================
        recognized = !recognized;

        if (recognized == false)
            return;
        //==============================================================

        //To check for direction for proper "Move" animation.
        //Negative means the player is entering from the left of this collider
        //Positive means the player is entering from the right of this collider
        float playerDirection = (playerCollider.gameObject.transform.position - transform.position).x;

        if (playerDirection > 0)
            commonAnimator.SetTrigger("MoveLeft");
        else
            commonAnimator.SetTrigger("MoveRight");

        //TODO: If you decide to make this for trees as well, look at Editor -> Tools -> ToolRecolorLevel2Spikes.cs
        //Why? Because u will have to re-do this, aka go to an object, and iterate to everything inside, and give it animator, box collider2d trigger, 
        //You are retarded^ Let me explain. Simply make a prefab for every tree, instead of making them just be random gameobjects. And when you want to update, its really simple ;)
        //Make a prefab for every tree type, not every tree!!@#!!!!
    }
}
