using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Should split it into inputStuff, spacebar+shifts, and misc.
public class MageInputManager : MonoBehaviour
{
    public WordManager wordManager;

    /// <summary>
    /// Set to automatically return, when input is disabled.
    /// </summary>
    private MageInputData defaultInputData;

    private MageInputData currentFrameInput;

    //temp/dummy **this** values. More or less copy-pasted to currentFrameInput.
        private bool disableSpells = false;
        private bool aboutToDodgeroll = false;
        private bool dodgerollRight = false;
        private bool aboutToJump = false;
        private string inputString;
        private string spellwordString;

        //Can warrior wall/double Jump with L1? Debugging purposes ofc.
        public bool warriorCanWalljump = true;

        //Can warrior dodgeroll with start/select?
        public bool warriorCanDodgeroll = true;

    //This madness... smh. Should be removed or tweaked near release.
    private bool simulateKeyboard = false;


    //Normally should start via GameManager.Initialize();
    private void Start()
    {
        //Subscribers below
            //Subscribes to WordManager for incoming spells.
            wordManager.spellActivated += SetSpellStringComplete;
            wordManager.incompleteSpellWritten += SetSpellStringIncomplete;

            //Subscribe to re-enable spells.
            transform.parent.gameObject.GetComponent<DialogueManager>().finishedLevelCutscene += EnableSpells;
            transform.parent.gameObject.GetComponent<DialogueManager>().finishedDeathDialogue += EnableSpells;

            //Subscribe to disable spells (on death)
            GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DiedEvent += DisableSpells;

        SetDefaultMageInput();
    }


    //This is called by InputManager
    public MageInputData CalculateInput()
    {
        //===============
        //===Dodgeroll===
        //===============

        //Dodgeroll is Not on cooldown.
        //if (!warriorBehaviour.GetDodgerollCooldown())
        //{
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            //Dodgeroll to happen below
            aboutToDodgeroll = true;

            //Dodgeroll direction
            dodgerollRight = false;
        }
        else if (Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.RightControl))
        {
            //Dodgeroll to happen below
            aboutToDodgeroll = true;

            //Dodgeroll direction
            dodgerollRight = true;
        }
        //}

        //================
        //===Spellwords===
        //================

        //tfw you didnt account for strings/spellwords when you refactored the input system for netcoding
        //So you have to make some really disgusting spagghetti. I honestly hope, no one touches this code.
        //It is a festering abomination... It must be destroyed.
        //Note: The reason I did not refactor it exclusively for netcoding
        //      is because I am waiting to remake spells from scratch, so a letter won't occupy a slot
        //      Hence, more than 3 letters will be able to occupy a letter. (it is why "help" was renamed to "aid")

            //If offline
            if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer())
            {

            }
            else
            {
                inputString = Input.inputString;
                foreach (char pickedLetter in inputString)
                {
                    //Debug.Log("Next char is: " + (int)pickedLetter);

                    if (disableSpells == false)
                        //Converted pickedLetter to small char
                        wordManager.TypeLetter(pickedLetter);

                }
            }




        //=============
        //===Jumping===
        //=============

        //Jump used to be recorded on spellword, but that catches only 1 frame.
        if (Input.GetKey(KeyCode.Space))
            aboutToJump = true;


        //===============
        //===Debugging===
        //===============

        if (GameManager.testing)
        {
            if (warriorCanWalljump && Input.GetButton("ButtonR1"))
                aboutToJump = true;
            if (warriorCanDodgeroll && (Input.GetButtonDown("ButtonStart") || Input.GetButtonDown("ButtonSelect")))
            {
                aboutToDodgeroll = true;
                if (Input.GetButtonDown("ButtonStart"))
                    dodgerollRight = true;
                else
                    dodgerollRight = false;
            }
        }

        //===========
        //===Misc====
        //===========

        //Aka keyboard inputs that shouldnt be stored, because they are useless.
        //Example: Insanity Toggle, Snapshot of the game.
        if (Input.GetKeyDown(KeyCode.End))
            GameObject.FindGameObjectWithTag("BackgroundManager").GetComponent<InsanityToggle>().ToggleInsanity();




        //Set the inputData, to be this.inputData.
        currentFrameInput.aboutToDodgeroll = aboutToDodgeroll;
        currentFrameInput.dodgerollRight = dodgerollRight;
        currentFrameInput.finishedSpellwordString = spellwordString;
        currentFrameInput.aboutToJump = aboutToJump;



        //Reset all values for next frame
        aboutToDodgeroll = false;
        spellwordString = "";
        aboutToJump = false;

        //Then send the result
        return currentFrameInput;
    }

    
    //Given by word manager. You may be wondering what the difference with inputString really is.
    //inputString is fed onto the wordManager to form a word.
    //The spellstring here, is the full word, ready to be inputed as is, onto the warriorBehaviour.
    public void SetSpellStringComplete(string spellWord)
    {
        spellwordString = spellWord;
    }

    public void SetSpellStringIncomplete(string spellWord)
    {
        spellwordString = spellWord;
    }

    public void EnableSpells()
    {
        disableSpells = false;
    }

    public void DisableSpells()
    {
        disableSpells = true;
    }

    public bool GetWarriorAccessToMage()
    {
        return warriorCanWalljump;
    }

    public void ToggleWarriorAccessToMage()
    {
        warriorCanDodgeroll = !warriorCanDodgeroll;
        warriorCanWalljump = !warriorCanWalljump;
    }

    //Used when input is disabled.
    public void SetDefaultMageInput()
    {
        //Create a new inputTimestamp for this frame
        defaultInputData = new MageInputData();

        defaultInputData.aboutToDodgeroll = false;
        defaultInputData.dodgerollRight = false;
        defaultInputData.finishedSpellwordString = "";
        defaultInputData.aboutToJump = false;
    }

    public MageInputData GetDefaultMageInput()
    {
        return defaultInputData;
    }

    public void SetSimulateKeyboard(bool value)
    {
        simulateKeyboard = value;
    }
}
