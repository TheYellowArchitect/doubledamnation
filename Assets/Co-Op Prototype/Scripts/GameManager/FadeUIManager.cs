using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeUIManager : MonoBehaviour
{
    //Animator starts fully faded!
    public Animator fadeAnimator;

    [Tooltip("How much time before the FadeOut happens, so as to extend the \"FadeIn\" filter.")]
    public float levelFadeOutDelay = 1f;

    //public float deathFadeDuration = 2f;

    

    private void Start()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedDying += DeathFadeIn;

        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedRevive += ReviveFadeOut;
    }

    /// <summary>
    /// When a level is loaded, this is called (screen is black), so, it fades out. Uses levelFadeDuration.
    /// </summary>
    public void LevelFade()
    {
        //if (LevelManager.currentLevel == 0)
            //StartCoroutine(LevelFadeOutCoroutine(0));
       // else
            StartCoroutine(LevelFadeOutCoroutine(levelFadeOutDelay));

    }


    public IEnumerator LevelFadeOutCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        LevelFadeOut();
    }

    public void LevelFadeIn()
    {
        //Fade In screen filter
        fadeAnimator.Play("LevelFadeIn");
    }

    public void LevelFadeOut()
    {
        //Fade Out screen filter.
        fadeAnimator.Play("LevelFadeOut");
    }

    //If boat level, and alternative ending, have gray fadeout
    public void LevelFadeOutGray()
    {
        fadeAnimator.Play("LevelFadeOutGray");
    }

    public void DeathFadeIn()
    {
        //Fade In screen filter
        fadeAnimator.Play("DeathFadeIn");
    }

    public void ReviveFadeOut()
    {
        //Fade Out screen filter.
        fadeAnimator.Play("DeathFadeOut");
    }

    public void FinalBattlePlayer1VictoryFadeIn()
    {
        //Fade In Screen filter.
        fadeAnimator.Play("FinalBattlePlayer1VictoryFadeIn");
    }

    public void FinalBattlePlayer1VictoryFadeWhiteToBlack()
    {
        //Fade Out Screen filter.
        fadeAnimator.Play("FinalBattlePlayer1VictoryFadeWhiteToBlack");
    }

    public void FinalBattlePlayer2VictoryFadeIn()
    {
        //Fade In Screen filter.
        fadeAnimator.Play("FinalBattlePlayer2VictoryFadeIn");
    }

    public void FinalBattlePlayer2VictoryFadeOut()
    {
        //Fade Out Screen filter.
        fadeAnimator.Play("FinalBattlePlayer2VictoryFadeOut");
    }

    public void FinalBattlePlayer2VictoryFadeGrayToBlack()
    {
        fadeAnimator.Play("FinalBattlePlayer2VictoryFadeGrayToBlack");
    }


    public void FadeBlack()
    {
        fadeAnimator.Play("FullyFadedBlack");
    }

    public void FadeWhite()
    {
        fadeAnimator.Play("FullyFadedWhite");
    }

    public void FadeGray()
    {
        fadeAnimator.Play("FullyFadedGray");
    }



    //Goes from 0 to gray, instead of 0 to blacc
    public void FadeInGray()
    {
        fadeAnimator.Play("FadeInGray");
    }
    
    //Goes from 0 to gray, instead of 0 to blacc
    public void FadeInWhite()
    {
        fadeAnimator.Play("FadeInWhite");
    }

}
