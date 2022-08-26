using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceVFXBehaviour : MonoBehaviour
{
    [Tooltip("When an object enters this layer, the VFX triggers (on/off)")]
    public Collider2D triggerCollider;

    public bool startPlaying = false;

    private ParticleSystem commonParticleSystem;
    private Collider2D storedPlayerCollider;

    private void Start()
    {
        commonParticleSystem = GetComponent<ParticleSystem>();

        storedPlayerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();

        if (startPlaying)
            commonParticleSystem.Play();
    }

    //PlayerExclusive Layer ftw
    private void OnTriggerEnter2D(Collider2D triggeredPlayerCollider)
    {
        if (triggeredPlayerCollider != storedPlayerCollider)
            return;

        Toggle();
    }

    private void OnTriggerExit2D(Collider2D triggeredPlayerCollider)
    {
        if (triggeredPlayerCollider != storedPlayerCollider)
            return;

        Toggle();
    }

    private void Toggle()
    {
        if (commonParticleSystem.isPlaying)
            commonParticleSystem.Stop();
        else
            commonParticleSystem.Play();
    }
}
