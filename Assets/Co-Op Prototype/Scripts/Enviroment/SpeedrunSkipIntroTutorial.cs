using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is attached to the "Start"(cutscene) gameobject (with triggercollider2d) of tutorial level.
//and all it does, is detect if player has skipped intro, and if so, auto-skip to the end of the level.
//After all, if speedrun mode is toggled on tutorial, it auto-skips, BUT, if cutscene happens and u press F12
//it cannot auto-skip since tutorial scene isnt a thing, so it detects it here!
public class SpeedrunSkipIntroTutorial : MonoBehaviour
{
    public bool skipTutorial = true;

	void Start ()
    {
        //If speedrun
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().GetSpeedrunMode() == true && skipTutorial)
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedLevelCutscene += SkipTutorial;
    }
	
	public void SkipTutorial()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().SkipLevel(true);

        //If you dont unsubscribe, then it gets re-called, which breaks the game!
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().finishedLevelCutscene -= SkipTutorial;
    }
}
