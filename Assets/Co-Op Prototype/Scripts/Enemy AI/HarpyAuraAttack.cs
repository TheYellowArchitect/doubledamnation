using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

//Keep in mind, this is not optimized for A.I. activation/deactivation. As a result, it will drain a lot of CPU.
public class HarpyAuraAttack : ManaManager //When you accidentally find something you actually designed good, so u utilize it. OOP? In final stages of project? what is this!
{
    [Header("Ranged Attack Properties")]

    [Tooltip("The rotating orbs that become projectiles, how many should this monster start with?")]
    [Slider(0,4)]
    public int amountOfMana = 2;

    [Required]
    [Tooltip("The projectile to \"spawn\" upon attack.")]
    public GameObject prefabProjectile;


    [Header("Misc")]
    [Tooltip("How much of a delay should be within a shot?")]
    public float timeBetweenShots;

    float timeElapsed = 0f;
    [SerializeField]
    bool canShoot = true;
    [ReadOnly]
    public bool playerInRange = false;

    [ReadOnly]
    public Transform playerTransform;

    private EnemyBehaviour harpyBehaviour;

    protected override void Start()
    {
        harpyBehaviour = transform.parent.gameObject.GetComponent<EnemyBehaviour>();

        currentChildIndex = 0;

        //Register the children
        foreach (Transform pickedChildTransform in transform)
        {
            //The only different line from base parent's Start().
            pickedChildTransform.gameObject.GetComponent<CircularMotionBehaviour>().playerOwned = false;

            pickedChildTransform.gameObject.GetComponent<CircularMotionBehaviour>().SetOwnerTransform(transform.parent);

            children.Add(pickedChildTransform.gameObject);

            pickedChildTransform.gameObject.SetActive(false);
        }

        ResetChildren();

        //Since Harpy. And time is low...
        SetStartingMana(amountOfMana);
    }

    void Update ()
    {
        //If online and client and synchronized
        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() == false && NetworkDamageShare.globalInstance.IsSynchronized())
            return;

        //Timer to get the "cooldown" for each shot.
        if (canShoot == false)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > timeBetweenShots)
            {
                timeElapsed = 0f;
                canShoot = true;
            }
        }
        //FIRE!
        else if (playerInRange && canShoot)
        {
            //If there is "ammo"/available floating orbs, also necessary because current is current-1, and if 0-1 -> [-1], it is out of index.
            if (currentMana == 0)
                return;

            ShootProjectile((playerTransform.position - children[currentMana - 1].transform.position).normalized, children[currentMana - 1].transform.position);
        }
            
        
	}

    public void ShootProjectile(Vector2 directionToPlayer, Vector2 spawnPosition)
    {
        //If there is "ammo"/available floating orbs, also necessary because current is current-1, and if 0-1 -> [-1], it is out of index.
        if (currentMana == 0)
            return;//Re-checks it here because this is triggered by client, without the check before this scope.

        //Play fire ring VFX//TODO: Change transform.position to fit? or change the VFX.
        manaRing.Play();
        //VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.FireRing, transform.position);

        canShoot = false;

        //Create projectile
        GameObject tempProjectile = Instantiate(prefabProjectile);
        tempProjectile.GetComponent<ProjectileDamageTrigger>().InitializeValues(directionToPlayer, spawnPosition, (short) harpyBehaviour.enemyListIndex);

        float angle = Mathf.Atan2(harpyBehaviour.GetDirectionToPlayer().y, harpyBehaviour.GetDirectionToPlayer().x) * Mathf.Rad2Deg;
        angle -= children[currentMana - 1].transform.rotation.z - 80;//80 is the default to face "normally"
        tempProjectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        /*
        //If client, and hence hasn't gotten in shooting range yet, but host has triggered this function to client
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        Debug.DrawRay(children[currentMana - 1].transform.position, playerTransform.position - children[currentMana - 1].transform.position, Color.cyan, 1f);
        //I suspect on the above, the Ring could be improved (swap orbs on fire) but itjustworks. This is such a small detail. And why does the ManaRing exist after all? ;)  ((spoilers: also for my shitty trigonometry coding))
        */

        //Disable one of the rings
        base.RemoveMana();

        //If online and host and synchronized, send the harpy ranged attack to client
        if (NetworkCommunicationController.globalInstance != null && NetworkCommunicationController.globalInstance.IsServer() && NetworkDamageShare.globalInstance.IsSynchronized())
            NetworkCommunicationController.globalInstance.SendHarpyRangedAttack(directionToPlayer, spawnPosition, harpyBehaviour.enemyListIndex);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //idk if im retarded or sleepy cuz crunching af (perhaps both? or neither?) but the collision on this is PlayerExclusively. so, wtf?!
        //Anyway, fix this because this if check shouldn't even happen, and it is checked against a LOT in-game.
        if (collision.CompareTag("Player") == false)
            return;

        playerInRange = true;

        playerTransform = collision.transform;

        Debug.Log("Player transform is: " + playerTransform.gameObject.name);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false)
            return;

        playerInRange = false;
    }

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}
