using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Mad spagghetti, excuse this (it works, but it could be FAR FAR cleaner)
public class PlayerSoundManager : MonoBehaviour
{

    //So every1 can access it, without searching for tags and bs like that.
    public static PlayerSoundManager globalInstance { set; get; }

    public enum AudioSourceName { SpearThrust, SpearHit, SpellCast1, SpellCast2, PlayerLanding, PlayerMovement, PlayerDash1, PlayerDash2, Player1Voice, Player2Voice, EnemyHit, TempHPGain, LavaSizzle, WarriorJump1, WarriorJump2, MageJump1, MageJump2, Glide, DashCooldown, ArmorSpell, Death, Revive, WaveSlam, BoatRow, UpSpell, Desync, Resync};

    [Header("(Warrior) Audio Sources")]

    [Tooltip("The audio source related when player attacks (generally, not related to any hit)")]
    public AudioSource SpearThrustSource;

    [Tooltip("The audio source related when player hits an enemy")]
    public AudioSource SpearHitSource;

    [Tooltip("The audio source related when player lands")]
    public AudioSource PlayerLandingSource;

    [Tooltip("The audio source for when player moves")]
    public AudioSource PlayerMovementSource;

    [Tooltip("The audio source related when cutscene & player1 talks")]
    public AudioSource Player1VoiceSource;

    [Tooltip("The audio source related when cutscene & player2 talks")]
    public AudioSource Player2VoiceSource;

    [Tooltip("The audio source when an enemy attack hits player")]
    public AudioSource EnemyHitSource;

    [Tooltip("The audio source for when player2 dodgerolls/wavedashes")]
    public AudioSource PlayerDashSource1;

    [Tooltip("The audio source for when player slow-falls or takes damage and flies away~")]
    public AudioSource PlayerDashSource2;

    [Tooltip("The audio source when warrior jumps, if it is playing it goes to 2")]
    public AudioSource WarriorJumpSource1;

    [Tooltip("The audio source when warrior jumps, if it is playing it goes to 2")]
    public AudioSource WarriorJumpSource2;

    [Tooltip("The audio source when mage jumps, if it is playing it goes to 2")]
    public AudioSource MageJumpSource1;

    [Tooltip("The audio source when mage jumps, if it is playing it goes to 2")]
    public AudioSource MageJumpSource2;

    [Tooltip("The audio source related when player casts a spell")]
    public AudioSource SpellCastSource1;

    [Tooltip("The audio source related when player casts a spell but 1 is playing.")]
    public AudioSource SpellCastSource2;

    [Tooltip("The audio source related when player's Dash refreshes/is out of cooldown")]
    public AudioSource DashCooldown;

    [Tooltip("The audio source when player becomes invulnerable, by damage or spell")]
    public AudioSource ArmorSpell;

    [Tooltip("The audio source when player dies")]
    public AudioSource Death;

    [Tooltip("The audio source when player revives")]
    public AudioSource Revive;

    [Tooltip("The audio source when 'up' spell is cast")]
    public AudioSource UpSpell;

    [Header("Misc")]
    [Tooltip("When player contacts lava")]
    public AudioSource LavaSizzleSource;

    [Tooltip("When you gain TempHP or enemy kills you")]
    public AudioSource TempHPGainSource;

    [Tooltip("When player glides")]
    public AudioSource GlideSource;

    [Tooltip("When the boat slams the sea in the intro")]
    public AudioSource WaveSlamSource;

    [Tooltip("Rowing the boat in the intro")]
    public AudioSource BoatRowSource;

    [Tooltip("When you desynchronize (e.g. branch spellword)")]
    public AudioSource DesyncSource;

    [Tooltip("When you resynchronize (e.g. link spellword)")]
    public AudioSource ResyncSource;

    //Happens at the very start of the game
    private void Awake()
    {
        globalInstance = this;
    }

