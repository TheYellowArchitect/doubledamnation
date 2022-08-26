using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLevelCutscene : MonoBehaviour
{
    public bool endOfLevel = false;

    public bool triggeredOnce = false;

    //To be utilized by KillLockedEndGate or other kind of similar scripts
    //When you want to make the cutscene unusable, until a condition is met by that script.
    public bool lockedGate = false;

    public bool entersLevelEditor = false;

    private void OnTriggerEnter2D(Collider2D player)
    {
        if (lockedGate)
            return;

        if (triggeredOnce == false)
            TriggerLevelGateCutscene(player);

    }

    //This exists purely if player has unlocked gate, but is still inside the 2D trigger so it doesnt activate. 
    //Hence, when he runs out, it will auto-work
    private void OnTriggerExit2D(Collider2D player)
    {
        if (triggeredOnce == false && lockedGate == false)
            TriggerLevelGateCutscene(player);
    }

    public void TriggerLevelGateCutscene(Collider2D player)
    {
        triggeredOnce = true;

        if (endOfLevel)
        {
            player.GetComponent<WarriorMovement>().targetDialogueAnimation = WarriorMovement.DialogueAnimation.PoseEnd;

            //Notify SpeedrunManager, that the current level is finished/done!
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().ClearedLevel();

            //So as intense music won't play over dialogue volume -_-
            MusicSoundManager.globalInstance.MusicFadeOut();
        }
        else
            player.GetComponent<WarriorMovement>().targetDialogueAnimation = WarriorMovement.DialogueAnimation.Idle;

        //Start Cutscene for player (aka idle/death animation) and do nothing
        player.GetComponent<WarriorMovement>().StartLevelCutscene();

        //Notify mage
        GameObject.FindGameObjectWithTag("Mage").GetComponent<MageBehaviour>().StartLevelCutsceneDialogue();

        //Disable spells
        MasterInputManager.globalInstance.DisableSpells();

        //Notify pause menu
        //Also, this is before DialogueManager.EnteredLevelCutscene() because it disables pause
        //and below, when it ends, it notifies to resume it. So if you swapped, when this whole code was done
        //the pause would be disabled since this would be the last line ;)
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<PauseMenu>().InLevelCutscene();

        //This is pretty much a last-moment hack, since the level transitions were designed to be forward-linear lol
        if (entersLevelEditor)
        {
            //Change the cutsceneDialogueIndex to 11 aka exit tutorial onwards level editor
            LevelManager.levelCutsceneDialogueIndex = 11;

            LevelManager.currentLevel = 6;//It will do ++ and go 7 aka Level Editor
        }

        //Notify Dialogue manager, aka where the actual stuff happens and dialogues n stuff happen as well :P
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredLevelCutscene();
    }
}


