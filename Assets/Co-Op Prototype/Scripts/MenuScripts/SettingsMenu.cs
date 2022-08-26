using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

//camera.aspect is automatically changed from SetResolution.
//TODO: Make a "UpdateThemeUI" class, that handles the update for all visuals (level 1,2,3) instead of this class. Include darkwind distortion's background image plz!
//TODO: Make this a globalInstance singleton
public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu globalInstance;

    //So as everyone can know if game is fullscreen or not
    public static bool GameIsFullscreen = true;

    public static bool GameIsVsynced = false;

    public static bool VibrationActive = true;

    //Level Change
    /////////////////////
    [Header("On Level Change")]

    public TMP_ColorGradient fireLevelGradient;
    public TMP_ColorGradient windLevelGradient;

    public Color fireLevelColorFill;
    public Color windLevelColorFill;

    public Color fireLevelColorText;
    public Color windLevelColorText;

    public Sprite fireLevelDiscordPalette;
    public Sprite windLevelDiscordPalette;

    public Sprite fireLevelArrow;
    public Sprite windLevelArrow;

    public Sprite fireLevelDarkwindDistortionBackground;
    public Sprite windLevelDarkwindDistortionBackground;

    private List<GameObject> handleFillAreas;
    private List<GameObject> tmpTexts;
    private List<GameObject> toggleTexts;
    private List<GameObject> arrows;
    private GameObject discordSprite;
    private GameObject darkwindDistortionBackground;

    ////////////////////
    [Header("The Menus")]
    public GameObject settingsMenuUI;

    public GameObject pauseMenuUI;

    public GameObject creditsMenuUI;

    public GameObject darkwindMenuUI;

    public GameObject levelEditorMenuUI;
    public GameObject levelEditorPlayUI;

    [Header("Misc")]

    //Main audio mixer; aka where all the audio goes in the end
    public AudioMixer mainAudioMixer;

    //The toggle for speedrun mode, cached so as to set it
    //when you press speedrun toggle, but speedrun mode is denied.
    public Toggle speedrunToggle;

    //The UI Dropdown menu that will contain the resolution strings
    public Dropdown resolutionDropdown;
    List<Resolution> possibleResolutions = new List<Resolution>();

    public GameObject pingUI;


    //Stores what resolution we are currently at the dropdown menu
    int currentResolutionIndex = 0;

    //List of strings that are going to be put in the dropdown menu (needs strings as parameter hence this)
    List<string> resolutionOptions = new List<string>();
    
    //Level-Dependant stuff. Wouldn't be possible if gameobjects were disabled on the start.
    private void Awake()
    {
        globalInstance = this;

        handleFillAreas = new List<GameObject>(GameObject.FindGameObjectsWithTag("HandleUI"));
        tmpTexts = new List<GameObject>(GameObject.FindGameObjectsWithTag("GradientText"));
        toggleTexts = new List<GameObject>(GameObject.FindGameObjectsWithTag("ToggleText"));
        arrows = new List<GameObject>(GameObject.FindGameObjectsWithTag("ArrowUI"));
        discordSprite = GameObject.FindGameObjectWithTag("DiscordButton");
        darkwindDistortionBackground = GameObject.FindGameObjectWithTag("DarkwindDistortionBackground");
    }

    private void Start()
    {
        //Deletes previous screen settings
        PlayerPrefs.DeleteAll();

        //To make sure we start clean/empty
        resolutionDropdown.ClearOptions();

        //Force all resolutions to have one refreshRate type. TODO: In the future, find the highest refresh rate possible, and add that. AKA TWEAK THE 60!
        InitializeResolutions(ref possibleResolutions, Screen.resolutions, 60);

        //Loop through all possible resolutions and for each of them format an string that displays our resolution
        //and we add it on our options string list, that is to be given to DropdownMenu
        for (int i = 0; i < possibleResolutions.Count; i++)
        {
            string option = possibleResolutions[i].width + " x " + possibleResolutions[i].height;

            //Checks if its the "same" because dat refresh rate (60Hz, 144Hz, etc etc) makes 4 exactly similar resolutions
            //and then checks refreshRate so as to make sure only the highest Hz makes it to the dropdown menu.//Not needed since here we add the strings.
            if (resolutionOptions.Contains(option) == false) // && possibleResolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            //It takes only the lowest refresh rates here.
            {
                resolutionOptions.Add(option);

                //If resolution wer currently looking at is equal to our current resolution
                if (possibleResolutions[i].width == Screen.currentResolution.width && possibleResolutions[i].height == Screen.currentResolution.height)
                    //Set what resolution index, is the suggested resolution.
                    currentResolutionIndex = i;
            }

        }

        //Debugging/testing dis index list: Finds an UI text object, and writes on it how many resolutions it can support.
        /*
        for (int i = 0; i < possibleResolutions.Count; i++)
        {
            //if (possibleResolutions[i].refreshRate == 60)
            GameObject.FindGameObjectWithTag("Finish").GetComponent<Text>().text += "\n" + possibleResolutions[i];
        }
        */

        //Puts to the resolution dropdown, an list/string of the possible resolutions
        resolutionDropdown.AddOptions(resolutionOptions);

        //Selects the current resolution.
        resolutionDropdown.value = currentResolutionIndex;

        //In order to actually display it
        resolutionDropdown.RefreshShownValue();


        //Makes the objects, disabled.
        SetMenusActive();
    }

    public void SetResolution(int resolutionIndex)
    {
        //Updates currentResolutionIndex
        currentResolutionIndex = resolutionIndex;

        //Debugging: finds a UI Text object and writes the current resolution
        //GameObject.FindGameObjectWithTag("Respawn").GetComponent<Text>().text = "Currently selected res is: " + possibleResolutions[currentResolutionIndex];


        Resolution targetResolution = possibleResolutions[resolutionIndex];
        Screen.SetResolution(targetResolution.width, targetResolution.height, GameIsFullscreen);

        //Useless shit below
        //Does this camera thingy work? I think I should do the adjustment below tbh.
        //Camera camera = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>();
        //camera.aspect = (float)targetResolution.width / (float)targetResolution.height;
        //GameObject.FindGameObjectWithTag("Respawn").GetComponent<Text>().text += "\n Aspect ratio is: " + camera.aspect;

        //camera.GetComponent<cameratest>().UpdateCameraRect(targetResolution.width, targetResolution.height);
    }

    public void InitializeResolutions(ref List<Resolution> possibleResolutions, Resolution[] screenResolutions, int forcedRefreshRate = 60)
    {
        //Fills the resolutions, while also making sure they are within the refresh rate
        for (int i = 0; i < screenResolutions.Length; i++)
        {
            //If it fits the refreshRate
            if (screenResolutions[i].refreshRate == forcedRefreshRate)
                possibleResolutions.Add(screenResolutions[i]);
        }

        //An alternative way, is to fill the resolutions fully, then .Remove() the ones with not-desired refresh rate.
    }

    public void SetSFXVolume(float targetVolume)
    {
        if (targetVolume == 0)
        {
            mainAudioMixer.SetFloat("SFXVolume", 0);
            return;
        }
        else
            mainAudioMixer.SetFloat("SFXVolume", Mathf.Log10(targetVolume) * 20);
    }

    public void SetMusicVolume(float targetVolume)
    {
        if (targetVolume == 0)
        {
            mainAudioMixer.SetFloat("MusicVolume", 0);
            return;
        }
        else
            mainAudioMixer.SetFloat("MusicVolume", Mathf.Log10(targetVolume) * 20);
    }

    //Temporary for leveleditor -_-
    public void MuteMusic()
    {   
        mainAudioMixer.SetFloat("MusicVolume", -70f);
    }

    public void SetVoiceVolume(float targetVolume)
    {
        if (targetVolume == 0)
        {
            mainAudioMixer.SetFloat("VoiceVolume", 0);
            return;
        }
        else
        {
            if (VoiceMuteManager.globalInstance.muteVoicePermanent == false)
                mainAudioMixer.SetFloat("VoiceVolume", Mathf.Log10(targetVolume) * 20);
            else//Mute voice!
                mainAudioMixer.SetFloat("VoiceVolume", Mathf.Log10(0.0001f) * 20);
        }
        
    }

    public void SetFullscreen(bool isFullscreen)
    {
        GameIsFullscreen = isFullscreen;
        SetResolution(currentResolutionIndex);
    }

    public void SetVsync(bool value)
    {
        GameIsVsynced = value;

        if (GameIsVsynced)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;//Activate Vsync
        }
        else
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;//No Vsync
        }
    }

    public void SetVibration(bool value)
    {
        VibrationActive = value;
    }

    public void SetSpeedrunMode(bool value)
    {
        //Tbh, I should hide the speedrun mode toggle, if this player hasnt played or finished the game b4
        //by simply checking the saveData pathfile, if it has any save/run. HOWEVER, that is bad overall.
        //Why?
        //Because, some1 will finish a game, and will NOT know there is a speedrun mode.
        //I cannot use ending's moments to say "hey, there is speedrun mode!" cuz that will break the immersion
        //so yeah, if he sees it at the menu, player will get it and try it out later.
        //Player freedom ftw, since players arent braindead to activate it on their first try.
        //
        //As for denying when not level 1... that idea is valid.
        //Since a speedrun starts from level 1, starting a speedrun from level 2 is kinda rip.
        //However, in the future, I may expand on this, and make you choose where to start from
        //like segmented speedrunning, so a speedrunner can play a certain level to git gut
        //instead of playing from the start.
        //So yeah, while speedrunning from level2+ is "invalid", it will be valid in due time
        //so im saving my future myself the effort, and giving a little freedom to speedrunners.
        //
        //In the future, make a menu page, exclusively for speedrunning 
        //which has speedrunning, time slider for font size, choosing a level, and more.

        Debug.Log("SETTINGSMENUSETTINGSMENU");

        //tl;dr: of the above, just put the value parameter on speedrunMode.
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().ToggleSpeedrunMode(value);

        //tbh, activating speedrun mode, when u have already passed the first gate is bad, but w/e.
        //super edge use case where some1 wants to speedrun-test level 2 and 3 I guess?
    }

    public void SetFPSDisplay(bool value)
    {
        FPSDisplayManager.globalInstance.SetFPSText(value);
    }

    public void StartCredits()
    {
        SetMenusActive(false, false, true, false, levelEditorMenuUI.activeSelf);

        creditsMenuUI.GetComponent<CreditsMenu>().RollCredits();
    }

    public void UpdateLevelMenu(int level)
    {
        //Fire Level
        if (level == 2)
        {
            ApplyToggleColor(fireLevelColorText);

            ApplyHandleColor(fireLevelColorFill);

            ApplyGradientColor(fireLevelGradient);

            ApplyArrowColor(fireLevelArrow);

            ApplyDarkwindDistortionBackgroundColor(fireLevelDarkwindDistortionBackground);

            ApplyDiscordColor(fireLevelDiscordPalette);
        }
        //Wind Level
        else if (level == 3)
        {
            ApplyToggleColor(windLevelColorText);

            ApplyHandleColor(windLevelColorFill);

            ApplyGradientColor(windLevelGradient);

            ApplyArrowColor(windLevelArrow);

            ApplyDarkwindDistortionBackgroundColor(windLevelDarkwindDistortionBackground);

            ApplyDiscordColor(windLevelDiscordPalette);
        }
    }

    //
    //3 in menu. I didn't use foreach for speed, but updating, this will break. Same for every ApplyX function.
    //
    public void ApplyToggleColor(Color value)
    {
        foreach (GameObject pickedGameObject in toggleTexts)
            pickedGameObject.GetComponent<Text>().color = value;
    }

    public void ApplyHandleColor(Color value)
    {
        foreach (GameObject pickedGameObject in handleFillAreas)
            pickedGameObject.GetComponent<Image>().color = value;
    }

    public void ApplyGradientColor(TMP_ColorGradient value)
    {
        foreach (GameObject pickedGameObject in tmpTexts)
            pickedGameObject.GetComponent<TextMeshProUGUI>().colorGradientPreset = value;
    }

    public void ApplyArrowColor(Sprite arrowPalette)
    {
        foreach (GameObject pickedGameObject in arrows)
            pickedGameObject.GetComponent<Image>().sprite = arrowPalette;
    }

    public void ApplyDarkwindDistortionBackgroundColor(Sprite newMarblePalette)
    {
        darkwindDistortionBackground.GetComponent<Image>().sprite = newMarblePalette;
    }

    public void ApplyDiscordColor(Sprite discordPalette)
    {
        discordSprite.GetComponent<Image>().sprite = discordPalette;
    }

    public void RedirectToWebsite(string URL)
    {
        Application.OpenURL(URL);
    }

    public void SetMenusActive(bool settingsActive = false, bool pauseActive = false, bool creditsActive = false, bool darkwindActive = false, bool levelEditorActive = false)
    {
        settingsMenuUI.SetActive(settingsActive);
        if (settingsActive)
        {
            if (NetworkCommunicationController.globalInstance == null)
                pingUI.SetActive(false);
            else
                pingUI.SetActive(true);
        }

        pauseMenuUI.SetActive(pauseActive);

        creditsMenuUI.SetActive(creditsActive);

        levelEditorMenuUI.SetActive(levelEditorActive);
        levelEditorPlayUI.SetActive(levelEditorActive);

        darkwindMenuUI.SetActive(darkwindActive);
        if (NetworkCommunicationController.globalInstance != null)
        {
            if (darkwindActive)
                NetworkCommunicationController.globalInstance.SendDarkwindDistortionChange(DarkwindMenu.ButtonClicked.opened);
            else
                NetworkCommunicationController.globalInstance.SendDarkwindDistortionChange(DarkwindMenu.ButtonClicked.closed);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
