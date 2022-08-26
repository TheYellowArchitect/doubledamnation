using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VoiceSoundManager : MonoBehaviour
{
    //So every1 can access it, without searching for tags and bs like that.
    public static VoiceSoundManager globalInstance { set; get; }

    [Header("Audio Sources")]
    public AudioSource warriorVoice1;
    public AudioSource warriorVoice2;
    public AudioSource mageVoice1;
    public AudioSource mageVoice2;

    //PowerUp -> Cutscene -> All of them
    //Cutscene -> PowerUp -> PowerUp simply isn't played.
    //Death/Level -> Interruption -> Interruption plays instantly.
    public bool playingMidCutscene;
    public bool playingPowerUp;
    public bool playingInterruption;

    public bool warriorSpeaking = false;
    public bool mageSpeaking = false;

    private void Awake()
    {
        globalInstance = this;
    }

    /// <summary>
    /// With the warrior color (0 or 255) I understand which AudioSource to use (and also set a helpful boolean)
    /// Then I stop the opposite audio source, and set up and play the audiosource intended.
    /// </summary>
    /// <param name="warriorColor"></param>
    /// <param name="voiceClip"></param>
    public void PlayVoice(float warriorColor, AudioClip voiceClip)
    {
        //Could work without this boolean, but being 100% clean/readable code ftw
        bool warrior;

        //WarriorColor is dialogueTextUI.color.r, with black having 0 and white having it 255.
        if (warriorColor == 0)
            warrior = true;
        else
            warrior = false;

        if (voiceClip == null)
        {
            Debug.LogError("VoiceClip in PlayVoice is null!\nRetard.");
            return;
        }

        if (warrior)
        {
            //warriorSpeaking = true;

            if (warriorVoice1.isPlaying == false)
            {
                warriorVoice1.clip = voiceClip;

                warriorVoice1.Play();
            }
            else
            {
                warriorVoice2.clip = voiceClip;

                warriorVoice2.Play();
            }

            
        }
        else
        {
            //mageSpeaking = true;

            if (mageVoice1.isPlaying == false)
            {
                mageVoice1.clip = voiceClip;

                mageVoice1.Play();
            }
            else if (mageVoice2.isPlaying == false)
            {
                mageVoice2.clip = voiceClip;

                mageVoice2.Play();
            }


        }
    }

    /// <summary>
    /// Used by dialogue manager's level cutscene dialogue, to skip it via space.
    /// Also used by InterruptionDialogue for obvious reasons.
    /// Stops the audio source of the boolean player, and sets the "speaking" flag to false
    /// </summary>
    /// <param name="warrior"></param>
    public void StopVoice(bool warrior = true)
    {
        if (mageVoice1.isPlaying)
            mageVoice1.Stop();
        else if (mageVoice2.isPlaying)
            mageVoice2.Stop();
        else if (warriorVoice1.isPlaying)
            warriorVoice1.Stop();
        else if (warriorVoice2.isPlaying)
            warriorVoice2.Stop();
        /*
        if (warrior)
        {
            warriorSpeaking = false;

            warriorVoice1.Stop();

            //With this, it is released from memory, cuz by setting sth else or null, garbage collector ignores it.
            if (Application.isEditor == false)
                Destroy(warriorVoice1.clip);
        }
        else
        {
            mageSpeaking = false;

            mageVoice1.Stop();

            //With this, it is released from memory, cuz by setting sth else or null, garbage collector ignores it.
            if (Application.isEditor == false)
                Destroy(mageVoice1.clip);
        }
        //Reminder this exists: #if DEVELOPMENT_BUILD || UNITY_EDITOR
        #endif
        */
    }

    /// <summary>
    /// Useless/Obsolete function.
    /// </summary>
    /// <returns></returns>
    public float GetVoiceClipLength()
    {
        //Hopefully playtests go well so i remove dis edge case
        if (warriorSpeaking && mageSpeaking)
        {
            Debug.LogError("In VoiceSoundManager, both warrior and mage are speaking. This should be impossibru.");
            return -1;
        }

        if (warriorSpeaking && warriorVoice1.clip != null)
            //return warriorVoice.clip.samples / (warriorVoice.clip.channels * warriorVoice.clip.frequency);
            //return AudioSettings.dspTime;
            return warriorVoice1.clip.length;
        else if (mageSpeaking && mageVoice1.clip != null)
            //return mageVoice.clip.samples / (mageVoice.clip.channels * mageVoice.clip.frequency);
            return mageVoice1.clip.length;
        else
            return -1;
    }

    public void SetMidCutscene(bool value = true)
    {
        playingMidCutscene = value;
    }

    public void SetPowerUp(bool value = true)
    {
        playingPowerUp = value;
    }

    public void SetInterruption(bool value = true)
    {
        playingInterruption = value;
    }

    /// <summary>
    /// Resets "playingCutscene" and "playingPowerUp" booleans, so no bugs happen
    /// Also resets the flags: "warriorSpeaking and mageSpeaking" to false, so there won't be any bugs when someone starts speaking, but temporarily deleted cuz obsolete/useless.
    /// </summary>
    public void ResetSpeakingFlags()
    {
        //warriorSpeaking = false;
        //mageSpeaking = false;

        playingMidCutscene = false;
        playingPowerUp = false;
        playingInterruption = false;
    }

    public bool IsPlayingMidCutscene()
    {
        return playingMidCutscene;
    }

    public bool IsPlayingPowerUp()
    {
        return playingPowerUp;
    }

    public bool IsPlayingInterruption()
    {
        return playingInterruption;
    }

    public bool IsPlayingMage()
    {
        if (mageVoice1.isPlaying)
            return true;
        else if (mageVoice2.isPlaying)
            return true;

        return false;
    }

    public bool IsPlayingWarrior()
    {
         if (warriorVoice1.isPlaying)
            return true;
        else if (warriorVoice2.isPlaying)
            return true;

        return false;
    }

    //Unused but ultimately not deleted since if you are reading this, it is a nice script to get into audio programming, I hope it helps :)
    //I don't claim credit for the below function. Credits to user Untherow https://answers.unity.com/questions/343057/how-do-i-make-unity-seamlessly-loop-my-background.html
    //Bless that guy! What a script, audio programming is rarely seen tbh.
    //As to why I wanted to use it, the optimal step to avoid .mp3 gaps is to simply mass-convert all my .mp3 clips to .OGG for that seamless loop/connection.
    //But, .mp3 is already a format with lost quality, and converting again will make the sound even worse quality-wise. So, RIP CPU for that quality.
    //^Plot twist, unused cuz i have to decompress on load all the clips for this to work + intense calculation kills the framerate + it doesn't really work?
    /// <summary>
    /// Trims silence from both ends in an AudioClip.
    /// Makes mp3 files seamlessly loopable.
    /// Obviously CPU-intensive since it essentially reads and re-creates the clip without the gaps. Use this when you have no other choice.
    /// </summary>
    /// <param name="inputAudio"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public AudioClip trimSilence(AudioClip inputAudio, float threshold = 0.05f)//threshold is volume level btw (not confirmed though)
    {

        // If you have no idea on audio programming, this is a nice introduction. The below float array, in essence, stores the entire audio clip.
        // ^Think of the waveform inside, and inside is stored every single point in the waveform. (we are talking about an extreme amount, in voiceclips it sometimes touches 1 million floats if not more)
        // Copy samples from input audio to an array. AudioClip uses interleaved format so the length in samples is multiplied by channel count
        float[] samplesOriginal = new float[inputAudio.samples * inputAudio.channels];//there are 2 channels usually (if not mono?)

        Debug.Log("The samplesOriginal length/size is: " + samplesOriginal.Length);

        //If decompressed on load, it gets the audio clip data(emphasis on data), and puts it in the float array.
        inputAudio.GetData(samplesOriginal, 0);

        //Looping through some points of it, to print the approximate waveform (this could be visualized btw)
        for (int i = 0; i < samplesOriginal.Length; i = i + 1000)
        {
            Debug.Log("Float inside is: " + samplesOriginal[i]);
        }

        // The below 2 forloops, are a replacement of the Array.Find(Last)Index ones.
        //What they essentially both do, is go in the audio clip data (samplesOriginal[]) and find the start and end of the data, where there is silence. (dat .mp3 reee)

        // Find first and last sample (from any channel) that exceed the threshold
        int audioStart = 0;
        for (int i = 0; i < samplesOriginal.Length; i++)
        {
            if (samplesOriginal[i] > threshold) //|| samplesOriginal[i] < threshold)
            {
                audioStart = i;
                break;
            }
        }

        int audioEnd = 0;
        for (int i = 0; i < samplesOriginal.Length; i++)
        {
            if (samplesOriginal[i] > threshold) //|| samplesOriginal[i] < threshold)
                audioEnd = i;
        }
        //

        /*
        // Find first and last sample (from any channel) that exceed the threshold //THRESHOLD * -1 cache it plz, ty! for the <
        int audioStart = Array.FindIndex(samplesOriginal, pickedSample => pickedSample > threshold);
        int audioEnd = Array.FindLastIndex(samplesOriginal, pickedSample => pickedSample > threshold);
        */

        Debug.Log("Audio Start is " + audioStart + " and audioEnd is " + audioEnd);

        // Copy trimmed audio data into another array
        float[] samplesTrimmed = new float[audioEnd - audioStart];//With end-start, the new size is equal to the size without the silences.

        //From here and below, I cannot fully 100% understand, but an approximate synopsis:
        //Copy samplesOriginal data to samplesTrimmed, but without the silences. Then, create a new audioclip, by using all of the previous data (mostly inputAudio and samplesTrimmed) and return it;
        Array.Copy(samplesOriginal, audioStart, samplesTrimmed, 0, samplesTrimmed.Length);
        // Create new AudioClip for trimmed audio data
        AudioClip trimmedAudio = AudioClip.Create(inputAudio.name, samplesTrimmed.Length / inputAudio.channels, inputAudio.channels, inputAudio.frequency, false);
        trimmedAudio.SetData(samplesTrimmed, 0);
        return trimmedAudio;
    }

    /*
    Actually, you can make a variation for every1 who is as fucked as me.
    Conditions/Notes: Needs "Pre-load Data" + Expensive on CPU since you use GetData.

    One of the variables is: bool SearchUntilEnd [tooltip("Set this to true if you know the voiceClip is .MP3 \nWhen this boolean is false, it stops searching at about 50 ms of time, instead of searching up until the end.")]
    Searches for audioStart, exactly like above (with the bool, you do a more complex forloop)
    Then, PlayScheduled (dspTime + the delay you found on audioStart)

    Asset store or not (free ofc), put some disclaimer/notes like:
    ".MP3s in gamedev is a very stupid move. However, if you have already done it, and don't want to convert them to another format because some quality will be lost, this asset is just for you. Though, GetData() is a little CPU-Expensive.)"
     */

}
