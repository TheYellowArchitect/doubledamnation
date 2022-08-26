using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used by WarriorAnimationManager script and WarriorManager, contains 3 overrideAnimatorControllers, to change animators/clips at runtime.
/// </summary>
[CreateAssetMenu(fileName = "New Animation Container", menuName = "AnimationContainer", order = 3)]
public class AnimationContainer : ScriptableObject
{
    [Header("Level1")]
    [Tooltip("By default activated/set on the player already")]
    public AnimatorOverrideController Level1Animator;

    [Header("Level2")]
    public AnimatorOverrideController Level2Animator;

    [Header("Level3")]
    public AnimatorOverrideController Level3Animator;

    /* Old animator, changing based on hp
    [Header("0 Damage Taken")]
    public AnimatorOverrideController HealthyAnimator;

    [Header("1 Damage Taken")]
    public AnimatorOverrideController DamagedAnimator;

    [Header("2 Damage Taken")]
    [Tooltip("Not dying, just 1 hit before death.")]
    public AnimatorOverrideController DyingAnimator;
    */

    /// <summary>
    /// By putting the health, it returns what Animator to use. In other words, health-animator relation.
    /// </summary>
    /// <param name="health"></param>
    /// <returns></returns>
    public AnimatorOverrideController GetAnimator()
    {
        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
            return Level1Animator;
        else if (LevelManager.currentLevel == 2)
            return Level2Animator;
        else if (LevelManager.currentLevel > 2)
            return Level3Animator;

        return null;
        /* Old health animator
        //Debug.Log("health: " + health);

        //Should be 3, but muh debugging.
        if (health > 2)
            return HealthyAnimator;
        else if (health == 2)
            return DamagedAnimator;
        else if (health < 2)
            return DyingAnimator;

        return null;
        */
    }
}
    /*
    [Tooltip("Activate this, if you want for the health to not matter. In other words, fill only 1 set of animations, instead of 3.")]
    public bool uniqueSet;

    [Header("Unique Animations")]
    public AnimationClip death;
    public AnimationClip deathConsume;//currently unused/inaccessible. Called by dialogueSystem
    public AnimationClip revive;

    [Header("0 Damage Taken")]

    public AnimationClip idle0;
    public AnimationClip run0;
    public AnimationClip jump0;
    public AnimationClip floating0;
    public AnimationClip wallslide0;
    public AnimationClip dodgeroll0;
    public AnimationClip wavedash0;
    public AnimationClip hitground0;
    public AnimationClip hitmidair0;
    

    public AnimationClip AMidairDiagonalDown0;
    public AnimationClip AMidairDiagonalUp0;
    public AnimationClip AMidairHorizontal0;
    public AnimationClip AMidairVerticalUp0;
    public AnimationClip AMidairVerticalDown0;

    public AnimationClip AGroundDiagonalDown0;
    public AnimationClip AGroundDiagonalUp0;
    public AnimationClip AGroundHorizontal0;
    public AnimationClip AGroundVertical0;

    public AnimationClip ADashDiagonalDown0;
    public AnimationClip ADashDiagonalUp0;
    public AnimationClip ADashHorizontal0;
    public AnimationClip ADashVeretical0;

    [Header("1 Damage Taken")]

    public AnimationClip idle1;
    public AnimationClip run1;
    public AnimationClip jump1;
    public AnimationClip floating1;
    public AnimationClip wallslide1;
    public AnimationClip dodgeroll1;
    public AnimationClip wavedash1;
    public AnimationClip hitground1;
    public AnimationClip hitmidair1;


    public AnimationClip AMidairDiagonalDown1;
    public AnimationClip AMidairDiagonalUp1;
    public AnimationClip AMidairHorizontal1;
    public AnimationClip AMidairVerticalUp1;
    public AnimationClip AMidairVerticalDown1;

    public AnimationClip AGroundDiagonalDown1;
    public AnimationClip AGroundDiagonalUp1;
    public AnimationClip AGroundHorizontal1;
    public AnimationClip AGroundVertical1;

    public AnimationClip ADashDiagonalDown1;
    public AnimationClip ADashDiagonalUp1;
    public AnimationClip ADashHorizontal1;
    public AnimationClip ADashVeretical1;

    [Header("2 Damage Taken")]

    public AnimationClip idle2;
    public AnimationClip run2;
    public AnimationClip jump2;
    public AnimationClip floating2;
    public AnimationClip wallslide2;
    public AnimationClip dodgeroll2;
    public AnimationClip wavedash2;
    public AnimationClip hitground2;
    public AnimationClip hitmidair2;


    public AnimationClip AMidairDiagonalDown2;
    public AnimationClip AMidairDiagonalUp2;
    public AnimationClip AMidairHorizontal2;
    public AnimationClip AMidairVerticalUp2;
    public AnimationClip AMidairVerticalDown2;

    public AnimationClip AGroundDiagonalDown2;
    public AnimationClip AGroundDiagonalUp2;
    public AnimationClip AGroundHorizontal2;
    public AnimationClip AGroundVertical2;

    public AnimationClip ADashDiagonalDown2;
    public AnimationClip ADashDiagonalUp2;
    public AnimationClip ADashHorizontal2;
    public AnimationClip ADashVeretical2;
    */