    public void PlayClip(AudioClip clipToPlay, AudioSourceName audioSourceToCover, float _pitch = 0.99f, float _volume = 0.79f)//0.79&0.99 is more like a "default bool" value.
    {
        //Could minimize that by having a "pointer"/reference to audiosource, and call the 3 lines just once.
        if (audioSourceToCover == AudioSourceName.SpearThrust)
        {
            if (_volume != 0.79f)
                SpearThrustSource.volume = _volume;
            SpearThrustSource.pitch = _pitch;
            SpearThrustSource.clip = clipToPlay;
            SpearThrustSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.SpearHit)
        {
            if (_volume != 0.79f)
                SpearHitSource.volume = _volume;
            if (_pitch != 0.99f)
                SpearHitSource.pitch = _pitch;
            SpearHitSource.clip = clipToPlay;
            SpearHitSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.PlayerLanding)
        {
            if (_volume != 0.79f)
                PlayerLandingSource.volume = _volume;
            if (_pitch != 0.99f)
                PlayerLandingSource.pitch = _pitch;
            PlayerLandingSource.clip = clipToPlay;
            PlayerLandingSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.PlayerMovement)
        {
            if (_volume != 0.79f)
                PlayerMovementSource.volume = _volume;
            //if (_pitch != 0.99f)
            //PlayerMovementSource.pitch = _pitch;
            PlayerMovementSource.clip = clipToPlay;
            PlayerMovementSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.PlayerDash1)
        {
            if (PlayerDashSource1.isPlaying == false)
            {
                if (_volume != 0.79f)
                    PlayerDashSource1.volume = _volume;
                if (_pitch != 0.99f)
                    PlayerDashSource1.pitch = _pitch;
                PlayerDashSource1.clip = clipToPlay;
                PlayerDashSource1.Play();
            }
            else if (PlayerDashSource2.isPlaying == false) //1 is still playing the previous, aka is occupied
            {
                if (_volume != 0.79f)
                    PlayerDashSource2.volume = _volume;
                if (_pitch != 0.99f)
                    PlayerDashSource2.pitch = _pitch;
                PlayerDashSource2.clip = clipToPlay;
                PlayerDashSource2.Play();
            }
            else //both are playing, so play the former/first. (aint going for 3 audio sources for such a rare use-case)
            {
                if (_volume != 0.79f)
                    PlayerDashSource1.volume = _volume;
                if (_pitch != 0.99f)
                    PlayerDashSource1.pitch = _pitch;
                PlayerDashSource1.clip = clipToPlay;
                PlayerDashSource1.Play();
            }

        }
        else if (audioSourceToCover == AudioSourceName.PlayerDash2)
        {
            if (_volume != 0.79f)
                PlayerDashSource2.volume = _volume;
            if (_pitch != 0.99f)
                PlayerDashSource2.pitch = _pitch;
            PlayerDashSource2.clip = clipToPlay;
            PlayerDashSource2.Play();
        }
        else if (audioSourceToCover == AudioSourceName.Player1Voice)
        {
            if (_volume != 0.79f)
                Player1VoiceSource.volume = _volume;
            if (_pitch != 0.99f)
                Player1VoiceSource.pitch = _pitch;
            Player1VoiceSource.clip = clipToPlay;
            Player1VoiceSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.Player2Voice)
        {
            if (_volume != 0.79f)
                Player2VoiceSource.volume = _volume;
            if (_pitch != 0.99f)
                Player2VoiceSource.pitch = _pitch;
            Player2VoiceSource.clip = clipToPlay;
            Player2VoiceSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.EnemyHit)
        {
            if (_volume != 0.79f)
                EnemyHitSource.volume = _volume;
            if (_pitch != 0.99f)
                EnemyHitSource.pitch = _pitch;
            EnemyHitSource.clip = clipToPlay;
            EnemyHitSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.LavaSizzle)
        {
            if (_volume != 0.79f)
                LavaSizzleSource.volume = _volume;
            if (_pitch != 0.99f)
                LavaSizzleSource.pitch = _pitch;
            LavaSizzleSource.clip = clipToPlay;
            LavaSizzleSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.WarriorJump1)
        {
            if (WarriorJumpSource1.isPlaying == false)
            {
                if (_volume != 0.79f)
                    WarriorJumpSource1.volume = _volume;
                if (_pitch != 0.99f)
                    WarriorJumpSource1.pitch = _pitch;
                WarriorJumpSource1.clip = clipToPlay;
                WarriorJumpSource1.Play();
            }
            else if (WarriorJumpSource2.isPlaying == false) //1 is still playing the previous, aka is occupied
            {
                if (_volume != 0.79f)
                    WarriorJumpSource2.volume = _volume;
                if (_pitch != 0.99f)
                    WarriorJumpSource2.pitch = _pitch;
                WarriorJumpSource2.clip = clipToPlay;
                WarriorJumpSource2.Play();
            }
            else//both are playing, so play the first.
            {
                if (_volume != 0.79f)
                    WarriorJumpSource1.volume = _volume;
                if (_pitch != 0.99f)
                    WarriorJumpSource1.pitch = _pitch;
                WarriorJumpSource1.clip = clipToPlay;
                WarriorJumpSource1.Play();
            }
        }
        else if (audioSourceToCover == AudioSourceName.MageJump1)
        {
            if (MageJumpSource1.isPlaying == false)
            {
                if (_volume != 0.79f)
                    MageJumpSource1.volume = _volume;
                if (_pitch != 0.99f)
                    MageJumpSource1.pitch = _pitch;
                MageJumpSource1.clip = clipToPlay;
                MageJumpSource1.Play();
            }
            else if (MageJumpSource2.isPlaying == false) //1 is still playing the previous, aka is occupied
            {
                if (_volume != 0.79f)
                    MageJumpSource2.volume = _volume;
                if (_pitch != 0.99f)
                    MageJumpSource2.pitch = _pitch;
                MageJumpSource2.clip = clipToPlay;
                MageJumpSource2.Play();
            }
            else//both are playing, so play the first.
            {
                if (_volume != 0.79f)
                    MageJumpSource1.volume = _volume;
                if (_pitch != 0.99f)
                    WarriorJumpSource1.pitch = _pitch;
                MageJumpSource1.clip = clipToPlay;
                MageJumpSource1.Play();
            }
        }
        else if (audioSourceToCover == AudioSourceName.SpellCast1 || audioSourceToCover == AudioSourceName.SpellCast2)
        {
            if (SpellCastSource1.isPlaying == false || (SpellCastSource2.isPlaying && SpellCastSource1.isPlaying))
            {
                if (_volume != 0.79f)
                    SpellCastSource1.volume = _volume;
                if (_pitch != 0.99f)
                    SpellCastSource1.pitch = _pitch;
                SpellCastSource1.clip = clipToPlay;
                SpellCastSource1.Play();
            }
            else if (SpellCastSource2.isPlaying == false)
            {
                if (_volume != 0.79f)
                    SpellCastSource2.volume = _volume;
                if (_pitch != 0.99f)
                    SpellCastSource2.pitch = _pitch;
                SpellCastSource2.clip = clipToPlay;
                SpellCastSource2.Play();
            }
        }
        else if (audioSourceToCover == AudioSourceName.TempHPGain)
        {
            if (_volume != 0.79f)
                TempHPGainSource.volume = _volume;
            if (_pitch != 0.99f)
                WarriorJumpSource1.pitch = _pitch;
            TempHPGainSource.clip = clipToPlay;
            TempHPGainSource.Play();
        }
        else if (audioSourceToCover == AudioSourceName.Glide)
        {
            if (_volume != 0.79f)
                GlideSource.volume = _volume;
            if (_pitch != 0.99f)
                GlideSource.pitch = _pitch;
            GlideSource.clip = clipToPlay;
            GlideSource.Play();

            //Debug.Log(GlideSource.clip);
        }
        else if (audioSourceToCover == AudioSourceName.DashCooldown)//As it should be.
            DashCooldown.Play();
        else if (audioSourceToCover == AudioSourceName.ArmorSpell)
            ArmorSpell.Play();
        else if (audioSourceToCover == AudioSourceName.UpSpell)
            UpSpell.Play();
            

    }

