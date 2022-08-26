using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaColliderManager : MonoBehaviour
{
    //REMINDER:  The layer of this object must be "PlayerExclusively" so you won't have to check if collision entered is player or not!
    //REMINDER2: The children **by default** are de-activated!

    private Collider2D playerRegisteredCollider;

    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        if (playerRegisteredCollider != null)
            return;

        //^If no player collider has been detected, activate and register
            //Register
            playerRegisteredCollider = playerCollider;

            //Activate the waves! Drop the frames!!!
            transform.GetChild(0).gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D playerCollider)
    {
        if (playerRegisteredCollider == null)
            return;

        if (playerRegisteredCollider == playerCollider)
        {
            //De-register ;)
            playerRegisteredCollider = null;

            //De-Activate the waves! Restore the frames!!!
            transform.GetChild(0).gameObject.SetActive(false);
        }
        
    }
}
