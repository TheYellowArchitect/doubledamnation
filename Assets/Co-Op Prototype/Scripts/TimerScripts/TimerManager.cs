using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//I would rather expand on this system in the future, hence, it is barebones here, so it won't be infected by the crippling sloppy design of the overall project.
//It is to be used (by others if they so wish) as an objectively the best TimerManager, with the following features in the future:
//Temp/Dummy timers (as in, created on the spot), clear/clean said timers. And all the system to ultimately, work in an other thread (multithreaded) by the C# Job System. (perhaps also make timers based on real-time and on game-time? dat time.timescale?)
public class TimerManager : MonoBehaviour
{
    //List<Timer> timerDictionary = new List<Timer>();
    private int maxIndex = 20;

    private enum TimerNames
    {
        Dodgeroll, TypingExpiration, Hitstun, Invulnerability,
        CastingMidairAttack, CastingGroundAttack, CastingDashAttack,
        RecoveryMidairAttack, RecoveryGroundAttack, RecoveryDashAttack,
        DodgerollDuration, PhantomDodgerollStart, PhantomDodgerollEnd,
        RunningDarkwind, BloodMeter, playerManaAlightOnKill
    }
    //Good job not using the above, retard. Surely remembering them all by heart via a comment will work.

    Dictionary<int, Timer> timerDictionary = new Dictionary<int, Timer>();
    int i;//used for Update() iteration loop

    /*Timer Table
     * 20 Hardcoded
        //Warrior Below
        0->DodgerollCooldown
        1->TypingExpiration
        2->Hitstun
        3->Invulnerability
        4->CastingMidairAttack
        5->CastingGroundAttack
        6->CastingDashAttack
        7->RecoveryMidairAttack
        8->RecoveryGroundAttack
        9->RecoveryDashAttack
        10->DodgerollDuration(End)
        11->PhantomDodgerollStart
        12->PhantomDodgerollEnd
        13->RunningDarkwindParticleEffect
        14->BloodMeter (FireLevel)
        15->PlayerManaOnKill

    */

    //Useless af? No, it is useless here, but in the future, this shall be used.
    Timer pickedTimer;

    // Update is called once per frame
    void Update()
    {
        //Iterates through timerDictionary to update all timers inside
        for (i = 0; i < maxIndex; i++)
        {
            //Checks if value exists
            if (timerDictionary.TryGetValue(i, out pickedTimer))
                timerDictionary[i].NextUpdate();
        }
    }

    public void AddTimer(Timer _timer)
    {
        //Should check if any Timers before are empty.(not looping + not active)

        //Skips already established values, so it won't clash with hard-coded timers.
        while (timerDictionary.ContainsKey(maxIndex))
            maxIndex++;

        //Assigns it the key and the timer
        timerDictionary.Add(maxIndex, _timer);

        maxIndex++;
    }

    public void AddTimer(int key, Timer _timer)
    {
        //Just in case I fuck up in the setup
        if (timerDictionary.ContainsKey(key))
        {
            Debug.Log("TimerManager's Key: " + key + " is already in the dictionary and its elapsedTime is: " + timerDictionary[key].waitTime);
            Debug.Break();
        }

        //Assigns it the key and the timer
        timerDictionary.Add(key, _timer);
    }

    //Works as a wrapper to Timer constructor, since it must link Timer with TimerManager but
    //without responsibility of Timer to link with TimerManager.(muh "not monobehaviour cannot find gameobjects)

    //Without Key -> Generic Timer
    public Timer CreateTimer(Timer createdTimer, float _waitTime = 2f, bool _isActive = true, bool _looping = false)
    {
        //Create new timer
        createdTimer = new Timer(_waitTime, _isActive, _looping);

        //Register new timer
        AddTimer(createdTimer);
        return createdTimer;
    }

    //With Key -> Important/Hardcoded Timer
    public Timer CreateTimer(Timer createdTimer, int key, float _waitTime, bool _isActive = true, bool _looping = false)
    {
        //Create new timer
        createdTimer = new Timer(_waitTime, _isActive, _looping);

        //Register new timer
        AddTimer(key, createdTimer);
        return createdTimer;
    }
}
