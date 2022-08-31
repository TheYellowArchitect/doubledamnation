using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    //TODO: These should be on GameManager!!!
    //Skipping for gameplay reasons duh.
    public bool skipIntroCutscene = true;
    public bool pseudoBuildMode = true;
    public short levelToLoadAfter = 0;//0 = Tutorial

    //Includes boat and player/warrior.
    private GameObject playerIntroAnimation;

    private bool hasLoadedNextLevel = false;

    // Use this for initialization
    void Start()
    {
        //Reminder that the animation plays independently of this.

        //==

        if (pseudoBuildMode)
        {
            StartCoroutine(EndIntro());
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().SkipAllDialogues();
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().DisableTestingUI();
        }
           

        if (skipIntroCutscene == false)
            StartCoroutine(IntroTasks());
        else
            StartCoroutine(EndIntro());

        
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public void Update()
    {
        //Detect to skip intro
        if (GameManager.testing)
            //Auto-detect the skip, and hence, skip the boat. Speedrunning POG
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().skipMidCutsceneDialogue = true;
    }
#endif

    IEnumerator IntroTasks()
    {
        //TODO: If no black/gray screen in the start of the game (preferably gray), then put it. It should be on ActiveScene tbh, and here it is only deleted cuz fade-Out animation happens from full blacc.
        //^ 3 seconds one, not 1 second or w/e lmao.

        //If no game manager, it means this scene is ran by the editor, ALONE. Aka debugging or w/e, so dont do anything, aka return.
        if (GameObject.FindGameObjectWithTag("GameManager") == null)
            yield break;

        //Starts the intro dialogue.
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(21);//8 is the default, 21 is alt

        //Check if alternative ending, so as to change camera animation (goes downwards instead of upwards)
        if (PlayerStatsManager.globalInstance.GetPreviousEnding() == Endings.Perfect)
            GameObject.FindGameObjectWithTag("IntroCamera").GetComponent<Animator>().Play("CameraIntroCutscenePerfectEnding");

        //Get the playerIntroAnimation, aka boat + warrior.
        playerIntroAnimation = GameObject.FindGameObjectWithTag("IntroBoatPlayer");

        //Hide the player
        GameObject.FindGameObjectWithTag("GameManager").transform.GetChild(2).gameObject.GetComponent<MasterInputManager>().DisableInput();

        //Wait for cutscene to end, it should be as many seconds the voice clips together + the fade-in.
        yield return new WaitForSeconds(18f);

        //Debug.Log("Current level is: " + LevelManager.currentLevel);//-1

        //Gets to tutorial level, and unloads this scene.
        if (hasLoadedNextLevel == false && LevelManager.currentLevel < 0)
            StartCoroutine( EndIntro() );
            
    }

    public IEnumerator EndIntro()
    {
        hasLoadedNextLevel = true;

        //TODO: Get a fade-in of like 5 seconds lmao.
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<FadeUIManager>().LevelFadeIn();

        yield return new WaitForSeconds(1);

        //Why enable input here, and not after loading next level?
        GameObject.FindGameObjectWithTag("GameManager").transform.GetChild(2).gameObject.GetComponent<MasterInputManager>().EnableInput();

        if (LevelManager.currentLevel < 0)
            SceneManagerScript.globalInstance.LoadTargetLevel(levelToLoadAfter);//Tutorial
    }

    //For debugging purposes ofc. Called from DialogueManager (needs gamemanager.testing == true)
    public void SkipIntro()
    {
        if (hasLoadedNextLevel == false)
            StartCoroutine(EndIntro());
    }

}
