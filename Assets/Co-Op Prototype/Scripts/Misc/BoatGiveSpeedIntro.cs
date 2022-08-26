using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatGiveSpeedIntro : MonoBehaviour
{
    [Tooltip("The speed of the boat going forward")]
    public float boatSpeed;

	// Use this for initialization
	void Start ()
    {
        GameObject.FindGameObjectWithTag("IntroBoatPlayer").GetComponent<Rigidbody2D>().velocity = new Vector2(boatSpeed, 0);
	}
	
	
}
