using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PauseMenu : MonoBehaviour
{
    //The event others subscribe to, so single-responsibility pattern is ensured.
    public event Action pauseEvent;
    public event Action resumeEvent;

    //For spagghetti netcoding -_-
    public Action pauseNotifyOtherPlayerEvent;
    public Action resumeNotifyOtherPlayerEvent;

    //So as every object can get this data
    public static bool GameIsPaused = false;
    public static bool disablePause = true;

    //Singleton
    public static PauseMenu globalInstance;

    //The panels
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;
    public GameObject pingLabelUI;
    public GameObject creditsMenuUI;

    //The event system (to select button)
    public EventSystem ES;
    [Tooltip("Used to select it on pause. (via EventSystem, so player can navigate!)")]
    public GameObject pauseMenuResumeButton;
    [Tooltip("Used to select it on menu. (via EventSystem, so player can navigate!)")]
    public GameObject settingsMenuResumeButton;

    private void Start()
    {
        disablePause = false;
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DiedEvent += DisablePause;
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedDeathDialogue += EnablePause;
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedLevelCutscene += EnablePause;
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedInterruptionDialogue += EnablePause;
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().skippedRevive += EnablePause;

        globalInstance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || (GameManager.testing == false && Input.GetButtonDown("ButtonStart")) || (GameManager.testing && Input.GetButtonUp("ButtonB")))
        {
            //Debug.Log("Is game paused? " + GameIsPaused);
            //Debug.Log("Is pause disabled? " + disablePause);
            if (GameIsPaused)
                Resume();
            else if (disablePause == false)//So it won't pause at death or cutscene or boss victory/enter.
                Pause();
        }
    }

    public void Resume(bool notifyOtherPlayer = true)
    {
        //Updates status
        GameIsPaused = false;

        //Gets time back to normal
        Time.timeScale = 1f;

        //Make cursor invisible
        CursorManager.globalInstance.Disable();
        //Cursor.visible = false;

        //If we were at settings to resume, it disables settings as well.
        if (settingsMenuUI.activeSelf == true)
            settingsMenuUI.SetActive(false);

        if (creditsMenuUI.activeSelf == true)
            creditsMenuUI.SetActive(false);

        //Deactivates the gameobject UI panel
        pauseMenuUI.SetActive(false);

        //Navigate UI by controller
        ES.sendNavigationEvents = false;

        //Notifies the subscribers!
        if (resumeEvent != null)
            resumeEvent();

        if (resumeNotifyOtherPlayerEvent != null && notifyOtherPlayer)
            resumeNotifyOtherPlayerEvent();
    }

    public void Pause(bool showPauseMenu = true, bool showCursor = true, bool notifyOtherPlayer = true)
    {
        //Updates status
        GameIsPaused = true;

        //Notifies the subscribers!
        if (pauseEvent != null)
            pauseEvent();

        if (pauseNotifyOtherPlayerEvent != null && notifyOtherPlayer)
            pauseNotifyOtherPlayerEvent();

        //"Freezes" time/speed in which time is passing
        Time.timeScale = 0f;
        //Fucking netcoding - cannot send resume or ANY RPC when paused, so it forces desync!!!
        if (NetworkCommunicationController.globalInstance != null)
            Time.timeScale = 1f;

        //Navigate UI by controller
        ES.sendNavigationEvents = true;

        //Make cursor visible
        if (showCursor == true)
            CursorManager.globalInstance.Activate();
        else
            CursorManager.globalInstance.Disable();
        //Cursor.visible = showCursor;

        if (showPauseMenu)
        {
            //Activates the gameobject UI panel
            pauseMenuUI.SetActive(true);

            //Selects the Resume Button
            ES.SetSelectedGameObject(pauseMenuResumeButton);
        }
    }

    public void LoadMenu()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        if (NetworkCommunicationController.globalInstance == null)
            pingLabelUI.SetActive(false);
        else
            pingLabelUI.SetActive(true);

        //Hacky way, but gotta confirm speedrun button (cuz if activated via F12, it does not update the toggle)
        if (PlayerStatsManager.globalInstance.GetSpeedrunMode() && GetComponent<SettingsMenu>().speedrunToggle.isOn == false)
        {
            Debug.Log("PAUSEMENU");

            GetComponent<SettingsMenu>().speedrunToggle.isOn = true;
        }


        //Selects the Resume Button
        ES.SetSelectedGameObject(settingsMenuResumeButton);
    }

    public void DisablePause()
    {
        disablePause = true;
    }

    //Called via TriggerLevelCutscene.cs
    public void InLevelCutscene()
    {
        disablePause = true;
    }

    public void EnablePause()
    {
        disablePause = false;
    }
}
