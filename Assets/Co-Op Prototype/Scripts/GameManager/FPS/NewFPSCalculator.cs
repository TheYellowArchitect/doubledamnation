using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Bless this page https://answers.unity.com/questions/64331/accurate-frames-per-second-count.html
public class NewFPSCalculator : MonoBehaviour
{
    public double updatesPerSecond = 4.0;  // updateRate

    private int frameCount = 0;
    private double deltaTimeFromLastFrame = 0.0;
    private double fps = 0.0;

    private bool calculate = true;

    public void Update()
    {
        if (calculate)
        {
            frameCount++;
            deltaTimeFromLastFrame += Time.deltaTime;
            if (deltaTimeFromLastFrame > 1.0 / updatesPerSecond)
            {
                fps = frameCount / deltaTimeFromLastFrame;
                frameCount = 0;
                deltaTimeFromLastFrame -= 1.0 / updatesPerSecond;
            }
        }
    }

    /// <summary>
    /// Should this script do FPS calculations?
    /// </summary>
    /// <param name="value"></param>
    public void SetCalculation(bool value)
    {
        calculate = value;
    }

    public double GetFPS()
    {
        return fps;
    }
}
