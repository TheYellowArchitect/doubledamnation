using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

//TODO: Rewind on Death if max mana, mostly done.
public class SpellRewind : Spell
{
    [Header ("Unique Spell Values")]//This header should be a standard for every spell!

    [Header ("Prefabs to spawn per level")]
    [Header ("Earth")]
        public GameObject staticLinesPrefabVFXEarth;
        public GameObject mageFlameTeleportIndicationVFXPrefabEarth;
        public GameObject mageFlameTeleportDoneVFXPrefabEarth;

    [Header ("Fire")]
    public GameObject staticLinesPrefabVFXFire;
    public GameObject mageFlameTeleportIndicationVFXPrefabFire;
    public GameObject mageFlameTeleportDoneVFXPrefabFire;

    [Header ("Wind")]
    public GameObject staticLinesPrefabVFXWind;
    public GameObject mageFlameTeleportIndicationVFXPrefabWind;
    public GameObject mageFlameTeleportDoneVFXPrefabWind;



    [Header ("Sound Effects/Clips")]
    public AudioClip bellSFX;
    public AudioClip mageQuoteSFX;

    [Header ("Values")]
    public Vector3 spawnFireVFXOffsetFromParticleLocation;

    public Vector3 spawnGatheringFireVFXOffsetFromParticleLocation;

    //[Tooltip("How many seconds back in time do you go with this?")]
    //public int rewindTime = 4;

    private MageBehaviour mageBehaviour;
    private WarriorMovement warriorScript;

    private PlayerManaManager playerManaManager;

    private ParticleSystem mainParticleSystem;

    private Vector3 targetParticlePosition;

    private GameObject createdGameObject;
    private GameObject staticLines;
    private ParticleSystem staticLinesParticleSystem;

    public Sprite spriteWhenRewinded;

    [Header("Voiceclips")]
    //[MinValue(0), MaxValue(100)]
    //public byte chancesToPlayVoiceclip = 0;

    public List<AudioClip> rewindClips = new List<AudioClip>();

    // Use this for initialization
    void Start ()
    {
        //Cache the mage
        mageBehaviour = GetComponent<MageBehaviour>();

        //Cache the player so you wont invoke him so often
        warriorScript = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

        //Cache the playerManaManager to change (max) mana.
        playerManaManager = GameObject.Find("ManaManager").GetComponent<PlayerManaManager>();//lazy. Coulda do it better.
    }

    public void LevelEditorCast()
    {
        StartCoroutine(RewindCoroutine(false, true));
    }

    public void DeathCast()
    {
        StartCoroutine(RewindCoroutine(true, false));
    }

    public override void Cast()
    {
        //Gotta use this to pause.
        StartCoroutine(RewindCoroutine(false, false));
    }

    public IEnumerator RewindCoroutine(bool playDeathVoiceline = false, bool levelEditorDeath = false)
    {

            //Use up the mana
            playerManaManager.AddPlayerMana(-1 * manaCost);

            //SFX
                //Play Bell SFX
                if (bellSFX != null)
                    PlayerSoundManager.globalInstance.PlayClip(bellSFX, PlayerSoundManager.AudioSourceName.SpellCast1);

                //Play MageQuote En Parel8on SFX
                if (mageQuoteSFX)
                    PlayerSoundManager.globalInstance.PlayClip(bellSFX, PlayerSoundManager.AudioSourceName.Player2Voice);

            //Player 2 animation :(
            mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Rewind);

            //Stop emission, move then restart ;)
            warriorScript.currentDarkwindTrail.Stop();

            if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
            {
                Time.timeScale = 0.1f;
            }
            else
            {
                //If offline/local -> Pause everything
                if (NetworkCommunicationController.globalInstance == null)
                    PauseMenu.globalInstance.Pause(false, false);
                else
                {
                    Time.timeScale = 0.1f;
                    //if (NetworkDamageShare.globalInstance.IsSynchronized() == false)
                        //NetworkDamageShare.globalInstance.DesynchronizeFully();
                }

            }

