using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ManaManager : MonoBehaviour
{
    [Header("Mana Limit")]
    public int maxManaLimit;//4
    [MinValue(0)]
    public int minManaLimit;//0

    [Header("Animators")]
    public RuntimeAnimatorController Animator1;
    public RuntimeAnimatorController Animator2;
    public RuntimeAnimatorController Animator3;

    //[Header("Sprites")]
    //public Sprite Sprite1;
    //public Sprite Sprite2;
    //public Sprite Sprite3;
    [Header("ManaRing")]
    [Tooltip("Child of warrior, just play it.")]
    public ParticleSystem manaRing;

    public Material material2;
    public Material material3;

    [Header("ReadOnly")]
    [ReadOnly]
    public float angleBetweenChildren;

    [ReadOnly]
    public int currentMana = 0;

    [ReadOnly]
    public MageBehaviour mageBehaviour;

    public static int maximumPlayerMana = 4;

    protected List<GameObject> children = new List<GameObject>();
    protected List<GameObject> activeChildren = new List<GameObject>();

    protected float tempAngle;
    protected int currentChildIndex;

    protected Sprite currentLevelSprite;
    protected RuntimeAnimatorController currentLevelAnimator;

    protected GameObject warrior;

    protected KillMeterManager killMeter;

    // Use this for initialization
    protected virtual void Start()
    {
        currentChildIndex = 0;

        //Register the children
        foreach (Transform pickedChildTransform in transform)
        {
            children.Add(pickedChildTransform.gameObject);

            pickedChildTransform.gameObject.SetActive(false);
        }

        ResetChildren();

        warrior = GameObject.FindGameObjectWithTag("Player");

        mageBehaviour = GameObject.FindGameObjectWithTag("Mage").GetComponent<MageBehaviour>();

        currentLevelAnimator = Animator1;
    }

    public void SetStartingMana(int startingMana)
    {
        for (int i = 0; i < startingMana; i++)
            AddMana();
    }

    public void RemoveMana()
    {
        if (activeChildren.Count == minManaLimit)
            return;

        Debug.Log("Removed mana!");

        currentChildIndex--;

        children[currentChildIndex].SetActive(false);

        activeChildren.Remove(children[currentChildIndex]);

        manaRing.Play();

        ResetChildren();
    }

    public void AddMana()
    {
        if (activeChildren.Count == maxManaLimit)
            return;

        //When fire level and 0 mana, activate dis.
        if (killMeter != null) //&& activeChildren.Count == minManaLimit)
            killMeter.ResetBloodMeter();
            

        children[currentChildIndex].SetActive(true);

        activeChildren.Add(children[currentChildIndex]);

        DetermineChildLevelAnimation();

        manaRing.Play();

        currentChildIndex++;

        ResetChildren();
    }

    protected void ResetChildren()
    {
        ResetChildrenPosition();

        CalculateAngleBetweenChildren();

        SetAngleBetweenChildren();

        //So player can know how much mana he has (cuz child index is private, and currentMana should be read-only from other classes.)
        currentMana = currentChildIndex;
    }

    void CalculateAngleBetweenChildren()
    {
        //A full circle in radians, is 6.3
        angleBetweenChildren = 6.3f / activeChildren.Count;
    }

    void SetAngleBetweenChildren()
    {
        tempAngle = 0f;

        foreach (GameObject pickedChild in activeChildren)
        {
            pickedChild.GetComponent<CircularMotionBehaviour>().angle = tempAngle;
            tempAngle += angleBetweenChildren;
        }
    }

    //In the end, I didn't need this.
    protected virtual void ResetChildrenPosition()
    {
        foreach (GameObject pickedChild in activeChildren)
            pickedChild.transform.position = Vector3.zero;
    }

    /// <summary>
    /// Depending on level, it gives the "orb", different animation (and sprite ofc). The most important is that it "fills" the children's with animator + sprite, since it is doesnt have an animatorController (and sprite)
    /// </summary>
    protected void DetermineChildLevelAnimation()
    {
        if (currentLevelAnimator == null)
            return;

        //Assigns to the child, the animator so it will play the "VFX"
        children[currentChildIndex].transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = currentLevelAnimator;
        children[currentChildIndex].transform.GetChild(0).GetComponent<Animator>().Play(0);

        //Could cache the children's animator HMMMMMMMMMMMMM
    }

    //Runs from WarriorMovement, by storing the player's, so this will never run for enemies.
    public void OnLevelLoad()
    {
        ResetMana();//Has to happen, otherwise same-colored from before stay, lul.

        //Should do the same on VFXManager, oh well.
        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
            currentLevelAnimator = Animator1;
        //currentLevelSprite = Sprite1;
        else if (LevelManager.currentLevel == 2)
            currentLevelAnimator = Animator2;
        else//if (LevelManager.currentLevel > 2)
            currentLevelAnimator = Animator3;

        if (LevelManager.currentLevel == 2 )
            GameObject.FindGameObjectWithTag("PlayerManaRing").GetComponent<ParticleSystemRenderer>().material = material2;
        else if (LevelManager.currentLevel > 2 && LevelManager.currentLevel != 7)
            GameObject.FindGameObjectWithTag("PlayerManaRing").GetComponent<ParticleSystemRenderer>().material = material3;

        if (LevelManager.currentLevel == 2)
            killMeter = GameObject.FindGameObjectWithTag("GameManager").GetComponent<KillMeterManager>();

        //Add mana based on level.
        if (LevelManager.currentLevel == 1)
            AddPlayerMana(2);//Pacifist speedrunners crying over the fact its not 1, as it was og planned. Like, AddPlayerMana(LevelManager.currentLevel); BEGONE!
        else if (LevelManager.currentLevel == 2)
            AddPlayerMana(3);
        else if (LevelManager.currentLevel == 3)
            AddPlayerMana(0);
        else if (LevelManager.currentLevel == 4)
            AddPlayerMana(4);
    }

    //Go back to 0
    public void ResetMana(bool player2Animation = false)
    {
        if (activeChildren.Count == 0)
            return;

        for (int i = 0; i < maxManaLimit; i++)
            RemoveMana();//auto-stops if it realises it cannot take more out.

        if (player2Animation)
            mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Push);//TODO: Change dis to proper animation.
            
    }

    //Called when mana is about to change, and it does any checks/limits here ;)
    //Should not be named PlayerMana, since this could be viable for enemy code too.
    public void AddPlayerMana(int value)
    {
        //if (value == 0 && GameManager.testing == false)
            //Debug.LogError("Lost/Gained 0 mana? Really? plz fix.");
        if (value == 0)//Possible without bugs.
            return;

        //Lose mana
        if (value < 0)
            for (int i = 0; i < value * -1; i++)
                RemoveMana();

        //Gain Mana
        else
            for (int i = 0; i < value; i++)
                AddMana();

    }

    public void ColorChildren(Color targetColor)
    {

    }

    public int GetCurrentChildIndex()
    {
        return currentChildIndex;
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public int GetMaxMana()
    {
        return maxManaLimit;
    }

    public bool GetIsCurrentManaMax()
    {
        if (currentMana == maxManaLimit)
            return true;
        else
            return false;
    }

    public List<GameObject> GetActiveChildren()
    {
        return activeChildren;
    }

    public int SetMaxMana(int newMaxMana)
    {
        if (newMaxMana <= minManaLimit)
            return -1;
        else if (newMaxMana < 0)
            return -1;

        maxManaLimit = newMaxMana;

        return maxManaLimit;
    }
}
