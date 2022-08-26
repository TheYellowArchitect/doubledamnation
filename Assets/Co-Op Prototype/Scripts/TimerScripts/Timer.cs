using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Timer
{
    public float waitTime;

    public float elapsed = 0f;

    public bool looping;

    public bool isActive;

    public event Action TriggerOnEnd;

    //Constructor
    public Timer (float _waitTime, bool _isActive = true, bool _looping = false)
    {
        waitTime = _waitTime;
        looping = _looping;
        isActive = _isActive;
    }

    public Timer()
    {
        waitTime = 2f;
        looping = false;
        isActive = true;
    }

    public void NextUpdate()
    {
        if (!isActive)
            return;

        elapsed = elapsed + Time.deltaTime;

        //If timer is over
        if (elapsed >= waitTime)
        {
            elapsed = 0f;

            isActive = false;

            if (TriggerOnEnd != null)
                //Do the event/action subscribed for!
                TriggerOnEnd();

            //Clears the subscriptions
            if (!looping)
                TriggerOnEnd = null;
        }
    }

    public void Configure(float _waitTime, bool _looping = false)
    {
        waitTime = _waitTime;
        looping = _looping;
    }

    //Note: this will never be 1.0, because when it reaches 1.0-> elapsed = 0!!
    public float GetTimePercent()
    {
        //1 elapsed 2 waitTime 0.5 result (1/2 = 0.5)
        //2 elapsed 2 waitTime 1   result (2/2 = 1)
        return elapsed / waitTime;
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Reset()//Disable.
    {
        isActive = false;
        elapsed = 0f;
    }

    public void ResetTime()
    {
        elapsed = 0f;
    }

    public void Restart()
    {
        elapsed = 0f;
        isActive = true;
    }
}