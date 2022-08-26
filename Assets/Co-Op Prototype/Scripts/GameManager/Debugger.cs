using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
    public bool resetLevel = true;

    //Cache
    private LevelManager commonLevelManager;

    private WarriorMovement warriorBehaviour;

    private void Start()
    {
        if (GameManager.testing == false)
            DisableDebugger();

        commonLevelManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<LevelManager>();

        warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

        GameObject.FindGameObjectWithTag("Canvas").GetComponent<DarkwindMenu>().darkwindMenuUnlocked = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            GameManager.testing = !GameManager.testing;
            GetComponent<GameManager>().DetermineDebugger();
        }

        if (GameManager.testing)
        {
            if (Input.GetKeyDown(KeyCode.F1) && GameObject.Find("EndLevelEditor") != null)
                    warriorBehaviour.transform.position = GameObject.Find("EndLevelEditor").transform.position;

            if (Input.GetKeyDown(KeyCode.PageDown) && resetLevel)
                commonLevelManager.RestartLevel();

            if (Input.GetKeyDown(KeyCode.Home))
                transform.GetChild(0).GetComponent<MusicSoundManager>().MusicFadeOut();

            if (Input.GetKeyDown(KeyCode.Delete) && LevelManager.currentLevel != 7)
                GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().SkipLevel();

            if (Input.GetKeyDown(KeyCode.F10) && Input.GetKey(KeyCode.RightShift))
                GetComponent<GameManager>().ToggleTestingUI();

            if (Input.GetKeyDown(KeyCode.F11) && Input.GetKey(KeyCode.RightShift))
                Debug.Log("Cutscene index is: " + LevelManager.levelCutsceneDialogueIndex);

            if (Input.GetKeyDown(KeyCode.F9) && Input.GetKey(KeyCode.RightShift))
                NetworkInputSnapshotManager.globalInstance.DeActivate();

            if (Input.GetKeyDown(KeyCode.F8) && Input.GetKey(KeyCode.RightShift))
                NetworkPositionInterpolationController.globalInstance.DeActivate();
            
            if (Input.GetKeyDown(KeyCode.F7) && Input.GetKey(KeyCode.RightShift))
                PlayerStatsManager.globalInstance.SetTotalDeaths(200);

            if (Input.GetKeyDown(KeyCode.F6) && Input.GetKey(KeyCode.RightShift))
                GetComponent<DialogueManager>().DontSkipAllDialogues();
                /*if (NetworkCommunicationController.globalInstance != null)
                {
                    foreach (KeyValuePair<ushort, List<PositionSnapshot>> pickedKey in NetworkEnemyPositionInterpolationController.globalInstance.snapshotDictionary)
                        Debug.Log("Key " + pickedKey.Key + " = " + pickedKey.Value[0].timestamp);
                }*/

            if (Input.GetKeyDown(KeyCode.F5) && Input.GetKey(KeyCode.RightShift))
                Debug.Log("CurrentKills: " + PlayerStatsManager.globalInstance.GetCurrentKills());

            if (Input.GetKeyDown(KeyCode.F4) && Input.GetKey(KeyCode.RightShift))
                NetworkInputSnapshotManager.globalInstance.PrintInputSnapshots();
                //GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(1236, 440, 0);//Le screenshot time

            //Re-Host/Re-Connect
            if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.RightShift))
                NetworkBootWrapper.globalInstance.CreateAndStartMultiplayerMenu();

            if (Input.GetKeyDown(KeyCode.ScrollLock))
                NetworkPositionInterpolationController.globalInstance.ClearSnapshots(4);
                //Debug.Log("Current death: " + PlayerStatsManager.globalInstance.GetTotalDeaths());

            //Refills player's HP
            if (Input.GetKeyDown(KeyCode.Pause))
                GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().CurrentHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().NormalMaxHealth;

            if (Input.GetKeyDown(KeyCode.Numlock))
                Debug.Log(PlayerStatsManager.globalInstance.GetMaxStatsSaveID());

            //Rewind time! (L2/R2 Triggers)
            if (Input.GetKeyDown(KeyCode.RightAlt))
                GameObject.FindGameObjectWithTag("Mage").GetComponent<SpellRewind>().Cast();

            //Made purely for marketing
            if (Input.GetAxis("Triggers") > 0 && Input.GetAxis("Triggers") < 0.7f)
                PrintScreenManager.globalInstance.TakePicture();
        }
    }

    //Cuz of destroyed, this is rip.
    public void DisableDebugger(bool destroyDebugger = false)
    {
        GameManager.testing = false;//doing this, cuz this function may be invoked externally.
        if (DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords == true)
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<DarkwindMenu>().ToggleManaLimitOnSpellwords();
        if (transform.GetChild(2).gameObject.GetComponent<MageInputManager>().GetWarriorAccessToMage() == true)
            transform.GetChild(2).gameObject.GetComponent<MageInputManager>().ToggleWarriorAccessToMage();

        GetComponent<GameManager>().DisableTestingUI();
        if (destroyDebugger)
            Destroy(this);
    }

    public void EnableDebugger()
    {
        GameManager.testing = true;
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<DarkwindMenu>().darkwindMenuUnlocked = true;

        if (DarkwindDistortionManager.globalInstance.noManaLimitOnSpellwords == false)
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<DarkwindMenu>().ToggleManaLimitOnSpellwords();
        
        if (transform.GetChild(2).gameObject.GetComponent<MageInputManager>().GetWarriorAccessToMage() == false)
            transform.GetChild(2).gameObject.GetComponent<MageInputManager>().ToggleWarriorAccessToMage();
        

        //GetComponent<GameManager>().EnableTestingUI();
    }
#endif
}
