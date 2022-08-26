using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

//TODO: If F12 is pressed, while on tutorial area, it automatically enables speedrun mode, and skips the tutorial.

/// <summary>
/// While it is named SPEEDRUN manager, it mainly tracks down the clear level times, regardless if speedrunning or not.
/// </summary>
public class SpeedrunManager : MonoBehaviour
{
    [Header("Cached via Inspector")]
    public PlayerStatsManager playerStatsManager;

    public GameObject currentRunTimeIntegerUI;
    public GameObject currentRunTimeDecimalsUI;

    [Header("CurrentLevelRun variables")]
    /// <summary>
    /// The moment you touch the gate, this is the time of this level's run!
    /// </summary>
    [ReadOnly]
    public double timeOfCurrentRun = -1;

    /// <summary>
    /// Aka when did the speedrun happen? At what second?
    /// </summary>
    [ReadOnly]
    public float timeSpeedrunGateActivated = -1;

    /// <summary>
    /// Did player start the speedrun in current level?
    /// Aka did he activate this level's speedrunGate?
    /// </summary>
    [ReadOnly]
    public bool levelSpeedrunStarted = false;    


    private TextMeshProUGUI currentRunTimeTextInteger;
    private TextMeshProUGUI currentRunTimeTextDecimals;

    //Pressing F12, skips current level, so u can train on a level of your choice.
    private bool skippedLevel = false;





    private void Start()
    {
        //Cache the TMProUGUIs, so you dont have to GetComponent<> every Update() to update them!
            currentRunTimeTextInteger = currentRunTimeIntegerUI.GetComponent<TextMeshProUGUI>();
            currentRunTimeTextDecimals = currentRunTimeDecimalsUI.GetComponent<TextMeshProUGUI>();

        //Deactivate them.
            currentRunTimeIntegerUI.SetActive(false);
            currentRunTimeDecimalsUI.SetActive(false);

        GetComponent<DialogueManager>().finishedDeathDialogue += ResetCurrentLevelRun;
    }

    public void SpeedrunGateActivated()
    {
        timeSpeedrunGateActivated = Time.time;

        timeOfCurrentRun = 0;

        levelSpeedrunStarted = true;

        //If speedrun mode is true/active
        if (playerStatsManager.GetSpeedrunMode() == true)
            ShowRunTimeUI (true);
            
    }

    public void ShowRunTimeUI(bool show)
    {
        if (timeOfCurrentRun == -1)
            return;

        currentRunTimeIntegerUI.SetActive(show);

        currentRunTimeDecimalsUI.SetActive(show);
    }

    private void Update()
    {
        if (levelSpeedrunStarted == true)
        {
            timeOfCurrentRun += Time.unscaledDeltaTime;//(double) cast is redundant, VS says.

            //If speedrun mode is true/active
            if (playerStatsManager.GetSpeedrunMode() == true)
            {
                //Making the speedrunUIText, show the currentRunTime
                //Using this https://stackoverflow.com/questions/1426857/truncate-number-of-digit-of-double-value-in-c-sharp

                currentRunTimeTextInteger.text = timeOfCurrentRun.ToString("0");

                currentRunTimeTextDecimals.text = (timeOfCurrentRun - System.Math.Truncate(timeOfCurrentRun)).ToString(".00");

                //Also, BLESS this guy. Would have done hacky shit to get integer and decimals to always have same distance!
                //https://youtube.com/watch?v=-piJYtbAkLI
                //It took me at least 2 HOURS to fucking find this video (youtube, google, duckduckgo algorithm wtf?!)
                //And I found it manually, by searching all familiar unity tutorial channels, then when I found this guy's
                //I did remember that I found it on a playlist of unity tips, and ta-da! 2+ Hours of wasted effort, not wasted completely.
                //Also, fuck electricity outage. Almost lost shit tons of code cuz of it
                //but it did somehow make me find the video by deepthonking while no light
            }

        }

    }

    /// <summary>
    /// Gets the accurate run times, and sends them to PlayerStatsManager.
    /// Then resets the speedrun variables, so they are clean for the next level
    /// </summary>
    public void ClearedLevel()
    {
        //Detects if skipped level
            if (skippedLevel)
            {
                //Resets the flag
                skippedLevel = false;

                //Intentionally scraps/resets the time
                //So skipping won't be used for cheating and abuse.
                timeOfCurrentRun = -1;
            }

        //Send the time to PlayerStatsManager
            if (LevelManager.currentLevel == 0)
                playerStatsManager.ClearedLevel0();
            else if (LevelManager.currentLevel == 1)
                playerStatsManager.ClearedLevel1(timeOfCurrentRun);
            else if (LevelManager.currentLevel == 2)
                playerStatsManager.ClearedLevel2(timeOfCurrentRun);
            else if (LevelManager.currentLevel == 3)
                playerStatsManager.ClearedLevel3(timeOfCurrentRun);
            else if (LevelManager.currentLevel == 4)
                playerStatsManager.ClearedLevel4(timeOfCurrentRun);

        //Save PlayerStats
            playerStatsManager.SavePlayerStats();


        //Reset the speedrun variables
        ResetSpeedrunVariables(false);
    }

