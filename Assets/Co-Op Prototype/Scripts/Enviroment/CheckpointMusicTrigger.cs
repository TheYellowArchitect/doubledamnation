using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interacts with the (global) MusicSoundManager, since that has the audioSources and hence, the tweaks. This here is more of an manager/wrapper. The "trigger" to start the actual thing :P
/// </summary>
public class CheckpointMusicTrigger : MonoBehaviour
{
    [Header("Level : Position")]
    public Vector3 level0;//rip here lel.
    public Vector3 level1;
    public Vector3 level2;
    public Vector3 level3;
    public Vector3 level4;//mini-bossfight, rip staff roll xd
    public Vector3 level5;//final fight! Cinematic af!//probably rip music there too.

    [Header("Just public properties, not to tweak")]
    public bool playerActivatedBattle = false;

    public bool playerInside = false;

    private void Start()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().DiedEvent += PlayerDied;
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedRevive += PlayerRevived;

        //Replace with proper intro music.
        //MusicSoundManager.globalInstance.PlayMusic(MusicSoundManager.AudioSourceType.checkpointStart, true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Random bugs fixed ez pz.
        if (playerInside == true)//TODO: Just go different layer and ggwp.
            return;

        //Because the first run on a level, starts with combat music. If it changes, change this too!
        if (PlayerStatsManager.globalInstance.IsFirstRun())
            return;

        //Going outside, and stopping ambience.
        if (playerActivatedBattle == false)
        {
            playerActivatedBattle = true;

            MusicSoundManager.globalInstance.PlayMusic(MusicSoundManager.AudioSourceType.combatStart, true);
        } 
        else if (playerActivatedBattle == true)
        {
            playerActivatedBattle = false;

            MusicSoundManager.globalInstance.PlayMusic(MusicSoundManager.AudioSourceType.checkpointStart, true);
        }


        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        playerInside = false;
    }

    //Triggered via event/action
    public void PlayerDied()
    {

        //Stop abruptly the combat music to make it more impactful? Perhaps an SFX/Violin snap (like the lvl1's 1:59 part)
        //Wait X seconds (or don't do anything else, and do the below on Revive?)
        //MusicSoundManager.globalInstance.PlayMusic(MusicSoundManager.AudioSourceType.checkpointStart, true);
    }

    //Triggered when player revives, via action/event
    public void PlayerRevived()
    {
        playerInside = false;//for that edge-case player dies while inside
        playerActivatedBattle = false;

        //MusicSoundManager.globalInstance.StopCurrent();//Just in case I fucked up the math lel. (or tweak the death duration.)
        //MusicSoundManager.globalInstance.PlayMusic(true, false);

        //MusicSoundManager.globalInstance.CombatLoop(false);
    }

    public void OnLevelLoad()
    {
        if (LevelManager.currentLevel == 0)
            transform.position = level0;
        else if (LevelManager.currentLevel == 1)
            transform.position = level1;
        else if (LevelManager.currentLevel == 2)
            transform.position = level2;
        else if (LevelManager.currentLevel == 3)
            transform.position = level3;
        else if (LevelManager.currentLevel == 4)
            transform.position = level4;
        else if (LevelManager.currentLevel == 5)
            transform.position = level5;
        else if (LevelManager.currentLevel == 7)
        {
            Debug.LogWarning("No music for level editor!");
            //Optimally it would be an iconic chill song, and one layer activates when pressing Play
        }
        else
        {
            Debug.LogError("Bug! Unexpected level!");
            Debug.Break();
        }

        playerActivatedBattle = false;
        playerInside = false;//idk, just making sure, some level configurations may fuck up and moving player may trigger it since moved via rigidbody? no idea, just confirming it works properly.
            
    }
}
