using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Attach a collider here, and it will damage as long as alive.
/// In other words, a moving hazard.
/// </summary>
public class SpikeBehaviour : MonoBehaviour
{
    [Header("Damage Stats")]
    public int damagePower;
    public int knockbackPowerX;
    public int knockbackPowerY;
    public float hitstunDuration;

    [Header("Misc")]
    public Collider2D damageCollider;
    public LayerMask WhatIsPlayer;//Is this really needed tho.
    public bool interruptChargingPlayer = true;
    [ReadOnly]
    public bool playerInside = false;

    [ReadOnly]
    public bool playerBoxInside = false;
    [ReadOnly]
    public bool playerCircleInside = false;

    //Cachebois
    private IDamageable tempDamageable;
    private EnemyBehaviour commonBehaviour;

    private Collider2D[] playerColliders;
    private ContactFilter2D playerFilter = new ContactFilter2D();

    private void Start()
    {
        commonBehaviour = GetComponentInParent<EnemyBehaviour>();
        playerColliders = new Collider2D[2];
        playerFilter.SetLayerMask(WhatIsPlayer);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        DeterminePlayerStatus();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        tempDamageable = collision.gameObject.GetComponent<IDamageable>();
        if (tempDamageable == null)//Quick bugfix for now, spikeboi's spike attacks, interact with platforms.
            return;

        playerInside = true;
        //Debug.Break();
        //Debug.Log("The knockback is?" + knockbackPowerX);
        Debug.DrawLine(damageCollider.bounds.center, collision.transform.position, Color.magenta, 0.5f);
        Debug.DrawLine(damageCollider.bounds.center, damageCollider.bounds.center + Vector3.down, Color.black, 0.5f);

        //Don't interrupt... It plays jankily, aka against the player cuz most players suck. It's also really hard to master and not worth punishing an already hard game. This reaches the "unfair" levels from most players, hence this part ftw.
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().GetChargingAttack() == true)//stupid code implementation but anyway.
            return;


        //If the player can be damaged
        if (WarriorHealth.isInvulnerable == false && WarriorHealth.dying == false)
            Damage();

        //After it ends, with a time delay it checks if player is still inside collider.
        StartCoroutine(DelayedCollisionCheck());

    }

    IEnumerator DelayedCollisionCheck()//perhaps use this on HazardDetection? lmao.
    {
        do
        {
            yield return new WaitForSeconds(1);

            if (WarriorHealth.isInvulnerable == false && WarriorHealth.dying == false)
                Damage();

        }
        while (playerInside);

        yield break;
    }

    public void Damage()
    {
        //so it won't bug
        if (playerInside && tempDamageable != null)
        {
            //Check if player dies from this
            if (damagePower >= GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().GetTotalCurrentHP())
            {
                Debug.Log("damagePower | TotalHP" + damagePower + " | " + GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>().GetTotalCurrentHP());

                commonBehaviour.AddPlayerKillerHP(2);
            }

            tempDamageable.TakeDamage(damagePower, damageCollider.bounds.center, knockbackPowerX, knockbackPowerY, hitstunDuration, false, true);
        }
            
    }

    public void DeterminePlayerStatus()
    {
        if (damageCollider.OverlapCollider(playerFilter, playerColliders) > 0)
            playerInside = true;
        else
            playerInside = false;
    }

}
