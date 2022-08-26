using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is attached to the brazier gameobject/prefab and should activate the flame if you died.
/// If the flame is activated, it also plays/stops the flame VFX, when player is out of range (collider2D trigger)
/// </summary>
public class BrazierBehaviour : MonoBehaviour
{
    public bool diedOnce = false;

    public bool ablazed = false;

    public bool unsubscribed = false;

    public ParticleSystem childFireVFX;
	
	void Start ()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedDying += OnAblaze;

        childFireVFX = transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
    }



    public void OnAblaze()
    {
        if (diedOnce == true)
        {
            //Unsubscribe so this never fires off again.
            if (unsubscribed == false)
                UnsubscribeFromPlayer();

            return;
        }

        diedOnce = true;

        ablazed = true;

        childFireVFX.Play();
    }

    //Below 2 are with the condition that player has died once.
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>())
            {
                //Debug.Log("Player detected (enter)");

                if (diedOnce && ablazed == false)
                {
                    ablazed = true;

                    childFireVFX.Play();
                }
                    
            }
            
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>())
            {
                //Debug.Log("Player detected (exit)");    

                if (diedOnce && ablazed == true)
                {
                    ablazed = false;

                    childFireVFX.Stop();
                }
                    
            }

            
        }

    //Unsubscribing so it won't bug! ((calling level 1 brazier for example on level 2))
    private void OnDestroy()
    {
        //Unsubscribe so this never fires off again.
        if (unsubscribed == false)
            UnsubscribeFromPlayer();
        
    }

    public void UnsubscribeFromPlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
            GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedDying -= OnAblaze;

        unsubscribed = true;
    }

}
