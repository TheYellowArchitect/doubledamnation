using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//This is made in inspiration from celeste's assist mode. 
//The game turned out to be too hard, but since it's supposed to be like this, this makes non-hardcore metroidvania players, beat the game with ease :) 
//Can't force every1 to play it as intended, after all. Enforcing it is stupid.
public class DarkwindMenu : MonoBehaviour
{
    public static DarkwindMenu globalInstance;

    [Header("UI GameObjects/Menus")]
    public GameObject pauseMenuUI;
    public GameObject darkwindMenuUI;

    [Header("Labels to change")]
    public TextMeshProUGUI healthCurrentValue;
    public TextMeshProUGUI noManaLimitOnSpellwordsValue;
    public TextMeshProUGUI dodgerollChainsValue;
    public TextMeshProUGUI allowCrouchDashValue;
    public TextMeshProUGUI allowWallJumpingValue;
    public TextMeshProUGUI allowInfiniteSpeedMomentumValue;
    public TextMeshProUGUI allowGroundWallSpeedCheeseValue;
    public TextMeshProUGUI ignoreSameAttackJoystickInputValue;
    public TextMeshProUGUI acceptAttackInputOnlyOnJoystickEdgesValue;
    public TextMeshProUGUI allowReverseDarkwindPullValue;
    public TextMeshProUGUI inversedCameraValue;
    public TextMeshProUGUI wallJumpResetValue;

    [Header("Read Only")]
    public bool canChangeHealth = false;//TODO: Unlocks in first level.
    public bool darkwindMenuUnlocked = false;
    //one bool here to check if player is in checkpoint, detect it via music (either source.isplaying, or by the music collider)
    public bool openedDarkwind = false;//isOpen is more accurate naming.
	
    //Below 2 are for voiceline trigger on opening darkwindmenu
    public bool hasActivatedOnce = false;
    public byte noGloryDialogueIndex = 20;

    private PauseMenu commonPause;

    public enum ButtonClicked {opened, closed, reset, healthRight, healthLeft, spellcastingRight, spellcastingLeft, chaindashingRight, chaindashingLeft, walljumpingRight, walljumpingLeft, momentumRight, momentumLeft, cheesewallRight, cheesewallLeft, crouchRight, crouchLeft, sameAttackRight, sameAttackLeft, acceptAttackRight, acceptAttackLeft, darkwindPullRight, darkwindPullLeft, walljumpRefreshRight, walljumpRefreshLeft, inversedCameraRight, inversedCameraLeft, resetHealth, resetSpellcasting, resetChaindashing, resetWalljumping, resetMomentum, resetCheesewall, resetCrouch, resetSameAttack, resetAcceptAttack, resetDarkwindPull, resetWalljumpRefresh, resetInversedCamera};

    private void Start()
    {
        globalInstance = this;

        commonPause = GetComponent<PauseMenu>();
        
        healthCurrentValue.text = romanStringNumerals.romanStringNumeralGenerator(DarkwindDistortionManager.globalInstance.GetCurrentMaxHealth());
    }

    // Update is called once per frame
    void Update ()
    {
        //darkwindMenu unlocks from gamemanager.testing = true, OR by reaching X death OR by reaching wind level.
        if (IsDarkwindMenuUnlocked())
            DetermineInputDarkwind();
	}

    //Should normally make darkwindMenuUnlocked = true via event, but the day I write this is August 25th - a day away from release.
    //And if I unlock via dialogue manager, there is the possible bug of not activating this if you skip that dialogue, so I'm doing this swift hack where I check everyframe -_-
    public bool IsDarkwindMenuUnlocked()
    {
        if (darkwindMenuUnlocked == true)
            return true;

        //Dialogue at winds of oblivion unlocks the darkwind, or did the dialogue of death
        if (LevelManager.currentLevel > 2 || PlayerStatsManager.globalInstance.GetTotalDeaths() > 4)
            darkwindMenuUnlocked = true;

        return false;
    }

    public void DetermineInputDarkwind()
    {
        if (ShouldOpenInputDarkwind())
        {
            if (openedDarkwind == false)
                ActivateDarkwindMenuWrapper();
            else
                DisableDarkwindMenuWrapper();
        }
        else if (openedDarkwind)
            DetermineCloseInputDarkwind();
    }

