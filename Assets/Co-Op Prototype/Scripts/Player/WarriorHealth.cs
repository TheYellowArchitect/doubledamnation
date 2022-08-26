using System.Collections;
using UnityEngine;
using System;
using NaughtyAttributes;
using EZCameraShake;

//Make Warrior Health like this:
//      Health
//     ^          ^
//WarriorHealth | EnemyHealth | DestructibleHealth
//ggwp, finally. Health has most of the below variables, but with children u can go for extra like TempHP. Not to mention, all healths can be upcasted.

[DisallowMultipleComponent]
[RequireComponent(typeof(WarriorMovement))]
public class WarriorHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    //Properties, mandatory, since interface IDamageable demands them, and Unity doesn't play well with properties.
    [Tooltip("After tutorial, what is the normal/expected max hp?\nRecommended: 5")]
    public int NormalMaxHealth = 5;
    [SerializeField]
    private int maxHealth;
    public int MaxHealth
    {
        get { return maxHealth; }
        set
        {
            if (value < 1) Debug.Log("Cannot be negative or 0, watcha doin?");
            else maxHealth = value;
        }
    }

    [SerializeField]
    private int currentHealth;
    public int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }

    [Header("Misc?")]
    [MinValue(0f)]
    [Tooltip("How much time will the player be invulnerable after a hit? Be generous, like 2+ seconds :P")]
    public float InvulnerabilityTimeAfterHit;

    [MinValue(0f)]
    [Tooltip("How far away should an attack throw the player?\nDefault is obviously 1.")]
    public float KnockbackMultiplier;

    [MinValue(0f)]
    [Tooltip("When player lands right atop a monster, for how long should he be hitstunned?")]
    public float playerStompHitstunDuration;

    [MinValue(0f)]
    [Tooltip("When player lands right atop a monster, how much force should be applied?")]
    public float playerStompKnockbackPower;

    [MinValue(0f)]
    [Tooltip("How further away should player be knocked away in the X axis, when it is the final blow?")]
    public float DeathKnockbackXMultiplier;

    [MinValue(0)]
    [Tooltip("How much will be added in influenceY, when it is the final blow?")]
    public int DeathKnockbackY = 10;

    [Tooltip("The particleSystem for when you are damaged")]
    public ParticleSystem darkwindDamaged;

    [Tooltip("The transform for rotating the particle system, so it matches the impact origin.")]
    public Transform darkwindDamagedDirection;

    private HealthIndicationManager commonHealthIndicationManager;

    [MinValue(0)]
    [Tooltip("When player takes damage, temporary HP is prioritized over actual HP. This shows how much player has.\nDefault 0 obv")]
    public int TempHP = 0;

    //Set by TakeDamage
    private Vector2 directionHit;

    public ParticleSystemRenderer enviromentalParticlesRenderer;

    private WarriorMovement warriorBehaviour;
    private AetherMage aetherBehaviour;

    //Timer variables
    Timer hitstunTimer;
    Timer invulnerabilityTimer;

    public static bool isInvulnerable = false;

    public Action DiedEvent;
    public Action DiedNotifyOtherPlayerEvent;
    public Action PoweredUpEvent;
    public Action PoweredUpNotifyOtherPlayerEvent;
    public Action<int> TookDamageEvent;
    public static bool dying = false;

    public int defaultMaxHealth;
    public bool showHPIndication = true;

    private void Awake()
    {
        //Sets Current Health = MaxHealth
        SetStartingHealth();

        defaultMaxHealth = MaxHealth;

        warriorBehaviour = GetComponent<WarriorMovement>();

        aetherBehaviour = warriorBehaviour.mageBehaviour.GetComponent<AetherMage>();

        //Timers
        TimerManager timerManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TimerManager>();

        //Hitstun
        hitstunTimer = timerManager.CreateTimer(hitstunTimer, 2, 3f, false, true);//3f doesnt matter since configured everytime before starting
        hitstunTimer.TriggerOnEnd += FinishedHitstun;

        //Invulnerability
        invulnerabilityTimer = timerManager.CreateTimer(invulnerabilityTimer, 3, InvulnerabilityTimeAfterHit, false, true);
        invulnerabilityTimer.TriggerOnEnd += FinishedInvulnerability;
    }

    private void Start()
    {
        commonHealthIndicationManager = GetComponent<HealthIndicationManager>();

        warriorBehaviour.startedRevive += StartedRevive;
    }

    public void FinishedHitstun()
    {
        warriorBehaviour.isHitstunned = false;
    }

    //Called if player cancels hitstun, via jump
    public void ResetHitstun()
    {
        hitstunTimer.Reset();
    }

    public void FinishedInvulnerability()
    {
        isInvulnerable = false;

        aetherBehaviour.EndAether();

        //TODO: Store "permanent" dmg sources, like hazards, and minotaur horns, like, register by collisionEnter/Exit via bool variable, and if true, damage instantly.
    }

    public void RewardTempHP(int addedTempHP)
    {
        TempHP = TempHP + addedTempHP;

        RewardTempHPAesthetics();

        //Send powerup to other player pretty much
        if (PoweredUpNotifyOtherPlayerEvent != null)
            PoweredUpNotifyOtherPlayerEvent();
    }

    //Fully VFX/SFX, except powerUpDialogue, which I guess is also aesthetics lol
    public void RewardTempHPAesthetics()
    {
        //VFX//Also cringe on why static is fail.
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.TempHPGain, GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(-6.5f, 8,0), GameObject.FindGameObjectWithTag("Player"));
        if (GameObject.FindGameObjectWithTag("TempHP") == null)
        {
            VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.TempHPAura, GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(-0.2f, -2.5f, 0), GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject);
            VFXManager.globalInstance.GetLastCreatedVFX().tag = "TempHP";
        }

        //SFX
        PlayerSoundManager.globalInstance.PlayTempHPGainSFX();

        //Notify the dialogueManager so the appropriate dialogue plays
        if (PoweredUpEvent != null)
            PoweredUpEvent();

        //Show the bonus HP -> Why not the full hp? Like, "current HP" + " + temp HP" ????
        StartTempHPIndication();

        //Just in case if 1 HP, so particles wont fly around like crazy
        DisableEnviromentalVFXPivotIntensity();
    }

    public void RemoveTempHP()
    {
        TempHP = 0;

        //So as to get the dangerHP to "" instead of "0".
        commonHealthIndicationManager.Reset();

        DisableEnviromentalVFXPivotIntensity();

        //Delete the playerkiller VFX if it exists
        if (GameObject.FindGameObjectWithTag("TempHP") != null)
            GameObject.FindGameObjectWithTag("TempHP").GetComponent<TempHPBehaviour>().Die(3f);
    }

    /// <summary>
    /// Passed directly from Warrior's OnLevelLoad, so you dont have to find which tag.
    /// </summary>
    /// <param name="enviromentalVFXGameObject"></param>
    public void CacheEnviromentalVFX(GameObject enviromentalVFXGameObject)
    {
        if (LevelManager.currentLevel == 1 || LevelManager.currentLevel == 2)
            enviromentalParticlesRenderer = enviromentalVFXGameObject.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
    }

    /// <summary>
    /// Doesn't check for already existing cache, since this runs on level load and should be brand new one.
    /// </summary>
    public void CacheEnviromentalVFX()
    {
        if (LevelManager.currentLevel == 1 || LevelManager.currentLevel == 2)
            enviromentalParticlesRenderer = GameObject.FindGameObjectWithTag("EnviromentVFX").transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
    }

    /// <summary>
    /// When you reach 3 HP, the enviromental VFX "subtly" start to moving/acting differently, so it makes the game feel more intense.
    /// </summary>
    public void DetermineEnviromentalVFXPivotIntensity()
    {
        if (currentHealth != 1)
            return;

        if (LevelManager.currentLevel == 1)
            enviromentalParticlesRenderer.pivot = new Vector3(55, 0, 0);
        else if (LevelManager.currentLevel == 2)
            enviromentalParticlesRenderer.pivot = new Vector3(0, 55, 0);
    }

    /// <summary>
    /// See "DetermineEnviromentalVFXPivotIntensity"
    /// What this does, is on death, reset it.
    /// </summary>
    public void DisableEnviromentalVFXPivotIntensity()
    {
        if (LevelManager.currentLevel == 1 || LevelManager.currentLevel == 2)
            enviromentalParticlesRenderer.pivot = Vector3.zero;
    }

    //IDamageable stuff
    [Button("Take 1 Damage")]
    public void ETakeDamage()
    {
        TakeDamage(1, Vector2.zero);
    }

    public void TakeDamage(int damageTaken, Vector3 damageOrigin, int knockbackPowerX = 0, int knockbackPowerY = 0, float hitstun = 0f, bool restrictY = true, bool hitByHazard = false)
    {

        //Checks if already hit recently/isInvulnerable
        if (isInvulnerable)
        {
            //TODO: VFX when hit while invulnerable, take knockback.

            return;//for now ;)
        }

        if (dying)//????
            return;

        if (TookDamageEvent != null)
            TookDamageEvent(damageTaken);

        //Update PlayerStats
        PlayerStatsManager.globalInstance.IncreaseDamageTakenCount();

        //Rotate if needed
        //Sets the direction to player (Normalized it so that it has a length of 1 world unit)
        directionHit = transform.position - damageOrigin;

        if (warriorBehaviour.GetFacingLeft() && directionHit.x < 0)
            warriorBehaviour.FlipPlayer();
        else if (warriorBehaviour.GetFacingLeft() == false && directionHit.x > 0)
            warriorBehaviour.FlipPlayer();

        //If taken damage before casting a spell, P2 will be on the ground from dialogue, this checks for that and fixes it :D
        warriorBehaviour.mageBehaviour.DetermineWarriorParenting();

        //On take damage, walljump available, so u can go for mad techniques by utilizing spikes and midair enemies.
        if (DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.KillAndHit || DarkwindDistortionManager.globalInstance.playerWalljumpReset == DarkwindDistortionManager.WallJumpReset.Hit)
            warriorBehaviour.ResetWalljump();
        //Above works but... It rewards taking damage. In simpler words, it will be abused by the majority, instead of the very few speedrunners' special use-cases.
        //Yeah fuck it, ima activate it. Since u also gain walljump on kill, you can rely on walls lul. -> Disabled again lul. Why give walljump for each kill since u get dash for new walls?
        //Tl;dr activated by default, because even if abused, it is positive feedback to the abusers. And the speedrunners/veterans will love it anw since it adds more depth.

        //^Also resets dash
        warriorBehaviour.OnKillResetDodgeroll();//Rename this function to "ResetDodgeroll(Variables)(){}"

        //If has armor/tempHP, prioritize dat.
        if (TempHP > 0)
        {
            TempHP = TempHP - damageTaken;

            //Just lost all of the TempHP
            if (TempHP <= 0)
            {
                //Checks if damage was more than TempHp -> e.g. 1 TempHP but 2 damage
                //So the rest of the damage goes to currentHealth, if more than tempHP
                CurrentHealth = CurrentHealth + TempHP;

                //Delete the VFX
                GameObject.FindGameObjectWithTag("TempHP").GetComponent<TempHPBehaviour>().Die(3f);
            }
            
        }
        else
            CurrentHealth = CurrentHealth - damageTaken;


        //Activate Invulnerable status(muh i-frames)
        ActivateDamagedInvulnerability();

        //==============================
        //REWIND CHECK
        //if about to die and full mana, go for rewind instead!
        if (CurrentHealth < 1 && MageBehaviour.globalInstance.IsCurrentManaMax())
        {
            //Give +1 health (so you lose it below as damage and not death)
            CurrentHealth = CurrentHealth + 1;

            //And cast rewind!
            //When MageBehaviour's refactoring is finalized, you won't have to GetComponent but simply use enumeration feelsgoodman
            MageBehaviour.globalInstance.GetComponent<SpellRewind>().DeathCast();

            //Reduce max mana by one
            //Don't forget to reset this on warrior's revive!
            //playerManaManager.SetMaxMana(playerManaManager.GetMaxMana() - 1);
            //^Bloat in both design, and code.
        }
        //==============================


        //Is player about to die? (damage calculation is done)
        if (CurrentHealth < 1 && LevelManager.currentLevel != 7)
        {
            Die();

            //Knockback
            //===================
            //directionHit is calculated above

            Debug.Log("Direction hit is: " + directionHit);

            Debug.DrawRay(transform.position, directionHit, Color.blue, 2f);
            //Debug.Log("Power and multiplier are: " + knockbackPowerY + " " + KnockbackMultiplier);
            //X axis
            if (directionHit.x > 0f)
                warriorBehaviour.CalculateForces(true, 1 * knockbackPowerX * KnockbackMultiplier * DeathKnockbackXMultiplier, false, false);
            else
                warriorBehaviour.CalculateForces(true, -1 * knockbackPowerX * KnockbackMultiplier * DeathKnockbackXMultiplier, false, false);

            //Debug.Log("before" + warriorBehaviour.GetInfluenceY());

            //Y axis
            if (directionHit.y > 0f)
                warriorBehaviour.CalculateForces(false, 1 * knockbackPowerY * KnockbackMultiplier + DeathKnockbackY, false, false);
            else
                warriorBehaviour.CalculateForces(false, -1 * knockbackPowerY * KnockbackMultiplier + DeathKnockbackY,false, false);

            //Rotates the player renderer, so death animation won't look janky
            if (warriorBehaviour.GetInfluenceX() == 0)
            {
                Debug.Log("InfluenceY is: " + warriorBehaviour.GetInfluenceY());
                if (warriorBehaviour.GetInfluenceY() > 0)
                    warriorBehaviour.rendererBehaviour.RotateVertically(true, warriorBehaviour.GetFacingLeft());
                else
                    warriorBehaviour.rendererBehaviour.RotateVertically(false, warriorBehaviour.GetFacingLeft());
            }

            //So as to get the dangerHP to "" instead of "0".
            commonHealthIndicationManager.Reset();
            /*
            Debug.Log("KnockbackPowerY is: " + knockbackPowerY);
            Debug.Log("influenceY: " + warriorBehaviour.GetInfluenceY());
            Debug.Break();
            */
        }
        else if (CurrentHealth < 1 && LevelManager.currentLevel == 7)
            LevelEditorDeath();
        else
        {
            //UI Latin Number above player
            StartDamagedHPIndication();

            //VFX of Damage. Spawns between player and damager, 0.8 towards player though.
            //VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedPlayer, Vector3.Lerp(transform.position, damageOrigin, 0.2f), this.gameObject);
            if(hitByHazard == false)
                VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DamagedPlayer, transform.position + new Vector3(3.5f,2.4f,0), this.gameObject);

            //the VFX of damaged, for now.
            aetherBehaviour.CastAether();

            //Hitstun Activated
            //Fuck events, directly notified.
            warriorBehaviour.justTookDamage = true;

            //Sets the timer to hitstun time
            hitstunTimer.Configure(hitstun, true);
            hitstunTimer.Activate();

            //Set Screen Shake appropriate to damage
            if (currentHealth > 2)
                CameraShaker.Instance.ShakeOnce(15f, 10f, .1f, 1f);
            else
                CameraShaker.Instance.ShakeOnce(25f, 15f, .1f, 1.1f);

            //Knockback
            //===================
            //directionhit is calculated above

            Debug.DrawRay(transform.position, directionHit, Color.blue, 2f);
            //Debug.Log("Power and multiplier are: " + knockbackPowerY + " " + KnockbackMultiplier);
            //X axis
            if (directionHit.x > 0)
                warriorBehaviour.CalculateForces(true, 1 * knockbackPowerX * KnockbackMultiplier, false, false);//I haven't removed the " 1 * knockbackPowerXblabla", to show its direction.
            else
                warriorBehaviour.CalculateForces(true, -1 * knockbackPowerX * KnockbackMultiplier, false, false);

            //Debug.Log("before" + warriorBehaviour.GetInfluenceY());

            //Y axis
            if (directionHit.y > 0)
                warriorBehaviour.CalculateForces(false, 1 * knockbackPowerY * KnockbackMultiplier, restrictY, false);
            else
                warriorBehaviour.CalculateForces(false, -1 * knockbackPowerY * KnockbackMultiplier, restrictY, false);

            //Vibrate Controller
            if (SettingsMenu.VibrationActive)
                StartCoroutine(VibrateController(false));

            //Make background particles more intense.
            DetermineEnviromentalVFXPivotIntensity();
        }

        //VFX
        //Perhaps put this only when not dead?

        //Reset Rotation
        //if (warriorBehaviour.GetFacingLeft() == false)
            //darkwindDamagedDirection.eulerAngles = new Vector3(0f, 0f, 90f);
        //else
            //darkwindDamagedDirection.eulerAngles = new Vector3(0f, 0f, 270f);

        //Rotate appropriately to direction.
        //darkwindDamagedDirection.LookAt(damageOrigin, Vector3.forward);//Without the second parameter, it takes vector3.up and fucks shit up. It only works in this way. Tricky to figure out, but bless Unity API with that function, saved me 30 lines :P

        //darkwindDamagedDirection.eulerAngles = new Vector3(0f, 0f, darkwindDamagedDirection.rotation.z);

        //2 lines below, to rotate the particle system, to fit with damage origin :)
        //Bless the interwebz. Hard finding it though.
        //https://answers.unity.com/questions/1023987/lookat-only-on-z-axis.html jimbobulus2 <3 (I hope I will find time after this game to study trigonometry)
        float rotationZ = Mathf.Atan2(directionHit.y, directionHit.x) * Mathf.Rad2Deg;
        darkwindDamagedDirection.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ + 90);

        //Alternative?: 
        //Vector3 lookAtPosition = new Vector3 (directionHit.x, directionHit.y, this.position.z);
        //this.transform.LookAt(lookAtPosition);

        darkwindDamaged.Play();


    }

    public void StompPushed(bool OriginIsRightDirection, int stompKnockbackPower = 0, float hitstunStompDuration = 0f)
    {

        //Hitstun Activated
        //Fuck events, directly notified.
        warriorBehaviour.justTookDamage = true;

        //Sets the timer to hitstun time
        if (hitstunStompDuration == 0)//Player is the one stomping
            hitstunTimer.Configure(playerStompHitstunDuration, true);
        else
            hitstunTimer.Configure(hitstunStompDuration, true);

        hitstunTimer.Activate();

        //Screen Shake
        CameraShaker.Instance.ShakeOnce(3f, 5f, .1f, 1f);
        
        //Knockback
        if (stompKnockbackPower == 0)//Player is the one stomping
        {
            if (OriginIsRightDirection)
                warriorBehaviour.CalculateForces(true, 1 * playerStompKnockbackPower * KnockbackMultiplier, false, false);
            else
                warriorBehaviour.CalculateForces(true, -1 * playerStompKnockbackPower * KnockbackMultiplier, false, false);
        }
        else
        {
            if (OriginIsRightDirection)
                warriorBehaviour.CalculateForces(true, 1 * stompKnockbackPower * KnockbackMultiplier, false, false);
            else
                warriorBehaviour.CalculateForces(true, -1 * stompKnockbackPower * KnockbackMultiplier, false, false);
        }
        
    }

    public void Die()
    {
        dying = true;

        if (DiedEvent != null)
            DiedEvent.Invoke();

        if (DiedNotifyOtherPlayerEvent != null)
            DiedNotifyOtherPlayerEvent.Invoke();

        //Player 2 animation!
        warriorBehaviour.mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Death);

        Debug.Log("Died");
        //Debug.Break();

        //Invulnerability for no bugs...
        ActivateArmorSpellInvulnerability(4f);

        if (SettingsMenu.VibrationActive)
            StartCoroutine(VibrateController(true));

        DisableEnviromentalVFXPivotIntensity();
    }

    [Button("Omae wa mo... Shindeiru.")]
    public void DieSimple()//Wth, this is very similar to Die() why have it seperate...
    {
        if (LevelManager.currentLevel == 7)
        {
            LevelEditorDeath();
            return;
        }

        dying = true;

        if (DiedEvent != null)
            DiedEvent.Invoke();

        if (DiedNotifyOtherPlayerEvent != null)
            DiedNotifyOtherPlayerEvent.Invoke();

        //Player 2 animation!
        warriorBehaviour.mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Death);

        Debug.Log("Died");

        RemoveTempHP();
    }

    //Received that the other player died, so call this!
    //Literally copy-paste of DieSimple without notify event
    //Why copy-paste? Because fucking interface ERRORS when I put one parameter into Die() -_-
    public void DieByNetwork()
    {
        dying = true;

        if (DiedEvent != null)
            DiedEvent.Invoke();

        //Player 2 animation!
        warriorBehaviour.mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Death);

        Debug.Log("Died");

        //So as to get the dangerHP to "" instead of "0".
        commonHealthIndicationManager.Reset();

        DisableEnviromentalVFXPivotIntensity();

        //Delete the playerkiller VFX if it exists
        if (GameObject.FindGameObjectWithTag("TempHP") != null)
            GameObject.FindGameObjectWithTag("TempHP").GetComponent<TempHPBehaviour>().Die(3f);
    }

    public void LevelEditorDeath()
    {
        SetStartingHealthEffects();
        GameObject.FindGameObjectWithTag("Mage").GetComponent<SpellRewind>().LevelEditorCast();

        if (LevelEditorMenu.isPlayMode)
        {
            //Move camera to checkpoint
            LevelEditorMenu.globalInstance.TogglePlayMode();
            MasterGridManager.globalInstance.cameraTileManager.GetLevelEditorCameraTransform().position = GameObject.FindGameObjectWithTag("Checkpoint").transform.position + new Vector3(0,0, MasterGridManager.globalInstance.cameraTileManager.GetLevelEditorCameraTransform().position.z);
        }

    }

    public void StartedRevive()
    {
        //Resets Health
        currentHealth = maxHealth;

        dying = false;
    }

    /// <summary>
    /// Made for testing, so I don't have to type currenthealth and maxhealth all the fucking time!
    /// </summary>
    [Button("Infinite HP")]
    public void MaximizeHP()
    {
        currentHealth = 999;
        maxHealth = 999;
        NormalMaxHealth = 999;
    }

    public IEnumerator VibrateController(bool dead = false)
    {
        //If running on Windows. Because the below library (XInput.NET) seems to run only on windows
        #if UNITY_STANDALONE_WIN


            float vibrationPowah;

            if (dead)
            {
                vibrationPowah = 0.6f;

                //Vibrate VERY MUCH
                XInputDotNetPure.GamePad.SetVibration(0, vibrationPowah, vibrationPowah);
            }
            //if just a hit
            else
            {
                vibrationPowah = 0.4f;

                //Vibrate a lot
                XInputDotNetPure.GamePad.SetVibration(0, vibrationPowah, 0);
            }

            yield return new WaitForSeconds(0.5f);//TODO: If this(and the below) don't work, Use vibrationPowah!

            //Reset vibration.
            XInputDotNetPure.GamePad.SetVibration(0, 0, 0);
    

        #endif

        yield break;
    }

    //Somehow, fuse this and the below function.
    public void ActivateDamagedInvulnerability()
    {
        isInvulnerable = true;
        invulnerabilityTimer.ResetTime();
        invulnerabilityTimer.Configure(InvulnerabilityTimeAfterHit, true);
        invulnerabilityTimer.Activate();


        //sad copypasta
        //warriorBehaviour.mageBehaviour.CastAether();//purely VFX for now
    }

    public void ActivateArmorSpellInvulnerability(float duration)
    {
        isInvulnerable = true;
        invulnerabilityTimer.ResetTime();
        invulnerabilityTimer.Configure(duration, true);
        invulnerabilityTimer.Activate();

        //sad copypasta
        //warriorBehaviour.mageBehaviour.CastAether();
    }

    public void SetDarkwindDamagedMaterial(Material _levelMaterial)
    {
        darkwindDamaged.gameObject.GetComponent<ParticleSystemRenderer>().material = _levelMaterial;
    }

    public void StartTempHPIndication()
    {
        //Player 2 animation!//His hand to the sky~
        warriorBehaviour.mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Health);//REPLACE DIS ANIMATION!

        commonHealthIndicationManager.InitializeTempHPIndication(NormalMaxHealth);
    }


    public void StartDamagedHPIndication()
    {
        if (showHPIndication)
        {
            //Player 2 animation!//His hand to the sky~
            warriorBehaviour.mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Health);//REPLACE DIS ANIMATION!

            //Also passes the direction, so it will "fly off" in the proper direction.
            commonHealthIndicationManager.InitializeDamageIndication(currentHealth + TempHP, directionHit.normalized);
        }
    }

    //Called on level load!
    public void SetStartingHealth()
    {
        CurrentHealth = MaxHealth;
    }

    //Unity-Interfaces is such a horrible combo! ffs, can't even recycle the above with an optional parameter if i wanted.
    public void SetStartingHealthEffects()
    {
        CurrentHealth = MaxHealth;

        //VFX
        VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.TempHPGain, GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(-6.5f, 8, 0), GameObject.FindGameObjectWithTag("Player"));

        //SFX
        PlayerSoundManager.globalInstance.PlayTempHPGainSFX();
    }

    //By DarkwindDistortionMenu (each arrow is +1)
    public void AddMaxHealth(int healthValueToAdd)
    {
        //If health about to reach 0
        if (healthValueToAdd < 0 && MaxHealth < 2)
            return;

        MaxHealth = MaxHealth + healthValueToAdd;

        SetStartingHealthEffects();

        if ((MaxHealth > 3 && LevelManager.currentLevel != 7) || (MaxHealth > 2 && LevelManager.currentLevel == 7))
            commonHealthIndicationManager.dangerHPIndication.text = "";
        else
            commonHealthIndicationManager.dangerHPIndication.text = romanStringNumerals.romanStringNumeralGenerator(MaxHealth);
    }

    //By LevelEditor Health
    public void SetMaxHealth(int targetHealthValue)
    {
        //1 hp is minimum
        if (targetHealthValue < 1)
            return;

        MaxHealth = targetHealthValue;

        SetStartingHealthEffects();

        if ((MaxHealth > 3 && LevelManager.currentLevel != 7) || (MaxHealth > 2 && LevelManager.currentLevel == 7))
            commonHealthIndicationManager.dangerHPIndication.text = "";
        else
            commonHealthIndicationManager.dangerHPIndication.text = romanStringNumerals.romanStringNumeralGenerator(MaxHealth);
    }

    public bool GetInvulnerability()
    {
        return isInvulnerable;
    }

    public int GetTotalCurrentHP()
    {
        return CurrentHealth + TempHP;
    }

    /*
    private void Update()
    {
        Debug.Log("TEmpHP: " + TempHP);
    }
    */
}
