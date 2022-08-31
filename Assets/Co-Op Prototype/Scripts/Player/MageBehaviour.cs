using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Make aether bs its own class, it makes this class highly unreadable.
//TODO: Same for spells. Gtfo them to their own class. -> Spell Costs and variables, should be a purely data "class". Aka, a struct, really.
public class MageBehaviour : MonoBehaviour
{
    //This shall be done on warrior some day as well.
    public static MageBehaviour globalInstance;

    [Required]
    public WarriorMovement warriorBehaviour;
    [Required]
    public WarriorHealth warriorHealthScript;

    [Header("Animators")]
    public RuntimeAnimatorController Animator1;
    public RuntimeAnimatorController Animator2;
    public RuntimeAnimatorController Animator3;

    [Header("Coloring Timing")]
    public float timeToStartTransparency = 0.1f;
    public float timeToEndTransparency = 0.7f;
    private float timeToEndTransparencyDeath = 0.5f;
    private float timeToEndTransparencyAlive;

    [Header("Sorting Layer")]
    public int defaultLayer = 2;
    public int dialogueLayer = -3;


    [Header("Spell SFX")]

    [Tooltip("The SFX heard when player casts a spell but has 0 man")]
    public AudioClip spellCastFail;


    public enum SpellAnimation { Armor, Fire, Idle, Push, Support, Health, Death, Observe, Help, Rewind, Argue1, Argue2, Argue3 };
    //Argue animations muddy the waters above, since it used to be exclusively for spells but whatever, it just works... feelsbadman.

    private List<Spell> spellList = new List<Spell>();

    private RuntimeAnimatorController currentLevelRuntimeAnimator;
    private Animator currentAnimator;
    private SpriteRenderer commonRenderer;
    private Color commonColor;
    private bool runningTransparencyCoroutine = false;
    private bool restartTransparencyCoroutine = false;
    private float LerpRate;

    private RipplePostProcessor cameraRippleBehaviour;

    private LevelDialogueMagePositions commonLevelDialoguePositions;

    private string spellString = null;
    private ManaManager manaManager;
    private bool dialogueGrounded = false;//While on dialogue, stuck on ground.
    //Used as a flag to cache if P2 is attached to wallsliding position or not, so it wont change position every time
    //E.g. if P1 not wallsliding, and P2 is also not wallsliding, dont change local position/scale.
    //private bool mageWallsliding = false;
    //private GameObject vfxHolder;

    // Use this for initialization
    void Start()
    {
        globalInstance = this;

        //Fill the spell list with <Spell>s :D
        GetComponents(spellList);

        currentLevelRuntimeAnimator = Animator1;
        currentAnimator = GetComponent<Animator>();
        currentAnimator.runtimeAnimatorController = currentLevelRuntimeAnimator;

        commonRenderer = GetComponent<SpriteRenderer>();

        commonLevelDialoguePositions = GetComponent<LevelDialogueMagePositions>();

        manaManager = GameObject.Find("ManaManager").GetComponent<ManaManager>();//lazy. Coulda do it better.
        //vfxHolder = transform.GetChild(0).gameObject;//Dont delete this until you fix playerSummonVFX!

        //Transparency while dying, should have a different timer than normal, hence this below.
        timeToEndTransparencyAlive = timeToEndTransparency;
        warriorBehaviour.startedDying += SetDeathTransparency;
        warriorBehaviour.startedRevive += StartReviveDialogue;
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedDeathDialogue += StartFadingTransparency;

        cameraRippleBehaviour = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RipplePostProcessor>();
    }

    //Runs from WarriorMovement, by storing the player's, so this will never run for enemies.
    public void OnLevelLoad()//copypasta from ManaManager, but slightly tweaked.
    {
        //Should do the same on VFXManager, oh well.
        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
            currentLevelRuntimeAnimator = Animator1;
        //currentLevelSprite = Sprite1;
        else if (LevelManager.currentLevel == 2)
            currentLevelRuntimeAnimator = Animator2;
        else if (LevelManager.currentLevel > 2)
            currentLevelRuntimeAnimator = Animator3;

        currentAnimator.runtimeAnimatorController = currentLevelRuntimeAnimator;
    }