    public bool ShouldOpenInputDarkwind()
    {
        if (NetworkCommunicationController.globalInstance == null)
        {
            if (Input.GetKeyDown(KeyCode.Pause))
                return true;

#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
            if ((Input.GetButtonDown("ButtonL1") && GameManager.testing)  || (Input.GetButtonUp("ButtonSelect") && GameManager.testing == false))
                return true;
#else
            if (Input.GetButtonDown("ButtonSelect"))
                return true;
#endif
            
        }
        else//Online
        {
            if (NetworkCommunicationController.globalInstance.IsServer())
            {
                if (Input.GetButtonDown("ButtonL1") && GameManager.testing)
                    return true;
                else if (Input.GetButtonUp("ButtonSelect") && GameManager.testing == false)
                    return true;
            }
            else if (Input.GetKeyDown(KeyCode.Pause))
                return true;
        }

        return false;
    }

    public void DetermineCloseInputDarkwind()
    {
        if (NetworkCommunicationController.globalInstance == null)
        {
            if (Input.GetButtonUp("ButtonB") || Input.GetKeyDown(KeyCode.Escape))
                DisableDarkwindMenu();
        }
        else//Online
        {
            if ((Input.GetButtonUp("ButtonB") && NetworkCommunicationController.globalInstance.IsServer()) || Input.GetKeyDown(KeyCode.Escape) && NetworkCommunicationController.globalInstance.IsServer() == false)
                DisableDarkwindMenu();
        }
    }

    public void ActivateDarkwindMenuWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.opened);

        ActivateDarkwindMenu();
    }

    public void ActivateDarkwindMenu()
    {
        if (hasActivatedOnce == false)
        {
            hasActivatedOnce = true;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene((int)noGloryDialogueIndex);
        }

        openedDarkwind = true;
        darkwindMenuUI.SetActive(true);
        commonPause.Pause(false);
    }

    public void DisableDarkwindMenuWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.closed);

        DisableDarkwindMenu();
    }

    public void DisableDarkwindMenu()
    {
        openedDarkwind = false;
        darkwindMenuUI.SetActive(false);
        commonPause.Resume();
    }


    //==========================
    //===Triggered by buttons===
    //==========================

    public void IncreaseHealthWrapper(int valueToAdd)
    {
        NetworkSendButtonClicked(ButtonClicked.healthRight);

        IncreaseHealth(valueToAdd);
    }

    public void IncreaseHealth(int valueToAdd)
    {
        DarkwindDistortionManager.globalInstance.AddMaxHealth(valueToAdd);

        healthCurrentValue.text = romanStringNumerals.romanStringNumeralGenerator(DarkwindDistortionManager.globalInstance.GetCurrentMaxHealth());
    }

    public void DecreaseHealthWrapper(int valueToDecrease)
    {
        NetworkSendButtonClicked(ButtonClicked.healthLeft);

        DecreaseHealth(valueToDecrease);
    }

    public void DecreaseHealth(int valueToDecrease)
    {
        DarkwindDistortionManager.globalInstance.AddMaxHealth(valueToDecrease * -1);

        healthCurrentValue.text = romanStringNumerals.romanStringNumeralGenerator(DarkwindDistortionManager.globalInstance.GetCurrentMaxHealth());
    }

    //To be used on many new toggles ;) after release long-term HYPE!
    //Changes automatically darkwindDistortionManager's values ;)
    public void ToggleBoolean(ref bool value, TextMeshProUGUI displayValue, string textOn, string textOff)
    {
        value = !value;

        if (value == true)
            displayValue.text = textOn;
        else
            displayValue.text = textOff;
    }

    //This would be sweet if you could make it work like the above!
    //I cannot figure out how to get the enum type type-casted properly, and find all of its possible values though
    public void ToggleEnum()
    {

    }

    //All below toggles are triggered from the buttons from canvas -> darkwindmenu UI.
    public void ToggleManaLimitOnSpellwordsWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.spellcastingRight);

        ToggleManaLimitOnSpellwords();
    }

    public void ToggleManaLimitOnSpellwords()
    {
        ToggleBoolean(ref DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords, noManaLimitOnSpellwordsValue, "unbound", "Restricted");
    }

    public void ToggleChaindashWrapper(bool rightButton)
    {
        NetworkSendButtonClicked(ButtonClicked.chaindashingRight, rightButton);

        ToggleChaindash(rightButton);
    }

    public void ToggleChaindash(bool rightButton)
    {
        //Ok so how this works for both the sides of the buttons.
        //Tempchaindash equals playerchaindash, so I can tweak it without fucking up if we go out of the range [0,2]
        //
        //Then, I add 1 or remove 1, based on button. I check if out of range, and set tempchaindash to the opposite range (if -1 for example, it should do a full circle and go to 2)
        //And now since I am ok with error use-cases of out of range, I simply set playerchaindash = tempchaindash.
        //And finally, depending on which enum playerchaindash is, i set the text ;)

        DarkwindDistortionManager.Chaindash tempChaindash = DarkwindDistortionManager.globalInstance.playerChaindash;

        if (rightButton)
            tempChaindash++;
        else
            tempChaindash--;

        if ((int) tempChaindash == -1)
            tempChaindash = DarkwindDistortionManager.Chaindash.Infinite;//2
        else if ((int) tempChaindash == 3)
            tempChaindash = DarkwindDistortionManager.Chaindash.Normal;//0

        DarkwindDistortionManager.globalInstance.playerChaindash = tempChaindash;

        //And now we check what the current chaindash variable is, so we set the text, ggwp.
        if (DarkwindDistortionManager.globalInstance.playerChaindash == DarkwindDistortionManager.Chaindash.Normal)
            dodgerollChainsValue.text = "Normal";
        else if (DarkwindDistortionManager.globalInstance.playerChaindash == DarkwindDistortionManager.Chaindash.Disabled)
            dodgerollChainsValue.text = "Restricted";
        else if (DarkwindDistortionManager.globalInstance.playerChaindash == DarkwindDistortionManager.Chaindash.Infinite)
            dodgerollChainsValue.text = "Infinite";

    }

    public void ToggleWallJumpingWrapper(bool rightButton)
    {
        NetworkSendButtonClicked(ButtonClicked.walljumpingRight, rightButton);

        ToggleWallJumping(rightButton);
    }

    //Literally copy-pasting the above/ToggleChaindash
    public void ToggleWallJumping(bool rightButton)
    {
        DarkwindDistortionManager.WallJumpingLimit tempWallJumpingLimit = DarkwindDistortionManager.globalInstance.playerWallJumpingLimit;

        //Increase or decrease based on button direction
            if (rightButton)
                tempWallJumpingLimit++;
            else
                tempWallJumpingLimit--;

        //Below if/else is for looping the enum so it doesn't go beyond its minimum or maximum values!

            //Reached negative, so loop to the end
            if ((int) tempWallJumpingLimit == -1)
                tempWallJumpingLimit = DarkwindDistortionManager.WallJumpingLimit.Infinite;//3
            //Reached max, so loop to the start
            else if ((int) tempWallJumpingLimit == 4)
                tempWallJumpingLimit = DarkwindDistortionManager.WallJumpingLimit.Never;//0


        //We set the value on Darkwind
            DarkwindDistortionManager.globalInstance.playerWallJumpingLimit = tempWallJumpingLimit;

        //And taking the value we set, we update the UI
            if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Never)
                allowWallJumpingValue.text = "Never";
            else if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Once)
                allowWallJumpingValue.text = "Once";
            else if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Twice)
                allowWallJumpingValue.text = "Twice";
            else if (DarkwindDistortionManager.globalInstance.playerWallJumpingLimit == DarkwindDistortionManager.WallJumpingLimit.Infinite)
                allowWallJumpingValue.text = "Infinite";
    }

    public void ToggleCrouchDashRestoreWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.crouchRight);

        ToggleCrouchDashRestore();
    }

    public void ToggleCrouchDashRestore()
    {
        if (DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Lazy)
            DarkwindDistortionManager.globalInstance.playerCrouchDashRestore = DarkwindDistortionManager.CrouchDashRestore.Perfect;
        else//if (DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Perfect)
            DarkwindDistortionManager.globalInstance.playerCrouchDashRestore = DarkwindDistortionManager.CrouchDashRestore.Lazy;

        //Update the UI
            if (DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Lazy)
                allowCrouchDashValue.text = "generoushalt";//Generous because at any influence, it refreshes P2 dash instead of wanting dash.
            else//if (DarkwindDistortionManager.globalInstance.playerCrouchDashRestore == DarkwindDistortionManager.CrouchDashRestore.Perfect)
                allowCrouchDashValue.text = "perfectslide";
                //It always slides here, but if its just sliding without refreshing P2 dash, it's not perfect, hence just crouchcancelslide
    }

    public void ToggleWallJumpResetWrapper(bool rightButton)
    {
        NetworkSendButtonClicked(ButtonClicked.walljumpRefreshRight, rightButton);

        ToggleWallJumpReset(rightButton);
    }

    public void ToggleWallJumpReset(bool rightButton)
    {
        DarkwindDistortionManager.WallJumpReset tempWallJumpReset = DarkwindDistortionManager.globalInstance.playerWalljumpReset;

        //Increase or decrease based on button direction
        if (rightButton)
            tempWallJumpReset++;
        else
            tempWallJumpReset--;

        //Below if/else is for looping the enum so it doesn't go beyond its minimum or maximum values!

        //Reached negative, so loop to the end
        if ((int) tempWallJumpReset == -1)
            tempWallJumpReset = DarkwindDistortionManager.WallJumpReset.KillAndHit;//3
            //Reached max, so loop to the start
        else if ((int) tempWallJumpReset == 4)
            tempWallJumpReset = DarkwindDistortionManager.WallJumpReset.GroundOnly;//0


        //We set the value on Darkwind
        DarkwindDistortionManager.globalInstance.playerWalljumpReset = tempWallJumpReset;

        //And taking the value we set, we update the UI
        if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.GroundOnly)
            wallJumpResetValue.text = "Exclusively ground";
        else if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.Hit)
            wallJumpResetValue.text = "getting hit";
        else if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.Kill)
            wallJumpResetValue.text = "Kill";
        else if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.KillAndHit)
            wallJumpResetValue.text = "getting hit and killing";
    }

    public void ToggleInversedCameraWrapper(bool rightButton)
    {
        NetworkSendButtonClicked(ButtonClicked.inversedCameraRight, rightButton);

        ToggleInversedCamera(rightButton);
    }

    public void ToggleInversedCamera(bool rightButton)
    {
        DarkwindDistortionManager.InversedCameraOptions tempInversedCameraOptions = DarkwindDistortionManager.globalInstance.mainCameraInversedOptions;

        //Increase or decrease based on button direction
        if (rightButton)
            tempInversedCameraOptions++;
        else
            tempInversedCameraOptions--;

        //Below if/else is for looping the enum so it doesn't go beyond its minimum or maximum values!

        //Reached negative, so loop to the end
        if ((int) tempInversedCameraOptions == -1)
            tempInversedCameraOptions = DarkwindDistortionManager.InversedCameraOptions.FlipBoth;//3
            //Reached max, so loop to the start
        else if ((int) tempInversedCameraOptions == 4)
            tempInversedCameraOptions = DarkwindDistortionManager.InversedCameraOptions.None;//0


        //We set the value on Darkwind
        DarkwindDistortionManager.globalInstance.mainCameraInversedOptions = tempInversedCameraOptions;

        //And taking the value we set, we update the UI
        if (DarkwindDistortionManager.globalInstance.mainCameraInversedOptions == DarkwindDistortionManager.InversedCameraOptions.None)
            inversedCameraValue.text = "None";
        else if (DarkwindDistortionManager.globalInstance.mainCameraInversedOptions == DarkwindDistortionManager.InversedCameraOptions.FlipX)
            inversedCameraValue.text = "Flip x";
        else if (DarkwindDistortionManager.globalInstance.mainCameraInversedOptions == DarkwindDistortionManager.InversedCameraOptions.FlipY)
            inversedCameraValue.text = "Flip y";
        else if (DarkwindDistortionManager.globalInstance.mainCameraInversedOptions == DarkwindDistortionManager.InversedCameraOptions.FlipBoth)
            inversedCameraValue.text = "Flip Both";
    }

    public void ToggleAllowGroundWallSpeedCheeseWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.cheesewallRight);

        ToggleAllowGroundWallSpeedCheese();
    }

    public void ToggleAllowGroundWallSpeedCheese()
    {
        ToggleBoolean(ref DarkwindDistortionManager.globalInstance.allowGroundWallSpeedCheese, allowGroundWallSpeedCheeseValue, "unbound", "Restricted");
    }

    public void ToggleAllowInfiniteSpeedMomentumWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.momentumRight);

        ToggleAllowInfiniteSpeedMomentum();
    }

    public void ToggleAllowInfiniteSpeedMomentum()
    {
        ToggleBoolean(ref DarkwindDistortionManager.globalInstance.allowInfiniteSpeedMomentum, allowInfiniteSpeedMomentumValue, "unbound", "Restricted");
    }

    public void ToggleIgnoreSameAttackJoystickInputWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.sameAttackRight);

        ToggleIgnoreSameAttackJoystickInput();
    }
    
    public void ToggleIgnoreSameAttackJoystickInput()
    {
        ToggleBoolean(ref DarkwindDistortionManager.globalInstance.ignoreSameAttackJoystickInput, ignoreSameAttackJoystickInputValue, "Ignore", "Allow");
    }

    public void ToggleAcceptAttackInputOnlyOnJoystickEdgesWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.acceptAttackRight);
        
        ToggleAcceptAttackInputOnlyOnJoystickEdges();
    }

    public void ToggleAcceptAttackInputOnlyOnJoystickEdges()
    {
        ToggleBoolean(ref DarkwindDistortionManager.globalInstance.acceptAttackInputOnlyOnJoystickEdges, acceptAttackInputOnlyOnJoystickEdgesValue, "On joystick edges", "on any tilt");
    }

    public void ToggleAllowReverseDarkwindPullWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.darkwindPullRight);

        ToggleAllowReverseDarkwindPull();
    }

    public void ToggleAllowReverseDarkwindPull()
    {
        ToggleBoolean(ref DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull, allowReverseDarkwindPullValue, "unbound", "Restricted");
    }



    

    //Reset Button
    public void ResetWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.reset);

        Reset();
    }
    //Also called by LevelManager on level load!
    public void Reset()
    {
        ResetHealth();

        ResetManaLimitOnSpellWords();

        ResetChaindashing();

        ResetWallJumping();

        ResetInfiniteSpeedMomentum();

        ResetGroundWallSpeedCheese();

        ResetCrouchDashRestore();

        ResetIgnoreSameAttackJoystickInput();

        ResetAcceptAttackInputOnlyOnJoystickEdges();

        ResetReverseDarkwindPull();

        ResetInversedCamera();
    }

    public void ResetHealthWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetHealth);

        ResetHealth();
    }

    public void ResetHealth()
    {
        DarkwindDistortionManager.globalInstance.ResetMaxHealth();

        healthCurrentValue.text = romanStringNumerals.romanStringNumeralGenerator(DarkwindDistortionManager.globalInstance.GetCurrentMaxHealth());
    }

    public void ResetManaLimitOnSpellWordsWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetSpellcasting);
        
        ResetManaLimitOnSpellWords();
    }

    public void ResetManaLimitOnSpellWords()
    {
        if (GameManager.testing)
        {
            DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords = true;
            noManaLimitOnSpellwordsValue.text = "unbound";
        }
        else if (DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords == true)
        {
            DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords = false;
            noManaLimitOnSpellwordsValue.text = "Restricted";
        }
    }

    public void ResetChaindashingWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetChaindashing);

        ResetChaindashing();
    }

    public void ResetChaindashing()
    {
        if (GameManager.testing)
        {
            DarkwindDistortionManager.globalInstance.playerChaindash = DarkwindDistortionManager.Chaindash.Infinite;
            dodgerollChainsValue.text = "Infinite";
        }
        else
        {
            DarkwindDistortionManager.globalInstance.playerChaindash = DarkwindDistortionManager.Chaindash.Normal;
            dodgerollChainsValue.text = "Normal";
        }
    }

    public void ResetWallJumpingWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetWalljumping);

        ResetWallJumping();
    }

    public void ResetWallJumping()
    {
        DarkwindDistortionManager.globalInstance.playerWallJumpingLimit = DarkwindDistortionManager.WallJumpingLimit.Once;
        allowWallJumpingValue.text = "Once";
    }

    public void ResetCrouchDashRestoreWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetCrouch);

        ResetCrouchDashRestore();
    }

    public void ResetCrouchDashRestore()
    {
        DarkwindDistortionManager.globalInstance.playerCrouchDashRestore = DarkwindDistortionManager.CrouchDashRestore.Perfect;
        allowCrouchDashValue.text = "perfectslide";
    }

    public void ResetGroundWallSpeedCheeseWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetCheesewall);

        ResetGroundWallSpeedCheese();
    }

    public void ResetGroundWallSpeedCheese()
    {
        if (DarkwindDistortionManager.globalInstance.allowGroundWallSpeedCheese == true)
        {
            DarkwindDistortionManager.globalInstance.allowGroundWallSpeedCheese = false;
            allowGroundWallSpeedCheeseValue.text = "Restricted";
        }
    }

    public void ResetInfiniteSpeedMomentumWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetMomentum);

        ResetInfiniteSpeedMomentum();
    }

    public void ResetInfiniteSpeedMomentum()
    {
        if (DarkwindDistortionManager.globalInstance.allowInfiniteSpeedMomentum == true)
        {
            DarkwindDistortionManager.globalInstance.allowInfiniteSpeedMomentum = false;
            allowInfiniteSpeedMomentumValue.text = "Restricted";
        }
    }

    public void ResetIgnoreSameAttackJoystickInputWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetSameAttack);

        ResetIgnoreSameAttackJoystickInput();
    }

    public void ResetIgnoreSameAttackJoystickInput()
    {
        if (DarkwindDistortionManager.globalInstance.ignoreSameAttackJoystickInput == true)
        {
            DarkwindDistortionManager.globalInstance.ignoreSameAttackJoystickInput = false;
            ignoreSameAttackJoystickInputValue.text = "Allow";
        }
    }

    public void ResetAcceptAttackInputOnlyOnJoystickEdgesWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetAcceptAttack);

        ResetAcceptAttackInputOnlyOnJoystickEdges();
    }

    public void ResetAcceptAttackInputOnlyOnJoystickEdges()
    {
        if (DarkwindDistortionManager.globalInstance.acceptAttackInputOnlyOnJoystickEdges == false)
        {
            DarkwindDistortionManager.globalInstance.acceptAttackInputOnlyOnJoystickEdges = true;
            acceptAttackInputOnlyOnJoystickEdgesValue.text = "On joystick edges";
        }
    }

    public void ResetReverseDarkwindPullWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetDarkwindPull);

        ResetReverseDarkwindPull();
    }

    public void ResetReverseDarkwindPull()
    {
        if (DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull == true)
        {
            DarkwindDistortionManager.globalInstance.allowReverseDarkwindPull = false;
            allowReverseDarkwindPullValue.text = "Restricted";
        }
    }

    public void ResetInversedCameraWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetInversedCamera);

        ResetInversedCamera();
    }

    public void ResetInversedCamera()
    {
        if (DarkwindDistortionManager.globalInstance.mainCameraInversedOptions != DarkwindDistortionManager.InversedCameraOptions.None)
        {
            DarkwindDistortionManager.globalInstance.mainCameraInversedOptions = DarkwindDistortionManager.InversedCameraOptions.None;
            inversedCameraValue.text = "None";
        }
    }

    public void ResetWallJumpResetWrapper()
    {
        NetworkSendButtonClicked(ButtonClicked.resetWalljumpRefresh);

        ResetWallJumpReset();
    }

    public void ResetWallJumpReset()
    {
        if (DarkwindDistortionManager.globalInstance.playerWalljumpReset != DarkwindDistortionManager.WallJumpReset.KillAndHit)
        {
            DarkwindDistortionManager.globalInstance.playerWalljumpReset = DarkwindDistortionManager.WallJumpReset.KillAndHit;
            wallJumpResetValue.text = "getting hit and killing";
        }
    }



    
    public void DetermineButtonClicked(byte buttonClicked)
    {
        DetermineButtonClicked((ButtonClicked)buttonClicked);
    }

    public void DetermineButtonClicked(ButtonClicked buttonClicked)
    {
        Debug.Log("Button clicked is: " + buttonClicked);

        switch(buttonClicked)
        {
            case ButtonClicked.opened:
                ActivateDarkwindMenu();
                break;
            
            case ButtonClicked.closed:
                DisableDarkwindMenu();
                break;

            case ButtonClicked.reset:
                Reset();
                break;

            case ButtonClicked.healthRight:
                IncreaseHealth(1);
                break;

            case ButtonClicked.healthLeft:
                DecreaseHealth(1);
                break;

            case ButtonClicked.spellcastingRight:
                ToggleManaLimitOnSpellwords();
                break;

            case ButtonClicked.spellcastingLeft:
                ToggleManaLimitOnSpellwords();
                break;

            case ButtonClicked.chaindashingRight:
                ToggleChaindash(true);
                break;

            case ButtonClicked.chaindashingLeft:
                ToggleChaindash(false);
                break;

            case ButtonClicked.walljumpingRight:
                ToggleWallJumping(true);
                break;

            case ButtonClicked.walljumpingLeft:
                ToggleWallJumping(false);
                break;

            case ButtonClicked.crouchRight:
                ToggleCrouchDashRestore();
                break;

            case ButtonClicked.crouchLeft:
                ToggleCrouchDashRestore();
                break;

            case ButtonClicked.walljumpRefreshRight:
                ToggleWallJumpReset(true);
                break;

            case ButtonClicked.walljumpRefreshLeft:
                ToggleWallJumpReset(false);
                break;

            case ButtonClicked.inversedCameraRight:
                ToggleInversedCamera(true);
                break;

            case ButtonClicked.inversedCameraLeft:
                ToggleInversedCamera(false);
                break;

            case ButtonClicked.cheesewallRight:
                ToggleAllowGroundWallSpeedCheese();
                break;

            case ButtonClicked.cheesewallLeft:
                ToggleAllowGroundWallSpeedCheese();
                break;

            case ButtonClicked.momentumRight:
                ToggleAllowInfiniteSpeedMomentum();
                break;

            case ButtonClicked.momentumLeft:
                ToggleAllowInfiniteSpeedMomentum();
                break;

            case ButtonClicked.sameAttackRight:
                ToggleIgnoreSameAttackJoystickInput();
                break;

            case ButtonClicked.sameAttackLeft:
                ToggleIgnoreSameAttackJoystickInput();
                break;

            case ButtonClicked.acceptAttackRight:
                ToggleAcceptAttackInputOnlyOnJoystickEdges();
                break;

            case ButtonClicked.acceptAttackLeft:
                ToggleAcceptAttackInputOnlyOnJoystickEdges();
                break;

            case ButtonClicked.darkwindPullRight:
                ToggleAllowReverseDarkwindPull();
                break;

            case ButtonClicked.darkwindPullLeft:
                ToggleAllowReverseDarkwindPull();
                break;

            case ButtonClicked.resetHealth:
                ResetHealth();
                break;

            case ButtonClicked.resetSpellcasting:
                ResetManaLimitOnSpellWords();
                break;

            case ButtonClicked.resetChaindashing:
                ResetChaindashing();
                break;

            case ButtonClicked.resetWalljumping:
                ResetWallJumping();
                break;

            case ButtonClicked.resetMomentum:
                ResetInfiniteSpeedMomentum();
                break;

            case ButtonClicked.resetCheesewall:
                ResetGroundWallSpeedCheese();
                break;

            case ButtonClicked.resetCrouch:
                ResetCrouchDashRestore();
                break;

            case ButtonClicked.resetSameAttack:
                ResetIgnoreSameAttackJoystickInput();
                break;

            case ButtonClicked.resetAcceptAttack:
                ResetAcceptAttackInputOnlyOnJoystickEdges();
                break;

            case ButtonClicked.resetDarkwindPull:
                ResetReverseDarkwindPull();
                break;

            case ButtonClicked.resetWalljumpRefresh:
                ResetWallJumpReset();
                break;

            case ButtonClicked.resetInversedCamera:
                ResetInversedCamera();
                break;

            default:
                break;
        }
    }

    public void NetworkSendButtonClicked(ButtonClicked buttonClicked)
    {
        if (NetworkCommunicationController.globalInstance != null)
            NetworkCommunicationController.globalInstance.SendDarkwindDistortionChange(buttonClicked);
    }

    public void NetworkSendButtonClicked(ButtonClicked buttonRight, bool rightButton)
    {
        if (NetworkCommunicationController.globalInstance != null)
        {
            if (rightButton)
                NetworkCommunicationController.globalInstance.SendDarkwindDistortionChange(buttonRight);
            else
                NetworkCommunicationController.globalInstance.SendDarkwindDistortionChange(buttonRight + 1);
        }
            
    }

}
