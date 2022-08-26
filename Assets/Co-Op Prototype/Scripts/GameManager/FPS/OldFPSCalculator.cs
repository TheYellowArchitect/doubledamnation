using UnityEngine;
using System.Collections;

//Converted from https://wiki.unity3d.com/index.php?title=FramesPerSecond
public class OldFPSCalculator : MonoBehaviour
{
    //public string formatedString = "{FPSValue} FPS ({millisecondsValue} ms)";

    private float deltaTime = 0.0f;
    private float milliseconds;
    private float fps;

    private bool calculate = true;

    void Update()
    {
        if (calculate)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            milliseconds = deltaTime * 1000.0f;
            fps = 1.0f / deltaTime;
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

    public float GetFPS()
    {
        return fps;
    }

    public float GetMilliseconds()
    {
        return milliseconds;
    }
}