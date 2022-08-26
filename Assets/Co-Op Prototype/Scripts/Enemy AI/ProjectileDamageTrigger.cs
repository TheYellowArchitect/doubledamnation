using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ProjectileDamageTrigger : MonoBehaviour
{
    //[Tooltip("Instantiates an object")]
    //public GameObject deathSFXObject;
    [Tooltip("The SFX when it dies, aka interacts with target or ground if interactive\nCurrently used only for Target.")]
    public AudioClip deathSFX;

    //the rigidbody of this prefab/projectile
    private Rigidbody2D commonRigidbody;

    
    [Header("Projectile Values")]
    public short damagePower;
    public short knockbackPowerX;
    public short knockbackPowerY;
    public float hitstun;
    public float speed;//Made public so "push" reverses it.
    public float deathTimer;
    public LayerMask WhatIsTarget;
    public LayerMask WhatIsGround;
    public bool interactWithGround;


    //These are given from the EnemyRangedAttack
    private Vector3 directionThrown;
    private EnemyBehaviour damagingMonsterBehaviour;
    private MageBehaviour damagingPlayerBehaviour;

    private bool dying = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.testing)
        {
            Debug.Log("PROJECTILE | Name is: " + gameObject.name);
            Debug.Log("PROJECTILE | Collision happened with: " + collision.gameObject.name);
        }

        //Don't ask, layermasks are weird.
        if ( (WhatIsTarget & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
        //if (((1 << collision.gameObject.layer) & WhatIsTarget) != 0)//doesnt work? lel, internet help failed me
        //if (collision.gameObject.layer == collision.gameObject.layer << WhatIsTarget)//doesnt work
        {
            if (GameManager.testing)
                Debug.Log("PROJECTILE " + gameObject.name + " hit " + collision.gameObject.name);

            //If it kills player
            if (collision.gameObject.CompareTag("Player") && damagePower >= collision.gameObject.GetComponent<WarriorHealth>().GetTotalCurrentHP() && WarriorHealth.isInvulnerable == false && WarriorHealth.dying == false)
                if (damagingMonsterBehaviour != null)
                    damagingMonsterBehaviour.AddPlayerKillerHP(2);

            if (collision.gameObject.CompareTag("EnemyProjectile") == false)
            {
                //Hacky but whatever. This class is already a mess. With this, it passes through the enemy and doesnt die :)
                if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
                    return;

                //No idea when this is true, but its an edge-case which happened in a desync netcoding playtest!
                //Also proof I must erase the IDamageable autism
                if (collision.gameObject.GetComponent<IDamageable>() != null)
                    //Damage target
                    collision.gameObject.GetComponent<IDamageable>().TakeDamage(damagePower, transform.position, knockbackPowerX, knockbackPowerY, hitstun);

                //Play SFX
                if (deathSFX != null)
                    PlayerSoundManager.globalInstance.PlayClip(deathSFX, PlayerSoundManager.AudioSourceName.EnemyHit, 1.3f, 0.6f);

                //It killed an enemy
                    //So fucking stupid. Should have KillReward() events that are registered from their parents, and just checks idamageable hp.
                    //and it should be simply invoked here, instead of hardcoded mess all in one class.
                    //Not to mention each projectile should have its stats independent of parent's functions, all in its prefab instead of hardcoded. Fireball is a shitshow.
                if (collision.gameObject.CompareTag("Enemy") && collision.gameObject.GetComponent<EnemyBehaviour>().CurrentHealth == 0)
                    damagingPlayerBehaviour.FireballKill(collision.gameObject.GetComponent<EnemyBehaviour>().giveManaOnDeath);
            }
            else//collision is projectile as well.
                collision.gameObject.GetComponent<ProjectileDamageTrigger>().Death();

            Death();
        }
        else if (interactWithGround && ((WhatIsGround & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
        {
            if (GameManager.testing)
                Debug.Log("PROJECTILE | Hit Ground");

            //If pillar and projectile is from player (fireball or pushed projectile)
            //Destroy the pillar!
            if (collision.gameObject.CompareTag("Pillar") && damagingPlayerBehaviour != null)
            {
                if (collision.gameObject.transform.parent.GetComponent<FinalPillarBehaviour>() != null)//If middle or top pillar
                    collision.gameObject.transform.parent.GetComponent<FinalPillarBehaviour>().StartSpellDeathCoroutine();
                else//if the small parts of top pillar -_-
                    collision.gameObject.transform.parent.parent.GetComponent<FinalPillarBehaviour>().StartSpellDeathCoroutine();
            }


            //TODO: Dummy VFX to "be stuck/decal'd" on the ground/wall//yeah, if i had like a month to polish, lmao.

            //By having a "public bool hazardProjectile", you could make them get stuck on ground, Invoke("Death") to detect it so as not to die, and damage players on contact. But it takes time fml! (Cyclops level 3 would be ideal for this)

            Death();


        }
#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
        else
            Debug.Log("PROJECTILE | Layer value of undefined object is: " + collision.gameObject.layer);
#endif
    }

    //By EnemyRangedAttack, upon Instantiate ;)
    //Single-responsibility pattern here, means that ranged attack is responsible for everything here, this is just a hollow shell to be filled with data.
    public void InitializeValues(Vector2 directionToTarget, Vector2 spawnPosition, short _DamagingMonsterIndex = -1)
    {
        directionThrown = directionToTarget;

        transform.position = spawnPosition;

        //Given so damage can be callbacked to original monster, and for example, claim the kill properly
        if (_DamagingMonsterIndex == -2)
            damagingPlayerBehaviour = GameObject.FindObjectOfType<MageBehaviour>();
        else if(_DamagingMonsterIndex != -1)
            damagingMonsterBehaviour = LevelManager.globalInstance.GetEnemyBehaviourFromIndex((ushort)_DamagingMonsterIndex);

        //Play animation, if any.
        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().Play(0);
    }

    public void Deflect(LayerMask whatIsDamageable)
    {
        //if (GameManager.testing)
            //Debug.Log("Deflected!");

        speed *= -1;
        WhatIsTarget = whatIsDamageable;
    }

    // Use this for initialization
    void Start ()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();

        //Timer to kill this
        Invoke("Death", deathTimer);
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (commonRigidbody != null)
            commonRigidbody.velocity = directionThrown * speed;
    }

    //Destroys gameobject with everything inside, aka this script as well. But first, it plays dat SFX :P
    public void Death()
    {
        //So it won't come here by Invoke("Death") + dying naturally, playing for example the sound 2 times lul.
        if (dying)
            return;
        else
            dying = true;

        StartCoroutine(DelayedDeath(2));
    }

    IEnumerator DelayedDeath(float secondsToDelay)
    {
        //So it won't move anymore.
        speed = 0;

        //Show nothing
        Destroy(GetComponent<SpriteRenderer>());

        //Destroy BoxCollider and Rigidbody so it won't damage player (and won't tax CPU)
        Destroy(GetComponent<BoxCollider2D>());
        Destroy(commonRigidbody);

        //The children have a VFX chain
        if (transform.childCount > 0)
            //Stop emitting more particles
            transform.GetChild(0).GetComponent<StopParticleEmission>().Initialize();


        yield return new WaitForSeconds(secondsToDelay);

        Destroy(gameObject);
    }

}
