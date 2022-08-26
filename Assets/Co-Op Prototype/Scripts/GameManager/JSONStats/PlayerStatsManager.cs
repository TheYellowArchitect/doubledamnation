using System.Collections.Generic;
using UnityEngine;

public enum Endings { NoEnding, Insanity, WarriorVictory, MageVictory, Deathless, Perfect };

/// <summary>
/// Processes the PlayerStats. PlayerStats should have no functions, just be a container.
/// </summary>
public class PlayerStatsManager : MonoBehaviour
{
    //So every1 can access it, without searching for tags and bs like that. Aka, a singleton.
    public static PlayerStatsManager globalInstance { set; get; }

    public JSONDataManager jsonDataManager;

    private PlayerCoreStats playerCoreStats;
    private PlayerPathStats playerPathStats;

    /// <summary>
    /// How many kills per run, to get dat soulpowahboosto!
    /// </summary>
    public int killReward = 13;

    //Called from GameManager, so as to get that random seed.
    public void Initialize()
    {
        globalInstance = this;

        InitializePlayerStats();
    }

    //TODO: Different filenames for each run. So a player can open/use many of these files, to make his own heatmap (e.g. where he died the most)
    //TODO: Delete dis.
    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            SavePlayerStats();

        if (Input.GetKeyDown(KeyCode.F2))
        {
            playerCoreStats = jsonDataManager.ReadCoreStatsData();
            playerPathStats = jsonDataManager.ReadPathStatsData();
        }

    }
    */

    /// <summary>
    /// Saves both core stats and path stats, via jsonDataManager
    /// </summary>
    public void SavePlayerStats()
    {
        SaveCoreStats();
        SavePathStats();
    }

    public void SaveCoreStats()
    {
        //Update the much-needed stats, before saving.
        playerCoreStats.lastPlayedDay = System.DateTime.Now.ToShortDateString();
        playerCoreStats.lastPlayedTime = System.DateTime.Now.ToShortTimeString();
        playerCoreStats.totalPlaytimeInSeconds = (int) Time.time;

        jsonDataManager.SaveCoreStatsData(playerCoreStats);
    }

    public void SavePathStats()
    {
        jsonDataManager.SavePathStatsData(playerPathStats);
    }


    public void InitializePlayerStats()
    {
        InitializePlayerCoreStats();
        InitializePlayerPathStats();
    }

    public void InitializePlayerCoreStats()
    {
        playerCoreStats = new PlayerCoreStats();

        //Initialize Damage&Kills
        playerCoreStats.damageTaken = 0;
        playerCoreStats.damageDealt = 0;
        playerCoreStats.currentRunDamageDealt = 0;
        playerCoreStats.totalKills = 0;
        playerCoreStats.currentRunKills = 0;

        //Initialize DialogueInterruptions
        playerCoreStats.dialogueWarriorInterruptions = 0;
        playerCoreStats.dialogueMageInterruptions = 0;

        //Initialize PowerUps
        playerCoreStats.totalPowerUps = 0;
        playerCoreStats.powerUpsLevel1 = 0;
        playerCoreStats.powerUpsLevel2 = 0;
        playerCoreStats.powerUpsLevel3 = 0;
        playerCoreStats.powerUpsLevel4 = 0;

        //Initialize Deaths
        playerCoreStats.totalDeaths = 0;
        playerCoreStats.level1Deaths = 0;
        playerCoreStats.level2Deaths = 0;
        playerCoreStats.level3Deaths = 0;
        playerCoreStats.level4Deaths = 0;

        //Initialize GameMode
        playerCoreStats.speedrunMode = false;
        playerCoreStats.purgeMode = false;
        playerCoreStats.playedAsHost = false;
        playerCoreStats.playedAsClient = false;
        playerCoreStats.steamLobbyId = 0;
        playerCoreStats.steamFriendId = 0;

        //Set RNG Seed
        playerCoreStats.currentRunSeed = Random.state;

        //Initialize LastPlayed
        playerCoreStats.lastPlayedDay = System.DateTime.Now.ToShortDateString();
        playerCoreStats.lastPlayedTime = System.DateTime.Now.ToShortTimeString();

        //Initialize Ending
        playerCoreStats.endingDone = Endings.NoEnding;
    }

    public void InitializePlayerPathStats()
    {
        playerPathStats = new PlayerPathStats();

        //Initialize DeathPositions
        playerPathStats.level1DeathPositions = new List<Vector2>();
        playerPathStats.level2DeathPositions = new List<Vector2>();
        playerPathStats.level3DeathPositions = new List<Vector2>();
        playerPathStats.level4DeathPositions = new List<Vector2>();

        //Initialize PlayerTrackingPositionsData
        playerPathStats.level0Positions = new List<List<PlayerPositionTrackingData>>();
        playerPathStats.level1Positions = new List<List<PlayerPositionTrackingData>>();
        playerPathStats.level2Positions = new List<List<PlayerPositionTrackingData>>();
        playerPathStats.level3Positions = new List<List<PlayerPositionTrackingData>>();
        playerPathStats.level4Positions = new List<List<PlayerPositionTrackingData>>();

        //Initialize InputData
        playerPathStats.level0Inputs = new List<List<InputTrackingData>>();
        playerPathStats.level1Inputs = new List<List<InputTrackingData>>();
        playerPathStats.level2Inputs = new List<List<InputTrackingData>>();
        playerPathStats.level3Inputs = new List<List<InputTrackingData>>();
        playerPathStats.level4Inputs = new List<List<InputTrackingData>>();
    }

    public void SetPlayerStatsSaveID(int value)
    {
        playerCoreStats.playerStatsID = value;
        playerPathStats.playerStatsID = value;
    }

    public void UpdateInputTrackingData(InputTrackingData dataToRegister)
    {
        if (LevelManager.currentLevel == 0)
            AddPlayerPathInput(ref playerPathStats.level0Inputs, dataToRegister);
        else if (LevelManager.currentLevel == 1)
            AddPlayerPathInput(ref playerPathStats.level1Inputs, dataToRegister);
        else if (LevelManager.currentLevel == 2)
            AddPlayerPathInput(ref playerPathStats.level2Inputs, dataToRegister);
        else if (LevelManager.currentLevel == 3)
            AddPlayerPathInput(ref playerPathStats.level3Inputs, dataToRegister);
        else if (LevelManager.currentLevel == 4)
            AddPlayerPathInput(ref playerPathStats.level4Inputs, dataToRegister);
    }

    public void UpdatePlayerPositionTrackingData(PlayerPositionTrackingData dataToRegister)
    {
        if (LevelManager.currentLevel == 0)
            AddPlayerPathPosition(ref playerPathStats.level0Positions, dataToRegister);
        else if (LevelManager.currentLevel == 1)
            AddPlayerPathPosition(ref playerPathStats.level1Positions, dataToRegister);
        else if (LevelManager.currentLevel == 2)
            AddPlayerPathPosition(ref playerPathStats.level2Positions, dataToRegister);
        else if (LevelManager.currentLevel == 3)
            AddPlayerPathPosition(ref playerPathStats.level3Positions, dataToRegister);
        else if (LevelManager.currentLevel == 4)
            AddPlayerPathPosition(ref playerPathStats.level4Positions, dataToRegister);
    }

    /// <summary>
    /// Used by Rewind spell, fetches the position exactly timeSlotInSeconds ago!
    /// NOT USED ANYMORE!
    /// </summary>
    /// <param name="timeSlotInSeconds"></param>
    /// <returns></returns>
    public PlayerPositionTrackingData GetPlayerPositionTrackingData(int timeSlotInSeconds)//Convert as close as possible to int
    {
        //=====================================================================================================
        //==First of all check if you can rewind that far back, since at start of the level, thats a problem.==
        //=====================================================================================================
        bool nothingPrior = false;

        //Check if any slot exists prior so i dont go out of argument index -_-
        if (LevelManager.currentLevel == 0 && playerPathStats.level0Positions[GetTotalDeaths()].Count - -timeSlotInSeconds * 6 < 0)//-timeslotInSeconds * 6
            nothingPrior = true;
        else if (LevelManager.currentLevel == 1 && playerPathStats.level1Positions[GetTotalDeaths()].Count - timeSlotInSeconds * 6 < 0)
            nothingPrior = true;
        else if (LevelManager.currentLevel == 2 && playerPathStats.level2Positions[GetTotalDeaths()].Count - timeSlotInSeconds * 6 < 0)
            nothingPrior = true;
        else if (LevelManager.currentLevel == 3 && playerPathStats.level3Positions[GetTotalDeaths()].Count - timeSlotInSeconds * 6 < 0)
            nothingPrior = true;
        else if (LevelManager.currentLevel == 4 && playerPathStats.level4Positions[GetTotalDeaths()].Count - timeSlotInSeconds * 6 < 0)
            nothingPrior = true;

        //Do anything if any slot doesnt exist prior. Consider this as returning -1; in classic programming.
        if (nothingPrior)
        {
            PlayerPositionTrackingData tempTracking = new PlayerPositionTrackingData();
            tempTracking.deltaTimeFromLastRegister = 48f;//So it can detect if it should go at the start.
            return tempTracking;
        }

        //==============================================================================================================
        //==Now that we are done with trivial checks above, go for giving the accurate position timeSlotInSeconds ago!==
        //==============================================================================================================

        if (LevelManager.currentLevel == 0)
            return playerPathStats.level0Positions[GetTotalDeaths()][playerPathStats.level0Positions.Count - timeSlotInSeconds * 6];//6 frames = 1 second
        else if (LevelManager.currentLevel == 1)
            return playerPathStats.level1Positions[GetTotalDeaths()][playerPathStats.level1Positions.Count - timeSlotInSeconds * 6];//6 frames = 1 second
        else if (LevelManager.currentLevel == 2)
            return playerPathStats.level2Positions[GetTotalDeaths()][playerPathStats.level2Positions.Count - timeSlotInSeconds * 6];//6 frames = 1 second
        else if (LevelManager.currentLevel == 3)
            return playerPathStats.level3Positions[GetTotalDeaths()][playerPathStats.level3Positions.Count - timeSlotInSeconds * 6];//6 frames = 1 second
        else if (LevelManager.currentLevel == 4)
            return playerPathStats.level4Positions[GetTotalDeaths()][playerPathStats.level4Positions.Count - timeSlotInSeconds * 6];//6 frames = 1 second
        else
            return new PlayerPositionTrackingData();//Impossible to get here, putting it just because compiler says nO oThEr PaTh rEtUrN bs
    }

    //Wrapper
    public InputTrackingData GetInputMillisecondsAgo(float secondsBeforeFromCurrentToFind)
    {
        return GetInputSecondsAgo(secondsBeforeFromCurrentToFind / 1000);
    }

    //Needed for netcoding -_-
    public InputTrackingData GetInputSecondsAgo(float secondsBeforeFromCurrentToFind)
    {
        InputTrackingData failedInputTrackingData = new InputTrackingData();
        failedInputTrackingData.deltaTimeFromLastRegister = -99f;

        List<InputTrackingData> levelInputs = new List<InputTrackingData>();

        if (LevelManager.currentLevel == 0)
            levelInputs = playerPathStats.level0Inputs[GetTotalDeaths()];
        else if (LevelManager.currentLevel == 1)
            levelInputs = playerPathStats.level1Inputs[GetTotalDeaths()];
        else if (LevelManager.currentLevel == 2)
            levelInputs = playerPathStats.level2Inputs[GetTotalDeaths()];
        else if (LevelManager.currentLevel == 3)
            levelInputs = playerPathStats.level3Inputs[GetTotalDeaths()];
        else// if (LevelManager.currentLevel == 4)
            levelInputs = playerPathStats.level4Inputs[GetTotalDeaths()];

        //If input exists -> 99% of the cases it exists -> Except new levels lmao!
        if (levelInputs.Count - secondsBeforeFromCurrentToFind * 6 <= 0)
            return failedInputTrackingData;


        /*Debug.Log("We reach here");
        if (LevelManager.currentLevel == 1)
        {
            for (int i = 0; i < levelInputs.Count; i++)
            {
                if (levelInputs[i].warriorInputData.movementInputDirection.x > 0.9f)
                    Debug.LogError("i is: " + i);
            }
        }*/


        //This adds the deltaTime of each input
        float timeAccumulatedBeforeCurrent = 0;

        //index starting at the end and going backwards
        for (int i = levelInputs.Count - 1; i > 0; i--)
        {
            timeAccumulatedBeforeCurrent = timeAccumulatedBeforeCurrent + levelInputs[i].deltaTimeFromLastRegister;

            //We went just a bit over it - doesn't matter.
            if (timeAccumulatedBeforeCurrent >= secondsBeforeFromCurrentToFind)
            {
                Debug.Log("i is: " + i + "latest i is: " + (levelInputs.Count - 1));
                return levelInputs[i];
            }

        }

        //How can player detect this?
        return failedInputTrackingData;
    }

    //Needed for Netcoding -_-
    //When a warrior input is received by the mage/client, it instantly registers the input as the latest
    //(Though doesn't run it immediately, because we are running 200 ms behind host)
    public void NetworkUpdateLatestWarriorLeftJoystick(Vector2 newLeftJoystick)
    {
        InputTrackingData latestClone = new InputTrackingData();

        //If level -> Clone, change, then replace original with clone ;)
        if (LevelManager.currentLevel == 0)
        {
            if (playerPathStats.level0Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level0Inputs[GetTotalDeaths()][playerPathStats.level0Inputs.Count - 1];
            latestClone.warriorInputData.movementInputDirection = newLeftJoystick;
            playerPathStats.level0Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level0Inputs.Count - 1);
            playerPathStats.level0Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 1)
        {
            //Debug.Log("Max input slots: " + playerPathStats.level1Inputs[GetTotalDeaths()].Count);

            if (playerPathStats.level1Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level1Inputs[GetTotalDeaths()][playerPathStats.level1Inputs.Count - 1];
            latestClone.warriorInputData.movementInputDirection = newLeftJoystick;
            playerPathStats.level1Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level1Inputs.Count - 1);
            playerPathStats.level1Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 2)
        {
            if (playerPathStats.level2Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level2Inputs[GetTotalDeaths()][playerPathStats.level2Inputs.Count - 1];
            latestClone.warriorInputData.movementInputDirection = newLeftJoystick;
            playerPathStats.level2Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level2Inputs.Count - 1);
            playerPathStats.level2Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 3)
        {
            if (playerPathStats.level3Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level3Inputs[GetTotalDeaths()][playerPathStats.level3Inputs.Count - 1];
            latestClone.warriorInputData.movementInputDirection = newLeftJoystick;
            playerPathStats.level3Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level3Inputs.Count - 1);
            playerPathStats.level3Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 4)
        {
            if (playerPathStats.level4Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level4Inputs[GetTotalDeaths()][playerPathStats.level4Inputs.Count - 1];
            latestClone.warriorInputData.movementInputDirection = newLeftJoystick;
            playerPathStats.level4Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level4Inputs.Count - 1);
            playerPathStats.level4Inputs[GetTotalDeaths()].Add(latestClone);
        }
    }

    //Needed for Netcoding -_-
    //When a warrior input is received by the mage/client, it instantly registers the input as the latest
    //(Though doesn't run it immediately, because we are running 200 ms behind host)
    public void NetworkUpdateLatestWarriorRightJoystick(Vector2 newRightJoystick)
    {
        InputTrackingData latestClone = new InputTrackingData();

        //If level -> Clone, change, then replace original with clone ;)
        if (LevelManager.currentLevel == 0)
        {
            if (playerPathStats.level0Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level0Inputs[GetTotalDeaths()][playerPathStats.level0Inputs.Count - 1];
            latestClone.warriorInputData.combatInputDirection = newRightJoystick;
            playerPathStats.level0Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level0Inputs.Count - 1);
            playerPathStats.level0Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 1)
        {
            if (playerPathStats.level1Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level1Inputs[GetTotalDeaths()][playerPathStats.level1Inputs.Count - 1];
            latestClone.warriorInputData.combatInputDirection = newRightJoystick;
            playerPathStats.level1Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level1Inputs.Count - 1);
            playerPathStats.level1Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 2)
        {
            if (playerPathStats.level2Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level2Inputs[GetTotalDeaths()][playerPathStats.level2Inputs.Count - 1];
            latestClone.warriorInputData.combatInputDirection = newRightJoystick;
            playerPathStats.level2Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level2Inputs.Count - 1);
            playerPathStats.level2Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 3)
        {
            if (playerPathStats.level3Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level3Inputs[GetTotalDeaths()][playerPathStats.level3Inputs.Count - 1];
            latestClone.warriorInputData.combatInputDirection = newRightJoystick;
            playerPathStats.level3Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level3Inputs.Count - 1);
            playerPathStats.level3Inputs[GetTotalDeaths()].Add(latestClone);
        }
        else if (LevelManager.currentLevel == 4)
        {
            if (playerPathStats.level4Inputs[GetTotalDeaths()].Count == 0)
                return;

            latestClone = playerPathStats.level4Inputs[GetTotalDeaths()][playerPathStats.level4Inputs.Count - 1];
            latestClone.warriorInputData.combatInputDirection = newRightJoystick;
            playerPathStats.level4Inputs[GetTotalDeaths()].RemoveAt(playerPathStats.level4Inputs.Count - 1);
            playerPathStats.level4Inputs[GetTotalDeaths()].Add(latestClone);
        }
    }

    //Currently called only by CheckpointMusicTrigger.
    public bool IsFirstRun()
    {
        if (LevelManager.currentLevel == 0 && playerCoreStats.totalDeaths == 0)
            return true;
        else if (LevelManager.currentLevel == 1 && playerCoreStats.level1Deaths == 0)
            return true;
        else if (LevelManager.currentLevel == 2 && playerCoreStats.level2Deaths == 0)
            return true;
        else if (LevelManager.currentLevel == 3 && playerCoreStats.level3Deaths == 0)
            return true;
        else if (LevelManager.currentLevel == 4 && playerCoreStats.level4Deaths == 0)
            return true;

        return false;
    }

    public void ResetCurrentRunStats()
    {
        playerCoreStats.currentRunKills = 0;
        playerCoreStats.currentRunDamageDealt = 0;
    }

    public void IncreaseDamageTakenCount()
    {
        playerCoreStats.damageTaken++;
    }

    public void IncreaseDamageDealtCount()
    {
        playerCoreStats.damageDealt++;
        playerCoreStats.currentRunDamageDealt++;
    }

    public void IncreaseKillCount()
    {
        playerCoreStats.totalKills++;
        playerCoreStats.currentRunKills++;

        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            return;//Synchronized client should never trigger powerup

        if (playerCoreStats.currentRunKills % killReward == 0)
            GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().RewardTempHP(GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().NormalMaxHealth);
    }

    public void DecreaseKillCount()
    {
        playerCoreStats.totalKills--;
        playerCoreStats.currentRunKills--;

        if (playerCoreStats.totalKills < 0)
            playerCoreStats.totalKills = 0;
        
        if (playerCoreStats.currentRunKills < 0)
            playerCoreStats.currentRunKills = 0;
    }

    public void IncreaseDeathCount()
    {
        playerCoreStats.totalDeaths++;

        if (LevelManager.currentLevel == 1)
            playerCoreStats.level1Deaths++;
        else if (LevelManager.currentLevel == 2)
            playerCoreStats.level2Deaths++;
        else if (LevelManager.currentLevel == 3)
            playerCoreStats.level3Deaths++;
        else if (LevelManager.currentLevel == 4)
            playerCoreStats.level4Deaths++;
    }

    /// <summary>
    /// Stores where the player died in the playerStats!
    /// </summary>
    /// <param name="deathPosition"></param>
    public void UpdateDeathPosition(Vector3 deathPosition)
    {
        if (LevelManager.currentLevel == 1)
            playerPathStats.level1DeathPositions.Add(deathPosition);
        else if (LevelManager.currentLevel == 2)
            playerPathStats.level2DeathPositions.Add(deathPosition);
        else if (LevelManager.currentLevel == 3)
            playerPathStats.level3DeathPositions.Add(deathPosition);
        else if (LevelManager.currentLevel == 4)
            playerPathStats.level4DeathPositions.Add(deathPosition);
        //else dont store the death anywhere.
        //you can see if player died at level 0, by comparing the length of this list and the total deaths.
    }

    //ClearedLevel() functions below, called from SpeedrunManager()
    public void ClearedLevel0()
    {
        //Obviously not the actual time, but w/e, who cares since this is not part of the speedrun, but its just a tutorial.
        playerCoreStats.timeToBeatLevel0 = Time.time;
    }

    public void ClearedLevel1(double timeToClearLevel)
    {
        playerCoreStats.timeToBeatLevel1 = timeToClearLevel;
    }

    public void ClearedLevel2(double timeToClearLevel)
    {
        playerCoreStats.timeToBeatLevel2 = timeToClearLevel;
    }

    public void ClearedLevel3(double timeToClearLevel)
    {
        playerCoreStats.timeToBeatLevel3 = timeToClearLevel;
    }

    public void ClearedLevel4(double timeToClearLevel)
    {
        playerCoreStats.timeToBeatLevel4 = timeToClearLevel;
    }

    //Invoked only from SpeedrunManager
    public void SetSpeedrunMode(bool value)
    {
        playerCoreStats.speedrunMode = value;
    }

    //TODO: Change this, to change it into program files, so ppl can take the saves easily.
    public void PrintPersistentDataPath()
    {
        Debug.Log(Application.persistentDataPath);
    }


    public void AddPlayerPathInput(ref List<List<InputTrackingData>> levelInput, InputTrackingData inputData)
    {
        //If levelInput has that slot, null
        //Each [] is a death slot! So, going at [1] you see the input of second death!
        while (GetTotalDeaths() >= levelInput.Count)
            levelInput.Add(new List<InputTrackingData>());

        levelInput[GetTotalDeaths()].Add(inputData);
    }

    public void AddPlayerPathPosition(ref List<List<PlayerPositionTrackingData>> levelInput, PlayerPositionTrackingData positionData)
    {
        //If levelInput has that slot, null
        //Each [] is a death slot! So, going at [1] you see the input of second death!
        while (GetTotalDeaths() >= levelInput.Count)
            levelInput.Add(new List<PlayerPositionTrackingData>());

        levelInput[GetTotalDeaths()].Add(positionData);
    }

    public bool TriggersPowerUp(int killsToAdd)
    {
        for (int i = 1; i < killsToAdd + 1; i++)
        {
            if ((playerCoreStats.currentRunKills + i) % killReward == 0)
                return true;
        }

        return false;
    }



    //Get+Set functions
    public int GetTotalDeaths()
    {
        return playerCoreStats.totalDeaths;
    }

    public int GetTotalKills()
    {
        return playerCoreStats.totalKills;
    }

    public int GetCurrentKills()
    {
        return playerCoreStats.currentRunKills;
    }

    public void SetTotalDeaths(int newTotalDeaths)
    {
        playerCoreStats.totalDeaths = newTotalDeaths;
    }

    public void SetTotalKills(int newTotalKills)
    {
        playerCoreStats.totalKills = newTotalKills;
    }

    public void SetCurrentKills(int newCurrentKills)
    {
        playerCoreStats.currentRunKills = newCurrentKills;
    }

    public int GetInterruptionsByWarrior()
    {
        return playerCoreStats.dialogueWarriorInterruptions;
    }

    public int GetInterruptionsByMage()
    {
        return playerCoreStats.dialogueMageInterruptions;
    }

    public void SetInterruptionsByWarrior(int newWarriorInterruptions)
    {
        playerCoreStats.dialogueWarriorInterruptions = newWarriorInterruptions;
    }

    public void SetInterruptionsByMage(int newMageInterruptions)
    {
        playerCoreStats.dialogueMageInterruptions = newMageInterruptions;
    }

    public int GetTotalPowerUps()
    {
        return playerCoreStats.totalPowerUps;
    }

    public void SetTotalPowerUps(int newPowerUpCount)
    {
        playerCoreStats.totalPowerUps = newPowerUpCount;
    }

    public Random.State GetRandomSeed()
    {
        return playerCoreStats.currentRunSeed;
    }

    public void SetRandomSeed(Random.State seedToSet)
    {
        playerCoreStats.currentRunSeed = seedToSet;
    }

    /// <summary>
    /// JSONDataManager WRAPPER
    /// </summary>
    public void SetPlayerClear()
    {
        jsonDataManager.SetPlayerClear();
    }

    public bool GetPlayerClear()
    {
        return jsonDataManager.GetPlayerClear();
    }



    //Pure Get Functions
    public bool GetSpeedrunMode()
    {
        return playerCoreStats.speedrunMode;
    }

    public int GetMaxStatsSaveID()
    {
        return jsonDataManager.GetMaxStatsSaveID();
    }

    public int GetPreviousCoreStatsTotalKills()
    {
        //Get previous coreStatsData
        PlayerCoreStats previousCoreStats = jsonDataManager.GetOffsetCoreStatsData(-1);

        return previousCoreStats.totalKills;
    }

    public int GetPreviousCoreStatsTotalDeaths()
    {
        //Get previous coreStatsData
        PlayerCoreStats previousCoreStats = jsonDataManager.GetOffsetCoreStatsData(-1);

        return previousCoreStats.totalDeaths;
    }

    /// <summary>
    /// Checks previous JSON's ending
    /// </summary>
    /// <returns></returns>
    public Endings GetPreviousEnding()
    {
        if (GetMaxStatsSaveID() == 0)
            return Endings.NoEnding;

        return jsonDataManager.GetOffsetCoreStatsData(-1).endingDone;
        /*
        if (GetPreviousCoreStatsTotalDeaths() == 0)
        {
            if (GetPreviousCoreStatsTotalKills() == 0)
                return Endings.Perfect;
            else
                return Endings.Deathless;
        }
        */
    }

    /// <summary>
    /// So you won't read/write for each ending check in JSON
    /// NoEnding, P1/P2 Endings return false, the rest return true
    /// </summary>
    /// <returns></returns>
    public bool IsPreviousEndingAlternative()
    {
        if (GetMaxStatsSaveID() == 0)
            return false;

        Endings previousEnding = jsonDataManager.GetOffsetCoreStatsData(-1).endingDone;

        //If no file exists at all, it returns default ending, aka noEnding ;)
        if (previousEnding == Endings.Insanity || previousEnding == Endings.Deathless || previousEnding == Endings.Perfect)
            return true;
        else
            return false;
    }

    public void SetEnding(Endings endingToSet)
    {
        playerCoreStats.endingDone = endingToSet;
    }

    public void SetPlayedAsHost(bool targetBool)
    {
        playerCoreStats.playedAsHost = targetBool;
    }

    public void SetPlayedAsClient(bool targetBool)
    {
        playerCoreStats.playedAsClient = targetBool;
    }

    public void SetSteamLobbyId(ulong lobbyID)
    {
        playerCoreStats.steamLobbyId = lobbyID;
    }

    public void SetSteamFriendId(ulong friendID)
    {
        playerCoreStats.steamFriendId = friendID;
    }


    public void IncreasePowerUpCount()
    {
        playerCoreStats.totalPowerUps++;

        if (LevelManager.currentLevel == 1)
            playerCoreStats.powerUpsLevel1++;
        else if (LevelManager.currentLevel == 2)
            playerCoreStats.powerUpsLevel2++;
        else if (LevelManager.currentLevel == 3)
            playerCoreStats.powerUpsLevel3++;
        else if (LevelManager.currentLevel == 4)
            playerCoreStats.powerUpsLevel4++;
    }

    public void IncreaseInterruptionsByWarrior()
    {
        playerCoreStats.dialogueWarriorInterruptions++;
    }

    public void IncreaseInterruptionsByMage()
    {
        playerCoreStats.dialogueMageInterruptions++;
    }






}