    public void ResetSpeedrunVariables(bool resetTimeOfCurrentRun = false)
    {
        levelSpeedrunStarted = false;
        
        timeSpeedrunGateActivated = -1;

        if (resetTimeOfCurrentRun == true)
            timeOfCurrentRun = -1;
        //else 
            //The previous run time remains.
    }

    //Invoked by SettingsMenu
    public void ToggleSpeedrunMode(bool value)
    {
        //TODO: Remove this when you handle speedrun on each level of level editor, properly
        if (LevelManager.currentLevel == 7)
            return;

        //If speedrun mode is activated
        if (value == true)
        {
            GetComponent<DialogueManager>().SkipAllDialogues();

            //Tutorial level doesnt have speedrun gate.
            if (LevelManager.currentLevel > 0)
                GameObject.FindGameObjectWithTag("SpeedrunGate").transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
            else//skip tutorial
                SkipLevel(true);
        }
        else
        {
            GetComponent<DialogueManager>().DontSkipAllDialogues();

            //Tutorial level doesnt have speedrun gate.
            if (LevelManager.currentLevel > 0)
                GameObject.FindGameObjectWithTag("SpeedrunGate").transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Stop();
        }
            

        //If toggles speedrun to true, and has already passed the gate
        if (value == true && timeSpeedrunGateActivated > -1)
            RestartCurrentLevelRun();

        //Notify playerStatsManager to update it.
        playerStatsManager.SetSpeedrunMode(value);
    }

    //Skipped the intro by pressing F12 when game started.
    public void ToggleIntroSpeedrunMode()
    {
        //Notify playerStatsManager to update it.
        playerStatsManager.SetSpeedrunMode(true);

        //Automatically skip all dialogues
        GetComponent<DialogueManager>().SkipAllDialogues();

        //Skip intro.
        GameObject.FindGameObjectWithTag("IntroManager").GetComponent<IntroManager>().SkipIntro();
    }

    /// <summary>
    /// Teleports the player to start of level. Resets the timer and speedrun gate too. Also resets the level.
    /// Invoked by pressing "Backspace" and by setting speedrun mode to true, after having already passed the speedrun gate.
    /// </summary>
    public void RestartCurrentLevelRun()
    {
        //Teleport the player back to start, and do proper value resets for him.
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().ResetAtLevelStart();

        if (LevelManager.currentLevel != 7)
        {
            //Reset level (enemies, platforms, corpses, coins)
            GameObject.FindGameObjectWithTag("SceneManager").GetComponent<LevelManager>().RestartLevel();

            ResetCurrentLevelRun();
        }
        //else, proper level editor speedrun timer recording stuff
        
    }

    //Called when restarting the current run^, or when a player dies.
    public void ResetCurrentLevelRun()
    {
        if (LevelManager.currentLevel == 7)
            return;

        //Reset timer and boolean.
        ResetSpeedrunVariables(true);

        //Resets speedrun gate's "Activated" boolean
        if (GameObject.FindGameObjectWithTag("SpeedrunGate") != null)
            GameObject.FindGameObjectWithTag("SpeedrunGate").GetComponent<SpeedrunGate>().speedgateActivated = false;

        if (PlayerStatsManager.globalInstance.GetSpeedrunMode() == true)
        {
            //Re-activates speedrun gate's VFX//If condition to check if already playing, though?
            GameObject.FindGameObjectWithTag("SpeedrunGate").transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
        }
            
    }

    public void SkipLevel(bool tutorial = false)
    {
        if (tutorial == false)
            skippedLevel = true;

        //Debug.Log("Skipped a level!");

        //If there is a lock on end gate, unlock it (lock = requirement/condition to enter beforehand)
        if (GameObject.Find("End").GetComponent<KillLockedEndGate>() != null)
            GameObject.Find("End").GetComponent<KillLockedEndGate>().UnlockEndGate();

        //Skip level
        GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>().position = GameObject.Find("End").transform.position;
    }

    public double GetTotalTime()
    {
        return timeSpeedrunGateActivated + timeOfCurrentRun;
    }

    public bool GetSpeedrunMode()
    {
        return playerStatsManager.GetSpeedrunMode();
    }

    //Frame-Drop Solution
        //Btw, there must be issues for low-hardware players in speedrunning.
        //Since, framedrop is a possibility...
        //While streaming via video ftw, and confirms their run (and input register log!)
        //I think, there is an easy way to find if frames are dropped?
        //https://docs.unity3d.com/ScriptReference/Time-frameCount.html
        //So many ways to use the above... I'm certain one of the many ways, is the solution.
        //(ah and ofc, a frame-counter, showing constantly, or showing when frame-drop happens while speedrunning in a level)?



    //lmao at this answer https://stackoverflow.com/questions/22804821/why-is-time-fixedtime-and-family-a-float-and-not-a-double
    //it seems interesting and accurate (5 billion on int) which while hilarious, seems to work actually.
}
