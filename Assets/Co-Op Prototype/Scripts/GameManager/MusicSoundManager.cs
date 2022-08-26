using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[DisallowMultipleComponent]
public class MusicSoundManager : MonoBehaviour
{
    //So every1 can access it, without searching for tags and bs like that.
    public static MusicSoundManager globalInstance { set; get; }

    public AudioClip CheckpointAreaMusic;//Change pitch for every level.

    [ReadOnly]
    public bool playingCombatMusic;

    [ReadOnly]
    [Tooltip("Purely for debugging reasons.")]
    public bool playingLoop = false;

    public enum AudioSourceType { checkpointStart, checkpointLoop, combatStart, combatLoop};

    //TODO: When player 1 hp, increase volume by 10% and decrease the pitch for every hit.
    [Header("Audio Source")]
    public AudioSource checkpointStartSource;
    public AudioSource checkpointLoopSource;
    public AudioSource combatStartSource;
    public AudioSource combatLoopSource;

    [Header("Music Animators")]
    public Animator checkpointStartAnimator;
    public Animator checkpointLoopAnimator;
    public Animator combatStartAnimator;
    public Animator combatLoopAnimator;

    [Header("Level 0/Shores of the Unknown")]
    public AudioClip Level0StartMusic;
    public AudioClip Level0LoopMusic;

    [Header("Level 1/Elusive Woods")]
    public AudioClip combatStartingLevel1;
    public AudioClip combatLoopLevel1;

    [Header("Level 2/Crimson Flames")]
    public AudioClip combatStartingLevel2;
    public AudioClip combatLoopLevel2;

    [Header("Level 3/Winds of Oblivion")]
    public AudioClip combatStartingLevel3;
    public AudioClip combatLoopLevel3;

    private AudioSource latestAudioSource;

    

    private float lerpVariable = 0;

    //Happens at the very start of the game
    private void Awake()
    {
        globalInstance = this;
    }

    void Start ()
    {
        playingCombatMusic = true;
        //checkpointLoopSource.clip = CheckpointAreaMusic;
        //checkpointLoopSource.Play();
        PlayMusic(AudioSourceType.combatStart);
	}

    private void Update()//Could optimize this, by not checking if Looping.
    {
        if (playingCombatMusic)
        {
            if (combatStartSource.isPlaying == false && combatLoopSource.isPlaying == false)
            {
                //Gets it back to normal volume, cuz fade out has gotten it to 0.
                combatLoopAnimator.Play("0_8");

                PlayMusic(AudioSourceType.combatLoop, false);
            }

        }
        else//checkpoint
        {
            if (checkpointStartSource.isPlaying == false && checkpointLoopSource.isPlaying == false)
            {
                checkpointLoopAnimator.Play("0_8");

                PlayMusic(AudioSourceType.checkpointLoop, false);
            }
                
                
        }
            
    }

    public void PlayMusic (AudioSourceType musicType, bool lerp = true)
    {
        DetermineAudioSource(musicType);

        DeterminePlayingCombatMusic(musicType);


        //When player goes back and forth, stops so there won't be any bugs.
        if (musicType == AudioSourceType.checkpointLoop || musicType == AudioSourceType.checkpointStart)
        {
            if (musicType == AudioSourceType.checkpointLoop)
            {
                checkpointStartSource.Stop();

                playingLoop = true;
            }
            else
            {
                checkpointLoopSource.Stop();

                playingLoop = false;
            }
                
        }
        else//Combat
        {
            if (musicType == AudioSourceType.combatLoop)
            {
                playingLoop = true;

                combatStartSource.Stop();
            }
            else
            {
                playingLoop = false;

                combatLoopSource.Stop();
            }

        }

        latestAudioSource.Play();//The clip + volume is set on "level load"

        if (lerp)
        {
            if (musicType == AudioSourceType.checkpointStart)
            {
                checkpointStartAnimator.Play("FadeIn");

                if (combatStartSource.isPlaying)
                    combatStartAnimator.Play("FadeOut");
                if (combatLoopSource.isPlaying)
                    combatLoopAnimator.Play("FadeOut");
            }
            else if (musicType == AudioSourceType.combatStart)
            {
                combatStartAnimator.Play("FadeIn");

                if (checkpointStartSource.isPlaying)
                    checkpointStartAnimator.Play("FadeOut");
                if (checkpointLoopSource.isPlaying)
                    checkpointLoopAnimator.Play("FadeOut");
            }
            else
            {
                //You see, if an AudioSourceType.LOOP plays, its impossible, really. Because it won't need/play lerp. Hence only 2 checks above and this catches the "mistakes"
                Debug.Log("Music BREAK!");
                Debug.Break();
            }
        }
        else//So they won't be stuck at animation "0".
        {
            if (musicType == AudioSourceType.checkpointLoop)
                checkpointLoopAnimator.Play("0.8");
            else if (musicType == AudioSourceType.combatLoop)
                combatLoopAnimator.Play("0.8");
        }
    }