            if (playDeathVoiceline)
            {
                //byte currentChances = (byte) UnityEngine.Random.Range(0, 100);
                //Debug.Log("Chances to play:rng " + chancesToPlayVoiceclip + ":" + currentChances);
                //if (chancesToPlayVoiceclip > currentChances)
                if (true)
                {
                    Debug.Log("Mage or warrior " + VoiceSoundManager.globalInstance.IsPlayingMage() + " or " + VoiceSoundManager.globalInstance.IsPlayingWarrior());
                    if (VoiceSoundManager.globalInstance.IsPlayingMage() == false && VoiceSoundManager.globalInstance.IsPlayingWarrior() == false)
                    {
                        byte clipIndexToPlay = (byte) UnityEngine.Random.Range(0, rewindClips.Count - 1);
                        Debug.Log("ClipIndexToPlay: " + clipIndexToPlay);
                        if (clipIndexToPlay > -1 && clipIndexToPlay < rewindClips.Count)//Safety for inclusive range idk
                            VoiceSoundManager.globalInstance.PlayVoice(255f, rewindClips[clipIndexToPlay]);
                    }
                        
                }
            }
            

            

            //Get the sprite on rewind and set it on sprite renderer
            spriteWhenRewinded = warriorScript.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;


            if (levelEditorDeath == false)
            {
                //========================================================
                //The core of this class, really.
                //Is a coroutine because it rests for 2 frames for 0 lags.
                StartCoroutine(SetTargetParticlePositionPosition());

                //Hackspagghetti. By the time the below happens, above coroutine will have finished -_-
                //Imagine debugging these spagghetti race conditions lmao!
                //tbh, if you remove the coroutine bs, i doubt the lag spike will be noticeable
                //but keep it as it is, to make sure its smooth.
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                //========================================================
            }
            else
                targetParticlePosition = GameObject.FindGameObjectWithTag("Checkpoint").transform.position;
            

            //Spawn VFX
                //FlameTeleportIndication VFX
                    //Level Difference
                    if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                        createdGameObject = Instantiate(mageFlameTeleportIndicationVFXPrefabEarth);
                    else if (LevelManager.currentLevel == 2)
                        createdGameObject = Instantiate(mageFlameTeleportIndicationVFXPrefabFire);
                    else// if (LevelManager.currentLevel > 2)
                        createdGameObject = Instantiate(mageFlameTeleportIndicationVFXPrefabWind);

                createdGameObject.transform.position = targetParticlePosition - spawnGatheringFireVFXOffsetFromParticleLocation;
                createdGameObject.GetComponent<DeathTimer>().DieInSeconds(createdGameObject.GetComponent<ParticleSystem>().duration);


                //Static Lines VFX
                    //Level Difference
                    if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                        staticLines = Instantiate(staticLinesPrefabVFXEarth);
                    else if (LevelManager.currentLevel == 2)
                        staticLines = Instantiate(staticLinesPrefabVFXFire);
                    else// if (LevelManager.currentLevel > 2)
                        staticLines = Instantiate(staticLinesPrefabVFXWind);

                staticLines.transform.position = warriorScript.transform.position;
                staticLinesParticleSystem = staticLines.GetComponent<ParticleSystem>();
                staticLines.GetComponent<DeathTimer>().DieInSeconds(staticLinesParticleSystem.duration);
                if (LevelManager.currentLevel == 7)
                {
                    //if (LevelEditorMenu.isPlayMode)
                        //staticLines.transform.SetParent(MasterGridManager.globalInstance.cameraTileManager.GetWarriorCameraTransform());
                    //else
                        staticLines.transform.SetParent(MasterGridManager.globalInstance.cameraTileManager.GetLevelEditorCameraTransform());
                }
                else
                    staticLines.transform.SetParent(GameObject.FindGameObjectWithTag("CameraHolder").transform);
                

                //Hacky af, to get the particle system to spawn and play, then pause it instantly, and tweak ofc.
                yield return new WaitForEndOfFrame();
                staticLinesParticleSystem.playbackSpeed = 0;
                ParticleSystem.EmissionModule staticLinesEmissionModule = staticLinesParticleSystem.emission;
                staticLinesEmissionModule.rateOverTime = 0f;

        //Display the same sprite as when rewind was just used
        warriorScript.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = spriteWhenRewinded;

