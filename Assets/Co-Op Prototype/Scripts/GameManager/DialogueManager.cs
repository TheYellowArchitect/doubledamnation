using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

//TODO: Go implement "No Subtitles" Then, if "no subtitles" + 0 voice audio, skip the chat entirely (to-be-used on speedrunning so make it good)
[System.Serializable]
public class DialogueManager : MonoBehaviour
{
    public DialogueContainer DeathDialogues;
    public DialogueContainer PowerUpDialogues;
    public DialogueContainer MidCutsceneDialogues;
    public DialogueContainer LevelCutsceneDialogues;
    public DialogueContainer InterruptionByWarriorDialogues;
    public DialogueContainer InterruptionByMageDialogues;

    public bool skipDeathDialogue = false;
    public bool skipPowerUpDialogue = false;
    public bool skipMidCutsceneDialogue = false;
    public bool skipLevelCutsceneDialogue = false;
    public bool skipInterruptDialogue = false;
  

    //Aka how many sentences a dialogue holds. A temp variable, so as to loop
    private int sentencesCount;

    [Header("References")]
    //Takes it by reference from mage, so as to notify for animations to play.
    private MageBehaviour mageScript;

    //Takes it by reference from warrior, so as to know when to stop the MidCutscene dialogues.
    private WarriorMovement warriorScript;

    public Action finishedDeathDialogue;
    public Action finishedMidCutsceneDialogue;
    public Action finishedLevelCutscene;
    public Action finishedInterruptionDialogue;

    public Action startedLevelCutsceneEvent;
    

    //Gets reference/stores the Dialogue text UI element.
    private TextMeshProUGUI dialogueTextUI;

    //Gets the time to next transition
    private float deathTeleportDuration;

    // Use this for initialization
    void Start()
    {
        //Store/Reference the UI so it won't be fetched more than once.
        dialogueTextUI = GameObject.FindGameObjectWithTag("Dialogue").GetComponent<TextMeshProUGUI>();

        //Cache the mageScript, for playing those dialogue animations where needed.
        mageScript = GameObject.FindGameObjectWithTag("Mage").GetComponent<MageBehaviour>();

        //Cache the warriorScript. Pretty useless, except I need that "dying" bool value.
        warriorScript = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

        //Subscribe to when player finishes revive
        warriorScript.finishedRevive += PlayerRevived;

        //Subscribe to when player powers up
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().PoweredUpEvent += PlayerPoweredUp;

        //Subscribe to when player skips a level cutscene, so as the game wont bug when the player skips it.
        warriorScript.skippedDialogue += PlayersInterruptedDialogue;

        //Get the timing to next level.
        deathTeleportDuration = warriorScript.deathTeleportDuration;

        //======= Fetching the dialogue containers depending on language =========
        //For now, language is english-only so the below is hacky. At next patch where u make the Main Menu (ala Xenoblade Chronicles1), asking for language before that ftw!
        //  ^A menu also fits a level editor, and that "desert" update ;)
        //^I simply dragged them to the field above FOR NOW. When many languages, you must somehow assign only 3, while not holding the data/CPU of the other language containers!
        //Remember, you cannot use AssetDatabase, so there are 2 choices:
        //1)Resources Folder
        //2) Some class wrapper that takes from the start all the containers (containers auto-assigned via toolMenuScript) and filters them, and puts only the language-needed here.
        //Actually go for 2) because more modular and the code here will be the same.

        //========================================================================
    }

    //Cuz retarded and dont know how to get an action/event to trigger an start Coroutine, so I have to make a wrapper smh.
    public void PlayerRevived()
    {
        StartCoroutine(PlayDeathDialogue());
    }

    public void PlayerPoweredUp()
    {
        StartCoroutine(PlayPowerUpDialogue());
    }

    //Triggered by "TriggerMidCutscene.cs" which also provides it the dialogueIndex to be used.
    public void PlayerEnteredMidCutscene(int dialogueIndex)
    {
        StartCoroutine(PlayMidCutsceneDialogue(dialogueIndex));
    }

