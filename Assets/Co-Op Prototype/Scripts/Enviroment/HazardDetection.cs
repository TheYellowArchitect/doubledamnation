using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class HazardDetection : MonoBehaviour
{
    //Variables
    [Range(0, 4)]
    [Tooltip("Damage from 0~4")]
    public int damagePower;
    private int appliedDamagePower;//In case special conditions make it 0, for example level editor pause

    [MinValue(0f)]
    [Tooltip("After player is hit, for how long he cannot act, aka for how long is he inside the Hit Animation?\nOver 1 second is retarded, don't do it.")]
    public float hitstun;

    [Tooltip("How far it knockbacks back the damaged target upon hit, on the X axis")]
    public int knockbackPowerX;

    [Tooltip("How far it knockbacks back the damaged target upon hit, on the Y axis")]
    public int knockbackPowerY;

    [Tooltip("For now, to make the lava sizzle SFX")]
    public bool isLava = false;

    [ShowIf("isLava")]
    [Tooltip("The SFX that plays when player contacts lava, that sounds like a fuse/roasted ala mario64")]
    public AudioClip lavaSizzleSFX;
    

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("What layer player is.")]
    public LayerMask WhatIsPlayer;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("What layer enemy is.")]
    public LayerMask WhatIsEnemy;

    protected IDamageable damageableObject;
    protected GameObject damagedObject;

    private bool knockbackVertically = true;
    private bool hitPlayer = false;
    private Vector2 contactPoint;

    //[Tooltip("After hit once, when does it re-check if it still collides?")]
    private float afterCollisionDelay = -1;

    private void OnCollisionEnter2D(Collision2D other)
    {
        //If hit wall horizontally
        if (((WhatIsPlayer | WhatIsEnemy) & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
        {
            contactPoint = other.contacts[0].normal;
            Debug.Log("Knockback point on Hazard is: " + contactPoint);

            if (contactPoint.y != 0)
                knockbackVertically = true;
            else
                knockbackVertically = false;

            //Checks if damageable inside, no worries.
            DetermineDamage(other.gameObject);

            //After it ends, with a time delay it checks if there is still someone in the collider. And damages if need be.
            StartCoroutine(DelayedCollisionCheck());
        }
    }

    //Returns true if it damaged, false if not
    public bool DetermineDamage(GameObject _damagedObject)
    {
        damagedObject = _damagedObject;
        //damageableObject is IDamageable. GetComponent should work.
        damageableObject = damagedObject.GetComponent<IDamageable>();//as Idamageable rip

        if (damageableObject != null)
        {
            if (damagedObject.CompareTag("Player"))
                hitPlayer = true;
            else
                hitPlayer = false;

            Damage(damagedObject);

            return true;
        }

        return false;
    }

    //Called from delayed as well, and made sure that the parameter isnt null.
    public void Damage(GameObject damagedObject)
    {
        //Play SFX
        if (isLava)
            PlayerSoundManager.globalInstance.PlayClip(lavaSizzleSFX, PlayerSoundManager.AudioSourceName.LavaSizzle);
        //else spike sound

        if (GameManager.testing && damagedObject.CompareTag("Player"))
            Debug.Log("Player's position is: " + GameObject.FindGameObjectWithTag("Player").transform.position + " and hazard's is: " + transform.position);

        if (hitPlayer)
        {
            //If level editor + pause mode
            if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
                appliedDamagePower = 0;
            else
                appliedDamagePower = damagePower;


            if (knockbackVertically)                
                damageableObject.TakeDamage(appliedDamagePower, transform.position, 0, knockbackPowerY, hitstun, false, true);
            else
            {
                //Detects if dodgeroll/wavedash, so as the knockback increases to make up for the boostPowah, cuz if no check, there is no knocback cuz it nullifies with the dash's.
                if (damagedObject.GetComponent<WarriorMovement>().GetCurrentState() == 6 || damagedObject.GetComponent<WarriorMovement>().GetCurrentState() == 7)
                    damageableObject.TakeDamage(appliedDamagePower, transform.position, knockbackPowerX + 20, 0, hitstun, false, true);
                else
                
                    damageableObject.TakeDamage(appliedDamagePower, transform.position, knockbackPowerX, 0, hitstun, false, true);
                    //damagedObject.GetComponent<WarriorMovement>().ResetForcesX();
            }

        }
        else //4-directional spagghetti
        {
            
            //If level editor + pause mode
            if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
                appliedDamagePower = 0;
            else
                appliedDamagePower = damagePower;

            //Insta-kill
            if (isLava)
            {
                damagedObject.GetComponent<EnemyBehaviour>().LavaHit(0.05f);

                damageableObject = null;
            }
            else if (knockbackVertically)
            {
                if (contactPoint.y > 0)
                    damageableObject.TakeDamage(appliedDamagePower, damagedObject.transform.position + Vector3.up, 0, knockbackPowerY, hitstun, false, true);
                else
                    damageableObject.TakeDamage(appliedDamagePower, damagedObject.transform.position + Vector3.down, 0, knockbackPowerY, hitstun, false, true);
            }
            else
            {
                if (contactPoint.x > 0)
                    damageableObject.TakeDamage(appliedDamagePower, damagedObject.transform.position + Vector3.right, knockbackPowerX, 0, hitstun, false, true);
                else
                    damageableObject.TakeDamage(appliedDamagePower, damagedObject.transform.position + Vector3.down, knockbackPowerX, 0, hitstun, false, true);
            }
                
        }
        
    }

    IEnumerator DelayedCollisionCheck()
    {
        //Not player, do not delay/damage.
        if (damageableObject == null)
        {
            damageableObject = null;
            yield break;
        }

        if (afterCollisionDelay == -1)
            afterCollisionDelay = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().InvulnerabilityTimeAfterHit + 0.2f;
            //afterCollisionDelay = damageableObject.GetComponent<WarriorHealth>().InvulnerabilityTimeAfterHit + 0.2f;


        yield return new WaitForSeconds(afterCollisionDelay/4f);

        //If there is still someone after the delay is done.
        if (damageableObject != null)
        {
            Damage(damagedObject);
            StartCoroutine(DelayedCollisionCheck());
        }
    }

    //inb4 bugs when 2+ camp in a bugged hazard, but srsly, how rare is that.
    private void OnCollisionExit2D(Collision2D collision)
    {
        //It removes the cached IDamageable.
        //if (damageableObject == collision.gameObject.GetComponent<IDamageable>())
            damageableObject = null;
    }

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}