        //Wait 1 second (as long as the bell to 2nd bell)
            yield return new WaitForSecondsRealtime(1.1f);

            //If offline/local -> Un-Pause everything
            if (NetworkCommunicationController.globalInstance == null)
                PauseMenu.globalInstance.Resume();
            else
                Time.timeScale = 1;

            //Play trail again
            warriorScript.currentDarkwindTrail.Play();

            //Play/Finish the screen lines.
            staticLinesParticleSystem.playbackSpeed = 1;

            //Move warrior to proper location
            warriorScript.transform.position = new Vector3(targetParticlePosition.x, targetParticlePosition.y, 0);

            //VFX Speed modifier to 5 so it does weird effect.
            //ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = mainParticleSystem.velocityOverLifetime;
            //velocityOverLifetimeModule.speedModifier = 5f;

        //FlameTeleportExplosion VFX
            //Level Difference
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                createdGameObject = Instantiate(mageFlameTeleportDoneVFXPrefabEarth);
            else if (LevelManager.currentLevel == 2)
                createdGameObject = Instantiate(mageFlameTeleportDoneVFXPrefabFire);
            else// if (LevelManager.currentLevel > 2)
                createdGameObject = Instantiate(mageFlameTeleportDoneVFXPrefabWind);

            createdGameObject.transform.position = targetParticlePosition - spawnFireVFXOffsetFromParticleLocation;

        //Insta-snap camera, cuz if it takes a while to return, it will be such a mess for fast movement.
        if (LevelManager.currentLevel != 7)
            GameObject.FindGameObjectWithTag("CameraHolder").transform.position = new Vector3(warriorScript.transform.position.x , warriorScript.transform.position.y, -1);//Gotta be -1 on Z!
            //Could stretch the camera if camera has no zoom queues, but too complicated for such a small thing. This just works.

        //Reset Dodgeroll
        warriorScript.OnKillResetDodgeroll();

        //Reset Jumps
        warriorScript.ResetJumps();

        //Now you wonder why would I reset the above instead of storing them and call me lazy lul
        //The thing is, its simply more fun. Also don't forget the cost is 4 mana!!!

        
        if (levelEditorDeath)
        {
            if (LevelManager.currentLevel != 7)
                yield break;

            //Notify BoundsKiller
            GameObject.FindGameObjectWithTag("LevelEditorManager").GetComponent<BoundsKiller>().PlayerRegisteredAlive();

            //Move camera to checkpoint
            MasterGridManager.globalInstance.cameraTileManager.GetLevelEditorCameraTransform().position = targetParticlePosition + new Vector3(0,0, MasterGridManager.globalInstance.cameraTileManager.GetLevelEditorCameraTransform().position.z);
        }
            

