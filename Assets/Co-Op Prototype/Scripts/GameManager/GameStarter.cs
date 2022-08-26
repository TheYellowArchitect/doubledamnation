using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Normally in games, what this class does is simply an InitGame() function/method from GameManager. 
/// The thing is, that GameManager doesn't exist in the very first scene, so this acts as a "replacement" for it, until the game fully starts.
/// In other words... This is the very first script that runs in the game, and runs scripts in the proper order that should be ran, until the game truly begins.
/// The order is Init() -> Does very important stuff where variables aren't needed like framerate, then CallsSceneManager to load fundamental scene which has gamemanager.
/// </summary>
public class GameStarter : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        StartTheGame();
    }

    void StartTheGame()
    {
        PlayerPrefs.DeleteAll();

        //Sets the frame rate
        Application.targetFrameRate = 60;

        //Disables Vsync
        QualitySettings.vSyncCount = 0;

        //Hides the cursor
        Cursor.visible = false;

        GetComponent<SceneManagerScript>().Initialize();
    }
}
