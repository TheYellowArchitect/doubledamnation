using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempHPCoinBehaviour : MonoBehaviour
{
    //Not needed, since it collides only with player.
    //[Tooltip("What layer is player in? So, only he, can pick it up")]
    //public LayerMask WhatIsPlayer;

    private bool toggled = false;

    //Picked up by player, PlayerExclusively LAYER collides ONLY with Player Layer, so no need to filter.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (toggled == false)
        {
            toggled = true;
            //Filters all of the collisions, to be ONLY the ones with player.//Not anymore, its by default.
            //if (collision.gameObject.CompareTag("Player")){}

            //Increases TempHP by 1
            collision.gameObject.GetComponent<WarriorHealth>().RewardTempHP(collision.gameObject.GetComponent<WarriorHealth>().NormalMaxHealth);

            //Instead of destroying this, I disable this gameobject.
            //Why? Because when player dies, level is reset, and >LevelManager< re-activates every coin, like this one :D
            this.gameObject.SetActive(false);
        }

    }

    private void OnEnable()
    {
        toggled = false;
    }
}