    public void CastSpellAnimation(SpellAnimation spell)
    {
        if (runningTransparencyCoroutine == false)
            StartCoroutine(ToTransparency());
        else
            restartTransparencyCoroutine = true;

        //Probably useful here ;)
        DetermineWarriorParenting();

        //VFX for mage spawning/casting.
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MageSummon, transform.position, gameObject);
        //^Shit is bugged. Clips if u look carefully. Happens when you turn around for the first time and summon mage.
        //For example, spam 99 "Push" when looking to the right at start of game. With pauses in between w/e. The moment you turn to the left and cast a "Push" it will break forever.
        //^Not even needed to spam 99 "Push". If you look to the left and cast a spell, its broken instantly.
        //VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.MageSummon, vfxHolder.transform.position, vfxHolder); -> vfxHolder is empty gameobject child of mageRenderer, default transform values.
        //And it didnt work either... wtf.

        //Literally currentAnimator.Play (spell) more or less.
        PlaySpellAnimation(spell);
    }

    public void PlaySpellAnimation(SpellAnimation spell)
    {
        if (spell == SpellAnimation.Armor)
            currentAnimator.Play("Armor", -1, 0f);
        else if (spell == SpellAnimation.Fire)
            currentAnimator.Play("Fire", -1, 0f);
        else if (spell == SpellAnimation.Idle)
            currentAnimator.Play("Idle", -1, 0f);
        else if (spell == SpellAnimation.Push)
            currentAnimator.Play("Push", -1, 0f);
        else if (spell == SpellAnimation.Support)
            currentAnimator.Play("Support", -1, 0f);
        else if (spell == SpellAnimation.Health)
            currentAnimator.Play("Health", -1, 0f);
        else if (spell == SpellAnimation.Observe)
            currentAnimator.Play("Observe", -1, 0f);
        else if (spell == SpellAnimation.Help)
            currentAnimator.Play("Help", -1, 0f);
        else if (spell == SpellAnimation.Rewind)
            currentAnimator.Play("Rewind", -1, 0f);
        else if (spell == SpellAnimation.Argue1)
            currentAnimator.Play("Argue1", -1, 0f);
        else if (spell == SpellAnimation.Argue2)
            currentAnimator.Play("Argue2", -1, 0f);
        else if (spell == SpellAnimation.Argue3)
            currentAnimator.Play("Argue3", -1, 0f);
    }

    /// <summary>
    /// Used by DialogueManager so as I won't have to store spellanimation type of variables.
    /// </summary>
    /// <param name="animationToPlay"></param>
    public void PlaySpellAnimationByString(string animationToPlay)
    {
        currentAnimator.Play(animationToPlay, -1, 0f);
    }

    public IEnumerator ToTransparency()
    {
        //Starting, sets the color to default/white and turns the coroutine flag on
        commonColor = Color.white;
        runningTransparencyCoroutine = true;

        //When spellcast is done, starts transparency
        if (timeToStartTransparency > 0)
            yield return new WaitForSeconds(timeToStartTransparency);

        LerpRate = 0;

        //Cycle here
        while (LerpRate < 1)//commonColor.a > 0)
        {
            if (restartTransparencyCoroutine)//for that moment when transparency/coroutine is running, and player re-casts fast af
            {
                commonColor = Color.white;
                LerpRate = 0;
                restartTransparencyCoroutine = false;

                //When spellcast is done, starts transparency
                if (timeToStartTransparency > 0)
                    yield return new WaitForSeconds(timeToStartTransparency);
            }

            //Decrease the transparency
            LerpRate += Time.deltaTime / timeToEndTransparency;
            commonColor.a = Mathf.Lerp(1, 0, LerpRate);//notice the 1->0 and not the reverse.

            commonRenderer.color = commonColor;

            yield return null;
        }

        runningTransparencyCoroutine = false;
        yield break;

    }

    //For when dialogue/revive happens, this is called, so P2 will appear normally.
    public void SetMaxTransparency()
    {
        commonRenderer.color = Color.white;
    }

    //For when dialogue ends or is skipped, or revive is skipped. 
    public void StartFadingTransparency()
    {
        if (runningTransparencyCoroutine == false)
            StartCoroutine(ToTransparency());
        else
            restartTransparencyCoroutine = true;
    }

    /// <summary>
    /// This function checks if P2 is grounded for dialogue, or parented to warrior. 
    /// If not parented to warrior, he gets attached properly :D
    /// </summary>
    public void DetermineWarriorParenting()
    {
        if (dialogueGrounded == true)
        {
            dialogueGrounded = false;
            ReParentWarrior();
            ResetSpriteRendererLayer();
        }

        //If wallsliding, P2 should rotate and move slightly.
        if (warriorBehaviour.GetIsWallsliding() == false)
        {
            transform.localPosition = new Vector3(-1, 2);
            transform.localScale = new Vector3(1.4f, 1.4f, 1);
        }
        else
        {
            transform.localPosition = new Vector3(0, 2);
            transform.localScale = new Vector3(-1.4f, 1.4f, 1);
        }
    }

    //If cannot be cast, spellString is simply, null.
    public void DetermineSpell(string _spellString)
    {
        if (_spellString == null || _spellString == "")
            return;

        if (warriorBehaviour.AreSpellsDisabledState())
            return;

        spellString = _spellString;

        //If talking/dialoguing, he wont even appear LUL
        DetermineWarriorParenting();

        //Spell Check below
            //Iterating all <Spells>s
            for (int i = 0; i < spellList.Count; i++)
            {
                //If its the spell we are looking for
                //aka the spell we typed
                if (spellString == spellList[i].spellName)//could have 3 if brackets in one, but hard to read.
                {
                    //If we have enough mana to cast it!
                    if (manaManager.GetCurrentMana() - spellList[i].manaCost > -1)
                        spellList[i].Cast();
                    else if (DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords)//Cheating ;)
                        spellList[i].Cast();
                    else if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)//Level Editor Paused
                        spellList[i].Cast();
                    else//Failed to cast spell
                    {
                        //Failed spell SFX
                        PlayerSoundManager.globalInstance.PlayClip(spellCastFail, PlayerSoundManager.AudioSourceName.SpellCast1);

                        //"Red" VFX aka, failed to cast.
                        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.SpellFail, transform.position);
                    }
                }

            }

        ResetSpellString();
    }

    public void ResetSpellString()
    {
        spellString = null;
    }

    public bool IsCurrentManaMax()
    {
        return manaManager.GetIsCurrentManaMax();
    }

    public void StartReviveDialogue()
    {
        //Have "Health" animation, as in, rising him "out" ;)
        PlaySpellAnimation(SpellAnimation.Health);

        //Move P2 to a dialogue distance, cuz otherwise it looks awkward
        DialogueSetRevivePosition();

        if (dialogueGrounded == false)
            //So as skipping dialogue won't make P1 move with P2 while on the ground.
            //With SetParent(false), it moves the transform but keeps the localPosition the same
            UnParentWarrior();

        //We are at the local position from warrior thus far, hence we add warrior's (local) position now!
        transform.localPosition += warriorBehaviour.checkpointPosition;

        //Get timeToEndTransparency for normal gameplay.
        UnSetDeathTransparency();

        //Literally everything important.
        StartStaticDialogue();

        //Set proper facing
        DialogueSetReviveFacing();
        //Set proper position far away (instead of rightwards which ruins facing too)
        if (LevelManager.currentLevel > 2)
            transform.localPosition = transform.localPosition + new Vector3(-17f, 0f, 0f);
    }

    public void StartLevelCutsceneDialogue()
    {
        //Have "Push" animation by default (cuz otherwise, P2 wont have an animation till he speaks once)
        PlaySpellAnimation(SpellAnimation.Push);

        if (dialogueGrounded == false)
            //So as skipping dialogue won't make P1 move with P2 while on the ground.
            //With SetParent(false), it moves the transform but keeps the localPosition the same
            UnParentWarrior();

        //Move P2 to a dialogue distance, cuz otherwise it looks awkward
        DialogueSetLevelPosition();

        //Important stuff that should happen in every dialogue.
        StartStaticDialogue();

    }

    public void StartStaticDialogue()
    {
        //Put P2's layer to -3, so he will literally be inside the earth, instead of in front of it ;)
        PushBackSpriteRendererLayer();

        //Knowing this helps a LOT (srsly, just see the references)
        dialogueGrounded = true;

        //So he will be visible for sure.
        SetMaxTransparency();

        DetermineFlip();
    }

    //If not facing warrior, flip!
    public void DetermineFlip()
    {
        //Race condition bug, when loaded from a level frame 1. Warrior behaviour's is previous level's
        //Debug.Log("Local scale: " + transform.localScale.x + " and transform position.x: " + transform.position.x + " and warrior positionx: " + warriorBehaviour.transform.position.x);

        //If facing right and player is to the left, face left
        if (transform.localScale.x > 0 && transform.position.x > warriorBehaviour.transform.position.x)
            transform.localScale = new Vector3(-1.4f, 1.4f, 1);
            //If facing left and player is to the right, face right
        else if (transform.localScale.x < 0 && transform.position.x < warriorBehaviour.transform.position.x)
            transform.localScale = new Vector3(1.4f, 1.4f, 1);
    }

    //When P2 is on the ground, warrior moving will make P2 move as well, which will look horrible.
    public void UnParentWarrior()
    {
        transform.SetParent(transform.parent.parent.parent, false);
    }

    //For when dialogue ends or is skipped (same with revive), happens on spellcast.
    public void ReParentWarrior()
    {
        transform.SetParent(warriorBehaviour.transform.GetChild(0).transform);
    }

    /// <summary>
    /// For when P2 must be moved for dialogue, cuz dialogue can't happen while in 『S T A N D O』
    /// Don't expand this. Please don't abuse this function, its perfect.
    /// </summary>
    public void DialogueSetRevivePosition()
    {
        transform.localPosition = new Vector3(5, -1.35f);
    }

    public void DialogueSetReviveFacing()
    {
        if (LevelManager.currentLevel < 3)
            transform.localScale = new Vector3(-1.4f, 1.4f, 1);
        else
            transform.localScale = new Vector3(1.4f, 1.4f, 1);
    }

    /// <summary>
    /// For when P2 must be moved for dialogue, cuz dialogue can't happen while in 『S T A N D O』
    /// Don't expand this. Please don't abuse this function, its perfect.
    /// </summary>
    public void DialogueSetLevelPosition()
    {
        //Opposite X of warrior, so it always faces him.
        if (warriorBehaviour.transform.localScale.x > 0)
            transform.localScale = new Vector3(-1.4f, 1.4f, 1);
        else
            transform.localScale = new Vector3(1.4f, 1.4f, 1);

        if (LevelManager.currentLevel == 0)
        {
            if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)//Doesn't work cuz skipped lol
                transform.position = commonLevelDialoguePositions.tutorialStart;
            else
            {
                //Down
                if (warriorBehaviour.transform.position.y < 148)
                    transform.position = commonLevelDialoguePositions.tutorialEndDown;
                else//Up
                    transform.position = commonLevelDialoguePositions.tutorialEndUp;
            }
        }
        else if (LevelManager.currentLevel == 1)
        {
            if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)
                transform.position = commonLevelDialoguePositions.level1Start;
            else
                transform.position = commonLevelDialoguePositions.level1End;
        }
        else if (LevelManager.currentLevel == 2)
        {
            if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)
                transform.position = commonLevelDialoguePositions.level2Start;
            else
                transform.position = commonLevelDialoguePositions.level2End;
        }
        else if (LevelManager.currentLevel == 3)
        {
            if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)
                transform.position = commonLevelDialoguePositions.level3Start;
            else
            {
                if (warriorBehaviour.transform.localScale.x > 0)
                    transform.position = commonLevelDialoguePositions.level3EndRight;
                else
                    transform.position = commonLevelDialoguePositions.level3EndLeft;
            }
                
        }
        else if (LevelManager.currentLevel == 4)
        {
            if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)
                transform.position = commonLevelDialoguePositions.level4Start;
            else
                transform.position = commonLevelDialoguePositions.level4End;
        }

    }




    /// <summary>
    /// Get sprite renderer's layer back to playable -3, so he will look like being inside the earth, instead of in front it ;)
    /// </summary>
    public void PushBackSpriteRendererLayer()
    {
        commonRenderer.sortingOrder = dialogueLayer;
    }

    /// <summary>
    /// After dialogue is done, the renderer's layer should be back to normal!
    /// Put P2's layer back to 2, instead of -3, so he won't be overshadowed by so many VFX and other visuals while playing.
    /// Used when grounded == true, instead of finished dialogue, otherwise the transparencyfading will look bad since he suddenly clips in front of ground.
    /// </summary>
    public void ResetSpriteRendererLayer()
    {
        commonRenderer.sortingOrder = defaultLayer;
    }

    //The below 2 are triggered via events.
    public void SetDeathTransparency()
    {
        timeToEndTransparency = timeToEndTransparencyDeath;
    }

    //Resets timetoendtransparency, since death changes it to a lot shorter.
    //tl;dr: gets timeToEndTransparency, back to normal, since death changes it while dying.
    public void UnSetDeathTransparency()
    {
        timeToEndTransparency = timeToEndTransparencyAlive;
    }

    public void FireballKill(int manaToReward)
    {
        warriorBehaviour.RangedKillReward(manaToReward);
    }

    public LevelDialogueMagePositions GetLevelDialoguePositions()
    {
        return commonLevelDialoguePositions;
    }

}
