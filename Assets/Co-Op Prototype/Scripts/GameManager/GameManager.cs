using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //The very first script called. Dont put much stuff here.
    //It should invoke other stuff, instead of running everything.
    //Example: Runs GameInit.cs -> blabla

    [Tooltip("Sets the GameManager.Testing(static) variable ;)")]
    public bool startGameTesting = true;

    //Singleton
    public static GameManager globalInstance = null;
    public static bool testing;

    //This is the resolution right when the game starts
    public static Resolution nativeResolution;

    public GameObject testingUI;
    public CanvasRenderer[] UIbuttons;

    private void Awake()
    {
        //Check if instance already exists
        if (globalInstance == null)            
            globalInstance = this;//if not, set instance to this

        //If instance already exists and it's not this:
        else if (globalInstance != this)
            Destroy(gameObject);//Then destroy this. There can only ever be one instance of a GameManager.

        testing = startGameTesting;

#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
        //So I don't fuck up and forget it as true on a release build!
        testing = false;
#endif

        InitGame();

        //GetComponent<Debugger>().DisableDebugger();
    }

    void InitGame()
    {
        nativeResolution = Screen.currentResolution;

        ShowButtons(false);

        //Disables the UI buttons top right, and FPS top left
        //if (testing == false)
        DisableTestingUI();

        //Set the seed (to be held in playerstats)
        //so with peer2peer, or other reasons, the seed/state must be shared (future replay POG)
        Random.InitState(Random.Range(0, 255));//why 255 tho

        //Initialize PlayerStatsManager, so as to give it the above RNG seed.
        GetComponent<PlayerStatsManager>().Initialize();
    }

    public void ShowButtons(bool show)
    {
        if (show == true)
            foreach (CanvasRenderer button in UIbuttons)
                button.SetAlpha(1f);
        else
            foreach (CanvasRenderer button in UIbuttons)
                button.SetAlpha(0f);
    }

    public void DisableTestingUI()
    {
        testingUI.SetActive(false);
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().ShowRunTimeUI(false);
        if (FPSDisplayManager.globalInstance != null)
            FPSDisplayManager.globalInstance.SetFPSText(false);
        ShowButtons(false);
    }

    public void EnableTestingUI()
    {
        testingUI.SetActive(true);
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().ShowRunTimeUI(true);
        if (FPSDisplayManager.globalInstance != null)
            FPSDisplayManager.globalInstance.SetFPSText(true);
        //ShowButtons(true);
    }

    public void ToggleTestingUI()
    {
        if (testingUI.activeSelf)
            DisableTestingUI();
        else
            EnableTestingUI();
    }
#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
    public void DetermineDebugger()
    {
        if (testing)
            GetComponent<Debugger>().EnableDebugger();
        else
            GetComponent<Debugger>().DisableDebugger();
    }
#endif
}
