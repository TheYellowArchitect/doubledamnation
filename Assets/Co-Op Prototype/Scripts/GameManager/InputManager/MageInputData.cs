using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MageInputData
{
    //===Dodgeroll===
    public bool aboutToDodgeroll;
    public bool dodgerollRight;


    //Spacebar
    public bool aboutToJump;

    //Input string from keyboard's characters, edited by WordManager.
    //Includes backspace/skipLevelIfSpeedrunning!
    public string finishedSpellwordString;

    //===Speedrunning===
    //TODO: You gotta decouple the logic from warriorInput, so it doesnt directly call the speedrun methods? idk.
    //public bool skipLevel;
	
}
