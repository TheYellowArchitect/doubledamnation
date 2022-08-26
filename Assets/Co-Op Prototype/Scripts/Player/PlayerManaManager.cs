using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Currently only for coloring, but eventually mana manager of monsters and player should be split.
/// Like, 3 classes in total. A playermanamanager, a monstermanamanager, and a manamanager that contains most functions.
/// A lot of stuff are bloat to be on monster, like, materials, killmeter, etc etc...
/// The seperation isnt done btw, this is merely a wrapper for now.
/// </summary>
public class PlayerManaManager : MonoBehaviour
{
    [Tooltip("Total time it takes for coloring to end")]
    public float coloringExpirationTime = 1f;
    [Tooltip("How fast should the color fade to original color")]
    public float coloringTimerDuration = 0.1f;

    private ManaManager commonManaManager;

    private Animator commonAnimator;//animatorController

    //Colors
    //======

    private List<Color> refreshManaColorPerLevel = new List<Color>();

    private Color targetColor;

    private float coloringCurrentDuration;

    private TimerManager timerManager;

    Timer playerManaColorOnKillTimer;

    //====

    private List<GameObject> directChildrenGameObjects;
    int amountOfOrbsActive;

    // Use this for initialization
    void Start ()
    {
        commonManaManager = GetComponent<ManaManager>();
        commonAnimator = GetComponent<Animator>();

        refreshManaColorPerLevel.Add(new Color(0, 1, 1, 1));
        refreshManaColorPerLevel.Add(new Color(0, 0, 0, 1));
        refreshManaColorPerLevel.Add(new Color(0.5f, 0.5f, 0.5f, 1));
        //Yes, I also like changing them via code... fml.

        timerManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TimerManager>();
        playerManaColorOnKillTimer = timerManager.CreateTimer(playerManaColorOnKillTimer, 15, coloringTimerDuration, false, true);
        playerManaColorOnKillTimer.TriggerOnEnd += FadeOrbColor;
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    /// <param name="Update"></param>
    /// <param name=""></param>
    public void AddPlayerMana(int manaToAdd)
    {
        commonManaManager.AddPlayerMana(manaToAdd);
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    public void RemoveMana()
    {
        commonManaManager.RemoveMana();
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    public void AddMana()
    {
        commonManaManager.AddMana();
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    public int GetCurrentMana()
    {
        return commonManaManager.GetCurrentMana();
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    public int GetMaxMana()
    {
        return commonManaManager.GetMaxMana();
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    public int SetMaxMana(int newMaxMana)
    {
        return commonManaManager.SetMaxMana(newMaxMana);
    }

    public void SetPlayerCurrentMana(byte finalMana)
    {
        SetPlayerCurrentMana((int)finalMana);
    }

    public void SetPlayerCurrentMana(int finalMana)
    {
        if (finalMana > GetCurrentMana())
        {
            Debug.Log("Final mana is: " + finalMana);

            for (int i = GetCurrentMana(); i < finalMana; i++)
                AddMana();
        }
        else if (finalMana < GetCurrentMana())
        {
            for (int i = GetCurrentMana(); i > finalMana; i--)
                RemoveMana();
        }
        //else do nothing aka they are the same mana
        
    }

    /// <summary>
    /// This is a wrapper, which calls ManaManager's
    /// </summary>
    /// <param name="level"></param>
    public void OnLevelLoad()
    {
        commonManaManager.OnLevelLoad();

        //Set targetColor depending on level
        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
            targetColor = refreshManaColorPerLevel[0];
        else if (LevelManager.currentLevel == 2)
            targetColor = refreshManaColorPerLevel[1];
        else if (LevelManager.currentLevel > 2)
            targetColor = refreshManaColorPerLevel[2];
    }

    public void AnimateOrbs()
    {
        directChildrenGameObjects = commonManaManager.GetActiveChildren();

        for (int i = 0; i < directChildrenGameObjects.Count; i++)
        {
            directChildrenGameObjects[i].transform.GetChild(0).gameObject.SetActive(false);

            directChildrenGameObjects[i].transform.GetChild(0).gameObject.SetActive(true);

            //directChildrenGameObjects[i].transform.GetChild(0).GetComponent<Animator>().Play("Start");//Legit no idea why it didnt work properly reeeee!

            //Debug.Log("i is" + i);
        }
            
    }

    /// <summary>
    /// Works but deprecated. Was supposed to be in place of animation orbs, but i found out animations exist and decided to utilise them fully since I made them!
    /// </summary>
    public void ColorOrbs()
    {
        amountOfOrbsActive = commonManaManager.GetCurrentChildIndex();
        directChildrenGameObjects = commonManaManager.GetActiveChildren();

        for (int i = 0; i < amountOfOrbsActive; i++)
            directChildrenGameObjects[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = targetColor;

        playerManaColorOnKillTimer.Activate();

        coloringCurrentDuration = 0;
    }

    /// <summary>
    /// Called by timer (around every 0.1f)
    /// </summary>
    public void FadeOrbColor()
    {
        amountOfOrbsActive = commonManaManager.GetCurrentChildIndex();
        directChildrenGameObjects = commonManaManager.GetActiveChildren();

        //Keep track of how much time passed since it ran originally.
        coloringCurrentDuration = coloringCurrentDuration + coloringTimerDuration;

        if (coloringCurrentDuration > coloringExpirationTime)
        {
            //Stop the timer
            playerManaColorOnKillTimer.Reset();

            //Reset the color
            for (int i = 0; i < amountOfOrbsActive; i++)
                directChildrenGameObjects[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;

            Debug.Log("Reset it!");
        }
        else
        {
            Debug.Log("Lerp time!");

            //Set a between (expiration time is 1 second so no worries)
            for (int i = 0; i < amountOfOrbsActive; i++)
                directChildrenGameObjects[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.Lerp(targetColor, Color.white, coloringCurrentDuration);

            //Without this it won't come here again lul.
            playerManaColorOnKillTimer.Activate();
        }
    }

    //Go back to 0
    public void ResetMana()
    {
        commonManaManager.ResetMana();
    }
}
