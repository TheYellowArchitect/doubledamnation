using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using NaughtyAttributes;

[DisallowMultipleComponent]
public class SceneManagerScript : MonoBehaviour
{
    //If it wasn't for this vid: https://www.youtube.com/watch?v=dA4oOm3wCIc I would go for the classic "DontDestroyOnLoad()" lul. Going for a messy singleton smh.

    //So every1 can access it, without searching for tags and bs like that. Aka, this is a singleton.
    public static SceneManagerScript globalInstance { set; get; }

    private LevelManager commonLevelManager;

    [ReadOnly]
    public string lastLoadedLevel;

    //Happens at the very start of the game
    private void Awake()
    {
        globalInstance = this;

        commonLevelManager = GetComponent<LevelManager>();
    }

    /// <summary>
    /// Called by GameStarter.cs, so some stuff are already set by it, like framerate.
    /// </summary>
    public void Initialize()
    {
        Load("FundamentalScene");

        //Contains boat, sea, a player sprite rowing, and it auto-triggers the dialogue, and notifies the sceneManager to play the tutorial scene when need be -> aka LoadNextLevel() and it fully does its job.
        Load("IntroScene");
    }

    public void Load(string givenSceneName)
    {
        //If not loaded already, Load it! (on top of current fyi)
        if (SceneManager.GetSceneByName(givenSceneName).isLoaded == false)
        {
            commonLevelManager.OnLevelPreLoad(givenSceneName);

            SceneManager.LoadScene(givenSceneName, LoadSceneMode.Additive);
        }
            

        lastLoadedLevel = givenSceneName;

        //Coroutine otherwise, race conditions.
        StartCoroutine(UpdateLevelManager(givenSceneName));
    }

    public IEnumerator UpdateLevelManager(string givenSceneName)
    {
        yield return new WaitForSeconds(.01f);

        commonLevelManager.OnLevelLoad(givenSceneName);
    }

    public void Unload(string givenSceneName)
    {
        //Debug.Log("Loaded is: " + SceneManager.GetSceneByName(givenSceneName) + "and last is: " + lastLoadedLevel);

        //If not loaded already, Load it! (on top of current fyi)
        if (SceneManager.GetSceneByName(givenSceneName).isLoaded && givenSceneName != "FundamentalScene")
            SceneManager.UnloadScene(givenSceneName);//why tf is it obsolete???? want me to async and fuck up? I have designed around "frozen" screen smh.
    }

    /// <summary>
    /// Same as Unload() but can delete FundamentalScene as well!
    /// </summary>
    /// <param name="givenSceneName"></param>
    public void UnloadAny(string givenSceneName)
    {
        //Debug.Log("Loaded is: " + SceneManager.GetSceneByName(givenSceneName) + "and last is: " + lastLoadedLevel);

        //If not loaded already, Load it! (on top of current fyi)
        if (SceneManager.GetSceneByName(givenSceneName).isLoaded)
            SceneManager.UnloadScene(givenSceneName);//why tf is it obsolete???? want me to async and fuck up? I have designed around "frozen" screen smh.
    }


    public void LoadNextLevel()
    {
        //Debug.Log("Current level is: " + LevelManager.currentLevel);
        LevelManager.IncreaseLevelIndex();
        Unload(lastLoadedLevel);
        Load(commonLevelManager.LevelToString(LevelManager.currentLevel));
        //Debug.Log("Current level is: " + LevelManager.currentLevel);

        Debug.Log("Loading next level!");
    }

    public void LoadTargetLevel(int targetLevel)
    {
        if (targetLevel > 7 || targetLevel < 0)
            return;

        //Set cutsceneDialogueIndex properly (also avoids a bug where it jumps a level forward!)
        LevelManager.SetCutsceneIndex(((targetLevel + 1) * 2) - 2);//same as below, but also covers tutorial level (0 * 2 reeeee)
        //LevelManager.SetCutsceneIndex(targetLevel * 2);

        LevelManager.currentLevel = targetLevel;
        Unload(lastLoadedLevel);
        Load(commonLevelManager.LevelToString(LevelManager.currentLevel));
    }
}