    public void PlayerEnteredLevelCutscene()//ignore animation lel.
    {
        StartCoroutine(PlayLevelCutsceneDialogue());
    }

    //Automatically triggered via event/action from warrior script, when players skip dialogue (state)!
    public void PlayersInterruptedDialogue()
    {
        StartCoroutine(PlayInterruptDialogue());
    }

    private IEnumerator PlayDeathDialogue()
    {
        //If other voices are playing, await them to end, then play this.
        while (VoiceSoundManager.globalInstance.IsPlayingMidCutscene() || VoiceSoundManager.globalInstance.IsPlayingPowerUp() || VoiceSoundManager.globalInstance.IsPlayingInterruption())
        {
            //TODO: When death happens, get both players to stfu, aka stop properly (not just voice but all flags and shit), so this won't even be needed.

            //Normally, you would put an action/event so as to inform this loop, and start 2 seconds later, instead of yolo-looping every 2 seconds
            //because with the latter (lazy) way, dialogue may end and instantly start which will sound very weird.

            yield return new WaitForSeconds(2);
        }

        //Aka how many sentences a dialogue holds.
        Debug.Log("The total deaths thus far is: " + PlayerStatsManager.globalInstance.GetTotalDeaths());
        //If Insanity
        if (PlayerStatsManager.globalInstance.GetTotalDeaths() > 74)
            PlayerStatsManager.globalInstance.SetTotalDeaths(75);
        if (PlayerStatsManager.globalInstance.GetTotalDeaths() > 74)
            skipDeathDialogue = false;//Shuts down speedrun.
        sentencesCount = DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside.Count;

        //For each sentence, we do a loop
        for (int i = 0; i < sentencesCount; i++)
        {

            //Below, it makes the midDialogue Skippable
            //GameManager.testing for sure for this, since even at release, pressing space to skip is rip cuz it interrupts gameplay by jumping.
            if (skipDeathDialogue)
            {
                //TODO: Make the inside of this {} a function, since every dialogue is skippable.
                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                VoiceSoundManager.globalInstance.StopVoice();

                //Notify warriorMovement, warriorHealth, and PauseMenu.
                if (finishedDeathDialogue != null)
                    finishedDeathDialogue.Invoke();

                Debug.Log("Skipping works");

                yield break;
            }

            //Check for null voice clip
            if (DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside[i].voiceClip == null)
            {
                Debug.LogError("The Death clip in dialogue " + (PlayerStatsManager.globalInstance.GetTotalDeaths() - 1) + ", in sentence " + i + " is null.");

                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                //Notify warriorMovement, warriorHealth, and PauseMenu.
                if (finishedDeathDialogue != null)
                    finishedDeathDialogue.Invoke();

                yield break;
            }

            //Display the dialogue text on UI, colors and all.
            DisplayDialogue(DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside[i]);

            //Play the voice clip that fits the dialogue above.
            VoiceSoundManager.globalInstance.PlayVoice(dialogueTextUI.color.r, DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside[i].voiceClip);

            //Play the animation to fit the dialogue!
            PlayDialogueAnimation(dialogueTextUI.color.r, DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside[i].warriorAnimation, DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside[i].mageAnimation);


            yield return new WaitForSeconds(DeathDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalDeaths() - 1].sentencesInside[i].voiceClip.length);

            //If interrupted, don't bother continuing. (also the resets/confirms that deathdialogue finishing does, are done via the interruption detection)
            if (VoiceSoundManager.globalInstance.IsPlayingInterruption())
                yield break;
        }

        dialogueTextUI.text = "";

        VoiceSoundManager.globalInstance.ResetSpeakingFlags();


        //Notify warriorMovement, warriorHealth, and PauseMenu.
        if (finishedDeathDialogue != null)
            finishedDeathDialogue.Invoke();