    public void PlayAudioSource(AudioSourceName audioSourceToPlay)
    {
        if (audioSourceToPlay == AudioSourceName.DashCooldown)
            DashCooldown.Play();
        else if (audioSourceToPlay == AudioSourceName.ArmorSpell)
            ArmorSpell.Play();
        else if (audioSourceToPlay == AudioSourceName.Death)
            Death.Play();
        else if (audioSourceToPlay == AudioSourceName.Revive)
            Revive.Play();
        else if (audioSourceToPlay == AudioSourceName.WaveSlam)
            WaveSlamSource.Play();
        else if (audioSourceToPlay == AudioSourceName.BoatRow)
            BoatRowSource.Play();
        else if (audioSourceToPlay == AudioSourceName.UpSpell)
            UpSpell.Play();
        else if (audioSourceToPlay == AudioSourceName.Desync)
            DesyncSource.Play();
        else if (audioSourceToPlay == AudioSourceName.Resync)
            ResyncSource.Play();
    }

    public void PlayTempHPGainSFX(float _pitch = 1f, float _volume = 0.5f)
    {
        TempHPGainSource.pitch = _pitch;
        TempHPGainSource.volume = _volume;
        TempHPGainSource.Play();
    }

    //This could be generic. Also, the enumeration could link directly.
    public float GetGlideVolume()
    {
        return GlideSource.volume;
    }

    public bool IsGlidePlaying()
    {
        if (GlideSource.isPlaying)
            return true;
        else
            return false;
    }

