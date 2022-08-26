using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

//This should subscribe to OnLoadLevel invocation!
//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [MinValue(-1)]
    [Tooltip("The level player is currently located")]//Indicates the checkpoint.
    public static int currentLevel;

        //-2->Boot?
        //-1->Intro/Boat
        //0 ->Tutorial
        //1 ->Earth
        //2 ->Fire
        //3 ->Wind/Oblivion
        //4 ->CreditBosses
        //5 ->Finalbaws
        //6 ->Epilogue
        //7 -> Level Editor

    public static int levelCutsceneDialogueIndex;

    public static LevelManager globalInstance;

    [Header("Storages")]
    private GameObject corpseStorage;//The object under which all corpses are listed/held, so it won't pollute the object hierarchy.

    private GameObject pillarStorage;//The object under which all pillars are held, so it won't pollute the object hierarchy

    [ReadOnly]
    [Tooltip("All of the mana coins ~~coins~~ in this level, in total")]
    public List<GameObject> manaCoinsList = new List<GameObject>();

    [ReadOnly]
    [Tooltip("All of the enemies/monsters in this level, in total")]
    public List<GameObject> enemyList = new List<GameObject>();

    [ReadOnly]
    [Tooltip("All of the corpses in this level, in total, directly one-by-one from enemyList")]
    public List<GameObject> enemyCorpses = new List<GameObject>();

    [ReadOnly]
    [Tooltip("All of the enemy chasing obstructed player booleans in this level, in total, directly one-by-one from enemyList\nUsed in netcoding")]
    public List<bool> enemyChaseObstructedPlayer;

    [ReadOnly]
    [Tooltip("All of the enemies/monsters's pathfinder component in this level, caching FTW!")]
    public Dictionary<GameObject, EnemyPathfinder> enemyPathfinderDictionary = new Dictionary<GameObject, EnemyPathfinder>();

    [ReadOnly]
    [Tooltip("All of the enemies/monsters's behaviour component in this level, caching FTW!")]
    public Dictionary<GameObject, EnemyBehaviour> enemyBehaviourDictionary = new Dictionary<GameObject, EnemyBehaviour>();
    public Dictionary<ushort, EnemyBehaviour> enemyBehaviourIndexDictionary = new Dictionary<ushort, EnemyBehaviour>();

    [ReadOnly]
    [Tooltip("All of the moving platforms' falling platform behaviour component in this level.")]
    public List<FallingPlatform> fallingPlatformList = new List<FallingPlatform>();

    private bool hasSubscribedRestart = false;

    public void Start()
    {
        globalInstance = this;
    }

    //When player dies
    public void RestartLevel()
    {
        RestartCoins();

        RestartEnemies();

        RestartCorpses();

        RestartFallingPlatforms();

        DeletePillars();
    }

    public void DetermineStorage()
    {
        if (corpseStorage == null)
        {
            if (GameObject.FindGameObjectWithTag("CorpseStorage") != null)
                corpseStorage = GameObject.FindGameObjectWithTag("CorpseStorage");
        }

        if (pillarStorage == null)
        {
            if (GameObject.FindGameObjectWithTag("PillarStorage") != null)
                pillarStorage = GameObject.FindGameObjectWithTag("PillarStorage");
        }
    }

    //Stores all the coins in this level.
    public void StoreCoins()
    {
        manaCoinsList = GameObject.FindGameObjectsWithTag("Coin").ToList<GameObject>();
    }

    public void StoreEnemies()
    {
        enemyList = GameObject.FindGameObjectsWithTag("Enemy").ToList<GameObject>();

        //Foreach cant fucking index
        for (ushort i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].GetComponent<EnemyBehaviour>().enemyListIndex = i;
            enemyBehaviourIndexDictionary.Add(i, enemyList[i].GetComponent<EnemyBehaviour>());
        }
            

        //Store Pathfinder & Behaviour
        foreach (GameObject pickedEnemy in enemyList)
        {
            enemyPathfinderDictionary.Add(pickedEnemy, pickedEnemy.GetComponent<EnemyPathfinder>());
            enemyBehaviourDictionary.Add(pickedEnemy, pickedEnemy.GetComponent<EnemyBehaviour>());
        }

        
    }

    public void StoreCorpses()
    {
        //Iterate through enemies 
        foreach (GameObject pickedEnemy in enemyList)
            enemyCorpses.Add(enemyPathfinderDictionary[pickedEnemy].GetCorpse());

        if (corpseStorage != null)
            foreach (GameObject corpse in enemyCorpses)//not optimized since i could use the above more but ah well
                corpse.transform.SetParent(corpseStorage.transform);
    }

    public void StoreChaseObstructedPlayer()
    {
        enemyChaseObstructedPlayer = new List<bool>();

        //Iterate through enemies
        foreach (GameObject pickedEnemy in enemyList)
            enemyChaseObstructedPlayer.Add(pickedEnemy.GetComponent<EnemyBehaviour>().chasesObstructions);
    }

    public void StoreFallingPlatforms()
    {
        List<GameObject> fallingPlatformGameobjectList = GameObject.FindGameObjectsWithTag("Platform").ToList<GameObject>();

        FallingPlatform tempFallingPlatform;

        //Iterate through every platform
        foreach(GameObject pickedPlatform in fallingPlatformGameobjectList)
        {
            tempFallingPlatform = pickedPlatform.GetComponent<FallingPlatform>();
            if (tempFallingPlatform != null)
                fallingPlatformList.Add(tempFallingPlatform);
        }
    }

    //Resets all the coins that player gathered.
    public void RestartCoins()
    {
        foreach (GameObject pickedCoin in manaCoinsList)
        {
            if (pickedCoin.activeSelf == false)
                pickedCoin.SetActive(true);
        }
    }

    //Resets all the enemies stored
    public void RestartEnemies()
    {
        Debug.Log("RESTARTING");

        foreach (GameObject pickedEnemy in enemyList)
        {
            //If dead
            if (pickedEnemy.activeSelf == false)
            {
                //Reactivates the gameobject/enemy
                pickedEnemy.SetActive(true);

                //pathfinder re-activates all components, and calls the revive shown below as well :P
                enemyPathfinderDictionary[pickedEnemy].enabled = true;
            }
            else //Alive.
            {
                //Resets hp and origin position.
                enemyBehaviourDictionary[pickedEnemy].Revive();
            }
            //Revive happens in both anyway, btw.
        }
    }

    public void RestartCorpses()
    {
        foreach (GameObject pickedCorpse in enemyCorpses)
            pickedCorpse.SetActive(false);
    }

    public void RestartFallingPlatforms()
    {
        foreach (FallingPlatform pickedFallingPlatform in fallingPlatformList)
            pickedFallingPlatform.ResetPlatform();
    }

    //When next level, delete dis
    public void DeleteCoins()
    {
        manaCoinsList.Clear();

        //foreach (GameObject pickedCoin in manaCoinsList)
           // manaCoinsList.Remove(pickedCoin);
    }

    //When next level, delete the enemies, and their references to pathfinder/behaviour.
    public void DeleteEnemies()
    {
        enemyPathfinderDictionary.Clear();
        enemyBehaviourDictionary.Clear();
        enemyBehaviourIndexDictionary.Clear();

        enemyList.Clear();
    }

    //Runs pre-level load, since the corpses are automatically loaded on levelLoad, and deleting destroys the new ones. ((single responsibility failed REE))
    public void DeleteCorpses()
    {
        foreach (GameObject pickedCorpse in enemyCorpses)
            GameObject.Destroy(pickedCorpse);

        enemyCorpses.Clear();
    }

    public void DeleteFallingPlatforms()
    {
        fallingPlatformList.Clear();
    }

    public void DeletePillars()
    {
        if (pillarStorage != null)
            for (int i = 0; i < pillarStorage.transform.childCount; i++)
                GameObject.Destroy(pillarStorage.transform.GetChild(i).gameObject);
    }

    //==========
    //Finds checkpoint on level load, and sets it to Player's
    public void SetWarriorCheckpoint()
    {
        //Retarded tagfinding, should cache the player but w/e
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().UpdateCheckpointPosition(GameObject.FindGameObjectWithTag("Checkpoint").transform.position);
    }

    public void SetRestartLevelEvent()
    {
        if (hasSubscribedRestart == true)
            return;

        hasSubscribedRestart = true;
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedRevive += RestartLevel;
    }

    //Used externally, by NetworkSynchronizer
    public void SetChaseObstructedPlayerWrapper(List<bool> newList)
    {
        enemyChaseObstructedPlayer = new List<bool>(newList);

        //Kinda rare bug when client enters tutorial before host (both were on boat/intro)
        //This bugfix doesnt set chase obstructed properly, just avoids the error leading to crash.
        if (enemyList.Count == 0 || enemyChaseObstructedPlayer.Count == 0)
            Invoke("SetChaseObstructedPlayer", 5);
        else
            SetChaseObstructedPlayer();
    }

    public void SetChaseObstructedPlayer()
    {
        if (enemyList.Count == 0)
        {
            Debug.LogError("Somehow, enemyList.Count is 0. Theoritically impossible, but practically real.");
            return;
        }

        if (enemyChaseObstructedPlayer.Count == 0)
        {
            Debug.LogWarning("Host had sent chaseObstructed list when he was on the boat, and so the list is empty.");//True bugfix is requesting to re-send.
            return;
        }

        //Debug.Log("LocalList Count is: " + enemyList.Count);
        //Debug.Log("ReceivedList Count is: " + enemyChaseObstructedPlayer.Count);

        //Set the list, since without this we have a list which isn't applied to the monsters!
        for (int i = 0; i < enemyList.Count; i++)
            enemyList[i].GetComponent<EnemyBehaviour>().chasesObstructions = enemyChaseObstructedPlayer[i];
            
    }

    public static void IncreaseCutsceneIndex()
    {
        levelCutsceneDialogueIndex++;
    }

    public static void SetCutsceneIndex(int targetCutsceneDialogueIndex)
    {
        levelCutsceneDialogueIndex = targetCutsceneDialogueIndex;
    }

    public static void IncreaseLevelIndex()
    {
        currentLevel++;
    }

    //Called 0.1f right after a level loads.
    public void OnLevelLoad(string levelName)
    {
        //DetermineLevelIndex(levelName);

        DeleteCoins();

        DeleteEnemies();

        //DeleteCorpses();//Now runs at pre-load

        DeletePillars();

        SetBackground();

        ScreenFade();

        NetworkLevelMatcherCallback();//If online and same level and havent finalized connection, callback networklevelmatcher to connect

        SteamRichPresenceUpdate();

        //If Final Scene 7 == "The End" or Scene 8 == "Intro", it won't play what is below
        if (SceneManager.GetSceneByBuildIndex(7).isLoaded == true || SceneManager.GetSceneByBuildIndex(8).isLoaded == true)
            return;

        DeleteFallingPlatforms();

        DetermineStorage();

        StoreCoins();

        StoreEnemies();

        StoreCorpses();

        StoreChaseObstructedPlayer();

        StoreFallingPlatforms();

        SetWarriorCheckpoint();

        SetRestartLevelEvent();

        ResetCamera();

        ResetPlayer();

        ResetMusicTransitionTrigger();//Maybe needs to be put above the {}

        MenuUpdate();

        SetCursor();

        DetermineMusic();//Maybe needs to be put above the {}

        DetermineSpellWords();

        DetermineUniqueLevelLogic();
    }

    public void OnLevelPreLoad(string levelName)
    {
        DeleteCorpses();
    }

    public void DetermineMusic()
    {
        MusicSoundManager.globalInstance.OnLevelLoad();
    }

    public void DetermineSpellWords()
    {
        AddSpellWords.globalInstance.OnLevelLoad();
    }

    public void DetermineUniqueLevelLogic()
    {
        DetermineKillMeter();

        DetermineLevelEditor();
    }

    public void DetermineKillMeter()
    {
        //Fire level -> Starts the blood effect
        if (currentLevel == 2)
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<KillMeterManager>().StartBloodMeter();
        else
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<KillMeterManager>().StopBloodMeter();
    }

    public void DetermineLevelEditor()
    {
        //Currently checks only Network
        if (currentLevel == 7)
        {
            if (NetworkCommunicationController.globalInstance != null)
                GameObject.FindObjectOfType<NetworkLevelEditor>().Activate();

            SettingsMenu.globalInstance.MuteMusic();//What a hack lol
        }
    }

    public void ResetMusicTransitionTrigger()
    {
        GameObject.FindGameObjectWithTag("MusicTransition").GetComponent<CheckpointMusicTrigger>().OnLevelLoad();
    }

    public void ResetCamera()
    {
        if (LevelManager.currentLevel != 7)
            GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().OnLevelLoad();
    }

    public void ResetPlayer()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().OnLevelLoad();
    }

    public void SetBackground()
    {
        //If not going to level editor
        if (LevelManager.currentLevel != 7)
            GameObject.FindGameObjectWithTag("BackgroundManager").GetComponent<BackgroundLevelChange>().OnLevelLoad();
        //if (currentLevel == 3)
            //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().backgroundColor = new Color(128, 128, 128, 0);//bugged af. find more details (that camera script) and report it to unity.
    }

    public void SetCursor()
    {
        if (CursorManager.globalInstance != null)
            CursorManager.globalInstance.OnLevelLoad();
    }

    /// <summary>
    /// Used by SceneManagerScript, to identify which scene to load.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public string LevelToString(int i)
    {
        #region //oldLevels.
        /*
        //0 -> Tutorial
        //1 -> Earth
        //2 -> Earth baws
        //3 -> Water
        //4 -> Water baws
        //5 -> Fire
        //6 -> Fire baws
        //7 -> Wind
        //8 -> Wind baws
        //9 -> Final baws(ses)
        if (i == 0)
            return "TutorialScene";
        else if (i == 1)
            return "EarthLevel";
        else if (i == 2)
            return "EarthLevelBoss";
        else if (i == 3)
            return "WaterLevel";
        else if (i == 4)
            return "WaterLevelBoss";
        else if (i == 5)
            return "FireLevel";
        else if (i == 6)
            return "FireLevelBoss";
        else if (i == 7)
            return "WindLevel";
        else if (i == 8)
            return "WindLevelBoss";
        else if (i == 9)
            Debug.LogError("Is this finalboss area? Or?");
        */
        #endregion

        //0 -> Tutorial
        //1 -> Earth
        //2 -> Fire
        //3 -> Oblivion
        //4 -> CreditBosses
        //5 -> Final baws.
        //6 -> Epilogue
        if (i == -1)
            return "IntroScene";
        else if (i == 0)
            return "TutorialScene";
        else if (i == 1)
            return "EarthLevel";
        else if (i == 2)
            return "FireLevel";
        else if (i == 3)
            return "WindLevel";
        else if (i == 4)
            return "WindLevelBoss";
        else if (i == 5)
            return "FinalLevel";
        //else if idk where tf i == 6 is lol
        else if (i == 7)
            return "LevelEditorScene";


        Debug.LogError("LevelIndex doesn't match any string! Load will fail.");
        return "";

    }

    public void MenuUpdate()
    {
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<SettingsMenu>().UpdateLevelMenu(currentLevel);

        GameObject.FindGameObjectWithTag("Canvas").GetComponent<DarkwindMenu>().Reset();
    }

    public void ScreenFade()
    {
        //If boat, and alternative ending, gray fade
        if (currentLevel == -1 && PlayerStatsManager.globalInstance.IsPreviousEndingAlternative())
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<FadeUIManager>().LevelFadeOutGray();
        else
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<FadeUIManager>().LevelFade();
    }

    /* CUTSCENES
     "Nothing Special"  : 0,2,4,6,8,10,12,14,16, aka %2 == 0
     "Simple Transition": 1,3,7,11,15
     "Death Transition" : 5,9,13,17 aka %4 == 1
    */

        //0-> Tutorial Enter
        //1-> TutorialFinish
        //2-> Earth Enter
        //3-> Earth Finish
        //4-> Fire Enter
        //5-> Fire Finish
        //6-> Wind Enter
        //7-> Wind Finish
        //8-> CreditBosses Enter
        //9-> CreditBosses Died
        //10->CreditBosses Finish
        //11->FinalBosses Enter
        //12->FinalBosses Died
        //13->FinalBoss Finish
        //12->Epilogue Start

    private void Awake()
    {
        RestartGame();
    }

    public void RestartGame()
    {
        currentLevel = -1;//Used to be 0, but its now -1 for the boat.

        levelCutsceneDialogueIndex = 0;
    }//MenuUpdate() needed?

    public void NetworkLevelMatcherCallback()
    {
        if (NetworkCommunicationController.globalInstance != null && NetworkLevelMatcher.globalInstance != null)
            if (NetworkLevelMatcher.receivedCurrentLevel == LevelManager.currentLevel || (NetworkLevelMatcher.receivedCurrentLevel == -1 && LevelManager.currentLevel == 0))
                NetworkCommunicationController.globalInstance.SendCurrentLevelBoth();
    }

    public void SteamRichPresenceUpdate()
    {
        if (SteamManager.Initialized == false)
            return;

        if (currentLevel == -1)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Sailing The Endless Sea...");
        else if (currentLevel == 0)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Lost, in The Shores of the Unknown...");
        else if (currentLevel == 1)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Navigating The Forest of Serenity...");
        else if (currentLevel == 2)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Ascending The Peak of Fire&Blood!");
        else if (currentLevel == 3)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Struggling in Nothingness...");
        else if (currentLevel == 4)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Challenging The Abyss...");
        else if (currentLevel == 5)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "???");
        else if (currentLevel == 6)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Wondering...");
        else if (currentLevel == 7)
            Steamworks.SteamFriends.SetRichPresence("steam_display", "Creating Endless Worlds...");

    }

    ///<Summary>
    ///LevelManager has an ordered index of every enemy (unique index for each)
    ///Using this, you fetch the enemy from the enemyIndexList, and get the Rigidbody2D
    ///</Summary>
    public Rigidbody2D GetEnemyRigidbodyFromIndex(ushort enemyIndex)
    {
        return enemyList[enemyIndex].GetComponent<Rigidbody2D>();
    }

    public Animator GetEnemyAnimatorFromIndex(ushort enemyIndex)
    {
        if (enemyList[enemyIndex] != null)
            return enemyList[enemyIndex].transform.GetComponentInChildren<Animator>();//Bloated bad performance...
        else
            return null;
    }

    public EnemyBehaviour GetEnemyBehaviourFromIndex(ushort enemyIndex)
    {
        return enemyBehaviourIndexDictionary[enemyIndex];
    }

    public EnemyPathfinder GetEnemyPathfinderFromIndex(ushort enemyIndex)
    {
        return enemyList[enemyIndex].GetComponent<EnemyPathfinder>();
    }

    public List<ushort> GetDeadEnemiesIndex()
    {
        List<ushort> deadEnemyIndexList = new List<ushort>();

        for (int i = 0; i < enemyCorpses.Count; i++)
        {
            if (enemyCorpses[i].activeSelf == true)
                deadEnemyIndexList.Add((ushort)i);
        }

        return deadEnemyIndexList;
    }

    //Not the best way as you cannot remove enemies (would mess up the index as the list moves one index backwards for each removal) but it just works.
    public void AddLevelEditorEnemy(GameObject enemyHolder)
    {
        ushort i = (ushort)enemyList.Count;
        enemyList.Add(enemyHolder.transform.GetChild(0).gameObject);
        enemyList[i].GetComponent<EnemyBehaviour>().enemyListIndex = i;
        enemyBehaviourIndexDictionary.Add(i, enemyList[i].GetComponent<EnemyBehaviour>());

        enemyPathfinderDictionary.Add(enemyList[i], enemyList[i].GetComponent<EnemyPathfinder>());
        enemyBehaviourDictionary.Add(enemyList[i], enemyList[i].GetComponent<EnemyBehaviour>());
    }

    public void AddActiveEnemiesToNetworkIndex()
    {
        for (ushort i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i] != null && enemyBehaviourDictionary[enemyList[i]].enabled == true)
            {
                Debug.Log(i + " index added!");
                NetworkEnemyPositionInterpolationController.globalInstance.AddEnemyIndex(i, GetEnemyRigidbodyFromIndex(i));
            }
        }
    }
}
