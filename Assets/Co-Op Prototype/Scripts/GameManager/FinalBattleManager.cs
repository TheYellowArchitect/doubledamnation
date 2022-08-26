using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalBattleManager : MonoBehaviour
{
    //So every1 can access it, without searching for tags and bs like that.
    public static FinalBattleManager globalInstance { set; get; }

    [Tooltip("Warrior/Fisherman/Fool")]
    public GameObject finalOpponent1;
    [Tooltip("Mage/Friend/Fiend")]
    public GameObject finalOpponent2;

    [Header("Prefabs to spawn")]
    [Tooltip("Pretty much everything visually needed for warrior")]
    public GameObject warriorRenderer;

    [Tooltip("The trail of the player at level3+")]
    public GameObject twistedDarkwindTrail;

    [Tooltip("To be spawned when P1 loses the final fight")]
    public GameObject finalDarkwindTrail;

    [Header("Misc")]
    [Tooltip("For when P2 gives him the offer ;)\nTo be used as a fadeinout, after the aforementioned.")]
    public Sprite warriorSpearSprite;
    //Like, at certain timing, have a particleThingy, ready to burst shit tons of darkwind particles, and it will serve as the fade-in. 
    //Some seconds after, player gets the spear and he also has the FinalDarkwind particles.->For epilogue, this is
    public GameObject finalBackgroundParent;

    //Not the true warrior, but its actually warrior's child(0), the warriorRenderer (prefab)
    private GameObject warrior;
    private GameObject defaultMainCameraGameobject;
    private FadeUIManager fadeUIManager;
    //private LevelManager commonLevelManager;

    public Action startingFinalFightEvent;

    private void Awake()
    {
        globalInstance = this;
    }

    // Use this for initialization
    void Start ()
    {
        warrior = Instantiate(warriorRenderer, transform);
        warrior.SetActive(false);

        //commonLevelManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<LevelManager>();

        ProcessCamera();

        Destroy(GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(0).gameObject);
        //Unsubscribe so Shift doesnt use dodgeroll and trigger disabled functions, muh spagghetti lmao
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().UnsubscribeDodgeroll();
        GameObject.FindGameObjectWithTag("Player").SetActive(false);
        //Destroy(GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>());
        //Destroy(GameObject.FindGameObjectWithTag("Player"));

        Destroy(GameObject.FindGameObjectWithTag("ManaManager"));

        //Disable pause menu and darkwind menu!
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<PauseMenu>().DisablePause();
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<DarkwindMenu>().darkwindMenuUnlocked = false;

        //Put clear save data to check for whatever needed in the future
        PlayerStatsManager.globalInstance.SetPlayerClear();

        MasterInputManager.globalInstance.DisableInput();
        MasterInputManager.globalInstance.DisableSpells();

        DetermineFinalCutscene();

        fadeUIManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<FadeUIManager>();
    }

    public void ProcessCamera()
    {
        //First of all, cache the fundamentalScene camera.
        defaultMainCameraGameobject = GameObject.FindGameObjectWithTag("MainCamera");

        //Get the background, to become static behind the arena
        defaultMainCameraGameobject.transform.GetChild(0).SetParent(finalBackgroundParent.transform);

        //Set the background in proper coordinates
        finalBackgroundParent.transform.GetChild(0).transform.position = new Vector3(0, 166, 0);

        //Destroy camera
        Destroy(GameObject.FindGameObjectWithTag("CameraHolder"));

        //Size 55 zoom out for the final encounter?
    }

    //TODO:
    //bizzaro magic SFX before fade-out, so as to buildup.

    public void DetermineFinalCutscene()
    {
        //If default ending, aka no 0 deaths and/or 0 kills
        if (GetComponent<AlternativeEndingsManager>().IsOnAlternativeEnding() == false)
        {
            //Subscribe to levelCutscene, so this script will be notified when dialogue ends.
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedLevelCutscene += FinishedDefaultFinalCutscene;

            //Notify dialogue manager to play the dialogue
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredLevelCutscene();

            //Notify pause menu
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<PauseMenu>().InLevelCutscene();
        }
        else//Do alternative ending
            GetComponent<AlternativeEndingsManager>().DetermineAlternativeEnding();



    }

    //Notify finalOpponents to drop the "InCutscene" bool, so they can actually move and play.
    public void FinishedDefaultFinalCutscene()
    {
        finalOpponent1.GetComponent<FinalCharacterBehaviour>().inCutscene = false;

        finalOpponent2.GetComponent<FinalCharacterBehaviour>().inCutscene = false;

        //Disable spells (cuz after a cutscene, spells are re-enabled)
        MasterInputManager.globalInstance.DisableSpells();

        //If online -_-
        if (NetworkCommunicationController.globalInstance != null)
            FindObjectOfType<NetworkFinalBattleInit>().Activate();
    }

    /// <summary>
    /// Invoked when a player has lost the fight. Either by out of bounds or by 3 ~~stocks~~ hits.
    /// </summary>
    public void InitiateCombatEnding(GameObject loser)//Do bounds even matter? Make 3rd hit have huge knockback, so player always gets knocked into Oblivion.
    {
        //SFX for the "dive"? or some wind at least? Very minor ofc.

        //Play DeathSFX. Tfw players are familiar with it and understand the impact ;)
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //Get camera to lock on P2 who is falling into gray. Hacky way but hey, if it works...
        GameObject.FindGameObjectWithTag("FinalCamera").AddComponent<FollowObjectSimple>();
        GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<FollowObjectSimple>().objectToFollow = loser;

        //Without this it will never follow cuz stuck at animation
        Destroy(GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<Animator>());

        //Stop movement from both and all logic
        finalOpponent1.GetComponent<FinalCharacterBehaviour>().inCutscene = true;
        finalOpponent2.GetComponent<FinalCharacterBehaviour>().inCutscene = true;

        //Set the gravity so camera can actually keep up
        DeleteFinalComponents(loser);
        loser.AddComponent<GravityNoAcceleration>();
        loser.GetComponent<GravityNoAcceleration>().gravitySpeed = 0.4f;

        //Rotate depending on which side loser fell in
        if (loser.transform.position.x > 0)//Fell to the right
            loser.transform.rotation = Quaternion.Euler(0, 0, -45);
        else//Fell to the left
            loser.transform.rotation = Quaternion.Euler(0, 0, 45);

        //Get the dialogueUI gameobject, so it will display in front of the faded in screen.//Shouldn't this be a thing by default?
        GameObject.FindGameObjectWithTag("Dialogue").transform.SetAsLastSibling();

        //P1 Won the fight
        if (loser == finalOpponent2)
            StartCoroutine( Player1CombatVictory(finalOpponent2));

        else//P2 Won the fight
            StartCoroutine( Player2CombatVictory(finalOpponent1));

    }

    public IEnumerator Player1CombatVictory(GameObject player2)
    {
        //Set the ending, for the JSON
        PlayerStatsManager.globalInstance.SetEnding(Endings.WarriorVictory);

        //Write the ending on the drive, aka save the .json on the pc
        PlayerStatsManager.globalInstance.SaveCoreStats();



        //Creates the darkwindTrail of level3+, also dem P2 implications...
        Instantiate(twistedDarkwindTrail, player2.transform);


        //===================================
        //=============Fade&Wait=============
        //===================================
        //Very fast/Swift fade-in
        fadeUIManager.FinalBattlePlayer1VictoryFadeIn();

        //Await for fade-in to fully happen. I think 3 is too much, maybe 1 second?
        yield return new WaitForSeconds(1);
        //===================================
        //===================================
        //===================================


        //Play cutscene. 9 is the number of the cutscene.
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(9);

        //15~16 is the seconds to finish the player1's talking dialogue. (15.628)
        yield return new WaitForSeconds(15f);

        //==================================================
        //=============Death SFX + Blacc screen=============
        //==================================================

        //Play DeathSFX
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //Get Color from White to Blacc (so as it fits restart game ;)
        //Fade-Out animation when the cutscene about to end
        fadeUIManager.FinalBattlePlayer1VictoryFadeWhiteToBlack();

        //Wait for the above animation to end
        yield return new WaitForSeconds(3);

        //== T H E   E N D ==

        //The End is... not?
        TheEndloop();
    }

    public IEnumerator Player2CombatVictory(GameObject player1)
    {
        //Set the ending, for the JSON
        PlayerStatsManager.globalInstance.SetEnding(Endings.MageVictory);

        //Write the ending on the drive, aka save the .json on the pc
        PlayerStatsManager.globalInstance.SaveCoreStats();

        //As for the cutscene, after it fades out, then it fades-in and the dialogue happens
        //then mid-cutscene on the Curse of the Darkwind, you fade out again, to fade-in

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Very fast/Swift fade-in, right when player falls into oblivion
        fadeUIManager.FinalBattlePlayer2VictoryFadeIn();

        //The time to fade in^
        yield return new WaitForSeconds(1);
        //===================================
        //===================================
        //===================================


        //Reactivate the warriorRenderer prefab
        warrior.SetActive(true);

        //Attach him to current falling
        warrior.transform.SetParent(player1.transform);

        //Reset his position cuz warrior has fallen deep into oblivion xd //cuz gravity component
        warrior.transform.localPosition = Vector3.zero;

        //Disable the current sprite of player1, because warrior should be visible, not player1.
        player1.transform.GetChild(0).gameObject.SetActive(false);

        //Destroy visuals because when you fade out with current waitseconds, it shows the lava next to pure gray, its horrible.
        Destroy(GameObject.FindGameObjectWithTag("FinalVisuals"));

        //Get Camera to look at the child that is upwards, so as the camera won't be centered at player
        //but player will be at the bottom of camera!
        GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<FollowObjectSimple>().objectToFollow = GameObject.FindGameObjectWithTag("FinalCameraAnchor");

        //Now wait sometime, so buildup happens.
        yield return new WaitForSeconds(2);

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Fade-Out
        fadeUIManager.FinalBattlePlayer2VictoryFadeOut();

        //The time to fade out
        yield return new WaitForSeconds(2);
        //===================================
        //===================================
        //===================================


        //Building up the suspense...
        yield return new WaitForSeconds(3);

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Very fast/Swift fade-in, right when player falls into oblivion
        fadeUIManager.FinalBattlePlayer2VictoryFadeIn();

        //The time to fade in^
        yield return new WaitForSeconds(2);
        //===================================
        //===================================
        //===================================

        //Play cutscene. 10 is the number of the cutscene.
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(10);

        //The time to exactly get to the curse of the darkwind re-deal. Either 10_16 at "a new... you..." make a pause here and put the SFX.
        //Time to get to the end of 10_10 is ~58 seconds.
        //Time to get from 10_11 to 10_16 is ~25.2 seconds
        yield return new WaitForSeconds(100.567f);//100.567f seconds for 10_23

        //Changes sprite to the spear one
        warrior.GetComponent<SpriteRenderer>().sprite = warriorSpearSprite;

        //Activates the "FinalDarkwind" VFX/Particle trail.
        Instantiate(finalDarkwindTrail, warrior.transform);

        //Make darkwind trail blacc instead of white
        //Making 2 temp variables for what could be a cache-less singular line of code #justunitythings
        ParticleSystem tempParticleSystem = GameObject.FindGameObjectWithTag("FinalPlayerDarkwindTrail").GetComponent<ParticleSystem>();
        ParticleSystem.TrailModule tempTrail = tempParticleSystem.trails;
        tempTrail.colorOverLifetime = Color.black;

        //TODO: Put DarkwindSFX here -> Could work but muh death.
        //Play DeathSFX
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Fade-Out
        fadeUIManager.FinalBattlePlayer2VictoryFadeOut();

        //The time to fade out
        yield return new WaitForSeconds(2);
        //===================================
        //=============Fade&Wait=============
        //===================================

        //Await up until 10_20 happens, aka the end of the dialogue.
        yield return new WaitForSeconds(14);

        //TODO: Play dialogue animation (like 1 hand forward in determination) at 10_19 aka when P1 speaks, with the darkwind, probably do dat yield return new again lul.
        //tfw entire sprite for a single moment feelsbadman. Would be so cool, especially if it was animation instead of just outright sprite changing which means its worth not making it :(

        //Fade-In
        fadeUIManager.FinalBattlePlayer2VictoryFadeGrayToBlack();

        //Wait for the above animation to end
        yield return new WaitForSeconds(4);

        //== T H E   E N D ==

        //The End is... not?
        TheEndloop();

        #region Ending where P2 speaks immediately instead of waiting a while
        //He speaks right before first fadeout, and P1 speaks when fade-in has happened so its good.
        //The reason however this is inferior to current, is because of the insinuation.
        //That silence, and P1 "gave in" to... whatever you, the reader, think that... Gray is. (inb4 precursor of the abyss/oblivion)
        //That is when both start to talk which makes more sense and is better imo.
        //It's deeper because it starts speaking when the elevation is deeper BIGBRAIN
        //Aristotle must be spinning at his grave like a fidget spinner at my deep writing.
        /*
        //As for the cutscene, after it fades out, then it fades-in and the dialogue happens
        //then mid-cutscene on the Curse of the Darkwind, you fade out again, to fade-in

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Very fast/Swift fade-in, right when player falls into oblivion
        fadeUIManager.FinalBattlePlayer2VictoryFadeIn();

        //The time to fade in^
        yield return new WaitForSeconds(1);
        //===================================
        //===================================
        //===================================

        

        //Reactivate the warriorRenderer prefab
        warrior.SetActive(true);

        //Attach him to current falling
        warrior.transform.SetParent(player1.transform);

        //Reset his position cuz warrior has fallen deep into oblivion xd //cuz gravity component
        warrior.transform.localPosition = Vector3.zero;

        //Disable the current sprite of player1, because warrior should be visible, not player1.
        player1.transform.GetChild(0).gameObject.SetActive(false);

        //Destroy visuals because when you fade out with current waitseconds, it shows the lava next to pure gray, its horrible.
        Destroy(GameObject.FindGameObjectWithTag("FinalVisuals"));

        //Get Camera to look at the child that is upwards, so as the camera won't be centered at player
        //but player will be at the bottom of camera!
        GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<FollowObjectSimple>().objectToFollow = GameObject.FindGameObjectWithTag("FinalCameraAnchor");

        //Now wait sometime, so buildup happens.
        yield return new WaitForSeconds(2);

        //Play cutscene. 10 is the number of the P2Victory cutscene.
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(10);

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Fade-Out
        fadeUIManager.FinalBattlePlayer2VictoryFadeOut();

        //The time to fade out
        yield return new WaitForSeconds(2);
        //===================================
        //===================================
        //===================================


        //Building up the suspense...
        yield return new WaitForSeconds(3);

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Very fast/Swift fade-in, right when player falls into oblivion
        fadeUIManager.FinalBattlePlayer2VictoryFadeIn();

        //The time to fade in^
        yield return new WaitForSeconds(1);
        //===================================
        //===================================
        //===================================

        //The time to exactly get to the curse of the darkwind re-deal. Either 10_16 at "a new... you..." make a pause here and put the SFX.
        //Time to get to the end of 10_10 is ~58 seconds.
        //Time to get from 10_11 to 10_16 is ~25.2 seconds
        yield return new WaitForSeconds(93.567f);//100.567f seconds for 10_23

        //Changes sprite to the spear one
        warrior.GetComponent<SpriteRenderer>().sprite = warriorSpearSprite;

        //Activates the "FinalDarkwind" VFX/Particle trail.
        Instantiate(finalDarkwindTrail, warrior.transform);

        //Make darkwind trail blacc instead of white
        //Making 2 temp variables for what could be a cache-less singular line of code #justunitythings
        ParticleSystem tempParticleSystem = GameObject.FindGameObjectWithTag("FinalPlayerDarkwindTrail").GetComponent<ParticleSystem>();
        ParticleSystem.TrailModule tempTrail = tempParticleSystem.trails;
        tempTrail.colorOverLifetime = Color.black;

        //TODO: Put DarkwindSFX here -> Could work but muh death.
        //Play DeathSFX
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //===================================
        //=============Fade&Wait=============
        //===================================
        //Fade-Out
        fadeUIManager.FinalBattlePlayer2VictoryFadeOut();

        //The time to fade out
        yield return new WaitForSeconds(2);
        //===================================
        //=============Fade&Wait=============
        //===================================

        //Await up until 10_20 happens, aka the end of the dialogue.
        yield return new WaitForSeconds(14);

        //TODO: Play dialogue animation (like 1 hand forward in determination) at 10_19 aka when P1 speaks, with the darkwind, probably do dat yield return new again lul.
        //tfw entire sprite for a single moment feelsbadman. Would be so cool, especially if it was animation instead of just outright sprite changing which means its worth not making it :(

        //Fade-In
        fadeUIManager.FinalBattlePlayer2VictoryFadeGrayToBlack();

        //Wait for the above animation to end
        yield return new WaitForSeconds(4);

        //== T H E   E N D ==

        //The End is... not?
        TheEndloop();
        */
        #endregion
    }

    /// <summary>
    /// Deletes the components: FinalCharacterBehaviour, BoxCollider2D, Rigidbody2D.
    /// </summary>
    /// <param name="resetObject"></param>
    public void DeleteFinalComponents(GameObject resetObject)
    {
        Destroy(resetObject.GetComponent<FinalCharacterInput>());
        Destroy(resetObject.GetComponent<FinalCharacterBehaviour>());
        Destroy(resetObject.GetComponent<BoxCollider2D>());
        Destroy(resetObject.GetComponent<Rigidbody2D>());
    }

    /// <summary>
    /// Called when ending of P1 or P2 winning ends
    /// Should start at the boat and generally reset pretty much everything lmao
    /// </summary>
    public void TheEndloop()
    {
        //Debug.Log("Reached End Loop!");
        //By using LoadSceneMode.Single, it deletes ALL scenes, and loads ActiveGameScene, just as it does when the game starts for the first time :)
        UnityEngine.SceneManagement.SceneManager.LoadScene("ActiveGameScene", LoadSceneMode.Single);

        #region old endloop testing untested cuz 1 line of code is C L E A N
        //Reset the variables that would otherwise bug the game, aka the variables that aren't reset by simply resetting the scenes!
        //commonLevelManager.RestartGame();
        //JSON + Speedrunning shit -> automatically made since FundamentalScene.

        //Delete the previous scenes
        //SceneManagerScript.globalInstance.UnloadAny("FundamentalScene");
        //SceneManagerScript.globalInstance.UnloadAny("FinalLevel");

        //Lets try this out, since by deleting all scenes and getting ActiveGameScene from scratch, it should reset its values!
        //SceneManagerScript.globalInstance.UnloadAny("ActiveGameScene");
        //SceneManagerScript.globalInstance.Load("ActiveGameScene");

        //Restart the game ;)
        //SceneManagerScript.globalInstance.Initialize();
        #endregion
    }
}