    public void ChangePitch(float pitch, AudioSourceName audioSourceToCover)
    {
        if (audioSourceToCover == AudioSourceName.SpearThrust)
            SpearThrustSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.SpearHit)
            SpearHitSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.SpellCast1)
            SpellCastSource1.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.SpellCast2)
            SpellCastSource2.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.PlayerLanding)
            PlayerLandingSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.PlayerMovement)
            PlayerMovementSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.PlayerDash1)
            PlayerDashSource1.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.PlayerDash2)
            PlayerDashSource2.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.Player1Voice)
            Player1VoiceSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.Player2Voice)
            Player2VoiceSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.EnemyHit)
            EnemyHitSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.WarriorJump1)
            WarriorJumpSource1.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.WarriorJump2)
            WarriorJumpSource2.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.MageJump1)
            MageJumpSource1.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.MageJump2)
            MageJumpSource2.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.TempHPGain)
            TempHPGainSource.pitch = pitch;
        else if (audioSourceToCover == AudioSourceName.Glide)
            GlideSource.pitch = pitch;
    }

    public void ChangeVolume(float volume, AudioSourceName audioSourceToCover)
    {
        if (audioSourceToCover == AudioSourceName.SpearThrust)
            SpearThrustSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.SpearHit)
            SpearHitSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.SpellCast1)
            SpellCastSource1.volume = volume;
        else if (audioSourceToCover == AudioSourceName.SpellCast2)
            SpellCastSource2.volume = volume;
        else if (audioSourceToCover == AudioSourceName.PlayerLanding)
            PlayerLandingSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.PlayerMovement)
            PlayerMovementSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.PlayerDash1)
            PlayerDashSource1.volume = volume;
        else if (audioSourceToCover == AudioSourceName.PlayerDash2)
            PlayerDashSource2.volume = volume;
        else if (audioSourceToCover == AudioSourceName.Player1Voice)
            Player1VoiceSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.Player2Voice)
            Player2VoiceSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.EnemyHit)
            EnemyHitSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.WarriorJump1)
            WarriorJumpSource1.volume = volume;
        else if (audioSourceToCover == AudioSourceName.WarriorJump2)
            WarriorJumpSource2.volume = volume;
        else if (audioSourceToCover == AudioSourceName.MageJump1)
            MageJumpSource1.volume = volume;
        else if (audioSourceToCover == AudioSourceName.MageJump2)
            MageJumpSource2.volume = volume;
        else if (audioSourceToCover == AudioSourceName.TempHPGain)
            TempHPGainSource.volume = volume;
        else if (audioSourceToCover == AudioSourceName.Glide)
            GlideSource.volume = volume;
    }

    public void Stop(AudioSourceName audioSourceToStop)
    {
        if (audioSourceToStop == AudioSourceName.SpearThrust)
            SpearThrustSource.Stop();
        else if (audioSourceToStop == AudioSourceName.SpearHit)
            SpearHitSource.Stop();
        else if (audioSourceToStop == AudioSourceName.SpellCast1)
            SpellCastSource1.Stop();
        else if (audioSourceToStop == AudioSourceName.SpellCast2)
            SpellCastSource2.Stop();
        else if (audioSourceToStop == AudioSourceName.PlayerLanding)
            PlayerLandingSource.Stop();
        else if (audioSourceToStop == AudioSourceName.PlayerMovement)
            PlayerMovementSource.Stop();
        else if (audioSourceToStop == AudioSourceName.PlayerDash1)
            PlayerDashSource1.Stop();
        else if (audioSourceToStop == AudioSourceName.PlayerDash2)
            PlayerDashSource2.Stop();
        else if (audioSourceToStop == AudioSourceName.Player1Voice)
            Player1VoiceSource.Stop();
        else if (audioSourceToStop == AudioSourceName.Player2Voice)
            Player2VoiceSource.Stop();
        else if (audioSourceToStop == AudioSourceName.EnemyHit)
            EnemyHitSource.Stop();
        else if (audioSourceToStop == AudioSourceName.WarriorJump1)
            WarriorJumpSource1.Stop();
        else if (audioSourceToStop == AudioSourceName.WarriorJump2)
            WarriorJumpSource2.Stop();
        else if (audioSourceToStop == AudioSourceName.MageJump1)
            MageJumpSource1.Stop();
        else if (audioSourceToStop == AudioSourceName.MageJump2)
            MageJumpSource2.Stop();
        else if (audioSourceToStop == AudioSourceName.TempHPGain)
            TempHPGainSource.Stop();
        else if (audioSourceToStop == AudioSourceName.Glide)
        {
            GlideSource.Stop();
            //Debug.Log("Stopping Glide SFX...");
        }
            
    }
}