    /// <summary>
    /// Given the musicType, it stores/sets the latestAudioSource to process/play.
    /// </summary>
    /// <param name="musicType"></param>
    public void DetermineAudioSource(AudioSourceType musicType)
    {
        if (musicType == AudioSourceType.checkpointLoop)
            latestAudioSource = checkpointLoopSource;
        else if (musicType == AudioSourceType.checkpointStart)
            latestAudioSource = checkpointStartSource;
        else if (musicType == AudioSourceType.combatLoop)
            latestAudioSource = combatLoopSource;
        else if (musicType == AudioSourceType.combatStart)
            latestAudioSource = combatStartSource;
    }

    /// <summary>
    /// Given the musicType, it detects if it is combat, or checkpoint, and sets the boolean playingCombatMusic according to it.
    /// </summary>
    public void DeterminePlayingCombatMusic(AudioSourceType musicType)
    {
        if (musicType == AudioSourceType.checkpointLoop || musicType == AudioSourceType.checkpointStart)
            playingCombatMusic = false;
        else if (musicType == AudioSourceType.combatLoop || musicType == AudioSourceType.combatStart)
            playingCombatMusic = true;
    }

    //Called usually when player revives
    public void StopCurrent()
    {
        checkpointLoopSource.Stop();
        combatLoopSource.Stop();
    }

    public void MusicFadeOut()
    {
        Debug.Log("Combat Start/Loop: " + combatStartSource.isPlaying + "/" + combatLoopSource.isPlaying);
        Debug.Log("Checkpoint Start/Loop: " + checkpointStartSource.isPlaying + "/" + checkpointLoopSource.isPlaying);

        if (playingCombatMusic)
        {
            if (combatStartSource.isPlaying)
            {
                //combatStartAnimator.isPlaying = false;
                combatStartAnimator.Play("FadeOut");
            }
            else if (combatLoopSource.isPlaying)
            {
                //combatLoopSource.isPlaying = false;
                combatLoopAnimator.Play("FadeOut");
            }
        }
        else
        {
            if (checkpointStartSource.isPlaying)
            {
                //checkpointStartSource.isPlaying = false;
                checkpointStartAnimator.Play("FadeOut");
            }
            else if (checkpointLoopSource.isPlaying)
            {
                //checkpointLoopSource.isPlaying = false;
                checkpointLoopAnimator.Play("FadeOut");
            }    
        }

        
        
    }

    /// <summary>
    /// Based on level, assigns the clips to audio sources.
    /// </summary>
    public void OnLevelLoad()
    {
        //TODO: Fix dis. Right now, it doesn't have seperate transition/loop clips, its all the same.
        if (LevelManager.currentLevel == 0)
        {
            combatStartSource.clip = Level0StartMusic;
            combatLoopSource.clip = Level0LoopMusic;
        }
        else if (LevelManager.currentLevel == 1)
        {
            combatStartSource.clip = combatStartingLevel1;
            combatLoopSource.clip = combatLoopLevel1;
        }
        else if (LevelManager.currentLevel == 2)
        {
            combatStartSource.clip = combatStartingLevel2;
            combatLoopSource.clip = combatLoopLevel2;
        }
        else if (LevelManager.currentLevel == 3)
        {
            combatStartSource.clip = combatStartingLevel3;
            combatLoopSource.clip = combatLoopLevel3;
        }
        else if (LevelManager.currentLevel == 4)
        {
            combatStartSource.clip = combatStartingLevel3;
            combatLoopSource.clip = combatLoopLevel3;
        }
        else if (LevelManager.currentLevel == 5)
        {
            //Perhaps level 0's winds?
            combatStartSource.clip = combatStartingLevel3;
            combatLoopSource.clip = combatLoopLevel3;
        }

        playingLoop = false;

        //Starts the level with combat, otherwise if checkpoint starts, its awkward.
        //if (LevelManager.currentLevel != 0)
            PlayMusic(AudioSourceType.combatStart);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == true)
        {
            //Does it really work tho.
            checkpointStartSource.volume = 0.8f;
            checkpointLoopSource.volume = 0.8f;
            combatStartSource.volume = 0.8f;
            combatLoopSource.volume = 0.8f;
        }
        

    }

}
