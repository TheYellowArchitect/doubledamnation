using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct PlayerCoreStats
{
    //All the below should be private, imo.
    //This entire class is used by PlayerStatsManager
    //as PRIVATE, so theoritically, some safety is in place.

    /// <summary>
    /// Used to identify, for which run this is used.
    /// So there wont be only 1 savefile that will be overwritten
    /// but each new run will have its own playerStats file.
    /// Should match the one with PlayerPathStats obviously.
    /// </summary>
    public int playerStatsID;

    //Damage&Kills (initialized to 0)
    public int damageTaken;

    public int damageDealt;
    public int currentRunDamageDealt;

    public int totalKills;
    public int currentRunKills;


    //Deaths (initialized to 0)
    public int totalDeaths;
    public int level1Deaths;
    public int level2Deaths;
    public int level3Deaths;
    public int level4Deaths;

    //Dialogue Interruptions
    public int dialogueWarriorInterruptions;
    public int dialogueMageInterruptions;

    //Total PowerUps
    public int totalPowerUps;
    //public int powerUpsLevel0;//Not a thing since not 13 enemies in first level/tutorial.
    public int powerUpsLevel1;
    public int powerUpsLevel2;
    public int powerUpsLevel3;
    public int powerUpsLevel4;


    //ClearLevelTimes (speedrunPOG)
    public double timeToBeatLevel0;
    public double timeToBeatLevel1;
    public double timeToBeatLevel2;
    public double timeToBeatLevel3;
    public double timeToBeatLevel4;

    

    //Gamemode (initialized to false)
    public bool speedrunMode;
    //public bool strictSpeedrunMode;//Aka the above, but when u die, it resets the game completely (starting from level 1).
    public bool purgeMode;
    //^Aka, to go to the next level, you must kill every monster (needs some kind of enemy-tracker to help for sure, Metroid POG)
    //Actually, let's talk a little about this mode.
    //If you have to kill ALL monsters, this will be tedious and take a lot of time to finish the run, while not really being challenging.
    //Hence, like low% and Max/All%, it will be rarely played since boring to the normal speedrun.
    //And that's why... Make a slider. To win the run of a level, either 60% of the level, either 70%, either 80%, either 90%, either 100%.
    //Which will be their own sub-categories in purgeMode ;)
    //Tbh, I think 60% is the most fun one. (50% thooo)
    //And ofc, while you are playing, you have a counter on how many more monsters you have to kill.
    //So, the UI will have this line somewhere: 
    //_MonstersToKill_ (in big font size and ez to see color) and in smaller font size next to it, _currentKills_/_maxKills_
    public bool playedAsClient;
    public bool playedAsHost;
    public ulong steamLobbyId;
    public ulong steamFriendId;

    //(RNG) Seed
    public Random.State currentRunSeed;

    //LastPlayed
    public string lastPlayedDay;
    public string lastPlayedTime;
    public int totalPlaytimeInSeconds;

    //Ending
    public Endings endingDone;

}
