using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WarriorAnimationManager : MonoBehaviour
{
    [Header("The actual animation container used in-game")]
    public AnimationContainer NormalMode;
    
    /*
    [InfoBox("To not confuse you, the below containers are useless. They were planned to be used, but are now scrapped. Kind of a shame when the data structure was finished but it's for the best.")]

    [Header("Colorless/\"Manga\"")]
    public bool colorlessMode = false;
    [Tooltip("Literally just one set instead of 3, colorless, helps for testing, and if possible a future mode to make everything black&white")]
    public AnimationContainer MangaMode;

    [Header("Earth Level")]
    [Tooltip("1st level, the ??? ????")]
    public AnimationContainer EarthLevel;

    [Header("Fire Level")]
    [Tooltip("2nd level, The Crimson Flames")]
    public AnimationContainer FireLevel;

    [Header("Wind Level")]
    [Tooltip("Final Level/Winds of Oblivion")]
    public AnimationContainer WindLevel;
    */

    public AnimationContainer LevelUpdateAnimations()
    {
        

        return NormalMode;
        /*
        int level = LevelManager.currentLevel;

        if (colorlessMode)
            return MangaMode;
        if (level == 0 || level == 1)
            return EarthLevel;
        else if (level == 2)
            return FireLevel;
        else if (level > 2)
            return WindLevel;
        
        Debug.LogError("Negative level detected in animationManager(Container)");
        return null;
        */
    }
}
