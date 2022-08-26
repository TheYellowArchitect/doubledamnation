using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//Bad design: Cannot indicate KillMeter without mana. Disabling the meter without mana, would however force the player to speedrun&avoid which is opposite why this exists.
//Wrong name, should be BloodMeter but ah well. At least it is not wrong, since killmeter is obvious, but bloodmeter may think it has to do with dmg. KillMeter doesnt indicate its relation to fire level mechanic though. 
public class KillMeterManager : MonoBehaviour
{
    //[Tooltip("Automatically turned on at Fire Level")]
    //public bool bloodMeterActive = false;

    [Tooltip("How many cycles until player gets damaged from 0 kill?")]
    public int cyclesToKill = 26;//13 seconds.

    [Tooltip("What is the cycle duration?")]
    public float cycleDuration = 0.5f;

    //I didn't go for every frame, or a more stable way, for performance/ez design
    [Tooltip("What is the speed boost every Cycle/when timer ends?")]
    public float speedPerCycle;

    private int bloodMeterCounter = 0;
    public static float totalSpeedBoost = 0;//Laziness, sorry.

    private TimerManager timerManager;
    private WarriorHealth warriorHealthScript;
    private ManaManager manaScript;

    private bool Activated = false;
    private bool currentlyInLevelCutscene = false;

    //Finishes every 0.5 seconds, and lasts up to secondsToKill. Everytime it finishes, it increases the timer.
    Timer cycleBloodTimer;

    public Action drainedManaEvent;
    public bool allowDrainMana = true;

    // Use this for initialization
    void Start ()
    {
        timerManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TimerManager>();
        warriorHealthScript = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>();
        manaScript = GameObject.FindGameObjectWithTag("ManaManager").GetComponent<ManaManager>();

        cycleBloodTimer = timerManager.CreateTimer(cycleBloodTimer, 14, cycleDuration, false, true);
        cycleBloodTimer.TriggerOnEnd += IncreaseKillMeter;            
    }
	
    //Called by LevelManager.OnLevelLoad().DetermineKillMeter();
    public void StartBloodMeter()
    {
        Activated = true;

        RestartBloodMeter();
    }

    /// <summary>
    /// Resets elapsed time and activates the timer. Doesnt do anything if not activated once(via StartBloodMeter())
    /// </summary>
    public void RestartBloodMeter()
    {
        if (Activated == false)
            return;

        cycleBloodTimer.Restart();
    }

    //Called by cycleKillTimer
    public void IncreaseKillMeter()
    {
        bloodMeterCounter++;

        if (currentlyInLevelCutscene)
            return;

        //Mana B E G O N E
        if (bloodMeterCounter > cyclesToKill)
        {
            if (allowDrainMana)
                DrainAllMana();
        }
        else
        {
            totalSpeedBoost += speedPerCycle;

            RestartBloodMeter();
        }
    }

    public void DrainAllMana()
    {
        manaScript.ResetMana(true);

        ResetBloodMeter();

        //Purely netcoding
        if (drainedManaEvent != null)
            drainedManaEvent();
    }

    public void ResetBloodMeter()
    {
        RestartBloodMeter();

        //Reset the meter
        bloodMeterCounter = 0;
        
        //Reset the speed
        totalSpeedBoost = 0;
    }

    public void StopBloodMeter()
    {
        Activated = false;

        cycleBloodTimer.isActive = false;
    }

    public void SetCutsceneStatus(bool value)
    {
        currentlyInLevelCutscene = value;
    }
}
