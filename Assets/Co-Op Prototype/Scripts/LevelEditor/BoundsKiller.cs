using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsKiller : MonoBehaviour 
{
	[Header("Kill when under")]
	public float bottom = -200f;
	public float left = -200f;

	private Transform playerTransform;
	private bool playerIsDead = false;

	// Use this for initialization
	void Start () 
	{
		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

		//Boolean flag.
		GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DiedEvent += PlayerRegisteredDead;
		GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().finishedRevive += PlayerRegisteredAlive;
		if (LevelManager.currentLevel == 7)
			LevelEditorMenu.toggledPlayModeEvent += PlayerRegisteredAlive;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (playerTransform.position.x < left)
			BoundsKill();
		else if (playerTransform.position.y < bottom)
			BoundsKill();
	}

	public void BoundsKill()
	{
		//TODO: Mage voiceline

		if (playerIsDead == false)
		{
			playerIsDead = true;

			GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DieSimple();
		}
	}

	public void PlayerRegisteredDead()
	{
		playerIsDead = true;
	}

	public void PlayerRegisteredAlive(bool isPlayMode)
	{
		PlayerRegisteredAlive();
	}

	public void PlayerRegisteredAlive()
	{
		playerIsDead = false;
	}
}
