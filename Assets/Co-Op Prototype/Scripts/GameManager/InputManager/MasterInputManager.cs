using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(WarriorInputManager))]
[RequireComponent(typeof(MageInputManager))]
public class MasterInputManager : MonoBehaviour //Takes input, and runs WarriorMovement.Movement(input);
{
    //So every1 can access it, without searching for tags and bs like that. Aka, a singleton.
    public static MasterInputManager globalInstance { set; get; }

    public MageBehaviour mageBehaviour;
    public WarriorMovement warriorBehaviour;
    public SpeedrunManager speedrunManager;
    public PlayerStatsManager playerStatsManager;

    [Tooltip("Would you like to (approximately) simulate movement with Keyboard?")]
    public bool simulateKeyboard;

    private WarriorInputManager localWarriorInputManager;
    private MageInputManager localMageInputManager;

    private WarriorInputData tempWarriorInputData;
    private MageInputData tempMageInputData;
    private InputTrackingData tempInputTrackingData;

    private bool disableInput = false;






    void Awake()
    {
        //Singleton
        globalInstance = this;

        localWarriorInputManager = GetComponent<WarriorInputManager>();
        localMageInputManager = GetComponent<MageInputManager>();

        if (warriorBehaviour == null)
        {
            Debug.LogError("InputManager, somehow, doesnt have a reference to warriorBehaviour!\nBy default, it is assigned from the inspector, so do confirm that!");
            return;
        }

        if (mageBehaviour == null)
            Debug.LogError("InputManager, somehow, doesn't have a reference to mageBehaviour!\nBy default, it is assigned from the inspector, so do confirm that!");

        //Subscribes to events
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<PauseMenu>().pauseEvent += DisableInput;
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<PauseMenu>().resumeEvent += EnableInput;
        warriorBehaviour.skippedDialogue += EnableSpells;
        warriorBehaviour.skippedRevive += EnableSpells;
    }

    // Update is called once per frame
    void Update()
    {
        //If player wants to switch from controller to keyboard...
        if (Input.GetKeyDown(KeyCode.PageUp) && GameManager.testing)
        {
            simulateKeyboard = !simulateKeyboard;

            localWarriorInputManager.SetSimulateKeyboard(simulateKeyboard);
            localMageInputManager.SetSimulateKeyboard(simulateKeyboard);
        }

        //==================
        //===Speedrunning===
        //==================
        //Tbh, I haven't fully thought on the design of JSON with speedrunning-exclusive input data.
        //Hence, I just dont register it at all, but from my short time of thinking about it
        //I think that making its own JSON/struct for speedrunning inputs ftw.
        //And includes skip+backspace, along with deltaTimeFromLastRegister and ofc, currentRun.
        //But yeah, for now, these are not registered at all.
        //If you want to find a speedrunner's route rn, using position data, you can find if he restarted level(Backspace)
        //and skip level(F12) is irrelevant cuz variables via levelManager.currentLevel, are assigned automatically, and properly.
        //So, with the above^, the below data is actually useless, if you apply some simple logic.

        if (speedrunManager.GetSpeedrunMode() == true)//coulda cache this via event, instead of getting it every frame...
        {
            //If restart run
            if (Input.GetKeyDown(KeyCode.Backspace))
                speedrunManager.RestartCurrentLevelRun();

            //Not needed to store it in any struct or JSON, cuz it auto-skips the level, and that means it changes variable location (levelx to levelx+1)
            //so it can be, theoritically speaking, automatically detected.
            if (Input.GetKeyDown(KeyCode.F12))
                speedrunManager.SkipLevel();
        }


        if (disableInput == false)
        {
            //Do look into CalculateInput() functions ;)
            tempWarriorInputData = localWarriorInputManager.CalculateInput();
            tempMageInputData = localMageInputManager.CalculateInput();
        }
        else
        {
            tempWarriorInputData = localWarriorInputManager.GetDefaultWarriorInput();
            tempMageInputData = localMageInputManager.GetDefaultMageInput();
        }

        //Tracking Input Data for PlayerStats on JSON
        //Create the input data that tracks the inputs of this frame
        tempInputTrackingData = new InputTrackingData();

        //Update it with the values of this frame
        tempInputTrackingData.warriorInputData = tempWarriorInputData;
        tempInputTrackingData.mageInputData = tempMageInputData;
        tempInputTrackingData.deltaTimeFromLastRegister = Time.unscaledDeltaTime;

        //Register the inputData, onto PlayerStats for JSON usage later ;)
        playerStatsManager.UpdateInputTrackingData(tempInputTrackingData);


        //TODO:
        //if (TASBOT and NOT online)
        // {
        //      ref the above structs like above onto network, and update them according to TASBot's input. Player input should override TASBot's.
        //      The TASBot input should be given onto MasterInputManager via a function, so MasterInputManager knows best when to add it.
        // }

        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.hasFinalizedConnection)
        {
            NetworkInputSnapshotManager.globalInstance.ProcessMasterInput(tempInputTrackingData);

            //The default netcoding logic
            if (NetworkDamageShare.globalInstance.IsSynchronized())
                NetworkPositionInterpolationController.globalInstance.EnterMasterInput(tempWarriorInputData, tempMageInputData);
        }
        else//If offline
        {
            warriorBehaviour.Movement(tempWarriorInputData, tempMageInputData);

            mageBehaviour.DetermineSpell(tempMageInputData.finishedSpellwordString);
        }



    }


    public void DisableInput()
    {
        disableInput = true;
    }

    public void EnableInput()
    {
        disableInput = false;
    }

    //When players skip revive or dialogue, this is triggered via event from WarriorMovement.
    public void EnableSpells()
    {
        localMageInputManager.EnableSpells();
    }

    //When final battle, or perhaps something really important
    public void DisableSpells()
    {
        localMageInputManager.DisableSpells();
    }

}
