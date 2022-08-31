using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialIntroMage : MonoBehaviour 
{

	private WarriorMovement warriorBehaviour;
	private MageBehaviour mageBehaviour;

	private float timeCurrent = 0f;
	[Tooltip("8.1f is both voicelines, 4.5f is P2 only")]//Tutorial
	public float timeVoiceEnd = 8.1f;//When no voicelines, do VanishMage();

	// Use this for initialization
	void Start () 
	{
		warriorBehaviour = WarriorMovement.globalInstance;
		mageBehaviour = MageBehaviour.globalInstance;

		mageBehaviour.StartLevelCutsceneDialogue();
		if (LevelManager.currentLevel == 0)
			mageBehaviour.transform.position = mageBehaviour.GetLevelDialoguePositions().tutorialStart;
		else if (LevelManager.currentLevel == 1)
			mageBehaviour.transform.position = mageBehaviour.GetLevelDialoguePositions().level1Start;
			
		mageBehaviour.transform.localScale = new Vector3(-1.4f, 1.4f, 1);

		//Debug.Log("target position is: " + mageBehaviour.GetLevelDialoguePositions().tutorialStart);
		if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().skipMidCutsceneDialogue)
			VanishMage();
	}

	public void Update()
	{
		//Warrior starts with facing left.
		//Could have a callback on flip of warrior, but whatever.
		if (LevelManager.currentLevel == 0 && warriorBehaviour.GetFacingLeft() == true)
			VanishMage();

		if (warriorBehaviour.GetHasMageJumped() == true)
			VanishMage();

		timeCurrent = timeCurrent + Time.deltaTime;
		if (timeCurrent > timeVoiceEnd)
			VanishMage();
			
	}
	
	public void OnTriggerExit2D(Collider2D collisionDetected)
	{
		Debug.Log("Exited!");
		VanishMage();
	}

	public void VanishMage()
	{
		mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Armor);

		Destroy(this);
	}
}
