using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LamentableMemoiresDistanceTrigger : MonoBehaviour
{
    public ParticleSystem siblingVFX;

    public bool toggled = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (toggled == true)
            return;

        siblingVFX.Play();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        siblingVFX.Stop();
    }

    public void SetToggled(bool value)
    {
        toggled = value;
    }

}
