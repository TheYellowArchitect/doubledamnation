using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]//so all variables show up in unity inspector
public class Word
{
    //Theoritically contains the word itself
    public string thisWord;

    //Used to store what letter we have typed so far/where we are in the sequence of letters.
    public int typeIndex = 0;

    //Constructor (how much time it's been xD)
    public Word(string _word)
    {
        thisWord = _word;
        typeIndex = 0;
    }

    public char GetNextLetter()
    {
        return thisWord[typeIndex];
    }

    public bool IsTypedEntirely()
    {
        //Typed entirely.
        if (typeIndex >= thisWord.Length)
        {
            //Remove the word on screen
            return true;
        }

        return false;  
    }

    public override string ToString()
    {
        return thisWord;
    }
}