        //Insanity Ending
        if (PlayerStatsManager.globalInstance.GetTotalDeaths() > 74)
        {
            GameObject.FindGameObjectWithTag("Player").AddComponent<WarriorPrefixedInput>();
            GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorPrefixedInput>().triggerInsanityEndingOnEnd = true;
        }


    }

    private IEnumerator PlayPowerUpDialogue()
    {
        //When a cutscene is playing, its more important than powerup, so it doesn't interrupt it. Awaiting is out of the question since the moment will have likely passed.
        //However, it won't do anything like increase the powerUp index.
        if (VoiceSoundManager.globalInstance.IsPlayingMidCutscene() || VoiceSoundManager.globalInstance.IsPlayingPowerUp())
            yield break;

        //If no more powerups.
        if (PlayerStatsManager.globalInstance.GetTotalPowerUps() > 100)
            yield break;

        //Set the flag that it is playing a cutscene
        VoiceSoundManager.globalInstance.SetPowerUp(true);

        //======= This part could be function'd since shared in all enumerators ===============

        //sentencesCount = the number of sentences inside the dialogue to be used.
        sentencesCount = PowerUpDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalPowerUps()].sentencesInside.Count;

        //For each sentence, we do a loop
        for (int i = 0; i < sentencesCount; i++)
        {
            //Below, it makes the powerUpDialogue Skippable
            if (skipPowerUpDialogue)
            {
                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                PlayerStatsManager.globalInstance.IncreasePowerUpCount();

                Debug.Log("Skipping works");

                yield break;
            }

            //Check for null voice clip
            if (PowerUpDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalPowerUps()].sentencesInside[i].voiceClip == null)
            {
                Debug.LogError("The PowerUp clip in dialogue " + PlayerStatsManager.globalInstance.GetTotalPowerUps() + ", in sentence " + i + " is null.");

                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                PlayerStatsManager.globalInstance.IncreasePowerUpCount();

                yield break;
            }


            DisplayDialogue(PowerUpDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalPowerUps()].sentencesInside[i]);

            VoiceSoundManager.globalInstance.PlayVoice(dialogueTextUI.color.r, PowerUpDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalPowerUps()].sentencesInside[i].voiceClip);

            yield return new WaitForSeconds(PowerUpDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetTotalPowerUps()].sentencesInside[i].voiceClip.length);

        }

        dialogueTextUI.text = "";

        //Resets pretty much every flag of VoiceSoundManager
        VoiceSoundManager.globalInstance.ResetSpeakingFlags();

        //======= This part could be function'd since shared in all enumerators ===============

        PlayerStatsManager.globalInstance.IncreasePowerUpCount();
    }

    //Reminder, this is an IEnumerator, dont simply call it by other classes. Call the EnteredCutscene!!!!
    private IEnumerator PlayMidCutsceneDialogue(int dialogueIndex)
    {
        if (dialogueIndex - 1 < 0)
        {
            Debug.LogError("About to play midCutscene, but the dialogue index is less than 0 wth?!");
            yield break;
        }

        //So as to not start from 0 in the MidCutsceneTriggers. (because saying start from dialogue 1, that dialogue 1 is stored in the array index of 0, so with this, trigger indexes match dialogue indexes!
        dialogueIndex = dialogueIndex - 1;

        //If other voices are playing, await them to end, then play this.
        while (VoiceSoundManager.globalInstance.IsPlayingPowerUp() || VoiceSoundManager.globalInstance.IsPlayingInterruption())
        {
            //Normally, you would put an action/event so as to inform this loop, and start 12 seconds later, instead of yolo-looping every 12 seconds
            //because with the latter (lazy) way, dialogue may end and instantly start which will sound very weird.

            yield return new WaitForSeconds(12f);
        }

        //If other voices are playing, await them to end, then play this.
        while (VoiceSoundManager.globalInstance.IsPlayingMidCutscene())
        {
            //Normally, you would put an action/event so as to inform this loop, and start 12 seconds later, instead of yolo-looping every 12 seconds
            //because with the latter (lazy) way, dialogue may end and instantly start which will sound very weird.

            yield return new WaitForSeconds(3f);
            //Above copypasted, it just works stfu, dont question my rank in spagghetticraftmanship
        }

        //Set the flag that it is playing a cutscene
        VoiceSoundManager.globalInstance.SetMidCutscene(true);

        //sentencesCount = the number of sentences inside the dialogue to be used.
        sentencesCount = MidCutsceneDialogues.totalDialogues[dialogueIndex].sentencesInside.Count;

        //For each sentence, we do a loop
        for (int i = 0; i < sentencesCount; i++)
        {
            //Below, it makes the midDialogue Skippable
            if (skipMidCutsceneDialogue || (Input.GetKey(KeyCode.Space) && GameManager.testing))
            {
                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                VoiceSoundManager.globalInstance.StopVoice();

                if (finishedMidCutsceneDialogue != null)
                    finishedMidCutsceneDialogue.Invoke();

                Debug.Log("Skipping midCutscenedialogue");

                //Skip intro.
                if (LevelManager.currentLevel == -1)
                {
                    //If F12/Speedrun'd
                    if (skipMidCutsceneDialogue)
                        GetComponent<SpeedrunManager>().ToggleIntroSpeedrunMode();
                    else
                        GameObject.FindGameObjectWithTag("IntroManager").GetComponent<IntroManager>().SkipIntro();
                }


                yield break;
            }


            //Check for null voice clip
            if (MidCutsceneDialogues.totalDialogues[dialogueIndex].sentencesInside[i].voiceClip == null)
            {
                Debug.LogError("The MidCutscene clip in dialogue " + dialogueIndex + ", in sentence " + i + " is null.");

                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                if (finishedMidCutsceneDialogue != null)
                    finishedMidCutsceneDialogue.Invoke();

                yield break;
            }

            //Dying means no more sentences so as to not ruin the death sequence.
            if (warriorScript.dying)
            {
                dialogueTextUI.text = "";

                VoiceSoundManager.globalInstance.ResetSpeakingFlags();

                if (finishedMidCutsceneDialogue != null)
                    finishedMidCutsceneDialogue.Invoke();

                yield break;
            }


            DisplayDialogue(MidCutsceneDialogues.totalDialogues[dialogueIndex].sentencesInside[i]);

            VoiceSoundManager.globalInstance.PlayVoice(dialogueTextUI.color.r, MidCutsceneDialogues.totalDialogues[dialogueIndex].sentencesInside[i].voiceClip);

            yield return new WaitForSeconds(MidCutsceneDialogues.totalDialogues[dialogueIndex].sentencesInside[i].voiceClip.length);
        }

        dialogueTextUI.text = "";

        //Resets pretty much every flag of VoiceSoundManager
        VoiceSoundManager.globalInstance.ResetSpeakingFlags();

        if (finishedMidCutsceneDialogue != null)
            finishedMidCutsceneDialogue.Invoke();
    }

    private IEnumerator PlayLevelCutsceneDialogue()
    {
        //If other voices are playing, await them to end, then play this.
        while (VoiceSoundManager.globalInstance.IsPlayingMidCutscene() || VoiceSoundManager.globalInstance.IsPlayingPowerUp())
        {
            //Normally, you would put an action/event so as to inform this loop, and start 1.5 seconds later, instead of yolo-looping every 2.5 seconds
            //because with the latter (lazy) way, dialogue may end and instantly start which will sound very weird.

            yield return new WaitForSeconds(1.5f);
        }

        if (startedLevelCutsceneEvent != null)
            startedLevelCutsceneEvent();

        //Btw if the following bug appears: "Someone dies, but the knockback gets him to trigger levelcutscene" bugfix it so as to go to next level!

        //sentencesCount = the number of sentences inside the dialogue to be used.
        sentencesCount = LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside.Count;

        //For each sentence, we do a loop
        for (int i = 0; i < sentencesCount; i++)
        {
            //Skip
            if (skipLevelCutsceneDialogue)// || (Input.GetKey(KeyCode.Space) && GameManager.testing))
            {
                Debug.Log("Skipping works");

                FinishLevelCutsceneDialogue();

                VoiceSoundManager.globalInstance.StopVoice();

                yield break;
            }

            //Check for null voice clip
            if (LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside[i].voiceClip == null)
            {
                Debug.LogWarning("The LevelCutscene clip in dialogue " + LevelManager.levelCutsceneDialogueIndex + ", in sentence " + i + " is null.");

                FinishLevelCutsceneDialogue();

                yield break;
            }


            DisplayDialogue(LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside[i]);

            VoiceSoundManager.globalInstance.PlayVoice(dialogueTextUI.color.r, LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside[i].voiceClip);

            //Play the animation to fit the dialogue!
            PlayDialogueAnimation(dialogueTextUI.color.r, LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside[i].warriorAnimation, LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside[i].mageAnimation);

            yield return new WaitForSeconds(LevelCutsceneDialogues.totalDialogues[LevelManager.levelCutsceneDialogueIndex].sentencesInside[i].voiceClip.length);

            //If interrupted, don't bother continuing, and fix flags/booleans properly.
            if (VoiceSoundManager.globalInstance.IsPlayingInterruption())
            {
                yield break;
                /*
                Debug.Log("Detected interruption. Index is: " + LevelManager.levelCutsceneDialogueIndex);

                
                //If its the startskip cutscene (yes, endlevelcutscene shouldnt be skipped normally)
                if (LevelManager.levelCutsceneDialogueIndex % 2 == 0)
                {
                    StartCoroutine(LevelCutsceneEnd());

                    yield break;
                }
                */

                /*
                //Commented out, because this skipping levels is situational af for players, and there were minor bugs with this working. 
                //^(mostly from level being loaded while interrupting)^
                if (LevelManager.levelCutsceneDialogueIndex % 2 == 1)
                {
                    //Stop whoever is speaking.
                    VoiceSoundManager.globalInstance.StopVoice();

                    FinishLevelCutsceneDialogue();

                    yield break;
                }
                else
                {
                    StartCoroutine(LevelCutsceneEnd());

                    yield break;
                }

                */

            }

        }

        FinishLevelCutsceneDialogue();
    }

    public void FinishLevelCutsceneDialogue()
    {
        dialogueTextUI.text = "";

        VoiceSoundManager.globalInstance.ResetSpeakingFlags();

        StartCoroutine(LevelCutsceneEnd());
    }

    public IEnumerator LevelCutsceneEnd()
    {
        /* CUTSCENES
         "Nothing Special"  : 0,2,4,6,8,10,12,14,16, aka %2 == 0
         "Simple Transition": 1,3,7,11,15

        
        *
        * 0 -> Enters tutorial level, opening thingy
        * 1 -> Exits  tutorial level
        * 2 -> Enters level 1
        * 3 -> Exits  level 1
        * 4 -> Enters level 2
        * 5 -> Exits  level 2
        * 6 -> Enters level 3
        * 7 -> Exits  level 3
        * 8 -> Enters mini bossfight
        * 9 -> Exits  mini bossfight
        * 10 -> Enters final fight
        * 11 -> Exits tutorial onto level editor
        * 12 -> Enters level editor
        */

        //Btw, the below if/else checks shouldn't be done in dialogueManager. Simple Responsibility pattern is ruined.
        //remember WarriorMovement.cs....? Also, this is more or less the only old dialogue system part salvaged.
        int tempLevelCutsceneIndex = LevelManager.levelCutsceneDialogueIndex;

        //If simple transition
        if (tempLevelCutsceneIndex % 2 == 1)
        {
            //Move away all enemies nearby -> It is delayed because enemies spawn a bit after you move...
            StartCoroutine(MoveEnemiesAwayInSeconds(0.2f));

            //Fade In screen filter.
            GetComponent<FadeUIManager>().LevelFadeIn();

            //Make player invulnerable so he won't get hit/killed while transitioning. lel.
            GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().ActivateArmorSpellInvulnerability(4f);

            //TODO: Play the animation of the transition/fadeinout controller!

            Debug.Log("Picked Cutscene index is: " + tempLevelCutsceneIndex);

            yield return new WaitForSeconds(1.3f);//that 1.3f is for the cutscene fade probably.

            SceneManagerScript.globalInstance.LoadNextLevel();
            //Debug.Log("Simple transition");
        }
        //If just a cutscene, aka, when its done gameplay continues normally
        //else if (tempLevelCutsceneIndex % 2 == 0)

        //Must Notify warriorMovement, PauseMenu
        if (finishedLevelCutscene != null)
            finishedLevelCutscene.Invoke();

        //Update the cutscene index.
        LevelManager.IncreaseCutsceneIndex();

        Debug.Log("Cutscene index, now increased by +1 is: " + LevelManager.levelCutsceneDialogueIndex);
    }

    public IEnumerator PlayInterruptDialogue()
    {
        List<Sentence> sentencesInsideCurrentDialogue;

        bool interruptedByWarrior = DetermineWarriorInterruption();

        Debug.Log("Is playing leveldialogue: " + warriorScript.InLevelCutscene);

        //Set the flag that it is playing a cutscene
        VoiceSoundManager.globalInstance.SetInterruption(true);

        if (interruptedByWarrior)
        {
            //If there are no more interruption dialogues, instantly gtfo so as not to bug the game!
            if (PlayerStatsManager.globalInstance.GetInterruptionsByWarrior() >= InterruptionByWarriorDialogues.totalDialogues.Count)
            {
                FinishInterruptionDialogue(interruptedByWarrior);

                yield break;//return; aka gtfo
            }
            else
                sentencesInsideCurrentDialogue = InterruptionByWarriorDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetInterruptionsByWarrior()].sentencesInside;
        }
        else//interruptedByMage
        {
            //If there are no more interruption dialogues, instantly gtfo so as not to bug the game!
            if (PlayerStatsManager.globalInstance.GetInterruptionsByMage() >= InterruptionByMageDialogues.totalDialogues.Count)
            {
                FinishInterruptionDialogue(interruptedByWarrior);

                yield break;//return; aka gtfo
            }
            else
                sentencesInsideCurrentDialogue = InterruptionByMageDialogues.totalDialogues[PlayerStatsManager.globalInstance.GetInterruptionsByMage()].sentencesInside;
        }
            

        //The number of sentences inside the dialogue to be used
        sentencesCount = sentencesInsideCurrentDialogue.Count;

        //For each sentence, we do a loop
        for (int i = 0; i < sentencesCount; i++)
        {
            //Below, it makes the interruptDialogue Skippable
            if (skipInterruptDialogue)
            {
                FinishInterruptionDialogue(interruptedByWarrior);

                Debug.Log("Skipping works");

                yield break;
            }

            Debug.Log("i is: " + i + "\nsentencesCount is: " + sentencesCount);

            //Check for null voice clip
            if (sentencesInsideCurrentDialogue[i].voiceClip == null)
            {
                if (interruptedByWarrior)
                    Debug.LogError("The Interruption clip in dialogue " + PlayerStatsManager.globalInstance.GetInterruptionsByWarrior() + ", in sentence " + i + " is null.");
                else
                    Debug.LogError("The Interruption clip in dialogue " + PlayerStatsManager.globalInstance.GetInterruptionsByMage() + ", in sentence " + i + " is null.");

                FinishInterruptionDialogue(interruptedByWarrior);

                yield break;
            }


            DisplayDialogue(sentencesInsideCurrentDialogue[i]);

            VoiceSoundManager.globalInstance.StopVoice();

            VoiceSoundManager.globalInstance.PlayVoice(dialogueTextUI.color.r, sentencesInsideCurrentDialogue[i].voiceClip);

            yield return new WaitForSeconds(sentencesInsideCurrentDialogue[i].voiceClip.length);

        }

        FinishInterruptionDialogue(interruptedByWarrior);

        //If interrupted cutscene, end the cutscene gracefully
        if (warriorScript.InLevelCutscene)
        {
            Debug.Log("Skipped level cutscene, with index: " + LevelManager.levelCutsceneDialogueIndex);

            //skipLevelCutsceneDialogue -> Instantly triggers FinishLevelCutscene, so running this here, jumps one level ahead!
            //All in all, needs some good refactoring for skipping a single level cutscene.

            //skipLevelCutsceneDialogue = false;

            //StartCoroutine(LevelCutsceneEnd());
        }
    }

    public void FinishInterruptionDialogue(bool interruptedByWarrior)
    {
        dialogueTextUI.text = "";

        VoiceSoundManager.globalInstance.ResetSpeakingFlags();

        if (interruptedByWarrior)
            PlayerStatsManager.globalInstance.IncreaseInterruptionsByWarrior();
        else
            PlayerStatsManager.globalInstance.IncreaseInterruptionsByMage();

        //Only PauseMenu subscribes here, in order to enable itself.
        if (finishedInterruptionDialogue != null)
            finishedInterruptionDialogue();
    }


    public void PlayDebugDialogue(DialogueContainer chosenContainer)
    {
        StartCoroutine(PlayDebugDialogueCoroutine(chosenContainer));
    }

    /// <summary>
    /// Currently played only from ToolPlayDialogues.cs
    /// It plays whatever container in the parameter, with no end
    /// </summary>
    /// <param name="chosenContainer"></param>
    /// <returns></returns>
    public IEnumerator PlayDebugDialogueCoroutine(DialogueContainer chosenContainer)
    {
        int dialoguesCount = chosenContainer.totalDialogues.Count;

        Debug.Log("Starting DebugDialogue, dialoguesCount is: " + dialoguesCount);

        //VoiceSoundManager.globalInstance.StopVoice();
        //VoiceSoundManager.globalInstance.ResetSpeakingFlags();

        //This loop below is so it won't stop when the last sentence of a dialogue ends, but when every dialogue ends.
        for (int dialogueIndex = 0; dialogueIndex < dialoguesCount; dialogueIndex++)
        {


            //So it won't stay forever, with pause it cancels.
            if (Input.GetKey(KeyCode.G))
                break;

            //sentencesCount = the number of sentences inside the dialogue to be used.
            sentencesCount = chosenContainer.totalDialogues[dialogueIndex].sentencesInside.Count;

            //For each sentence, we do a loop
            for (int i = 0; i < sentencesCount; i++)
            {
                if (chosenContainer.totalDialogues[dialogueIndex].sentencesInside[i].voiceClip == null)
                    break;

                DisplayDialogue(chosenContainer.totalDialogues[dialogueIndex].sentencesInside[i]);

                VoiceSoundManager.globalInstance.PlayVoice(dialogueTextUI.color.r, chosenContainer.totalDialogues[dialogueIndex].sentencesInside[i].voiceClip);

                yield return new WaitForSeconds(chosenContainer.totalDialogues[dialogueIndex].sentencesInside[i].voiceClip.length);
            }

            dialogueTextUI.text = "";


        }
    }



    /// <summary>
    /// Takes a sentence, and takes the textToDisplay + color + font, and sets them onto the local dialogueTextUI to be shown in-game
    /// </summary>
    /// <param name="sentenceToDisplay"></param>
    public void DisplayDialogue(Sentence sentenceToDisplay)
    {
        dialogueTextUI.text = sentenceToDisplay.textToDisplay;

        dialogueTextUI.font = sentenceToDisplay.textFont;

        dialogueTextUI.color = sentenceToDisplay.textColor;

        //Outline color and width aren't set here, because they are set via the font's material.
        //Color could be also put here, but I think it is wiser to have it manually set, for future expansion cases.

        //TODO: Report a bug to TextMeshPro, that when you change textColor, it changes the color of outline as well.
        //TODO: Report another bug, where when you set a color to faceColor, it doesn't implicitly convert it.//Aka (0,0,0,1) becomes (255,255,255,255) like, wth?


        /* Spagghetti below, trying to set color + outline color. Changed the material outline settings and ggwp.
        if (sentenceToDisplay.textColor.r == 0)
            dialogueTextUI.faceColor = new Color32(0, 0, 0, 1);
        else
            dialogueTextUI.faceColor = new Color32(255, 255, 255, 255);

        dialogueTextUI.outlineColor = new Color32(0, 0, 0, 255);
        dialogueTextUI.outlineColor = Color.black;
            */
    }

    /// <summary>
    /// If "None", then ignore
    /// </summary>
    /// <param name="warriorColor"></param>
    /// <param name="animationToPlay"></param>
    public void PlayDialogueAnimation(float warriorColor, string warriorAnimation, string mageAnimation)
    {
        //Literally every dialogue aside of death/level cutscenes.
        if (warriorAnimation == "None" && mageAnimation == "None")
            return;
    }

    public bool DetermineWarriorInterruption()
    {
        //If the text is warrior's, mage did the interruption
        if (dialogueTextUI.color.r < 0.1f)
            return false;
        else//if the text is mage's, warrior did the interruption
            return true;
            
    }

    //Invoked by toggling speedrun mode on.
    public void SkipAllDialogues()
    {
        skipDeathDialogue = true;
        skipPowerUpDialogue = true;
        skipMidCutsceneDialogue = true;
        skipLevelCutsceneDialogue = true;
        skipInterruptDialogue = true;
    }

    //Invoked by toggling speedrun mode off
    public void DontSkipAllDialogues()
    {
        skipDeathDialogue = false;
        skipPowerUpDialogue = false;
        skipMidCutsceneDialogue = false;
        skipLevelCutsceneDialogue = false;
        skipInterruptDialogue = false;
    }

    public void SkipAllDialogues(float timeToTrigger)
    {
        StartCoroutine(SkipAllDialoguesCoroutine(timeToTrigger));
    }

    public IEnumerator SkipAllDialoguesCoroutine(float timeToTrigger)
    {
        yield return new WaitForSeconds(timeToTrigger);
        SkipAllDialogues();
    }

    public void DontSkipAllDialogues(float timeToTrigger)
    {
        StartCoroutine(DontSkipAllDialoguesCoroutine(timeToTrigger));
    }

    public IEnumerator DontSkipAllDialoguesCoroutine(float timeToTrigger)
    {
        yield return new WaitForSeconds(timeToTrigger);
        DontSkipAllDialogues();
    }

    public IEnumerator MoveEnemiesAwayInSeconds(float secondsToDelay)
    {
        yield return new WaitForSeconds(secondsToDelay);

        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(warriorScript.transform.position, 70, warriorScript.WhatIsDamageable | 1 << LayerMask.NameToLayer("Default"));
        //Iterate through every "enemy"
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            if (enemyColliders[i].gameObject.CompareTag("Enemy"))
                enemyColliders[i].transform.position = Vector3.zero;
            else if (enemyColliders[i].gameObject.CompareTag("EnemyProjectile"))
                enemyColliders[i].gameObject.GetComponent<ProjectileDamageTrigger>().Death();
        }

        Debug.Log("enemyColliders detected: " + enemyColliders.Length);
        Debug.Log("position is: " + warriorScript.transform.position);
    }

    /// <summary>
    /// Used to fuck with the cutscenes, and this was bugfix spagghetti.
    /// </summary>
    public void ResetBloodMeter()
    {
        GetComponent<KillMeterManager>().ResetBloodMeter();
    }


    /// <summary>
    /// The added component of WarriorPrefixedInput will call this if its boolean "triggerInsanityOnEnd" is true, which it should be.
    /// </summary>
    public void TriggerInsanityEnding()
    {
        GetComponent<FadeUIManager>().FadeInWhite();

        //Set the ending
        PlayerStatsManager.globalInstance.SetEnding(Endings.Insanity);

        //Write the ending on the drive, aka save the .json on the pc
        PlayerStatsManager.globalInstance.SaveCoreStats();

        //Would turn off the music too but whatever lmao
        StartCoroutine(ShutDownGameInSeconds(3));
    }

    public IEnumerator ShutDownGameInSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Application.Quit();
    }


}
