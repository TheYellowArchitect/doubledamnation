using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircleBehaviour : MonoBehaviour
{
    [Tooltip("How much time for the magic circle itself to collapse")]
    public float timeToShatter;

    [Tooltip("If true, it plays the particle system on start, of 2nd child.")]
    public bool hasStartParticles;

	void Start ()
    {
        if (hasStartParticles)
            transform.GetChild(1).GetComponent<ParticleSystem>().Play();

        Invoke("MagicCircleDeath", timeToShatter);
	}
	
    //Plays kid's particle system, since scale is 0, and it will fuck up the emission here.
	void MagicCircleDeath()
    {
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        Destroy(GetComponent<SpriteRenderer>());
    }
}
