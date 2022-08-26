using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordDisplay : MonoBehaviour
{
    public TMP_Text thisText;

    public void Start()
    {
        thisText.text = "";
    }

    public void SetWord(string displayWord)
    {
        thisText.text = displayWord;
    }

    //Colors first letter, but its smart af, cuz in reality it splits the string.
    public void CompleteLetter(string wordToBeDisplayed, int letterLocation)
    {
        thisText.text = "<color=#000000ff>" + wordToBeDisplayed.Substring(0,letterLocation+1) + "</color>" + wordToBeDisplayed.Remove(0, letterLocation+1);

        //To see other color hexcodes: https://docs.unity3d.com/Manual/StyledText.html

        //Debug.Log("temp is: " + "<color=#00ff50ff>" + wordToBeDisplayed.Remove(0, letterLocation+1) + "</color>");        
    }

    public void Reset()
    {
        //Maybe some tiny VFX in the future?
        thisText.text = "";
    }

}
