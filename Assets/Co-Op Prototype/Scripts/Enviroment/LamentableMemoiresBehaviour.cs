using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LamentableMemoiresBehaviour : MonoBehaviour
{
    //TODO: Player-Exclusive Layer + Give it proper tag so levermanager picks it up properly.

    private bool toggled = false;

    public ParticleSystem siblingVFX;

    public LamentableMemoiresDistanceTrigger distanceBehaviour;

    //Picked up by player, PlayerExclusively LAYER collides ONLY with Player Layer, so no need to filter.
    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        if (toggled == false)
        {
            toggled = true;

            distanceBehaviour.toggled = true;

            //Maximize dat mana!
            playerCollider.gameObject.GetComponent<WarriorMovement>().SetMana(ManaManager.maximumPlayerMana);

            //TempHP POG
            playerCollider.gameObject.GetComponent<WarriorHealth>().RewardTempHP(playerCollider.gameObject.GetComponent<WarriorHealth>().NormalMaxHealth);

            //Stops the sibling's VFX.
            siblingVFX.Stop();

            //Instead of destroying this, I disable this gameobject.
            //Why? Because when player dies, level is reset, and >LevelManager< re-activates every coin, like this one :D
            this.gameObject.SetActive(false);
        }

    }

    private void OnEnable()
    {
        toggled = false;

        distanceBehaviour.toggled = false;
    }
}

//Leaving this note here, as a hint for those who will really dive deep to find more cool stuff about the game ;)

/*

    Lamentable
    adj.

Inspiring or deserving of lament or regret; deplorable or pitiable. synonym: pathetic.
To be lamented; exciting or calling for sorrow; grievous: as, a lamentable deterioration of morals.
Expressive of grief; mournful: as, a lamentable cry.

(Source of the above: Wordnik from The American Heritage® Dictionary of the English Language, 5th Edition. )

*/