        //TODO:
        //For level 3 trail, recolor towards the curve with a white gradient ;)
    }



    /// <summary>
    /// Teleport player on the thicc part of the trail, see particle curve.
    /// </summary>
    public IEnumerator SetTargetParticlePositionPosition()
    {
        //Teleport to trail!
        mainParticleSystem = warriorScript.currentDarkwindTrail;
        ParticleSystem.Particle [] darkwindParticles = new ParticleSystem.Particle[mainParticleSystem.main.maxParticles];
        int numberOfParticlesAlive = mainParticleSystem.GetParticles(darkwindParticles);

        Debug.Log("Particles Alive: " + numberOfParticlesAlive);

        //To get our particle:
        //1. We sort by remaining lifetime (0 to higher numbers, so the curve is flipped)
        //2. Then find highest size
        //3. Use the index of highest size, and give the same position to warrior's position
        //Of course, this is specific for this curve. See curve and you will understand how this works.

        //Sort by remainingLifetime. Hence, [0] is at the end of our curve ;)
        //LINQ comes to save the day, because I would use braindead sorting, which more or less checkswaps one by one, then starts from start
        //Which would KILL performance. Anyway, I copypasted the line below, I get the part to the right, but not the part to the left lul.
        //IOrderedEnumerable<ParticleSystem.Particle> noideawtfthisis = particles.OrderBy(sortedParticle => sortedParticle.remainingLifetime).ToArray<ParticleSystem.Particle>();
        darkwindParticles = darkwindParticles.OrderBy(sortedParticle => sortedParticle.remainingLifetime).ToArray<ParticleSystem.Particle>();


        //Wait before iterating so less of a frame drop happens!
        yield return new WaitForEndOfFrame();

        //Get index of where the curve has the biggest size, starting from the end of the curve towards the start.
        //If not found, then give the oldest point of the trail.
        int targetParticleIndex = GetBiggestSizeParticleIndex(ref darkwindParticles, ref mainParticleSystem, mainParticleSystem.main.maxParticles);


        //Place/Position player on the particle's position.
        if (targetParticleIndex != -1)//-1 means no trail at all, and so, you would teleport to (0,0,0) lmao
            targetParticlePosition = darkwindParticles[targetParticleIndex].position;
        else
        {
            //Fill back the mana, as the rewind was minimal.
            playerManaManager.AddPlayerMana(1 * manaCost);

            targetParticlePosition = warriorScript.transform.position;
        }

        //warriorScript.transform.position = darkwindParticles[targetParticleIndex].position;
    }

    public int GetBiggestSizeParticleIndex(ref ParticleSystem.Particle[] particles, ref ParticleSystem mainParticleSystem, int maxParticles)
    {
        #region DisplayParticles
        /* This loop existed purely to find all particles and show their data like size/lifetime.
        for (int i = 0; i < maxParticles; i++)
        {
            if (particles[i].remainingLifetime < 0.01f)
                continue;

            //Could just store the GetSize() seperately instead of getting the entire mainparticlesystem
            Debug.Log("RemainingLifetime[" + i + "] lifetime: " + particles[i].remainingLifetime + "\nSize: " + particles[i].GetCurrentSize(mainParticleSystem));
        }

        Debug.Log("==New Loop==");
        */
        #endregion

        for (int i = 0; i < maxParticles; i++)
        {
            if (particles[i].remainingLifetime < 0.01f)
                continue;

            //Detect the top of the curve, so exit.
            if (i + 1 < maxParticles && particles[i].GetCurrentSize(mainParticleSystem) > particles[i + 1].GetCurrentSize(mainParticleSystem))
                return i;
        }

        //Exited without finding peak curve. Which means, the trail is F R E S H (I meant very new by this.)
        //Hence, now, we gotta find the latest particle of the trail, aka the oldest trail, aka the one with remainingLifetime closest to 0.
        //But, the array is already sorted for that! xD
        //Below forloop returns the oldest particle detected ;)

        for (int i = 0; i < maxParticles; i++)
        {
            if (particles[i].remainingLifetime < 0.01f)
                continue;

            return i;
        }

        //If code ever reaches this return, there is a problem.
        return -1;
    }

    //Sad note...
    //To get this spell on the peak polish and without flaws... you need to utilize another camera...
    //Because when you press rewind, and are fast, you do not know where you go.
    //TeleportVFX is very big, but even then, its not visible.
    //The camera can also not stretch that far, since if it surpasses the bound limit, it will show some... ugliness easily covered.
    //The solution is to render an entire camera over that point, probably with split-screen (diagonally) for badassness.
    //Either put that camera inside an orb or whatever near the player so he can see (minimized though) or straight up split his screen.
    //Or even fade-in his location with a shader starting from the point you teleport on!
    //All of them, as you can see, not only require lots of effort and Q&A (boring af) but also kill performance.
    //At least, I can type these words here, for what will never be.

    #region Rewind TIME
        /* This works for pure time-related rewind. But we don't want that.
        //Get the data to get the position he was rewindTime ago
        //slicedData = playerStatsManager.GetPlayerPositionTrackingData(rewindTime);//5 seconds back

        //If position doesnt exist (aka used at start of level) then get him to start of level.
        //if (slicedData.deltaTimeFromLastRegister != 48f)//48 in this case is the return -1; in classic programming
            //slicedPosition = slicedData.position;
        //else
            //slicedPosition = new Vector3(0, 0, 0);//Start of every level!

        //Set position to the position he was rewindTime ago!
        //warriorScript.transform.position = slicedPosition;
        */
    #endregion

}
