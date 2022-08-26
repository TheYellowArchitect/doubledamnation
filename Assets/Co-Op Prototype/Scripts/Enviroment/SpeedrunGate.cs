using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Notifies SpeedrunManager, that this gate is activated.
/// Every level (except level editor) has its own speedrun gate.
/// </summary>
public class SpeedrunGate : MonoBehaviour
{
    [ReadOnly] public bool speedgateActivated = false;

    private void Start()
    {
        //Sets the speedrun Gate to be active VISUALLY (has animated sprite and VFX or sth)
        //The visuals fall 100% on the child, hence... ;)
        //SpeedrunGate activates the child instead of PlayerStatsManager
        //because PlayerStatsManager activates the gate's child only when speedrun is toggled to true

        if (PlayerStatsManager.globalInstance.GetSpeedrunMode() == true)
            transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        else
            transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (speedgateActivated == false)
        {
            speedgateActivated = true;

            //Coulda cache SpeedrunManager, but it would be less read-able, and double in text lmao
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().SpeedrunGateActivated();

            //If speedrun mode, stop the VFX emission
            if (PlayerStatsManager.globalInstance.GetSpeedrunMode() == true)
            {
                transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Stop();

                //TODO: Get some SFX for activating/passing through it.
            }

        }
        
    }

    
}
